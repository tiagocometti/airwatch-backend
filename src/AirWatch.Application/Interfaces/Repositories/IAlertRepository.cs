using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface IAlertRepository
{
    Task<Alert?> GetRecentAsync(Guid deviceId, DateTime since);
    Task AddAsync(Alert alert);
}
