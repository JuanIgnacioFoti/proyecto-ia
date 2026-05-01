namespace EsportsApp.Domain.Entities;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public Guid MainVideogameId { get; set; }

    public User User { get; set; } = null!;
    public Videogame MainVideogame { get; set; } = null!;
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public ICollection<TeamInvitation> ReceivedInvitations { get; set; } = new List<TeamInvitation>();
}
