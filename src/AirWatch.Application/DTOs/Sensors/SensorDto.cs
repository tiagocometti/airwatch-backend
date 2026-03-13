namespace AirWatch.Application.DTOs.Sensors;

public record SensorDto(
    Guid Id,
    string ExternalId,
    string Name,
    string Location,
    bool IsActive,
    DateTime RegisteredAt
);
