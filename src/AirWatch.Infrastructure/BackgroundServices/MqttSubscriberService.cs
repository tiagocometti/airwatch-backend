using System.Text;
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
/// persiste as medições recebidas e atualiza o estado online/offline dos dispositivos.
/// </summary>
public class MqttSubscriberService(
    IServiceScopeFactory    scopeFactory,
    IDeviceStatusNotifier   statusNotifier,
    IMeasurementNotifier    measurementNotifier,
    ICalibrationSampleHandler calibrationHandler,
    IConfiguration          configuration,
    ILogger<MqttSubscriberService> logger) : BackgroundService
{
    private readonly string _host = configuration["MqttSubscriber:Host"]!;
    private readonly int    _port = configuration.GetValue<int>("MqttSubscriber:Port", 8883);
    private readonly string _user = configuration["MqttSubscriber:Username"]!;
    private readonly string _pass = configuration["MqttSubscriber:Password"]!;

    // Tópico de telemetria com wildcard — airwatch/{deviceId}/telemetry
    private readonly string _telemetryTopicFilter = configuration.GetValue<string>(
        "MqttSubscriber:Topic", "airwatch/+/telemetry")!;

    private const string StatusTopicFilter      = "airwatch/devices/+/status";
    private const string CalibrationTopicFilter = "airwatch/+/calibration";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Serviço MQTT iniciado. Broker: {Host}:{Port}, tópicos: [{Telemetry}] e [{Status}]",
            _host, _port, _telemetryTopicFilter, StatusTopicFilter);

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
            new MqttTopicFilterBuilder().WithTopic(_telemetryTopicFilter).Build(),
            stoppingToken);

        await client.SubscribeAsync(
            new MqttTopicFilterBuilder().WithTopic(StatusTopicFilter).Build(),
            stoppingToken);

        await client.SubscribeAsync(
            new MqttTopicFilterBuilder().WithTopic(CalibrationTopicFilter).Build(),
            stoppingToken);

        logger.LogInformation(
            "MQTT: inscrito nos tópicos '{Telemetry}', '{Calibration}' e '{Status}'.",
            _telemetryTopicFilter, CalibrationTopicFilter, StatusTopicFilter);

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

        if (IsStatusTopic(topic))      return HandleStatusMessageAsync(args);
        if (IsCalibrationTopic(topic)) return HandleCalibrationMessageAsync(args);
        return HandleTelemetryMessageAsync(args);
    }

    // ─── Roteamento ────────────────────────────────────────────────────────────

    private static bool IsStatusTopic(string topic)
    {
        var parts = topic.Split('/');
        return parts.Length == 4
            && parts[0] == "airwatch"
            && parts[1] == "devices"
            && parts[3] == "status";
    }

    private static bool IsCalibrationTopic(string topic)
    {
        var parts = topic.Split('/');
        return parts.Length == 3
            && parts[0] == "airwatch"
            && parts[2] == "calibration";
    }

    private static string ExtractExternalId(string topic) => topic.Split('/')[1];

    private static string ExtractExternalIdFromStatus(string topic) => topic.Split('/')[2];

    // ─── Handler de status (online / offline via LWT) ──────────────────────────

    private async Task HandleStatusMessageAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic      = args.ApplicationMessage.Topic;
        var payload    = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).Trim().ToLower();
        var externalId = ExtractExternalIdFromStatus(topic);

        logger.LogDebug("MQTT: status recebido — dispositivo '{DeviceId}', estado '{Payload}'.",
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

    // ─── Handler de calibração (mesmo CSV, durante modo de calibração) ─────────

    private async Task HandleCalibrationMessageAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic   = args.ApplicationMessage.Topic;
        var raw     = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).Trim();
        var deviceId = ExtractExternalId(topic);

        logger.LogDebug("MQTT: amostra de calibração para '{DeviceId}': {Payload}", deviceId, raw);

        try
        {
            await calibrationHandler.ProcessSampleAsync(deviceId, raw);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MQTT: erro ao processar amostra de calibração para '{DeviceId}'.", deviceId);
        }
    }

    // ─── Handler de telemetria (CSV: deviceId,adc_mq3,adc_mq5,adc_mq135) ──────

    private async Task HandleTelemetryMessageAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var raw = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).Trim();

        logger.LogDebug("MQTT: telemetria recebida em '{Topic}': {Payload}",
            args.ApplicationMessage.Topic, raw);

        var parts = raw.Split(',');
        if (parts.Length != 4)
        {
            logger.LogWarning("MQTT: payload CSV inválido (esperado 4 campos): '{Payload}'. Ignorando.", raw);
            return;
        }

        var externalId = parts[0].Trim();

        if (!int.TryParse(parts[1], out var adcMq3)   ||
            !int.TryParse(parts[2], out var adcMq5)   ||
            !int.TryParse(parts[3], out var adcMq135))
        {
            logger.LogWarning("MQTT: valores ADC inválidos no payload: '{Payload}'. Ignorando.", raw);
            return;
        }

        try
        {
            using var scope       = scopeFactory.CreateScope();
            var deviceRepo        = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
            var calcSvc           = scope.ServiceProvider.GetRequiredService<MeasurementCalculationService>();
            var measurementSvc    = scope.ServiceProvider.GetRequiredService<MeasurementService>();

            var device = await deviceRepo.GetByExternalIdAsync(externalId);
            if (device is null)
            {
                logger.LogWarning(
                    "MQTT: dispositivo '{DeviceId}' não cadastrado, mensagem ignorada.",
                    externalId);
                return;
            }

            var measurement = await calcSvc.CalculateAsync(device, adcMq3, adcMq5, adcMq135);
            if (measurement is null)
            {
                logger.LogDebug("MQTT: '{DeviceId}' sem calibração ativa — medição descartada.", externalId);
                return;
            }

            var dto = await measurementSvc.RecordAsync(measurement, device.ExternalId);

            await deviceRepo.UpdateLastSeenAsync(device.Id, measurement.Timestamp);
            await measurementNotifier.NotifyNewMeasurementAsync(dto);

            logger.LogInformation(
                "MQTT: medição salva para '{DeviceId}' — IQAI: {Iqai:F3} ({Category}).",
                externalId, dto.Iqai, dto.IqaiCategory);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MQTT: erro ao processar telemetria do dispositivo '{DeviceId}'.", externalId);
        }
    }
}
