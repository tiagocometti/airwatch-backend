namespace AirWatch.Application.DTOs.Measurements;

public record MeasurementDto(
    Guid     Id,
    string   DeviceId,
    DateTime Timestamp,
    int      Mq3Adc,
    int      Mq5Adc,
    int      Mq135Adc,
    double   RatioMq3,
    double   RatioMq5,
    double   RatioMq135,
    double   DegMq3,
    double   DegMq5,
    double   DegMq135,
    double   Iqai,
    string   IqaiCategory
);
