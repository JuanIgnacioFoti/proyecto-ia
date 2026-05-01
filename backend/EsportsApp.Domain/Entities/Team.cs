namespace EsportsApp.Domain.Entities;

public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid VideogameId { get; set; }
    public Guid CaptainId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Videogame Videogame { get; set; } = null!;
    public Player Captain { get; set; } = null!;
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TeamInvitation> Invitations { get; set; } = new List<TeamInvitation>();
    public ICollection<TournamentRegistration> Registrations { get; set; } = new List<TournamentRegistration>();
}
