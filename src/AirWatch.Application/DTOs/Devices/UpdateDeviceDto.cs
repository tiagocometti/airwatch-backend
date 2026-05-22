namespace AirWatch.Application.DTOs.Devices;

public record UpdateDeviceDto(string Name, string Location, bool? IsActive);
