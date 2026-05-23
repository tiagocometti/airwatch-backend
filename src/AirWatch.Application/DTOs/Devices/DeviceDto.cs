namespace AirWatch.Application.DTOs.Devices;

public record DeviceDto(
    Guid Id,
    string ExternalId,
    string Name,
    bool IsActive,
    bool IsOnline,
    DateTime? LastSeen,
    DateTime RegisteredAt
);
