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

    public async Task<Device?> GetByExternalIdAsync(string externalId, Guid userId)
    {
        return await context.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ExternalId == externalId && d.UserId == userId);
    }

    public async Task<IEnumerable<Device>> GetAllAsync()
    {
        return await context.Devices
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Device>> GetAllByUserAsync(Guid userId)
    {
        return await context.Devices
            .AsNoTracking()
            .Where(d => d.UserId == userId)
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

    public async Task UpdateLastSeenAsync(Guid deviceId, DateTime lastSeen)
    {
        await context.Devices
            .Where(d => d.Id == deviceId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(d => d.LastSeen, lastSeen));
    }

    public async Task SetOnlineAsync(string externalId, DateTime lastSeen)
    {
        await context.Devices
            .Where(d => d.ExternalId == externalId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(d => d.IsOnline, true)
                .SetProperty(d => d.LastSeen, lastSeen));
    }

    public async Task SetOfflineAsync(string externalId)
    {
        await context.Devices
            .Where(d => d.ExternalId == externalId)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.IsOnline, false));
    }
}
