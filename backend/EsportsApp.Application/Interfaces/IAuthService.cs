using EsportsApp.Application.DTOs.Auth;

namespace EsportsApp.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RegisterPlayerAsync(RegisterPlayerRequestDto request);
    Task<AuthResultDto> RegisterOrganizerAsync(RegisterOrganizerRequestDto request);
    Task<AuthResultDto> LoginAsync(LoginRequestDto request);
}
