using System.ComponentModel.DataAnnotations;

namespace EsportsApp.Application.DTOs.Registrations;

public class RegisterTeamRequestDto
{
    [Required] public Guid TeamId { get; set; }
}

public class RegistrationDto
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
