using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class TournamentRepository : ITournamentRepository
{
    private readonly AppDbContext _context;
    public TournamentRepository(AppDbContext context) => _context = context;

    public Task<Tournament?> GetByIdAsync(Guid id) =>
        _context.Tournaments
            .Include(t => t.Videogame)
            .Include(t => t.Organizer).ThenInclude(o => o.User)
            .Include(t => t.ScoringSystem)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Tournament>> GetPublicListAsync() =>
        await _context.Tournaments
            .Include(t => t.Videogame)
            .Include(t => t.Organizer).ThenInclude(o => o.User)
            .Where(t => t.Status != TournamentStatus.Draft)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Tournament>> GetByOrganizerAsync(Guid organizerId) =>
        await _context.Tournaments
            .Include(t => t.Videogame)
            .Include(t => t.ScoringSystem)
            .Where(t => t.OrganizerId == organizerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Tournament tournament) => await _context.Tournaments.AddAsync(tournament);

    public Task UpdateAsync(Tournament tournament) { _context.Tournaments.Update(tournament); return Task.CompletedTask; }

    public Task DeleteAsync(Tournament tournament) { _context.Tournaments.Remove(tournament); return Task.CompletedTask; }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
