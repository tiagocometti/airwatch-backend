using AirWatch.Application.DTOs.Users;
using AirWatch.Application.Interfaces;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Domain.Exceptions;

namespace AirWatch.Application.Services;

public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
{
    public async Task<UserDto> RegisterAsync(RegisterUserDto dto)
    {
        var existing = await userRepository.GetByEmailAsync(dto.Email);
        if (existing is not null)
            throw new ConflictException($"Já existe um usuário cadastrado com o e-mail '{dto.Email}'.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = passwordHasher.Hash(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user);

        return ToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(ToDto);
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Usuário com id '{id}' não encontrado.");

        return ToDto(user);
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.Name, u.Email, u.CreatedAt);
}
