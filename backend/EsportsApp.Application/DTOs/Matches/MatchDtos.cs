using System.ComponentModel.DataAnnotations;

namespace EsportsApp.Application.DTOs.Matches;

public class RecordMatchRequestDto
{
    [Required] public Guid HomeTeamId { get; set; }
    [Required] public Guid AwayTeamId { get; set; }
    [Range(0, int.MaxValue)] public int HomeScore { get; set; }
    [Range(0, int.MaxValue)] public int AwayScore { get; set; }
    [Required] public DateTime PlayedAt { get; set; }
}

public class MatchDto
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public Guid HomeTeamId { get; set; }
    public string HomeTeamName { get; set; } = string.Empty;
    public Guid AwayTeamId { get; set; }
    public string AwayTeamName { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public DateTime PlayedAt { get; set; }
    public DateTime RecordedAt { get; set; }
}
