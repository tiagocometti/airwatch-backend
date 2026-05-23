using System.Reflection;
using System.Text;
using AirWatch.Application.Interfaces;
using AirWatch.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace AirWatch.Infrastructure.Services;

public class EmailSettings
{
    public string SmtpHost     { get; set; } = "smtp.gmail.com";
    public int    SmtpPort     { get; set; } = 587;
    public string SenderEmail  { get; set; } = string.Empty;
    public string SenderName   { get; set; } = "AirWatch";
    public string AppPassword  { get; set; } = string.Empty;
    public string DashboardUrl { get; set; } = "http://localhost:4200";
}

public class MailKitEmailService(
    IOptions<EmailSettings> options,
    ILogger<MailKitEmailService> logger)
    : IEmailService
{
    private static readonly TimeZoneInfo BrasiliaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

    private readonly EmailSettings _settings = options.Value;

    public async Task SendAlertAsync(string recipientEmail, Device device, Measurement measurement)
    {
        var measurementTime = TimeZoneInfo.ConvertTimeFromUtc(measurement.Timestamp, BrasiliaTimeZone)
                                         .ToString("dd/MM/yyyy HH:mm:ss");

        var template = await LoadTemplateAsync();
        var body = template
            .Replace("{{DeviceName}}",      device.Name)
            .Replace("{{MeasurementTime}}", measurementTime)
            .Replace("{{IqaiValue}}",       measurement.Iqai.ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
            .Replace("{{DashboardUrl}}",    _settings.DashboardUrl);

        var message = new MimeMessage();
        message.Date = DateTimeOffset.Now;
        message.From.Add(new MailboxAddress("AirWatch Monitor IoT", _settings.SenderEmail));
        message.To.Add(new MailboxAddress(string.Empty, recipientEmail));
        message.Subject    = "AirWatch — Alerta de Medição Perigosa";
        message.Importance = MessageImportance.High;
        message.Headers.Add("X-Mailer",    "AirWatch Monitor IoT");
        message.Headers.Add("Organization", "AirWatch");

        var htmlPart = new TextPart(TextFormat.Html);
        htmlPart.SetText(Encoding.UTF8, body);
        message.Body = htmlPart;

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);

        logger.LogDebug("Email enviado para {Email}.", recipientEmail);
    }

    private static async Task<string> LoadTemplateAsync()
    {
        const string resourceName = "AirWatch.Infrastructure.Email.Templates.alert-email.html";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Template de email não encontrado: {resourceName}");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
