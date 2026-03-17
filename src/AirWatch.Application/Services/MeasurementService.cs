using AirWatch.Application.DTOs.Common;
using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Domain.Exceptions;

namespace AirWatch.Application.Services;

public class MeasurementService(IMeasurementRepository measurementRepository, ISensorRepository sensorRepository)
{
    public async Task<MeasurementDto> RecordAsync(CreateMeasurementDto dto)
    {
        var sensor = await sensorRepository.GetByExternalIdAsync(dto.SensorId)
            ?? throw new NotFoundException($"Sensor '{dto.SensorId}' não encontrado.");

        var measurement = new Measurement
        {
            Id          = Guid.NewGuid(),
            SensorId    = sensor.Id,
            GasValue    = dto.GasValue,
            Temperature = dto.Temperature,
            Humidity    = dto.Humidity,
            Timestamp   = dto.Timestamp
        };

        await measurementRepository.AddAsync(measurement);

        return ToDto(measurement, sensor.ExternalId);
    }

    public async Task<PagedResultDto<MeasurementDto>> GetBySensorExternalIdAsync(string externalId, int page, int pageSize)
    {
        var sensor = await sensorRepository.GetByExternalIdAsync(externalId)
            ?? throw new NotFoundException($"Sensor '{externalId}' não encontrado.");

        var (items, total) = await measurementRepository.GetBySensorIdAsync(sensor.Id, page, pageSize);

        return new PagedResultDto<MeasurementDto>
        {
            Items      = items.Select(m => ToDto(m, externalId)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<PagedResultDto<MeasurementDto>> GetLatestAsync(int page, int pageSize)
    {
        var (items, total) = await measurementRepository.GetLatestAsync(page, pageSize);

        return new PagedResultDto<MeasurementDto>
        {
            Items      = items.Select(m => ToDto(m, m.Sensor.ExternalId)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<PagedResultDto<MeasurementDto>> GetByPeriodAsync(DateTime from, DateTime to, int page, int pageSize)
    {
        var (items, total) = await measurementRepository.GetByPeriodAsync(from, to, page, pageSize);

        return new PagedResultDto<MeasurementDto>
        {
            Items      = items.Select(m => ToDto(m, m.Sensor.ExternalId)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    private static MeasurementDto ToDto(Measurement m, string sensorExternalId) =>
        new(m.Id, m.SensorId, sensorExternalId, m.GasValue, m.Temperature, m.Humidity, m.Timestamp);
}
