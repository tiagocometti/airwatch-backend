using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeasurementsController(MeasurementService measurementService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeasurementDto dto)
    {
        var result = await measurementService.RecordAsync(dto);
        return CreatedAtAction(nameof(GetBySensor), new { sensorId = result.SensorExternalId }, result);
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] int limit = 50)
    {
        var measurements = await measurementService.GetLatestAsync(limit);
        return Ok(measurements);
    }

    [HttpGet("sensor/{sensorId}")]
    public async Task<IActionResult> GetBySensor(string sensorId, [FromQuery] int limit = 100)
    {
        var measurements = await measurementService.GetBySensorExternalIdAsync(sensorId, limit);
        return Ok(measurements);
    }

    [HttpGet("period")]
    public async Task<IActionResult> GetByPeriod([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var measurements = await measurementService.GetByPeriodAsync(from, to);
        return Ok(measurements);
    }
}
