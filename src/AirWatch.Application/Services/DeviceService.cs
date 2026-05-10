using AirWatch.Application.DTOs.Devices;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Domain.Exceptions;

namespace AirWatch.Application.Services;

public class DeviceService(IDeviceRepository deviceRepository)
{
    public async Task<DeviceDto> RegisterAsync(CreateDeviceDto dto, Guid userId)
    {
        var existing = await deviceRepository.GetByExternalIdAsync(dto.ExternalId, userId);
        if (existing is not null)
            throw new ConflictException($"Você já possui um dispositivo cadastrado com o identificador '{dto.ExternalId}'.");

        var device = new Device
        {
            Id           = Guid.NewGuid(),
            UserId       = userId,
            ExternalId   = dto.ExternalId,
            Name         = dto.Name,
            Location     = dto.Location,
            IsActive     = true,
            IsOnline     = false,
            RegisteredAt = DateTime.UtcNow
        };

        await deviceRepository.AddAsync(device);
        return ToDto(device);
    }

    public async Task<IEnumerable<DeviceDto>> GetAllAsync(Guid userId)
    {
        var devices = await deviceRepository.GetAllByUserAsync(userId);
        return devices.Select(ToDto);
    }

    public async Task<DeviceDto?> GetByExternalIdAsync(string externalId, Guid userId)
    {
        var device = await deviceRepository.GetByExternalIdAsync(externalId, userId);
        return device is null ? null : ToDto(device);
    }

    public async Task<DeviceDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var device = await deviceRepository.GetByIdAsync(id);
        if (device is null || device.UserId != userId) return null;
        return ToDto(device);
    }

    public async Task<DeviceDto> UpdateAsync(Guid id, Guid userId, UpdateDeviceDto dto)
    {
        var device = await deviceRepository.GetByIdAsync(id);
        if (device is null || device.UserId != userId)
            throw new NotFoundException($"Dispositivo '{id}' não encontrado.");

        device.Name     = dto.Name;
        device.Location = dto.Location;
        await deviceRepository.UpdateAsync(device);
        return ToDto(device);
    }

    private static DeviceDto ToDto(Device d) =>
        new(d.Id, d.ExternalId, d.Name, d.Location, d.IsActive, d.IsOnline, d.LastSeen, d.RegisteredAt);
}
