using EsportsApp.Domain.Enums;

namespace EsportsApp.Domain.Entities;

public class Tournament
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid VideogameId { get; set; }
    public Guid OrganizerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EstimatedEndDate { get; set; }
    public int MaxTeams { get; set; }
    public int MinMembersPerTeam { get; set; } = 5;
    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;
    public TournamentStatus? PreSuspensionStatus { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Videogame Videogame { get; set; } = null!;
    public Organizer Organizer { get; set; } = null!;
    public ScoringSystem ScoringSystem { get; set; } = null!;
    public ICollection<TournamentRegistration> Registrations { get; set; } = new List<TournamentRegistration>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<Standing> Standings { get; set; } = new List<Standing>();
}
