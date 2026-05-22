using System.Collections.Concurrent;
using AirWatch.Application.Interfaces;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirWatch.Infrastructure.BackgroundServices;

public class CalibrationBackgroundService(
    IServiceScopeFactory    scopeFactory,
    ICalibrationNotifier    notifier,
    IMqttPublisher          mqttPublisher,
    ILogger<CalibrationBackgroundService> logger)
    : IHostedService, ICalibrationManager, ICalibrationSampleHandler
{
    private const int DuracaoSegundos = 60;
    private const string LedCalibrationOff   = "led_calibration_off";
    private const string LedCalibrationBlink = "led_calibration_blink";
    private const string LedCalibrationOn    = "led_calibration_on";

    private readonly ConcurrentDictionary<string, CalibrationSession> _sessions = new();

    // ─── IHostedService ────────────────────────────────────────────────────────

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = CleanupInProgressAsync();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var session in _sessions.Values)
            session.UserCts.Cancel();
        return Task.CompletedTask;
    }

    // ─── ICalibrationManager ───────────────────────────────────────────────────

    public async Task<Guid> StartCalibrationAsync(Guid deviceId, string location)
    {
        Device device;
        using (var scope = scopeFactory.CreateScope())
        {
            var deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
            device = await deviceRepo.GetByIdAsync(deviceId)
                     ?? throw new NotFoundException($"Dispositivo '{deviceId}' não encontrado.");
        }

        if (_sessions.ContainsKey(device.ExternalId))
            throw new ConflictException($"O dispositivo '{device.ExternalId}' já possui uma calibração em andamento.");

        var calibrationId = Guid.NewGuid();
        var now           = DateTime.UtcNow;

        using (var scope = scopeFactory.CreateScope())
        {
            var calibrationRepo = scope.ServiceProvider.GetRequiredService<ICalibrationRepository>();
            await calibrationRepo.AddAsync(new Calibration
            {
                Id              = calibrationId,
                DeviceId        = deviceId,
                StartedAt       = now,
                Status          = CalibrationStatus.InProgress,
                Location        = location,
                SampleCount     = 0,
                DuracaoSegundos = DuracaoSegundos
            });
        }

        var commandTopic = $"airwatch/{device.ExternalId}/commands";
        await mqttPublisher.PublishAsync(commandTopic, "start_calibration");
        await mqttPublisher.PublishAsync(commandTopic, LedCalibrationBlink);

        await notifier.NotifyStartedAsync(device.ExternalId, calibrationId, now, DuracaoSegundos);

        var session = new CalibrationSession
        {
            CalibrationId = calibrationId,
            DeviceId      = deviceId,
            ExternalId    = device.ExternalId,
            RlMq3         = device.RlMq3,
            RlMq5         = device.RlMq5,
            RlMq135       = device.RlMq135,
            Inicio        = now,
            UserCts       = new CancellationTokenSource()
        };

        _sessions[device.ExternalId] = session;

        _ = RunTimerAsync(device.ExternalId, session);

        logger.LogInformation("Calibração {Id} iniciada para '{DeviceId}'.", calibrationId, device.ExternalId);
        return calibrationId;
    }

    public async Task CancelCalibrationAsync(Guid calibrationId)
    {
        var entry = _sessions.FirstOrDefault(kv => kv.Value.CalibrationId == calibrationId);
        if (entry.Key is null)
        {
            logger.LogWarning("CancelCalibration: sessão para calibração {Id} não encontrada.", calibrationId);
            return;
        }

        var externalId = entry.Key;
        var session    = entry.Value;

        session.UserCts.Cancel();
        _sessions.TryRemove(externalId, out _);

        int sampleCount;
        lock (session.Lock) { sampleCount = session.AmostrasMq3.Count; }

        await FinalizarNoBancoAsync(calibrationId, sampleCount, CalibrationStatus.Cancelled,
            completedAt: DateTime.UtcNow);

        var commandTopic = $"airwatch/{externalId}/commands";
        await mqttPublisher.PublishAsync(commandTopic, "stop_calibration");
        await PublishCalibrationLedStateAsync(externalId, session.DeviceId);

        await notifier.NotifyCancelledAsync(externalId, calibrationId);

        logger.LogInformation("Calibração {Id} cancelada para '{DeviceId}'.", calibrationId, externalId);
    }

    // ─── ICalibrationSampleHandler ─────────────────────────────────────────────

    public async Task ProcessSampleAsync(string deviceId, string csvPayload)
    {
        if (!_sessions.TryGetValue(deviceId, out var session)) return;

        var parts = csvPayload.Split(',');
        if (parts.Length < 4) return;

        if (!int.TryParse(parts[1].Trim(), out var adcMq3)   ||
            !int.TryParse(parts[2].Trim(), out var adcMq5)   ||
            !int.TryParse(parts[3].Trim(), out var adcMq135))
            return;

        const double Vc     = 5.0;
        const int    AdcMax = 1023;

        double VrlMq3   = adcMq3   * (Vc / AdcMax);
        double VrlMq5   = adcMq5   * (Vc / AdcMax);
        double VrlMq135 = adcMq135 * (Vc / AdcMax);
        if (VrlMq3   <= 0) VrlMq3   = 0.01;
        if (VrlMq5   <= 0) VrlMq5   = 0.01;
        if (VrlMq135 <= 0) VrlMq135 = 0.01;

        double rsMq3   = ((Vc / VrlMq3)   - 1.0) * session.RlMq3;
        double rsMq5   = ((Vc / VrlMq5)   - 1.0) * session.RlMq5;
        double rsMq135 = ((Vc / VrlMq135) - 1.0) * session.RlMq135;

        lock (session.Lock)
        {
            session.AmostrasMq3.Add(rsMq3);
            session.AmostrasMq5.Add(rsMq5);
            session.AmostrasMq135.Add(rsMq135);
        }

        double mediaMq3, mediaMq5, mediaMq135;
        int    sampleCount;
        lock (session.Lock)
        {
            mediaMq3    = session.AmostrasMq3.Average();
            mediaMq5    = session.AmostrasMq5.Average();
            mediaMq135  = session.AmostrasMq135.Average();
            sampleCount = session.AmostrasMq3.Count;
        }

        double elapsed  = (DateTime.UtcNow - session.Inicio).TotalSeconds;
        double progress = Math.Min(elapsed / DuracaoSegundos * 100.0, 100.0);

        await notifier.NotifyProgressAsync(
            deviceId, session.CalibrationId, progress, sampleCount,
            mediaMq3, mediaMq5, mediaMq135);
    }

    // ─── Timer ─────────────────────────────────────────────────────────────────

    private async Task RunTimerAsync(string externalId, CalibrationSession session)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(DuracaoSegundos), session.UserCts.Token);
            await FinalizarCalibracaoAsync(externalId);
        }
        catch (OperationCanceledException) when (session.UserCts.IsCancellationRequested)
        {
            // Cancelado explicitamente pelo usuário via /cancel — cleanup já feito em CancelCalibrationAsync
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro no timer de calibração para '{DeviceId}'.", externalId);
        }

        // Safety timer: só age se for exatamente esta sessão (evita cancelar nova calibração
        // que iniciou logo após esta terminar)
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(120));
            if (_sessions.TryGetValue(externalId, out var s) && s.CalibrationId == session.CalibrationId)
            {
                logger.LogWarning("Safety timeout ativado para '{DeviceId}'.", externalId);
                await CancelCalibrationAsync(s.CalibrationId);
            }
        });
    }

    // ─── Finalização ───────────────────────────────────────────────────────────

    private async Task FinalizarCalibracaoAsync(string externalId)
    {
        if (!_sessions.TryRemove(externalId, out var session)) return;

        double[] amMq3, amMq5, amMq135;
        lock (session.Lock)
        {
            amMq3   = [.. session.AmostrasMq3];
            amMq5   = [.. session.AmostrasMq5];
            amMq135 = [.. session.AmostrasMq135];
        }

        if (amMq3.Length == 0)
        {
            await FinalizarNoBancoAsync(session.CalibrationId, 0, CalibrationStatus.Failed, DateTime.UtcNow);
            await mqttPublisher.PublishAsync($"airwatch/{externalId}/commands", "stop_calibration");
            await PublishCalibrationLedStateAsync(externalId, session.DeviceId);
            await notifier.NotifyFailedAsync(externalId, session.CalibrationId, "Nenhuma amostra coletada durante a calibração.");
            logger.LogWarning("Calibração {Id} falhou para '{DeviceId}': sem amostras.", session.CalibrationId, externalId);
            return;
        }

        double mediaMq3   = amMq3.Average();
        double mediaMq5   = amMq5.Average();
        double mediaMq135 = amMq135.Average();
        var now = DateTime.UtcNow;

        using (var scope = scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ICalibrationRepository>();

            await repo.DeactivateAllByDeviceIdAsync(session.DeviceId, session.CalibrationId);

            var calibration = await repo.GetByIdAsync(session.CalibrationId);
            if (calibration is not null)
            {
                calibration.Status      = CalibrationStatus.Completed;
                calibration.CompletedAt = now;
                calibration.R0Mq3       = mediaMq3;
                calibration.R0Mq5       = mediaMq5;
                calibration.R0Mq135     = mediaMq135;
                calibration.SampleCount = amMq3.Length;
                calibration.IsActive    = true;
                await repo.UpdateAsync(calibration);
            }
        }

        await mqttPublisher.PublishAsync($"airwatch/{externalId}/commands", "stop_calibration");
        await mqttPublisher.PublishAsync($"airwatch/{externalId}/commands", LedCalibrationOn);
        await notifier.NotifyCompletedAsync(externalId, session.CalibrationId, mediaMq3, mediaMq5, mediaMq135);

        logger.LogInformation(
            "Calibração {Id} concluída para '{DeviceId}': R0 MQ3={R0Mq3:F0} MQ5={R0Mq5:F0} MQ135={R0Mq135:F0}.",
            session.CalibrationId, externalId, mediaMq3, mediaMq5, mediaMq135);
    }

    private async Task FinalizarNoBancoAsync(
        Guid calibrationId, int sampleCount, CalibrationStatus status,
        DateTime? completedAt = null,
        double? r0Mq3 = null, double? r0Mq5 = null, double? r0Mq135 = null)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICalibrationRepository>();

        var calibration = await repo.GetByIdAsync(calibrationId);
        if (calibration is null) return;

        calibration.Status      = status;
        calibration.CompletedAt = completedAt ?? DateTime.UtcNow;
        calibration.SampleCount = sampleCount;
        calibration.R0Mq3       = r0Mq3;
        calibration.R0Mq5       = r0Mq5;
        calibration.R0Mq135     = r0Mq135;
        await repo.UpdateAsync(calibration);
    }

    private async Task PublishCalibrationLedStateAsync(string externalId, Guid deviceId)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICalibrationRepository>();

        var activeCalibration = await repo.GetActiveByDeviceIdAsync(deviceId);
        var payload = activeCalibration is null ? LedCalibrationOff : LedCalibrationOn;

        await mqttPublisher.PublishAsync($"airwatch/{externalId}/commands", payload);
    }

    // ─── Startup cleanup ───────────────────────────────────────────────────────

    private async Task CleanupInProgressAsync()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ICalibrationRepository>();
            var deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

            var stale = await repo.GetInProgressAsync();
            foreach (var c in stale)
            {
                c.Status      = CalibrationStatus.Failed;
                c.CompletedAt = DateTime.UtcNow;
                await repo.UpdateAsync(c);

                var device = await deviceRepo.GetByIdAsync(c.DeviceId);
                if (device is not null)
                    await PublishCalibrationLedStateAsync(device.ExternalId, c.DeviceId);

                logger.LogWarning("Calibração {Id} marcada como Failed (restart do servidor).", c.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao limpar calibrações InProgress na inicialização.");
        }
    }

    // ─── Inner types ───────────────────────────────────────────────────────────

    private sealed class CalibrationSession
    {
        public Guid   CalibrationId { get; init; }
        public Guid   DeviceId      { get; init; }
        public string ExternalId    { get; init; } = string.Empty;
        public double RlMq3         { get; init; }
        public double RlMq5         { get; init; }
        public double RlMq135       { get; init; }
        public DateTime Inicio      { get; init; }
        public CancellationTokenSource UserCts { get; init; } = null!;

        public readonly List<double> AmostrasMq3   = [];
        public readonly List<double> AmostrasMq5   = [];
        public readonly List<double> AmostrasMq135 = [];
        public readonly object Lock = new();
    }
}
