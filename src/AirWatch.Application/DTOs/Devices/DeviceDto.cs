namespace AirWatch.Application.DTOs.Devices;

public record DeviceDto(
    Guid Id,
    string ExternalId,
    string Name,
    string Location,
    bool IsActive,
    DateTime RegisteredAt
);
