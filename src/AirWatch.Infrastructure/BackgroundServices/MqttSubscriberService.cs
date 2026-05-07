using System.Text;
using System.Text.Json;
using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Interfaces;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;

namespace AirWatch.Infrastructure.BackgroundServices;

/// <summary>
/// Conecta ao broker MQTT via TLS, assina os tópicos de telemetria e de status,
/// persiste as medições recebidas e atualiza o estado online/offline dos dispositivos
/// com base em mensagens LWT publicadas pelo próprio broker.
/// </summary>
public class MqttSubscriberService(
    IServiceScopeFactory scopeFactory,
    IDeviceStatusNotifier statusNotifier,
    IConfiguration configuration,
    ILogger<MqttSubscriberService> logger) : BackgroundService
{
    private readonly string _host         = configuration["MqttSubscriber:Host"]!;
    private readonly int    _port         = configuration.GetValue<int>("MqttSubscriber:Port", 8883);
    private readonly string _user         = configuration["MqttSubscriber:Username"]!;
    private readonly string _pass         = configuration["MqttSubscriber:Password"]!;
    private readonly string _sensorsTopic = configuration.GetValue<string>("MqttSubscriber:Topic", "airwatch/sensors")!;

    // Tópico de status: airwatch/devices/{externalId}/status  (+ = wildcard de um nível)
    private const string StatusTopicFilter = "airwatch/devices/+/status";

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Serviço MQTT iniciado. Broker: {Host}:{Port}, tópicos: [{Sensors}] e [{Status}]",
            _host, _port, _sensorsTopic, StatusTopicFilter);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndListenAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "MQTT: conexão encerrada inesperadamente. Reconectando em 5s…");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        logger.LogInformation("Serviço MQTT encerrado.");
    }

    private async Task ConnectAndListenAsync(CancellationToken stoppingToken)
    {
        using var client = new MqttFactory().CreateMqttClient();

        client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_host, _port)
            .WithCredentials(_user, _pass)
            .WithTlsOptions(o => o.UseTls())
            .WithCleanSession()
            .Build();

        await client.ConnectAsync(options, stoppingToken);
        logger.LogInformation("MQTT: conectado ao broker {Host}:{Port}.", _host, _port);

        // Assinar tópico de telemetria
        await client.SubscribeAsync(
            new MqttTopicFilterBuilder().WithTopic(_sensorsTopic).Build(),
            stoppingToken);

        // Assinar tópico de status (LWT + birth messages do ESP)
        // O broker entregará imediatamente a última mensagem retida de cada dispositivo.
        await client.SubscribeAsync(
            new MqttTopicFilterBuilder().WithTopic(StatusTopicFilter).Build(),
            stoppingToken);

        logger.LogInformation(
            "MQTT: inscrito nos tópicos '{Sensors}' e '{Status}'.",
            _sensorsTopic, StatusTopicFilter);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        client.DisconnectedAsync += _ =>
        {
            logger.LogWarning("MQTT: desconectado do broker.");
            tcs.TrySetResult(true);
            return Task.CompletedTask;
        };

        await using var reg = stoppingToken.Register(() => tcs.TrySetCanceled());
        await tcs.Task;

        if (client.IsConnected)
            await client.DisconnectAsync(cancellationToken: CancellationToken.None);
    }

    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;

        if (IsStatusTopic(topic))
            return HandleStatusMessageAsync(args);

        return HandleSensorMessageAsync(args);
    }

    // ─── Roteamento ────────────────────────────────────────────────────────────

    /// <summary>Retorna true se o tópico segue o padrão airwatch/devices/{id}/status.</summary>
    private static bool IsStatusTopic(string topic)
    {
        var parts = topic.Split('/');
        return parts.Length == 4
            && parts[0] == "airwatch"
            && parts[1] == "devices"
            && parts[3] == "status";
    }

    /// <summary>Extrai o externalId do tópico airwatch/devices/{externalId}/status.</summary>
    private static string ExtractExternalId(string topic) => topic.Split('/')[2];

    // ─── Handler de status (online / offline via LWT) ──────────────────────────

    private async Task HandleStatusMessageAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic      = args.ApplicationMessage.Topic;
        var payload    = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).Trim().ToLower();
        var externalId = ExtractExternalId(topic);

        logger.LogDebug("MQTT: mensagem de status recebida — dispositivo '{DeviceId}', estado '{Payload}'.",
            externalId, payload);

        if (payload is not ("online" or "offline"))
        {
            logger.LogWarning(
                "MQTT: payload de status inválido para '{DeviceId}': '{Payload}'. Ignorando.",
                externalId, payload);
            return;
        }

        try
        {
            using var scope          = scopeFactory.CreateScope();
            var deviceRepository     = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

            var device = await deviceRepository.GetByExternalIdAsync(externalId);
            if (device is null)
            {
                logger.LogWarning(
                    "MQTT: dispositivo '{DeviceId}' não cadastrado, mensagem de status ignorada.",
                    externalId);
                return;
            }

            var isOnline = payload == "online";
            var now      = DateTime.UtcNow;

            if (isOnline)
            {
                await deviceRepository.SetOnlineAsync(externalId, now);
                logger.LogInformation("Dispositivo '{DeviceId}' está ONLINE.", externalId);
            }
            else
            {
                await deviceRepository.SetOfflineAsync(externalId);
                logger.LogInformation("Dispositivo '{DeviceId}' está OFFLINE.", externalId);
            }

            await statusNotifier.NotifyStatusChangedAsync(
                externalId,
                isOnline,
                isOnline ? now : device.LastSeen);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MQTT: erro ao processar status do dispositivo '{DeviceId}'.", externalId);
        }
    }

    // ─── Handler de telemetria (medições dos sensores) ─────────────────────────

    private async Task HandleSensorMessageAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var raw = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

        logger.LogDebug("MQTT: mensagem de telemetria recebida em '{Topic}': {Payload}",
            args.ApplicationMessage.Topic, raw);

        MqttPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<MqttPayload>(raw, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "MQTT: payload JSON inválido, mensagem ignorada.");
            return;
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.DeviceId))
        {
            logger.LogWarning("MQTT: payload sem deviceId, mensagem ignorada. Payload: {Payload}", raw);
            return;
        }

        try
        {
            using var scope            = scopeFactory.CreateScope();
            var deviceRepository       = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
            var measurementService     = scope.ServiceProvider.GetRequiredService<MeasurementService>();

            var device = await deviceRepository.GetByExternalIdAsync(payload.DeviceId);
            if (device is null)
            {
                logger.LogWarning(
                    "MQTT: dispositivo '{DeviceId}' não cadastrado, mensagem ignorada.",
                    payload.DeviceId);
                return;
            }

            var timestamp = payload.Timestamp == default ? DateTime.UtcNow : payload.Timestamp;

            // Salvar medições
            List<CreateMeasurementDto> dtos;
            if (payload.Sensors.Count > 0)
            {
                dtos = payload.Sensors.Select(kvp => new CreateMeasurementDto
                {
                    DeviceId   = device.Id,
                    SensorType = kvp.Key,
                    Calibrated = payload.Calibrated,
                    Timestamp  = timestamp,
                    AdcRaw     = kvp.Value.AdcRaw,
                    VoltageV   = kvp.Value.VoltageV,
                    RsOhm      = kvp.Value.RsOhm,
                    RsR0Ratio  = kvp.Value.RsR0Ratio,
                    Ppm        = kvp.Value.Ppm
                }).ToList();
            }
            else
            {
                // Payload de calibração: sem leituras de sensor, grava registros zerados
                dtos = new[] { "mq3", "mq5", "mq135" }.Select(sensorType => new CreateMeasurementDto
                {
                    DeviceId   = device.Id,
                    SensorType = sensorType,
                    Calibrated = false,
                    Timestamp  = timestamp,
                    AdcRaw     = 0,
                    VoltageV   = 0,
                    RsOhm      = 0,
                    RsR0Ratio  = 0,
                    Ppm        = 0
                }).ToList();
            }

            await measurementService.RecordManyAsync(dtos);

            // Atualizar apenas o timestamp da última leitura — IsOnline é controlado pelo tópico de status
            await deviceRepository.UpdateLastSeenAsync(device.Id, timestamp);

            logger.LogInformation(
                "MQTT: {Count} medição(ões) salvas para dispositivo '{DeviceId}'.",
                dtos.Count, payload.DeviceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MQTT: erro ao salvar medições para dispositivo '{DeviceId}'.", payload.DeviceId);
        }
    }

    // ─── Tipos internos ────────────────────────────────────────────────────────

    private sealed class MqttPayload
    {
        public string DeviceId { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public bool Calibrated { get; init; }
        public Dictionary<string, SensorReading> Sensors { get; init; } = new();
    }

    private sealed class SensorReading
    {
        public int AdcRaw { get; init; }
        public double VoltageV { get; init; }
        public double RsOhm { get; init; }
        public double RsR0Ratio { get; init; }
        public double Ppm { get; init; }
    }
}
