using System.Text;
using System.Text.Json;
using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;

namespace AirWatch.Infrastructure.BackgroundServices;

/// <summary>
/// Serviço que se conecta a um broker MQTT via TLS e persiste as medições
/// recebidas no banco de dados usando o mesmo fluxo do simulador.
/// </summary>
public class MqttSubscriberService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<MqttSubscriberService> logger) : BackgroundService
{
    private readonly string _host   = configuration["MqttSubscriber:Host"]!;
    private readonly int    _port   = configuration.GetValue<int>("MqttSubscriber:Port", 8883);
    private readonly string _user   = configuration["MqttSubscriber:Username"]!;
    private readonly string _pass   = configuration["MqttSubscriber:Password"]!;
    private readonly string _topic  = configuration.GetValue<string>("MqttSubscriber:Topic", "airwatch/sensors")!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Serviço MQTT iniciado. Broker: {Host}:{Port}, tópico: {Topic}",
            _host, _port, _topic);

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

    // -----------------------------------------------------------------------

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

        // Aguarda até desconexão ou cancelamento
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        client.DisconnectedAsync += args =>
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
        var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

        logger.LogDebug("MQTT: mensagem recebida em '{Topic}': {Payload}",
            args.ApplicationMessage.Topic, payload);

        MqttPayload? data;
        try
        {
            data = JsonSerializer.Deserialize<MqttPayload>(payload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "MQTT: payload JSON inválido, mensagem ignorada.");
            return;
        }

        if (data is null || string.IsNullOrWhiteSpace(data.SensorId))
        {
            logger.LogWarning("MQTT: payload sem sensorId, mensagem ignorada. Payload: {Payload}", payload);
            return;
        }

        var dto = new CreateMeasurementDto
        {
            SensorId    = data.SensorId,
            GasValue    = data.GasValue,
            Temperature = data.Temperature,
            Humidity    = data.Humidity,
            Timestamp   = data.Timestamp == default ? DateTime.UtcNow : data.Timestamp
        };

        try
        {
            using var scope = scopeFactory.CreateScope();
            var measurementService = scope.ServiceProvider.GetRequiredService<MeasurementService>();
            await measurementService.RecordAsync(dto);

            logger.LogInformation(
                "MQTT: medição salva para sensor '{SensorId}' — gás={Gas}, temp={Temp}°C, umidade={Humidity}%",
                dto.SensorId, dto.GasValue, dto.Temperature, dto.Humidity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MQTT: erro ao salvar medição para sensor '{SensorId}'.", dto.SensorId);
        }
    }

    // -----------------------------------------------------------------------

    private sealed class MqttPayload
    {
        public string   SensorId    { get; init; } = string.Empty;
        public double   GasValue    { get; init; }
        public double   Temperature { get; init; }
        public double   Humidity    { get; init; }
        public DateTime Timestamp   { get; init; }
    }
}
