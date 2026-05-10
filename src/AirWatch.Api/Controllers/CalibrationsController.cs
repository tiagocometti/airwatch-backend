using System.Security.Claims;
using AirWatch.Application.DTOs.Calibrations;
using AirWatch.Application.Interfaces;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Gerenciamento de calibrações de dispositivos.
/// </summary>
[Authorize]
[ApiController]
[Route("api/calibrations")]
[Produces("application/json")]
public class CalibrationsController(
    ICalibrationManager  calibrationManager,
    CalibrationService   calibrationService) : ControllerBase
{
    /// <summary>
    /// Inicia uma sessão de calibração para o dispositivo especificado.
    /// </summary>
    /// <response code="200">Calibração iniciada. Retorna o calibrationId.</response>
    /// <response code="409">Já existe uma calibração em andamento para este dispositivo.</response>
    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Start([FromBody] StartCalibrationDto dto)
    {
        var calibrationId = await calibrationManager.StartCalibrationAsync(dto.DeviceId, dto.Location);
        return Ok(new { calibrationId });
    }

    /// <summary>
    /// Cancela uma calibração em andamento.
    /// </summary>
    /// <response code="204">Cancelada com sucesso.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await calibrationManager.CancelCalibrationAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Ativa uma calibração concluída, tornando-a a referência para cálculo de PPM.
    /// </summary>
    /// <response code="204">Ativada com sucesso.</response>
    /// <response code="404">Calibração não encontrada.</response>
    /// <response code="409">Apenas calibrações com status Completed podem ser ativadas.</response>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(Guid id)
    {
        await calibrationService.ActivateCalibrationAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Retorna todas as calibrações de um dispositivo, em ordem decrescente.
    /// </summary>
    /// <response code="200">Lista retornada.</response>
    [HttpGet("device/{deviceId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<CalibrationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDevice(Guid deviceId)
    {
        var items = await calibrationService.GetByDeviceIdAsync(deviceId);
        return Ok(items);
    }

    /// <summary>
    /// Retorna a calibração ativa de um dispositivo, ou 204 se não houver.
    /// </summary>
    /// <response code="200">Calibração ativa encontrada.</response>
    /// <response code="204">Nenhuma calibração ativa.</response>
    [HttpGet("device/{deviceId:guid}/active")]
    [ProducesResponseType(typeof(CalibrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetActiveByDevice(Guid deviceId)
    {
        var item = await calibrationService.GetActiveByDeviceIdAsync(deviceId);
        return item is null ? NoContent() : Ok(item);
    }
}
