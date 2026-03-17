using AirWatch.Application.DTOs.Auth;
using AirWatch.Application.Interfaces;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Exceptions;

namespace AirWatch.Application.Services;

public class AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userRepository.GetByEmailAsync(dto.Email)
            ?? throw new NotFoundException("E-mail ou senha inválidos.");

        if (!passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new NotFoundException("E-mail ou senha inválidos.");

        var (token, expiresAt) = jwtTokenGenerator.Generate(user);

        return new TokenResponseDto(token, expiresAt, user.Name, user.Email);
    }
}
