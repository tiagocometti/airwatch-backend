using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Infrastructure.Repositories;

public class MeasurementRepository(AppDbContext context) : IMeasurementRepository
{
    public async Task AddManyAsync(IEnumerable<Measurement> measurements)
    {
        await context.Measurements.AddRangeAsync(measurements);
        await context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByDeviceIdAsync(Guid deviceId, int page, int pageSize)
    {
        var query = context.Measurements
            .Where(m => m.DeviceId == deviceId)
            .OrderByDescending(m => m.Timestamp);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Device)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByDeviceIdAndSensorTypeAsync(
        Guid deviceId, string sensorType, int page, int pageSize)
    {
        var query = context.Measurements
            .Where(m => m.DeviceId == deviceId && m.SensorType == sensorType)
            .OrderByDescending(m => m.Timestamp);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Device)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByPeriodAsync(DateTime from, DateTime to, int page, int pageSize)
    {
        var query = context.Measurements
            .Where(m => m.Timestamp >= from && m.Timestamp <= to)
            .OrderByDescending(m => m.Timestamp);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Device)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetLatestAsync(int page, int pageSize)
    {
        var query = context.Measurements
            .OrderByDescending(m => m.Timestamp);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Device)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }
}
