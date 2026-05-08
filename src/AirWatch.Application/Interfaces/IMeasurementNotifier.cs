using AirWatch.Application.DTOs.Measurements;

namespace AirWatch.Application.Interfaces;

public interface IMeasurementNotifier
{
    Task NotifyNewMeasurementAsync(MeasurementDto measurement);
}
