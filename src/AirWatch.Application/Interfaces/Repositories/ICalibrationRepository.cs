using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface ICalibrationRepository
{
    Task<Calibration?> GetByIdAsync(Guid id);
    Task<Calibration?> GetActiveByDeviceIdAsync(Guid deviceId);
    Task<IEnumerable<Calibration>> GetByDeviceIdAsync(Guid deviceId);
    Task<IEnumerable<Calibration>> GetInProgressAsync();
    Task AddAsync(Calibration calibration);
    Task UpdateAsync(Calibration calibration);
    Task DeactivateAllByDeviceIdAsync(Guid deviceId, Guid exceptCalibrationId);
}
