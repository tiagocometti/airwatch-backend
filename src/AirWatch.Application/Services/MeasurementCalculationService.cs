using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;

namespace AirWatch.Application.Services;

public class MeasurementCalculationService(ISensorCoefficientRepository coeffRepo)
{
    private const double Vc     = 5.0;
    private const int    AdcMax = 1023;

    public async Task<Measurement> CalculateAsync(Device device, int adcMq3, int adcMq5, int adcMq135)
    {
        var allCoefs = (await coeffRepo.GetAllAsync()).ToList();

        var coeffAlcohol = allCoefs.FirstOrDefault(c => c.SensorType == "MQ3"   && c.GasTarget == "Alcohol");
        var coeffLpg     = allCoefs.FirstOrDefault(c => c.SensorType == "MQ5"   && c.GasTarget == "LPG");
        var coeffCo2     = allCoefs.FirstOrDefault(c => c.SensorType == "MQ135" && c.GasTarget == "CO2");
        var coeffNh3     = allCoefs.FirstOrDefault(c => c.SensorType == "MQ135" && c.GasTarget == "NH3");

        return new Measurement
        {
            Id         = Guid.NewGuid(),
            DeviceId   = device.Id,
            Timestamp  = DateTime.UtcNow,
            Mq3Adc     = adcMq3,
            Mq5Adc     = adcMq5,
            Mq135Adc   = adcMq135,
            PpmAlcohol = ComputePpm(adcMq3,   device.RlMq3,   device.R0Mq3,   coeffAlcohol),
            PpmLpg     = ComputePpm(adcMq5,   device.RlMq5,   device.R0Mq5,   coeffLpg),
            PpmCo2     = ComputePpm(adcMq135,  device.RlMq135, device.R0Mq135, coeffCo2),
            PpmNh3     = ComputePpm(adcMq135,  device.RlMq135, device.R0Mq135, coeffNh3)
        };
    }

    private static double ComputePpm(int adc, double rl, double r0, SensorCoefficient? coef)
    {
        if (coef is null) return 0.0;

        double vrl = adc * (Vc / AdcMax);
        if (vrl <= 0) vrl = 0.01;

        double rs    = ((Vc / vrl) - 1.0) * rl;
        double ratio = rs / r0;

        return coef.CoefA * Math.Pow(ratio, coef.CoefB);
    }
}
