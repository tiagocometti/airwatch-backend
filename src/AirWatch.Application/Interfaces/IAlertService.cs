using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces;

public interface IAlertService
{
    Task ProcessAsync(Device device, Measurement measurement);
}
