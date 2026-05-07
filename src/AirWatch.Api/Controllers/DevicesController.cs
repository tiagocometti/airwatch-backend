using System.Security.Claims;
using AirWatch.Application.DTOs.Devices;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Gerenciamento dos dispositivos do usuário autenticado.
/// </summary>
[Authorize]
[ApiController]
[Route("api/devices")]
[Produces("application/json")]
public class DevicesController(DeviceService deviceService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>
    /// Cadastra um novo dispositivo para o usuário autenticado.
    /// </summary>
    /// <param name="dto">Dados do dispositivo a ser cadastrado.</param>
    /// <returns>O dispositivo recém-cadastrado.</returns>
    /// <response code="201">Dispositivo cadastrado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">Você já possui um dispositivo com este <c>externalId</c>.</response>
    [HttpPost]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] CreateDeviceDto dto)
    {
        var result = await deviceService.RegisterAsync(dto, UserId);
        return CreatedAtAction(nameof(GetByExternalId), new { externalId = result.ExternalId }, result);
    }

    /// <summary>
    /// Retorna os dispositivos cadastrados pelo usuário autenticado.
    /// </summary>
    /// <returns>Lista de dispositivos em ordem alfabética pelo nome.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeviceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var devices = await deviceService.GetAllAsync(UserId);
        return Ok(devices);
    }

    /// <summary>
    /// Retorna um dispositivo pelo identificador externo.
    /// </summary>
    /// <param name="externalId">Identificador externo do dispositivo.</param>
    /// <returns>Dados do dispositivo encontrado.</returns>
    /// <response code="200">Dispositivo encontrado.</response>
    /// <response code="404">Dispositivo não encontrado ou não pertence ao usuário.</response>
    [HttpGet("{externalId}")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByExternalId(string externalId)
    {
        var device = await deviceService.GetByExternalIdAsync(externalId, UserId);
        if (device is null)
            return NotFound();

        return Ok(device);
    }
}
