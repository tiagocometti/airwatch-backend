using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirWatch.Infrastructure.BackgroundServices;

public class MqttSimulatorService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<MqttSimulatorService> logger) : BackgroundService
{
    private readonly int _intervalSeconds = configuration.GetValue<int>("Simulator:IntervalSeconds", 30);
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Simulador MQTT iniciado. Intervalo: {Interval}s", _intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SimulateAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }

        logger.LogInformation("Simulador MQTT encerrado.");
    }

    private async Task SimulateAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
        var measurementService = scope.ServiceProvider.GetRequiredService<MeasurementService>();

        var devices = await deviceRepository.GetAllAsync();
        var activeDevices = devices.Where(d => d.IsActive).ToList();

        if (activeDevices.Count == 0)
        {
            logger.LogDebug("Simulador: nenhum dispositivo ativo encontrado.");
            return;
        }

        var sensorTypes = new[] { "mq3", "mq5", "mq135" };
        var timestamp = DateTime.UtcNow;

        foreach (var device in activeDevices)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var dtos = sensorTypes.Select(sensorType => new CreateMeasurementDto
            {
                DeviceId   = device.Id,
                SensorType = sensorType,
                Calibrated = true,
                Timestamp  = timestamp,
                AdcRaw     = _random.Next(300, 600),
                VoltageV   = Math.Round(1.5 + _random.NextDouble() * 1.5, 2),
                RsOhm      = Math.Round(8000.0 + _random.NextDouble() * 6000.0, 1),
                RsR0Ratio  = Math.Round(0.6 + _random.NextDouble() * 0.6, 2),
                Ppm        = Math.Round(50.0 + _random.NextDouble() * 600.0, 1)
            }).ToList();

            await measurementService.RecordManyAsync(dtos);

            logger.LogDebug(
                "Simulador: {Count} medições geradas para '{DeviceId}'.",
                dtos.Count, device.ExternalId);
        }

        logger.LogInformation("Simulador: medições geradas para {Count} dispositivo(s).", activeDevices.Count);
    }
}
