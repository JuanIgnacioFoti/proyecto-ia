using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;

namespace EsportsApp.Infrastructure.Interfaces;

public interface ITeamInvitationRepository
{
    Task<TeamInvitation?> GetByIdAsync(Guid id);
    Task<IEnumerable<TeamInvitation>> GetPendingByPlayerAsync(Guid playerId);
    Task<bool> PendingInvitationExistsAsync(Guid teamId, Guid playerId);
    /// <summary>Reads the current Status directly from the DB, bypassing the change tracker.</summary>
    Task<InvitationStatus?> GetCurrentStatusAsync(Guid invitationId);
    Task AddAsync(TeamInvitation invitation);
    Task UpdateAsync(TeamInvitation invitation);
    Task SaveChangesAsync();
}
