using EsportsApp.Domain.Entities;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly AppDbContext _context;
    public TeamRepository(AppDbContext context) => _context = context;

    public Task<Team?> GetByIdAsync(Guid id) =>
        _context.Teams.Include(t => t.Videogame).Include(t => t.Captain).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == id);

    public Task<Team?> GetByIdWithMembersAsync(Guid id) =>
        _context.Teams
            .Include(t => t.Videogame)
            .Include(t => t.Captain).ThenInclude(c => c.User)
            .Include(t => t.Members).ThenInclude(m => m.Player).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Team>> GetByPlayerAsync(Guid playerId) =>
        await _context.Teams
            .Include(t => t.Members)
            .Where(t => t.Members.Any(m => m.PlayerId == playerId))
            .ToListAsync();

    public Task<bool> NameExistsForGameAsync(string name, Guid videogameId) =>
        _context.Teams.AnyAsync(t => t.Name == name && t.VideogameId == videogameId);

    public async Task AddAsync(Team team) => await _context.Teams.AddAsync(team);

    public async Task AddMemberAsync(TeamMember member) => await _context.Set<TeamMember>().AddAsync(member);

    public Task UpdateAsync(Team team) { _context.Teams.Update(team); return Task.CompletedTask; }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
