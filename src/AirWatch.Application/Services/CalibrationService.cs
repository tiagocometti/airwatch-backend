using AirWatch.Application.DTOs.Calibrations;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Domain.Exceptions;

namespace AirWatch.Application.Services;

public class CalibrationService(ICalibrationRepository calibrationRepo)
{
    public async Task<IEnumerable<CalibrationDto>> GetByDeviceIdAsync(Guid deviceId)
    {
        var items = await calibrationRepo.GetByDeviceIdAsync(deviceId);
        return items.Select(ToDto);
    }

    public async Task<CalibrationDto?> GetActiveByDeviceIdAsync(Guid deviceId)
    {
        var item = await calibrationRepo.GetActiveByDeviceIdAsync(deviceId);
        return item is null ? null : ToDto(item);
    }

    public async Task ActivateCalibrationAsync(Guid calibrationId)
    {
        var calibration = await calibrationRepo.GetByIdAsync(calibrationId)
            ?? throw new NotFoundException($"Calibração '{calibrationId}' não encontrada.");

        if (calibration.Status != CalibrationStatus.Completed)
            throw new ConflictException("Apenas calibrações com status Completed podem ser ativadas.");

        await calibrationRepo.DeactivateAllByDeviceIdAsync(calibration.DeviceId, calibrationId);

        calibration.IsActive = true;
        await calibrationRepo.UpdateAsync(calibration);
    }

    private static CalibrationDto ToDto(Calibration c) =>
        new(c.Id, c.DeviceId, c.StartedAt, c.CompletedAt,
            c.Status.ToString(), c.Location,
            c.R0Mq3, c.R0Mq5, c.R0Mq135,
            c.SampleCount, c.DuracaoSegundos,
            c.IsActive);
}
