using EsportsApp.Domain.Entities;

namespace EsportsApp.Infrastructure.Interfaces;

public interface IStandingRepository
{
    Task<Standing?> GetByTournamentAndTeamAsync(Guid tournamentId, Guid teamId);
    Task<IEnumerable<Standing>> GetByTournamentAsync(Guid tournamentId);
    Task AddAsync(Standing standing);
    Task UpdateAsync(Standing standing);
    Task DeleteAsync(Standing standing);
    Task SaveChangesAsync();
}
