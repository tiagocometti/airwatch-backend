using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;

namespace AirWatch.Application.Services;

public class MeasurementCalculationService(ICalibrationRepository calibrationRepo)
{
    private const double Vc     = 5.0;
    private const int    AdcMax = 1023;

    public async Task<Measurement?> CalculateAsync(Device device, int adcMq3, int adcMq5, int adcMq135)
    {
        var calibration = await calibrationRepo.GetActiveByDeviceIdAsync(device.Id);
        if (calibration is null) return null;

        var (ratioMq3,   degMq3)   = ComputeRatioAndDeg(adcMq3,   device.RlMq3,   calibration.R0Mq3!.Value);
        var (ratioMq5,   degMq5)   = ComputeRatioAndDeg(adcMq5,   device.RlMq5,   calibration.R0Mq5!.Value);
        var (ratioMq135, degMq135) = ComputeRatioAndDeg(adcMq135, device.RlMq135, calibration.R0Mq135!.Value);

        var iqai     = Math.Sqrt((Math.Pow(degMq3, 2) + Math.Pow(degMq5, 2) + Math.Pow(degMq135, 2)) / 3.0);
        var category = iqai switch
        {
            <= 0.35 => "Boa",
            <= 0.70 => "Moderada",
            <= 1.20 => "Alerta",
            _       => "Perigo"
        };

        return new Measurement
        {
            Id           = Guid.NewGuid(),
            DeviceId     = device.Id,
            Timestamp    = DateTime.UtcNow,
            Mq3Adc       = adcMq3,
            Mq5Adc       = adcMq5,
            Mq135Adc     = adcMq135,
            RatioMq3     = ratioMq3,
            RatioMq5     = ratioMq5,
            RatioMq135   = ratioMq135,
            DegMq3       = degMq3,
            DegMq5       = degMq5,
            DegMq135     = degMq135,
            Iqai         = iqai,
            IqaiCategory = category
        };
    }

    private static (double ratio, double deg) ComputeRatioAndDeg(int adc, double rl, double r0)
    {
        double vrl = adc * (Vc / AdcMax);
        if (vrl <= 0) vrl = 0.01;

        double rs    = ((Vc / vrl) - 1.0) * rl;
        double ratio = rs / r0;
        double deg   = Math.Max(0.0, -Math.Log(ratio));

        return (ratio, deg);
    }
}
