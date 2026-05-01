using EsportsApp.Application.DTOs.Users;

namespace EsportsApp.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task UpdatePlayerProfileAsync(Guid userId, UpdatePlayerProfileRequestDto request);
    Task UpdateOrganizerProfileAsync(Guid userId, UpdateOrganizerProfileRequestDto request);
}
