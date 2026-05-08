using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces.Repositories;

public interface ISensorCoefficientRepository
{
    Task<IEnumerable<SensorCoefficient>> GetAllAsync();
}
