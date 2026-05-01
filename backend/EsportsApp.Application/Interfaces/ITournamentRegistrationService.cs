using EsportsApp.Application.DTOs.Registrations;

namespace EsportsApp.Application.Interfaces;

public interface ITournamentRegistrationService
{
    Task<RegistrationDto> RegisterAsync(Guid captainPlayerId, Guid tournamentId, RegisterTeamRequestDto request);
    Task WithdrawAsync(Guid captainPlayerId, Guid tournamentId, Guid teamId);
    Task<IEnumerable<RegistrationDto>> GetByTournamentAsync(Guid tournamentId);
}
