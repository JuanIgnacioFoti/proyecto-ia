using System.ComponentModel.DataAnnotations;
using EsportsApp.Domain.Enums;

namespace EsportsApp.Application.DTOs.Tournaments;

public class CreateTournamentRequestDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public Guid VideogameId { get; set; }
    [Required] public DateTime StartDate { get; set; }
    [Required] public DateTime EstimatedEndDate { get; set; }
    [Range(2, int.MaxValue)] public int MaxTeams { get; set; }
    [Range(1, int.MaxValue)] public int MinMembersPerTeam { get; set; } = 5;
    [Required] public ScoringSystemRequestDto ScoringSystem { get; set; } = null!;
}

public class ScoringSystemRequestDto
{
    [Required] public string Type { get; set; } = "Standard";
    public int? WinPoints { get; set; }
    public int? DrawPoints { get; set; }
    public int? LossPoints { get; set; }
}

public class UpdateTournamentRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public int? MaxTeams { get; set; }
    public int? MinMembersPerTeam { get; set; }
}

public class TournamentSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string VideogameName { get; set; } = string.Empty;
    public Guid VideogameId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EstimatedEndDate { get; set; }
    public int MaxTeams { get; set; }
    public int RegisteredTeams { get; set; }
}

public class TournamentDetailDto : TournamentSummaryDto
{
    public Guid OrganizerId { get; set; }
    public string? Description { get; set; }
    public int MinMembersPerTeam { get; set; }
    public ScoringSystemDto ScoringSystem { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ScoringSystemDto
{
    public string Type { get; set; } = string.Empty;
    public int WinPoints { get; set; }
    public int DrawPoints { get; set; }
    public int LossPoints { get; set; }
}
