namespace AirWatch.Application.DTOs.Measurements;

public record MeasurementHistoryDto(
    DateTime Timestamp,
    double   PpmCo2,
    double   PpmNh3,
    double   PpmLpg,
    double   PpmAlcohol
);
