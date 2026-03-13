using AirWatch.Application.DTOs.Sensors;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Gerenciamento dos sensores cadastrados no sistema AirWatch.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SensorsController(SensorService sensorService) : ControllerBase
{
    /// <summary>
    /// Cadastra um novo sensor no sistema.
    /// </summary>
    /// <remarks>
    /// O <c>externalId</c> é o identificador único do sensor definido no firmware do ESP8266
    /// (ex: <c>lab-sensor-01</c>). Ele será usado nas mensagens MQTT e nas consultas de medições.
    /// Não é possível cadastrar dois sensores com o mesmo <c>externalId</c>.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/sensors
    ///     {
    ///         "externalId": "lab-sensor-01",
    ///         "name": "Sensor do Laboratório",
    ///         "location": "Laboratório de Redes — Bloco B"
    ///     }
    /// </remarks>
    /// <param name="dto">Dados do sensor a ser cadastrado.</param>
    /// <returns>O sensor recém-cadastrado com seu identificador interno.</returns>
    /// <response code="201">Sensor cadastrado com sucesso.</response>
    /// <response code="400">Dados inválidos — verifique os campos obrigatórios.</response>
    /// <response code="409">Já existe um sensor cadastrado com este <c>externalId</c>.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SensorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterSensorDto dto)
    {
        var result = await sensorService.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetByExternalId), new { externalId = result.ExternalId }, result);
    }

    /// <summary>
    /// Retorna a lista de todos os sensores cadastrados.
    /// </summary>
    /// <remarks>
    /// Os sensores são retornados em ordem alfabética pelo nome.
    /// O campo <c>isActive</c> indica se o sensor está habilitado a enviar dados.
    /// </remarks>
    /// <returns>Lista de sensores cadastrados.</returns>
    /// <response code="200">Lista retornada com sucesso. Pode ser vazia se nenhum sensor estiver cadastrado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SensorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var sensors = await sensorService.GetAllAsync();
        return Ok(sensors);
    }

    /// <summary>
    /// Retorna os dados de um sensor pelo seu identificador externo.
    /// </summary>
    /// <remarks>
    /// O <c>externalId</c> é o mesmo identificador definido no firmware do sensor
    /// e utilizado nas mensagens MQTT (ex: <c>lab-sensor-01</c>).
    /// </remarks>
    /// <param name="externalId">Identificador externo do sensor (definido no firmware).</param>
    /// <returns>Dados do sensor encontrado.</returns>
    /// <response code="200">Sensor encontrado e retornado com sucesso.</response>
    /// <response code="404">Nenhum sensor encontrado com o <c>externalId</c> informado.</response>
    [HttpGet("{externalId}")]
    [ProducesResponseType(typeof(SensorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByExternalId(string externalId)
    {
        var sensor = await sensorService.GetByExternalIdAsync(externalId);
        if (sensor is null)
            return NotFound();

        return Ok(sensor);
    }
}
