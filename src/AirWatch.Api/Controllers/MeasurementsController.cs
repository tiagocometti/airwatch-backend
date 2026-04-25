using AirWatch.Application.DTOs.Common;
using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Gerenciamento das medições coletadas pelos sensores MQ conectados ao Arduino.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MeasurementsController(MeasurementService measurementService) : ControllerBase
{
    /// <summary>
    /// Retorna as medições mais recentes de todos os dispositivos com paginação.
    /// </summary>
    /// <param name="page">Número da página (começa em 1). Padrão: 1.</param>
    /// <param name="pageSize">Itens por página. Padrão: 20.</param>
    /// <returns>Página de medições ordenadas da mais recente para a mais antiga.</returns>
    /// <response code="200">Página retornada com sucesso.</response>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(PagedResultDto<MeasurementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatest([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await measurementService.GetLatestAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Retorna o histórico paginado de medições de um dispositivo específico.
    /// </summary>
    /// <param name="deviceId">Identificador externo do dispositivo (campo <c>externalId</c>).</param>
    /// <param name="sensorType">Filtro opcional por tipo de sensor: mq3, mq5 ou mq135.</param>
    /// <param name="page">Número da página (começa em 1). Padrão: 1.</param>
    /// <param name="pageSize">Itens por página. Padrão: 20.</param>
    /// <returns>Página de medições do dispositivo informado.</returns>
    /// <response code="200">Página retornada com sucesso.</response>
    /// <response code="404">Dispositivo não encontrado.</response>
    [HttpGet("device/{deviceId}")]
    [ProducesResponseType(typeof(PagedResultDto<MeasurementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDevice(
        string deviceId,
        [FromQuery] string? sensorType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await measurementService.GetByDeviceExternalIdAsync(deviceId, sensorType, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Retorna medições de todos os dispositivos dentro de um período de tempo.
    /// </summary>
    /// <param name="from">Data e hora inicial (UTC).</param>
    /// <param name="to">Data e hora final (UTC).</param>
    /// <param name="page">Número da página (começa em 1). Padrão: 1.</param>
    /// <param name="pageSize">Itens por página. Padrão: 20.</param>
    /// <returns>Página de medições dentro do período informado.</returns>
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
        var result = await measurementService.GetByPeriodAsync(from, to, page, pageSize);
        return Ok(result);
    }
}
