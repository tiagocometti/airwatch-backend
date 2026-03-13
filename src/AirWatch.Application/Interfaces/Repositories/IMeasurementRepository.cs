using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface IMeasurementRepository
{
    Task AddAsync(Measurement measurement);
    Task<IEnumerable<Measurement>> GetBySensorIdAsync(Guid sensorId, int limit = 100);
    Task<IEnumerable<Measurement>> GetByPeriodAsync(DateTime from, DateTime to);
    Task<IEnumerable<Measurement>> GetLatestAsync(int limit = 50);
}
