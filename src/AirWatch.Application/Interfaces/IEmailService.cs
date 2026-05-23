using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces;

public interface IEmailService
{
    Task SendAlertAsync(string recipientEmail, Device device, Measurement measurement);
}
