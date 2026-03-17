using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface IMeasurementRepository
{
    Task AddAsync(Measurement measurement);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetBySensorIdAsync(Guid sensorId, int page, int pageSize);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByPeriodAsync(DateTime from, DateTime to, int page, int pageSize);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetLatestAsync(int page, int pageSize);
}
