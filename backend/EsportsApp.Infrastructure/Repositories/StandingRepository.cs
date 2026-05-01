using EsportsApp.Domain.Entities;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class StandingRepository : IStandingRepository
{
    private readonly AppDbContext _context;
    public StandingRepository(AppDbContext context) => _context = context;

    public Task<Standing?> GetByTournamentAndTeamAsync(Guid tournamentId, Guid teamId) =>
        _context.Standings.FirstOrDefaultAsync(s => s.TournamentId == tournamentId && s.TeamId == teamId);

    public async Task<IEnumerable<Standing>> GetByTournamentAsync(Guid tournamentId) =>
        await _context.Standings
            .Include(s => s.Team)
            .Where(s => s.TournamentId == tournamentId)
            .OrderByDescending(s => s.Points)
            .ThenBy(s => s.Team.Name)
            .ToListAsync();

    public async Task AddAsync(Standing standing) => await _context.Standings.AddAsync(standing);

    public Task UpdateAsync(Standing standing) { _context.Standings.Update(standing); return Task.CompletedTask; }

    public Task DeleteAsync(Standing standing) { _context.Standings.Remove(standing); return Task.CompletedTask; }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
