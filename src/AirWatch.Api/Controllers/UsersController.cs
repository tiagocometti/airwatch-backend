using AirWatch.Application.DTOs.Users;
using AirWatch.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers;

/// <summary>
/// Gerenciamento dos usuários do sistema AirWatch.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController(UserService userService) : ControllerBase
{
    /// <summary>
    /// Cadastra um novo usuário no sistema.
    /// </summary>
    /// <remarks>
    /// A senha é armazenada de forma segura utilizando hash BCrypt — nunca em texto puro.
    /// Não é possível cadastrar dois usuários com o mesmo e-mail.
    ///
    /// **Atenção:** autenticação via JWT será implementada em uma versão futura.
    /// Por enquanto este endpoint apenas persiste o usuário.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/users
    ///     {
    ///         "name": "Tiago Cometti",
    ///         "email": "tiago@exemplo.com",
    ///         "password": "minhasenha123"
    ///     }
    /// </remarks>
    /// <param name="dto">Dados do usuário a ser cadastrado.</param>
    /// <returns>O usuário recém-cadastrado (sem a senha).</returns>
    /// <response code="201">Usuário cadastrado com sucesso.</response>
    /// <response code="400">Dados inválidos — verifique e-mail e tamanho mínimo da senha (6 caracteres).</response>
    /// <response code="409">Já existe um usuário cadastrado com este e-mail.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var result = await userService.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retorna a lista de todos os usuários cadastrados.
    /// </summary>
    /// <remarks>
    /// Os usuários são retornados em ordem alfabética pelo nome.
    /// O campo <c>passwordHash</c> nunca é exposto — a resposta inclui apenas dados não sensíveis.
    /// </remarks>
    /// <returns>Lista de usuários cadastrados.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(users);
    }

    /// <summary>
    /// Retorna os dados de um usuário pelo seu identificador interno.
    /// </summary>
    /// <param name="id">Identificador único (UUID) do usuário.</param>
    /// <returns>Dados do usuário encontrado.</returns>
    /// <response code="200">Usuário encontrado e retornado com sucesso.</response>
    /// <response code="404">Nenhum usuário encontrado com o <c>id</c> informado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await userService.GetByIdAsync(id);
        return Ok(user);
    }
}
