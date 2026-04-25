using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Infrastructure.Repositories;

public class DeviceRepository(AppDbContext context) : IDeviceRepository
{
    public async Task<Device?> GetByIdAsync(Guid id)
    {
        return await context.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Device?> GetByExternalIdAsync(string externalId)
    {
        return await context.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ExternalId == externalId);
    }

    public async Task<IEnumerable<Device>> GetAllAsync()
    {
        return await context.Devices
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Device device)
    {
        await context.Devices.AddAsync(device);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Device device)
    {
        context.Devices.Update(device);
        await context.SaveChangesAsync();
    }
}
