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
        var sensorRepository = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
        var measurementService = scope.ServiceProvider.GetRequiredService<MeasurementService>();

        var sensors = await sensorRepository.GetAllAsync();
        var activeSensors = sensors.Where(s => s.IsActive).ToList();

        if (activeSensors.Count == 0)
        {
            logger.LogDebug("Simulador: nenhum sensor ativo encontrado.");
            return;
        }

        foreach (var sensor in activeSensors)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var dto = new CreateMeasurementDto
            {
                SensorId   = sensor.ExternalId,
                GasValue   = _random.Next(200, 801),
                Temperature = Math.Round(20.0 + _random.NextDouble() * 15.0, 1),
                Humidity   = Math.Round(40.0 + _random.NextDouble() * 40.0, 1),
                Timestamp  = DateTime.UtcNow
            };

            await measurementService.RecordAsync(dto);

            logger.LogDebug(
                "Simulador: medição gerada para '{SensorId}' — gás={Gas}, temp={Temp}°C, umidade={Humidity}%",
                sensor.ExternalId, dto.GasValue, dto.Temperature, dto.Humidity);
        }

        logger.LogInformation("Simulador: {Count} medição(ões) gerada(s).", activeSensors.Count);
    }
}
