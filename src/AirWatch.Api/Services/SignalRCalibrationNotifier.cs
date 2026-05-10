using AirWatch.Api.Hubs;
using AirWatch.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AirWatch.Api.Services;

public class SignalRCalibrationNotifier(IHubContext<DeviceStatusHub> hubContext) : ICalibrationNotifier
{
    public Task NotifyStartedAsync(string deviceId, Guid calibrationId, DateTime startedAt, int duracaoSegundos)
        => hubContext.Clients.All.SendAsync("CalibrationStarted", new { deviceId, calibrationId, startedAt, duracaoSegundos });

    public Task NotifyProgressAsync(string deviceId, Guid calibrationId, double progressPercent, int sampleCount,
        double currentR0Mq3, double currentR0Mq5, double currentR0Mq135)
        => hubContext.Clients.All.SendAsync("CalibrationProgress", new
        {
            deviceId, calibrationId, progressPercent, sampleCount,
            currentR0Mq3, currentR0Mq5, currentR0Mq135
        });

    public Task NotifyCompletedAsync(string deviceId, Guid calibrationId, double r0Mq3, double r0Mq5, double r0Mq135)
        => hubContext.Clients.All.SendAsync("CalibrationCompleted", new { deviceId, calibrationId, r0Mq3, r0Mq5, r0Mq135 });

    public Task NotifyFailedAsync(string deviceId, Guid calibrationId, string reason)
        => hubContext.Clients.All.SendAsync("CalibrationFailed", new { deviceId, calibrationId, reason });

    public Task NotifyCancelledAsync(string deviceId, Guid calibrationId)
        => hubContext.Clients.All.SendAsync("CalibrationCancelled", new { deviceId, calibrationId });
}
