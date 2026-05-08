using System.Security.Claims;
using AirWatch.Application.DTOs.Common;
using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Medições coletadas pelos sensores MQ conectados ao Arduino do usuário.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MeasurementsController(MeasurementService measurementService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>
    /// Retorna as medições mais recentes dos dispositivos do usuário autenticado.
    /// </summary>
    /// <param name="page">Número da página (começa em 1). Padrão: 1.</param>
    /// <param name="pageSize">Itens por página. Padrão: 20.</param>
    /// <response code="200">Página retornada com sucesso.</response>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(PagedResultDto<MeasurementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatest([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await measurementService.GetLatestAsync(UserId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Retorna medições de um dispositivo específico do usuário autenticado.
    /// </summary>
    /// <param name="deviceId">Identificador externo do dispositivo.</param>
    /// <param name="page">Número da página (começa em 1). Padrão: 1.</param>
    /// <param name="pageSize">Itens por página. Padrão: 20.</param>
    /// <response code="200">Página retornada com sucesso.</response>
    /// <response code="404">Dispositivo não encontrado ou não pertence ao usuário.</response>
    [HttpGet("device/{deviceId}")]
    [ProducesResponseType(typeof(PagedResultDto<MeasurementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDevice(
        string deviceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await measurementService.GetByDeviceExternalIdAsync(deviceId, UserId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Retorna medições dos dispositivos do usuário dentro de um período.
    /// </summary>
    /// <param name="from">Data e hora inicial (UTC).</param>
    /// <param name="to">Data e hora final (UTC).</param>
    /// <param name="page">Número da página (começa em 1). Padrão: 1.</param>
    /// <param name="pageSize">Itens por página. Padrão: 20.</param>
    /// <response code="200">Página retornada com sucesso.</response>
    /// <response code="400">Datas inválidas ou ausentes.</response>
    [HttpGet("period")]
    [ProducesResponseType(typeof(PagedResultDto<MeasurementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByPeriod(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await measurementService.GetByPeriodAsync(UserId, from, to, page, pageSize);
        return Ok(result);
    }
}
