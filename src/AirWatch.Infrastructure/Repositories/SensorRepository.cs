using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Infrastructure.Repositories;

public class SensorRepository(AppDbContext context) : ISensorRepository
{
    public async Task<Sensor?> GetByIdAsync(Guid id)
    {
        return await context.Sensors
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sensor?> GetByExternalIdAsync(string externalId)
    {
        return await context.Sensors
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == externalId);
    }

    public async Task<IEnumerable<Sensor>> GetAllAsync()
    {
        return await context.Sensors
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Sensor sensor)
    {
        await context.Sensors.AddAsync(sensor);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Sensor sensor)
    {
        context.Sensors.Update(sensor);
        await context.SaveChangesAsync();
    }
}
