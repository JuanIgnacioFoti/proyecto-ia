using EsportsApp.Domain.Entities;

namespace EsportsApp.Infrastructure.Interfaces;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id);
    Task<IEnumerable<Match>> GetByTournamentAsync(Guid tournamentId);
    Task<bool> DuplicateExistsAsync(Guid tournamentId, Guid team1Id, Guid team2Id, DateTime playedAt);
    Task AddAsync(Match match);
    Task UpdateAsync(Match match);
    Task SaveChangesAsync();
}
