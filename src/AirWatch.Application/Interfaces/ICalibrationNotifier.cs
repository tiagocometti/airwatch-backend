namespace AirWatch.Application.Interfaces;

public interface ICalibrationNotifier
{
    Task NotifyStartedAsync(string deviceId, Guid calibrationId, DateTime startedAt, int duracaoSegundos);
    Task NotifyProgressAsync(string deviceId, Guid calibrationId, double progressPercent, int sampleCount, double currentR0Mq3, double currentR0Mq5, double currentR0Mq135);
    Task NotifyCompletedAsync(string deviceId, Guid calibrationId, double r0Mq3, double r0Mq5, double r0Mq135);
    Task NotifyFailedAsync(string deviceId, Guid calibrationId, string reason);
    Task NotifyCancelledAsync(string deviceId, Guid calibrationId);
}
