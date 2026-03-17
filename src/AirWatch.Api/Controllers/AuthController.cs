using AirWatch.Application.DTOs.Auth;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Autenticação de usuários do sistema AirWatch.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(AuthService authService) : ControllerBase
{
    /// <summary>
    /// Realiza o login e retorna um token JWT.
    /// </summary>
    /// <remarks>
    /// Envie e-mail e senha. Em caso de sucesso, utilize o token retornado no header
    /// <c>Authorization: Bearer {token}</c> em todas as requisições protegidas.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/auth/login
    ///     {
    ///         "email": "tiago@exemplo.com",
    ///         "password": "minhasenha123"
    ///     }
    /// </remarks>
    /// <param name="dto">Credenciais do usuário.</param>
    /// <returns>Token JWT com data de expiração.</returns>
    /// <response code="200">Login realizado com sucesso. Utilize o token retornado nas próximas requisições.</response>
    /// <response code="400">Dados inválidos — verifique o formato do e-mail.</response>
    /// <response code="404">E-mail ou senha inválidos.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return Ok(result);
    }
}
