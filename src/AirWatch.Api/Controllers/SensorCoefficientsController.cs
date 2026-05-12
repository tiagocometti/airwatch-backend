using AirWatch.Application.DTOs.SensorCoefficients;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Consulta de coeficientes dos sensores de gás.
/// </summary>
[ApiController]
[Route("api/sensor-coefficients")]
[Produces("application/json")]
public class SensorCoefficientsController(SensorCoefficientService sensorCoefficientService) : ControllerBase
{
    /// <summary>
    /// Retorna os limiares de concentração (thresholds) para todos os gases monitorados.
    /// </summary>
    /// <returns>Lista de thresholds por gás.</returns>
    /// <response code="200">Thresholds retornados com sucesso.</response>
    [HttpGet("thresholds")]
    [ProducesResponseType(typeof(IEnumerable<SensorCoefficientThresholdDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThresholds()
    {
        var thresholds = await sensorCoefficientService.GetThresholdsAsync();
        return Ok(thresholds);
    }
}
