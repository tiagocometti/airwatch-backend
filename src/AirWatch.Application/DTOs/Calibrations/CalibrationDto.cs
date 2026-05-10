namespace AirWatch.Application.DTOs.Calibrations;

public record CalibrationDto(
    Guid Id,
    Guid DeviceId,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string Status,
    string Location,
    double? R0Mq3,
    double? R0Mq5,
    double? R0Mq135,
    int SampleCount,
    int DuracaoSegundos,
    bool IsActive
);
