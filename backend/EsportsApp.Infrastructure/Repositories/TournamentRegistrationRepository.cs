using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class TournamentRegistrationRepository : ITournamentRegistrationRepository
{
    private readonly AppDbContext _context;
    public TournamentRegistrationRepository(AppDbContext context) => _context = context;

    public Task<TournamentRegistration?> GetByIdAsync(Guid id) =>
        _context.TournamentRegistrations.Include(r => r.Team).Include(r => r.Tournament)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<TournamentRegistration?> GetByTournamentAndTeamAsync(Guid tournamentId, Guid teamId) =>
        _context.TournamentRegistrations
            .FirstOrDefaultAsync(r => r.TournamentId == tournamentId && r.TeamId == teamId);

    public async Task<IEnumerable<TournamentRegistration>> GetActiveByTournamentAsync(Guid tournamentId) =>
        await _context.TournamentRegistrations
            .Include(r => r.Team)
            .Where(r => r.TournamentId == tournamentId && r.Status == RegistrationStatus.Active)
            .ToListAsync();

    public Task<int> GetActiveCountByTournamentAsync(Guid tournamentId) =>
        _context.TournamentRegistrations
            .CountAsync(r => r.TournamentId == tournamentId && r.Status == RegistrationStatus.Active);

    public Task<bool> IsTeamRegisteredInGameAsync(Guid teamId, Guid videogameId, Guid excludeTournamentId) =>
        _context.TournamentRegistrations
            .Include(r => r.Tournament)
            .AnyAsync(r => r.TeamId == teamId
                && r.Status == RegistrationStatus.Active
                && r.TournamentId != excludeTournamentId
                && r.Tournament.VideogameId == videogameId
                && (r.Tournament.Status == TournamentStatus.Open || r.Tournament.Status == TournamentStatus.InProgress));

    public async Task AddAsync(TournamentRegistration registration) =>
        await _context.TournamentRegistrations.AddAsync(registration);

    public Task UpdateAsync(TournamentRegistration registration)
    {
        _context.TournamentRegistrations.Update(registration);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TournamentRegistration registration)
    {
        _context.TournamentRegistrations.Remove(registration);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
