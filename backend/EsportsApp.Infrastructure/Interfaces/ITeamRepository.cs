using EsportsApp.Domain.Entities;

namespace EsportsApp.Infrastructure.Interfaces;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id);
    Task<Team?> GetByIdWithMembersAsync(Guid id);
    Task<IEnumerable<Team>> GetByPlayerAsync(Guid playerId);
    Task<bool> NameExistsForGameAsync(string name, Guid videogameId);
    Task AddAsync(Team team);
    Task AddMemberAsync(TeamMember member);
    Task UpdateAsync(Team team);
    Task SaveChangesAsync();
}
