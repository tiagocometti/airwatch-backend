namespace AirWatch.Application.Interfaces;

public interface ICalibrationManager
{
    Task<Guid> StartCalibrationAsync(Guid deviceId, string location);
    Task CancelCalibrationAsync(Guid calibrationId);
}
