using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class TeamInvitationRepository : ITeamInvitationRepository
{
    private readonly AppDbContext _context;
    public TeamInvitationRepository(AppDbContext context) => _context = context;

    public Task<TeamInvitation?> GetByIdAsync(Guid id) =>
        _context.TeamInvitations
            .Include(i => i.Team)
            .Include(i => i.InvitedPlayer).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IEnumerable<TeamInvitation>> GetPendingByPlayerAsync(Guid playerId) =>
        await _context.TeamInvitations
            .Include(i => i.Team).ThenInclude(t => t.Videogame)
            .Where(i => i.InvitedPlayerId == playerId && i.Status == InvitationStatus.Pending)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

    public Task<bool> PendingInvitationExistsAsync(Guid teamId, Guid playerId) =>
        _context.TeamInvitations.AnyAsync(i =>
            i.TeamId == teamId && i.InvitedPlayerId == playerId && i.Status == InvitationStatus.Pending);

    public async Task<InvitationStatus?> GetCurrentStatusAsync(Guid invitationId)
    {
        // Use AsNoTracking so this read bypasses the change tracker and hits the DB directly,
        // giving us the true committed state regardless of in-flight tracked changes.
        var row = await _context.TeamInvitations
            .AsNoTracking()
            .Where(i => i.Id == invitationId)
            .Select(i => (InvitationStatus?)i.Status)
            .FirstOrDefaultAsync();
        return row;
    }

    public async Task AddAsync(TeamInvitation invitation) => await _context.TeamInvitations.AddAsync(invitation);

    public Task UpdateAsync(TeamInvitation invitation)
    {
        // Calling dbSet.Update() would cascade EntityState.Modified to all loaded navigation
        // properties (Team, InvitedPlayer), conflicting with concurrent team member additions
        // and causing DbUpdateConcurrencyException. Setting state directly on the entry
        // marks only the invitation's own scalar columns as modified.
        var entry = _context.Entry(invitation);
        if (entry.State == EntityState.Detached)
            _context.TeamInvitations.Attach(invitation);
        entry.State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
