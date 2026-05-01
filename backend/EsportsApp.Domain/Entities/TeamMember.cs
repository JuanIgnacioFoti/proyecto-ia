namespace EsportsApp.Domain.Entities;

public class TeamMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
