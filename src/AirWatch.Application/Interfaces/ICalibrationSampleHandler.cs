namespace AirWatch.Application.Interfaces;

public interface ICalibrationSampleHandler
{
    Task ProcessSampleAsync(string deviceId, string csvPayload);
}
