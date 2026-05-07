namespace AirWatch.Application.Interfaces;

public interface IDeviceStatusNotifier
{
    Task NotifyStatusChangedAsync(string deviceExternalId, bool isOnline, DateTime? lastSeen);
}
