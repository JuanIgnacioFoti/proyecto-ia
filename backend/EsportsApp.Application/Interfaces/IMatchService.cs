using EsportsApp.Application.DTOs.Matches;
using EsportsApp.Application.DTOs.Standings;

namespace EsportsApp.Application.Interfaces;

public interface IMatchService
{
    Task<MatchDto> RecordAsync(Guid organizerId, Guid tournamentId, RecordMatchRequestDto request);
    Task<MatchDto> CorrectAsync(Guid organizerId, Guid tournamentId, Guid matchId, RecordMatchRequestDto request);
    Task<IEnumerable<MatchDto>> GetByTournamentAsync(Guid tournamentId);
}

public interface IStandingsService
{
    Task<IEnumerable<StandingDto>> GetByTournamentAsync(Guid tournamentId);
}

public interface IAdminService
{
    Task<IEnumerable<DTOs.Users.UserSummaryDto>> GetUsersAsync();
    Task SetRoleAsync(Guid adminId, Guid targetUserId, string role);
    Task SuspendUserAsync(Guid adminId, Guid targetUserId);
    Task ReinstateUserAsync(Guid adminId, Guid targetUserId);
}

public interface IVideogameService
{
    Task<IEnumerable<DTOs.Videogames.VideogameDto>> GetAllAsync();
}
