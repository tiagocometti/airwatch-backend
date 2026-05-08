namespace AirWatch.Application.DTOs.Measurements;

public record MeasurementDto(
    Guid     Id,
    string   DeviceId,
    DateTime Timestamp,
    int      Mq3Adc,
    int      Mq5Adc,
    int      Mq135Adc,
    double   PpmAlcohol,
    double   PpmLpg,
    double   PpmCo2,
    double   PpmNh3
);
