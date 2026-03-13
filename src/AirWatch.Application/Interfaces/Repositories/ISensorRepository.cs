using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface ISensorRepository
{
    Task<Sensor?> GetByIdAsync(Guid id);
    Task<Sensor?> GetByExternalIdAsync(string externalId);
    Task<IEnumerable<Sensor>> GetAllAsync();
    Task AddAsync(Sensor sensor);
    Task UpdateAsync(Sensor sensor);
}
