using AirWatch.Api.Hubs;
using AirWatch.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AirWatch.Api.Services;

public class SignalRDeviceStatusNotifier(IHubContext<DeviceStatusHub> hubContext) : IDeviceStatusNotifier
{
    public async Task NotifyStatusChangedAsync(string deviceExternalId, bool isOnline, DateTime? lastSeen)
    {
        await hubContext.Clients.All.SendAsync("DeviceStatusChanged", new
        {
            deviceId = deviceExternalId,
            isOnline,
            lastSeen
        });
    }
}
