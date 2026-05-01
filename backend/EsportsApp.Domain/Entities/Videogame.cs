namespace EsportsApp.Domain.Entities;

public class Videogame
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    public ICollection<Player> Players { get; set; } = new List<Player>();
}
