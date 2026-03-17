using AirWatch.Domain.Entities;

namespace AirWatch.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(User user);
}
