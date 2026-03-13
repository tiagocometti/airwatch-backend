namespace AirWatch.Application.DTOs.Users;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt
);
