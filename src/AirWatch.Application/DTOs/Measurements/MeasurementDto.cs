namespace AirWatch.Application.DTOs.Measurements;

public record MeasurementDto(
    Guid Id,
    Guid SensorId,
    string SensorExternalId,
    double GasValue,
    double Temperature,
    double Humidity,
    DateTime Timestamp
);
