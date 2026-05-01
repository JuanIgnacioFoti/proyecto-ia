using EsportsApp.Domain.Enums;

namespace EsportsApp.Domain.Entities;

public class TeamInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamId { get; set; }
    public Guid InvitedPlayerId { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;
    public Player InvitedPlayer { get; set; } = null!;

    public bool IsExpired => Status == InvitationStatus.Pending && CreatedAt.AddDays(7) < DateTime.UtcNow;

    public InvitationStatus EffectiveStatus =>
        Status == InvitationStatus.Pending && IsExpired ? InvitationStatus.Expired : Status;
}
