namespace AirWatch.Application.DTOs.SensorCoefficients;

public record SensorCoefficientThresholdDto(
    string GasTarget,
    double SafeMax,
    double GoodMax,
    double AlertMax
);
