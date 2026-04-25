using AirWatch.Application.DTOs.Devices;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Gerenciamento dos dispositivos cadastrados no sistema AirWatch.
/// </summary>
[Authorize]
[ApiController]
[Route("api/devices")]
[Produces("application/json")]
public class DevicesController(DeviceService deviceService) : ControllerBase
{
    /// <summary>
    /// Cadastra um novo dispositivo no sistema.
    /// </summary>
    /// <remarks>
    /// O <c>externalId</c> é o identificador único do dispositivo definido no firmware do Arduino
    /// (ex: <c>arduino-01</c>). Ele será usado nas mensagens MQTT e nas consultas de medições.
    ///
    /// Exemplo:
    ///
    ///     POST /api/devices
    ///     {
    ///         "externalId": "arduino-01",
    ///         "name": "Arduino 01",
    ///         "location": "Lab 1"
    ///     }
    /// </remarks>
    /// <param name="dto">Dados do dispositivo a ser cadastrado.</param>
    /// <returns>O dispositivo recém-cadastrado com seu identificador interno.</returns>
    /// <response code="201">Dispositivo cadastrado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">Já existe um dispositivo com este <c>externalId</c>.</response>
    [HttpPost]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] CreateDeviceDto dto)
    {
        var result = await deviceService.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetByExternalId), new { externalId = result.ExternalId }, result);
    }

    /// <summary>
    /// Retorna a lista de todos os dispositivos cadastrados.
    /// </summary>
    /// <returns>Lista de dispositivos em ordem alfabética pelo nome.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeviceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var devices = await deviceService.GetAllAsync();
        return Ok(devices);
    }

    /// <summary>
    /// Retorna os dados de um dispositivo pelo seu identificador externo.
    /// </summary>
    /// <param name="externalId">Identificador externo do dispositivo (definido no firmware).</param>
    /// <returns>Dados do dispositivo encontrado.</returns>
    /// <response code="200">Dispositivo encontrado.</response>
    /// <response code="404">Dispositivo não encontrado.</response>
    [HttpGet("{externalId}")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByExternalId(string externalId)
    {
        var device = await deviceService.GetByExternalIdAsync(externalId);
        if (device is null)
            return NotFound();

        return Ok(device);
    }
}
