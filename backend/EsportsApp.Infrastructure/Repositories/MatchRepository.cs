using EsportsApp.Domain.Entities;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly AppDbContext _context;
    public MatchRepository(AppDbContext context) => _context = context;

    public Task<Match?> GetByIdAsync(Guid id) =>
        _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<Match>> GetByTournamentAsync(Guid tournamentId) =>
        await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.TournamentId == tournamentId)
            .OrderBy(m => m.PlayedAt)
            .ToListAsync();

    public Task<bool> DuplicateExistsAsync(Guid tournamentId, Guid team1Id, Guid team2Id, DateTime playedAt) =>
        _context.Matches.AnyAsync(m =>
            m.TournamentId == tournamentId
            && m.PlayedAt == playedAt
            && ((m.HomeTeamId == team1Id && m.AwayTeamId == team2Id)
                || (m.HomeTeamId == team2Id && m.AwayTeamId == team1Id)));

    public async Task AddAsync(Match match) => await _context.Matches.AddAsync(match);

    public Task UpdateAsync(Match match) { _context.Matches.Update(match); return Task.CompletedTask; }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
