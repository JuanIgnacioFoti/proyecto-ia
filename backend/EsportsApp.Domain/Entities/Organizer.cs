namespace EsportsApp.Domain.Entities;

public class Organizer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrganizationName { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
}
