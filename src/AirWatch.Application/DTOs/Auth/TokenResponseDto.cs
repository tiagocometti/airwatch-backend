namespace AirWatch.Application.DTOs.Auth;

public record TokenResponseDto(
    string Token,
    DateTime ExpiresAt,
    string UserName,
    string Email
);
