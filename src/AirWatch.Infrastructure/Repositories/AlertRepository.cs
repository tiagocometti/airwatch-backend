using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirWatch.Infrastructure.Repositories;

public class AlertRepository(AppDbContext context) : IAlertRepository
{
    public async Task<Alert?> GetRecentAsync(Guid deviceId, DateTime since)
        => await context.Alerts
            .Where(a => a.DeviceId == deviceId && a.TriggeredAt >= since)
            .OrderByDescending(a => a.TriggeredAt)
            .AsNoTracking()
            .FirstOrDefaultAsync();

    public async Task AddAsync(Alert alert)
    {
        await context.Alerts.AddAsync(alert);
        await context.SaveChangesAsync();
    }
}
