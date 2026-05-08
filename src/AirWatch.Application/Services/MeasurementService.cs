using AirWatch.Application.DTOs.Common;
using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Domain.Exceptions;

namespace AirWatch.Application.Services;

public class MeasurementService(IMeasurementRepository measurementRepository, IDeviceRepository deviceRepository)
{
    public async Task<MeasurementDto> RecordAsync(Measurement measurement, string deviceExternalId)
    {
        await measurementRepository.AddAsync(measurement);
        return ToDto(measurement, deviceExternalId);
    }

    public async Task<PagedResultDto<MeasurementDto>> GetByDeviceExternalIdAsync(
        string externalId, Guid userId, int page, int pageSize)
    {
        var device = await deviceRepository.GetByExternalIdAsync(externalId, userId)
            ?? throw new NotFoundException($"Dispositivo '{externalId}' não encontrado.");

        var (items, total) = await measurementRepository.GetByDeviceIdAsync(device.Id, page, pageSize);

        return new PagedResultDto<MeasurementDto>
        {
            Items      = items.Select(m => ToDto(m, externalId)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<PagedResultDto<MeasurementDto>> GetLatestAsync(Guid userId, int page, int pageSize)
    {
        var (items, total) = await measurementRepository.GetLatestAsync(userId, page, pageSize);

        return new PagedResultDto<MeasurementDto>
        {
            Items      = items.Select(m => ToDto(m, m.Device.ExternalId)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<PagedResultDto<MeasurementDto>> GetByPeriodAsync(
        Guid userId, DateTime from, DateTime to, int page, int pageSize)
    {
        var (items, total) = await measurementRepository.GetByPeriodAsync(userId, from, to, page, pageSize);

        return new PagedResultDto<MeasurementDto>
        {
            Items      = items.Select(m => ToDto(m, m.Device.ExternalId)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    private static MeasurementDto ToDto(Measurement m, string deviceExternalId) =>
        new(m.Id, deviceExternalId, m.Timestamp,
            m.Mq3Adc, m.Mq5Adc, m.Mq135Adc,
            m.PpmAlcohol, m.PpmLpg, m.PpmCo2, m.PpmNh3);
}
