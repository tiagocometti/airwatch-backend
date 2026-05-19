using AirWatch.Application.DTOs.Measurements;
using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface IMeasurementRepository
{
    Task AddAsync(Measurement measurement);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByDeviceIdAsync(Guid deviceId, int page, int pageSize);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByPeriodAsync(Guid userId, DateTime from, DateTime to, int page, int pageSize);
    Task<(IEnumerable<Measurement> Items, int TotalCount)> GetLatestAsync(Guid userId, int page, int pageSize);
    Task<Dictionary<Guid, DateTime>> GetLatestTimestampsByDeviceIdsAsync(IEnumerable<Guid> deviceIds);
    Task<IEnumerable<MeasurementHistoryDto>> GetHistoryAsync(Guid deviceId, DateTime from, DateTime to, string granularity);
}
