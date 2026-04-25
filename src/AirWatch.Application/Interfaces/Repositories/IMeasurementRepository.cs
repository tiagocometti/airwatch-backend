using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface IMeasurementRepository
{
    Task AddManyAsync(IEnumerable<Measurement> measurements);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByDeviceIdAsync(Guid deviceId, int page, int pageSize);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByDeviceIdAndSensorTypeAsync(Guid deviceId, string sensorType, int page, int pageSize);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByPeriodAsync(DateTime from, DateTime to, int page, int pageSize);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetLatestAsync(int page, int pageSize);
}
