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

    public async Task<IEnumerable<Measurement>> GetBySensorIdAsync(Guid sensorId, int limit = 100)
    {
        return await context.Measurements
            .Where(m => m.SensorId == sensorId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .Include(m => m.Sensor)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Measurement>> GetByPeriodAsync(DateTime from, DateTime to)
    {
        return await context.Measurements
            .Where(m => m.Timestamp >= from && m.Timestamp <= to)
            .OrderByDescending(m => m.Timestamp)
            .Include(m => m.Sensor)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Measurement>> GetLatestAsync(int limit = 50)
    {
        return await context.Measurements
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .Include(m => m.Sensor)
            .AsNoTracking()
            .ToListAsync();
    }
}
