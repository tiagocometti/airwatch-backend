using System.Text;
using System.Text.Json;
using AirWatch.Application.DTOs.Measurements;
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
/// Conecta ao broker MQTT via TLS e persiste as medições recebidas do Arduino.
/// </summary>
public class MqttSubscriberService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<MqttSubscriberService> logger) : BackgroundService
{
    private readonly string _host  = configuration["MqttSubscriber:Host"]!;
    private readonly int    _port  = configuration.GetValue<int>("MqttSubscriber:Port", 8883);
    private readonly string _user  = configuration["MqttSubscriber:Username"]!;
    private readonly string _pass  = configuration["MqttSubscriber:Password"]!;
    private readonly string _topic = configuration.GetValue<string>("MqttSubscriber:Topic", "airwatch/sensors")!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Serviço MQTT iniciado. Broker: {Host}:{Port}, tópico: {Topic}", _host, _port, _topic);

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

        await client.SubscribeAsync(
            new MqttTopicFilterBuilder().WithTopic(_topic).Build(),
            stoppingToken);

        logger.LogInformation("MQTT: inscrito no tópico '{Topic}'.", _topic);

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

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var raw = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

        logger.LogDebug("MQTT: mensagem recebida em '{Topic}': {Payload}", args.ApplicationMessage.Topic, raw);

        MqttPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<MqttPayload>(raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            using var scope = scopeFactory.CreateScope();
            var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
            var measurementService = scope.ServiceProvider.GetRequiredService<MeasurementService>();

            var device = await deviceRepository.GetByExternalIdAsync(payload.DeviceId);
            if (device is null)
            {
                logger.LogWarning("MQTT: dispositivo '{DeviceId}' não cadastrado, mensagem ignorada.", payload.DeviceId);
                return;
            }

            var dtos = payload.Sensors.Select(kvp => new CreateMeasurementDto
            {
                DeviceId   = device.Id,
                SensorType = kvp.Key,
                Calibrated = payload.Calibrated,
                Timestamp  = payload.Timestamp == default ? DateTime.UtcNow : payload.Timestamp,
                AdcRaw     = kvp.Value.AdcRaw,
                VoltageV   = kvp.Value.VoltageV,
                RsOhm      = kvp.Value.RsOhm,
                RsR0Ratio  = kvp.Value.RsR0Ratio,
                Ppm        = kvp.Value.Ppm
            }).ToList();

            await measurementService.RecordManyAsync(dtos);

            logger.LogInformation(
                "MQTT: {Count} medição(ões) salvas para dispositivo '{DeviceId}'.",
                dtos.Count, payload.DeviceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MQTT: erro ao salvar medições para dispositivo '{DeviceId}'.", payload.DeviceId);
        }
    }

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
