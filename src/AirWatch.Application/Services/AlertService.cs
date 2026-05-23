using AirWatch.Application.Interfaces;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;

namespace AirWatch.Application.Services;

public class AlertService(
    IAlertRepository alertRepository,
    IUserRepository  userRepository,
    IEmailService    emailService)
    : IAlertService
{
    public async Task ProcessAsync(Device device, Measurement measurement)
    {
        if (measurement.IqaiCategory != "Perigo")
            return;

        var cutoff      = measurement.Timestamp.AddMinutes(-60);
        var recentAlert = await alertRepository.GetRecentAsync(device.Id, cutoff);

        var alert = new Alert
        {
            Id          = Guid.NewGuid(),
            DeviceId    = device.Id,
            UserId      = device.UserId,
            TriggeredAt = measurement.Timestamp,
            EmailSent   = false
        };

        if (recentAlert is not null)
        {
            await alertRepository.AddAsync(alert);
            return;
        }

        var user = await userRepository.GetByIdAsync(device.UserId);
        if (user is not null && user.EmailNotificationsEnabled)
        {
            try
            {
                await emailService.SendAlertAsync(user.Email, device, measurement);
                alert.EmailSent = true;
            }
            catch
            {
                // Falha de email não deve impedir o registro do alerta
            }
        }

        await alertRepository.AddAsync(alert);
    }
}
