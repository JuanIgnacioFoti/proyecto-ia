using EsportsApp.Domain.Entities;

namespace EsportsApp.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
