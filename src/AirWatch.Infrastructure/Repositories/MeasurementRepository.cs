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

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetBySensorIdAsync(Guid sensorId, int page, int pageSize)
    {
        var query = context.Measurements
            .Where(m => m.SensorId == sensorId)
            .OrderByDescending(m => m.Timestamp);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sensor)
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
            .Include(m => m.Sensor)
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
            .Include(m => m.Sensor)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }
}
