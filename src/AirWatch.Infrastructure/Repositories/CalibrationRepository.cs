using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Infrastructure.Repositories;

public class CalibrationRepository(AppDbContext context) : ICalibrationRepository
{
    public async Task<Calibration?> GetByIdAsync(Guid id)
        => await context.Calibrations
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Calibration?> GetActiveByDeviceIdAsync(Guid deviceId)
        => await context.Calibrations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.DeviceId == deviceId && c.IsActive);

    public async Task<IEnumerable<Calibration>> GetByDeviceIdAsync(Guid deviceId)
        => await context.Calibrations
            .AsNoTracking()
            .Where(c => c.DeviceId == deviceId)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync();

    public async Task<IEnumerable<Calibration>> GetInProgressAsync()
        => await context.Calibrations
            .Where(c => c.Status == CalibrationStatus.InProgress)
            .ToListAsync();

    public async Task AddAsync(Calibration calibration)
    {
        await context.Calibrations.AddAsync(calibration);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Calibration calibration)
    {
        context.Calibrations.Update(calibration);
        await context.SaveChangesAsync();
    }

    public async Task DeactivateAllByDeviceIdAsync(Guid deviceId, Guid exceptCalibrationId)
    {
        await context.Calibrations
            .Where(c => c.DeviceId == deviceId && c.Id != exceptCalibrationId && c.IsActive)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsActive, false));
    }

    public async Task DeleteAsync(Calibration calibration)
    {
        context.Calibrations.Remove(calibration);
        await context.SaveChangesAsync();
    }
}
