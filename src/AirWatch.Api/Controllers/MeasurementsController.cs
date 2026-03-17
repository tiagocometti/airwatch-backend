using AirWatch.Application.DTOs.Common;
using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Gerenciamento das medições coletadas pelos sensores de qualidade do ar.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MeasurementsController(MeasurementService measurementService) : ControllerBase
{
    /// <summary>
    /// Registra uma nova medição de qualidade do ar.
    /// </summary>
    /// <remarks>
    /// Utilizado pelo backend MQTT (ou simulador) para persistir os dados recebidos do sensor.
    /// O campo <c>sensorId</c> deve corresponder ao <c>externalId</c> de um sensor previamente cadastrado.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/measurements
    ///     {
    ///         "sensorId": "lab-sensor-01",
    ///         "gasValue": 320,
    ///         "temperature": 26.5,
    ///         "humidity": 55.0,
    ///         "timestamp": "2026-03-12T18:00:00Z"
    ///     }
    /// </remarks>
    /// <param name="dto">Dados da medição enviada pelo sensor.</param>
    /// <returns>A medição registrada com seu identificador interno.</returns>
    /// <response code="201">Medição registrada com sucesso.</response>
    /// <response code="400">Dados inválidos — verifique os campos obrigatórios e os intervalos permitidos.</response>
    /// <response code="404">Sensor não encontrado — o <c>sensorId</c> informado não está cadastrado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(MeasurementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateMeasurementDto dto)
    {
        var result = await measurementService.RecordAsync(dto);
        return CreatedAtAction(nameof(GetBySensor), new { sensorId = result.SensorExternalId }, result);
    }

    /// <summary>
    /// Retorna as medições mais recentes de todos os sensores com paginação.
    /// </summary>
    /// <remarks>
    /// Útil para exibir o estado atual do ambiente no dashboard do frontend.
    /// As medições são ordenadas da mais recente para a mais antiga.
    /// </remarks>
    /// <param name="page">Número da página (começa em 1). Padrão: 1.</param>
    /// <param name="pageSize">Quantidade de itens por página. Padrão: 20.</param>
    /// <returns>Página de medições com metadados de paginação.</returns>
    /// <response code="200">Página retornada com sucesso.</response>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(PagedResultDto<MeasurementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatest([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await measurementService.GetLatestAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Retorna o histórico paginado de medições de um sensor específico.
    /// </summary>
    /// <remarks>
    /// Permite consultar o histórico de um sensor pelo seu identificador externo (ex: <c>lab-sensor-01</c>).
    /// As medições são ordenadas da mais recente para a mais antiga.
    /// </remarks>
    /// <param name="sensorId">Identificador externo do sensor (campo <c>externalId</c> no cadastro).</param>
    /// <param name="page">Número da página (começa em 1). Padrão: 1.</param>
    /// <param name="pageSize">Quantidade de itens por página. Padrão: 20.</param>
    /// <returns>Página de medições do sensor informado.</returns>
    /// <response code="200">Página retornada com sucesso.</response>
    /// <response code="404">Sensor não encontrado.</response>
    [HttpGet("sensor/{sensorId}")]
    [ProducesResponseType(typeof(PagedResultDto<MeasurementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySensor(string sensorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await measurementService.GetBySensorExternalIdAsync(sensorId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Retorna as medições de todos os sensores dentro de um período de tempo, com paginação.
    /// </summary>
    /// <remarks>
    /// Útil para gerar gráficos históricos no frontend com base em um intervalo de datas selecionado pelo usuário.
    ///
    /// Exemplo: <c>GET /api/measurements/period?from=2026-03-01T00:00:00Z&amp;to=2026-03-12T23:59:59Z&amp;page=1&amp;pageSize=20</c>
    /// </remarks>
    /// <param name="from">Data e hora inicial do período (UTC).</param>
    /// <param name="to">Data e hora final do período (UTC).</param>
    /// <param name="page">Número da página (começa em 1). Padrão: 1.</param>
    /// <param name="pageSize">Quantidade de itens por página. Padrão: 20.</param>
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
