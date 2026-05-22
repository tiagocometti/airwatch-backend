namespace AirWatch.Application.DTOs.Measurements;

public record MeasurementHistoryDto(
    DateTime Timestamp,
    double   Iqai,
    string   IqaiCategory
);
