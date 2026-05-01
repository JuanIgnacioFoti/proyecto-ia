using EsportsApp.Domain.Enums;

namespace EsportsApp.Domain.Entities;

public class TournamentRegistration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TournamentId { get; set; }
    public Guid TeamId { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Active;

    public Tournament Tournament { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
