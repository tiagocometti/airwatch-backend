namespace AirWatch.Application.Interfaces;

public interface IMqttPublisher
{
    Task PublishAsync(string topic, string payload);
}
