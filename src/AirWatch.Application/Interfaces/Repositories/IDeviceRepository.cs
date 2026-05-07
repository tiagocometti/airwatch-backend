using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id);
    Task<Device?> GetByExternalIdAsync(string externalId);
    Task<Device?> GetByExternalIdAsync(string externalId, Guid userId);
    Task<IEnumerable<Device>> GetAllAsync();
    Task<IEnumerable<Device>> GetAllByUserAsync(Guid userId);
    Task AddAsync(Device device);
    Task UpdateAsync(Device device);
    Task UpdateLastSeenAsync(Guid deviceId, DateTime lastSeen);
    Task SetOnlineAsync(string externalId, DateTime lastSeen);
    Task SetOfflineAsync(string externalId);
}
