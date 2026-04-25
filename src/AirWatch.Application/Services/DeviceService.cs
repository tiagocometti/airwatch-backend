using AirWatch.Application.DTOs.Devices;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Domain.Exceptions;

namespace AirWatch.Application.Services;

public class DeviceService(IDeviceRepository deviceRepository)
{
    public async Task<DeviceDto> RegisterAsync(CreateDeviceDto dto)
    {
        var existing = await deviceRepository.GetByExternalIdAsync(dto.ExternalId);
        if (existing is not null)
            throw new ConflictException($"Já existe um dispositivo cadastrado com o identificador '{dto.ExternalId}'.");

        var device = new Device
        {
            Id           = Guid.NewGuid(),
            ExternalId   = dto.ExternalId,
            Name         = dto.Name,
            Location     = dto.Location,
            IsActive     = true,
            RegisteredAt = DateTime.UtcNow
        };

        await deviceRepository.AddAsync(device);

        return ToDto(device);
    }

    public async Task<IEnumerable<DeviceDto>> GetAllAsync()
    {
        var devices = await deviceRepository.GetAllAsync();
        return devices.Select(ToDto);
    }

    public async Task<DeviceDto?> GetByExternalIdAsync(string externalId)
    {
        var device = await deviceRepository.GetByExternalIdAsync(externalId);
        return device is null ? null : ToDto(device);
    }

    private static DeviceDto ToDto(Device d) =>
        new(d.Id, d.ExternalId, d.Name, d.Location, d.IsActive, d.RegisteredAt);
}
