namespace AirWatch.Application.DTOs.Measurements;

public record MeasurementDto(
    Guid Id,
    string DeviceExternalId,
    string SensorType,
    bool Calibrated,
    int AdcRaw,
    double VoltageV,
    double RsOhm,
    double RsR0Ratio,
    double Ppm,
    DateTime Timestamp
);
