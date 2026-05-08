using AirWatch.Api.Hubs;
using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AirWatch.Api.Services;

public class SignalRMeasurementNotifier(IHubContext<DeviceStatusHub> hubContext) : IMeasurementNotifier
{
    public async Task NotifyNewMeasurementAsync(MeasurementDto measurement)
    {
        await hubContext.Clients.All.SendAsync("NewMeasurement", measurement);
    }
}
