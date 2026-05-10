using AirWatch.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;

namespace AirWatch.Infrastructure.Services;

public class MqttPublisherService : IMqttPublisher, IAsyncDisposable
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly ILogger<MqttPublisherService> _logger;

    public MqttPublisherService(IConfiguration config, ILogger<MqttPublisherService> logger)
    {
        _logger = logger;
        _client = new MqttFactory().CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(
                config["MqttSubscriber:Host"]!,
                config.GetValue<int>("MqttSubscriber:Port", 8883))
            .WithCredentials(
                config["MqttSubscriber:Username"]!,
                config["MqttSubscriber:Password"]!)
            .WithTlsOptions(o => o.UseTls())
            .WithCleanSession()
            .Build();
    }

    public async Task PublishAsync(string topic, string payload)
    {
        try
        {
            await EnsureConnectedAsync();

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await _client.PublishAsync(message);
            _logger.LogDebug("MQTT publicado: {Topic} = {Payload}", topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MQTT: falha ao publicar em '{Topic}'.", topic);
        }
    }

    private async Task EnsureConnectedAsync()
    {
        if (_client.IsConnected) return;

        await _connectLock.WaitAsync();
        try
        {
            if (!_client.IsConnected)
                await _client.ConnectAsync(_options);
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected)
            await _client.DisconnectAsync(cancellationToken: CancellationToken.None);
        _client.Dispose();
    }
}
