using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Infrastructure.Repositories;

public class SensorCoefficientRepository(AppDbContext context) : ISensorCoefficientRepository
{
    public async Task<IEnumerable<SensorCoefficient>> GetAllAsync()
    {
        return await context.SensorCoefficients
            .AsNoTracking()
            .ToListAsync();
    }
}
