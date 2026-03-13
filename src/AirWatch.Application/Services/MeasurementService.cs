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
            ?? throw new NotFoundException($"Sensor '{dto.SensorId}' not found.");

        var measurement = new Measurement
        {
            Id = Guid.NewGuid(),
            SensorId = sensor.Id,
            GasValue = dto.GasValue,
            Temperature = dto.Temperature,
            Humidity = dto.Humidity,
            Timestamp = dto.Timestamp
        };

        await measurementRepository.AddAsync(measurement);

        return ToDto(measurement, sensor.ExternalId);
    }

    public async Task<IEnumerable<MeasurementDto>> GetBySensorExternalIdAsync(string externalId, int limit = 100)
    {
        var sensor = await sensorRepository.GetByExternalIdAsync(externalId)
            ?? throw new NotFoundException($"Sensor '{externalId}' not found.");

        var measurements = await measurementRepository.GetBySensorIdAsync(sensor.Id, limit);

        return measurements.Select(m => ToDto(m, externalId));
    }

    public async Task<IEnumerable<MeasurementDto>> GetLatestAsync(int limit = 50)
    {
        var measurements = await measurementRepository.GetLatestAsync(limit);

        return measurements.Select(m => ToDto(m, m.Sensor.ExternalId));
    }

    public async Task<IEnumerable<MeasurementDto>> GetByPeriodAsync(DateTime from, DateTime to)
    {
        var measurements = await measurementRepository.GetByPeriodAsync(from, to);

        return measurements.Select(m => ToDto(m, m.Sensor.ExternalId));
    }

    private static MeasurementDto ToDto(Measurement m, string sensorExternalId) =>
        new(m.Id, m.SensorId, sensorExternalId, m.GasValue, m.Temperature, m.Humidity, m.Timestamp);
}
