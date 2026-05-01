using EsportsApp.Domain.Entities;

namespace EsportsApp.Infrastructure.Interfaces;

public interface ITournamentRegistrationRepository
{
    Task<TournamentRegistration?> GetByIdAsync(Guid id);
    Task<TournamentRegistration?> GetByTournamentAndTeamAsync(Guid tournamentId, Guid teamId);
    Task<IEnumerable<TournamentRegistration>> GetActiveByTournamentAsync(Guid tournamentId);
    Task<int> GetActiveCountByTournamentAsync(Guid tournamentId);
    Task<bool> IsTeamRegisteredInGameAsync(Guid teamId, Guid videogameId, Guid excludeTournamentId);
    Task AddAsync(TournamentRegistration registration);
    Task UpdateAsync(TournamentRegistration registration);
    Task DeleteAsync(TournamentRegistration registration);
    Task SaveChangesAsync();
}
