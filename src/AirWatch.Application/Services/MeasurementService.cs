using AirWatch.Application.DTOs.Common;
using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Domain.Exceptions;

namespace AirWatch.Application.Services;

public class MeasurementService(IMeasurementRepository measurementRepository, IDeviceRepository deviceRepository)
{
    public async Task RecordManyAsync(IEnumerable<CreateMeasurementDto> dtos)
    {
        var measurements = dtos.Select(dto => new Measurement
        {
            Id         = Guid.NewGuid(),
            DeviceId   = dto.DeviceId,
            SensorType = dto.SensorType,
            Calibrated = dto.Calibrated,
            AdcRaw     = dto.AdcRaw,
            VoltageV   = dto.VoltageV,
            RsOhm      = dto.RsOhm,
            RsR0Ratio  = dto.RsR0Ratio,
            Ppm        = dto.Ppm,
            Timestamp  = dto.Timestamp
        });

        await measurementRepository.AddManyAsync(measurements);
    }

    public async Task<PagedResultDto<MeasurementDto>> GetByDeviceExternalIdAsync(
        string externalId, string? sensorType, int page, int pageSize)
    {
        var device = await deviceRepository.GetByExternalIdAsync(externalId)
            ?? throw new NotFoundException($"Dispositivo '{externalId}' não encontrado.");

        var (items, total) = string.IsNullOrEmpty(sensorType)
            ? await measurementRepository.GetByDeviceIdAsync(device.Id, page, pageSize)
            : await measurementRepository.GetByDeviceIdAndSensorTypeAsync(device.Id, sensorType, page, pageSize);

        return new PagedResultDto<MeasurementDto>
        {
            Items      = items.Select(m => ToDto(m, externalId)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<PagedResultDto<MeasurementDto>> GetLatestAsync(int page, int pageSize)
    {
        var (items, total) = await measurementRepository.GetLatestAsync(page, pageSize);

        return new PagedResultDto<MeasurementDto>
        {
            Items      = items.Select(m => ToDto(m, m.Device.ExternalId)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<PagedResultDto<MeasurementDto>> GetByPeriodAsync(DateTime from, DateTime to, int page, int pageSize)
    {
        var (items, total) = await measurementRepository.GetByPeriodAsync(from, to, page, pageSize);

        return new PagedResultDto<MeasurementDto>
        {
            Items      = items.Select(m => ToDto(m, m.Device.ExternalId)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    private static MeasurementDto ToDto(Measurement m, string deviceExternalId) =>
        new(m.Id, deviceExternalId, m.SensorType, m.Calibrated, m.AdcRaw, m.VoltageV, m.RsOhm, m.RsR0Ratio, m.Ppm, m.Timestamp);
}
