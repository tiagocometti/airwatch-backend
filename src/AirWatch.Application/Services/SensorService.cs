using AirWatch.Application.DTOs.Sensors;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Domain.Exceptions;

namespace AirWatch.Application.Services;

public class SensorService(ISensorRepository sensorRepository)
{
    public async Task<SensorDto> RegisterAsync(RegisterSensorDto dto)
    {
        var existing = await sensorRepository.GetByExternalIdAsync(dto.ExternalId);
        if (existing is not null)
            throw new ConflictException($"Já existe um sensor cadastrado com o identificador '{dto.ExternalId}'.");

        var sensor = new Sensor
        {
            Id = Guid.NewGuid(),
            ExternalId = dto.ExternalId,
            Name = dto.Name,
            Location = dto.Location,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow
        };

        await sensorRepository.AddAsync(sensor);

        return ToDto(sensor);
    }

    public async Task<IEnumerable<SensorDto>> GetAllAsync()
    {
        var sensors = await sensorRepository.GetAllAsync();
        return sensors.Select(ToDto);
    }

    public async Task<SensorDto?> GetByExternalIdAsync(string externalId)
    {
        var sensor = await sensorRepository.GetByExternalIdAsync(externalId);
        return sensor is null ? null : ToDto(sensor);
    }

    private static SensorDto ToDto(Sensor s) =>
        new(s.Id, s.ExternalId, s.Name, s.Location, s.IsActive, s.RegisteredAt);
}
