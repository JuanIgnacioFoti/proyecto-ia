using EsportsApp.Application.DTOs.Tournaments;

namespace EsportsApp.Application.Interfaces;

public interface ITournamentService
{
    Task<TournamentDetailDto> GetByIdAsync(Guid id, Guid? requestingUserId);
    Task<IEnumerable<TournamentSummaryDto>> GetPublicListAsync();
    Task<IEnumerable<TournamentSummaryDto>> GetMyTournamentsAsync(Guid organizerId);
    Task<TournamentDetailDto> CreateAsync(Guid organizerId, CreateTournamentRequestDto request);
    Task<TournamentDetailDto> UpdateAsync(Guid organizerId, Guid tournamentId, UpdateTournamentRequestDto request);
    Task AdvanceStatusAsync(Guid organizerId, Guid tournamentId);
    Task DeleteAsync(Guid organizerId, Guid tournamentId);
    Task SuspendAsync(Guid adminId, Guid tournamentId);
    Task ReinstateAsync(Guid adminId, Guid tournamentId);
}
