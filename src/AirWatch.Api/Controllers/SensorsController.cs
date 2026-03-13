using AirWatch.Application.DTOs.Sensors;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorsController(SensorService sensorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterSensorDto dto)
    {
        var result = await sensorService.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetByExternalId), new { externalId = result.ExternalId }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sensors = await sensorService.GetAllAsync();
        return Ok(sensors);
    }

    [HttpGet("{externalId}")]
    public async Task<IActionResult> GetByExternalId(string externalId)
    {
        var sensor = await sensorService.GetByExternalIdAsync(externalId);
        if (sensor is null)
            return NotFound();

        return Ok(sensor);
    }
}
