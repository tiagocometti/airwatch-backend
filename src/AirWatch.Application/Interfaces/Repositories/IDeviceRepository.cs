using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id);
    Task<Device?> GetByExternalIdAsync(string externalId);
    Task<IEnumerable<Device>> GetAllAsync();
    Task AddAsync(Device device);
    Task UpdateAsync(Device device);
}
