using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Infrastructure.Repositories;

public class MeasurementRepository(AppDbContext context) : IMeasurementRepository
{
    public async Task AddAsync(Measurement measurement)
    {
        await context.Measurements.AddAsync(measurement);
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

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByPeriodAsync(
        Guid userId, DateTime from, DateTime to, int page, int pageSize)
    {
        var query = context.Measurements
            .Where(m => m.Device.UserId == userId && m.Timestamp >= from && m.Timestamp <= to)
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

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetLatestAsync(Guid userId, int page, int pageSize)
    {
        var query = context.Measurements
            .Where(m => m.Device.UserId == userId)
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

    public async Task<Dictionary<Guid, DateTime>> GetLatestTimestampsByDeviceIdsAsync(IEnumerable<Guid> deviceIds)
    {
        var ids = deviceIds.ToList();
        return await context.Measurements
            .Where(m => ids.Contains(m.DeviceId))
            .GroupBy(m => m.DeviceId)
            .Select(g => new { DeviceId = g.Key, Latest = g.Max(m => m.Timestamp) })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Latest);
    }
}
