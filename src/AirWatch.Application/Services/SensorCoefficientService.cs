using AirWatch.Application.DTOs.SensorCoefficients;
using AirWatch.Application.Interfaces.Repositories;

namespace AirWatch.Application.Services;

public class SensorCoefficientService(ISensorCoefficientRepository repository)
{
    public async Task<IEnumerable<SensorCoefficientThresholdDto>> GetThresholdsAsync()
    {
        var coefficients = await repository.GetAllAsync();
        return coefficients.Select(c => new SensorCoefficientThresholdDto(
            c.GasTarget,
            c.SafeMax,
            c.GoodMax,
            c.AlertMax
        ));
    }
}
