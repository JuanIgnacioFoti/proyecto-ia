using System.ComponentModel.DataAnnotations;

namespace EsportsApp.Application.DTOs.Teams;

public class CreateTeamRequestDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public Guid VideogameId { get; set; }
}

public class UpdateTeamRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class InvitePlayerRequestDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
}

public class TeamDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid VideogameId { get; set; }
    public string VideogameName { get; set; } = string.Empty;
    public Guid CaptainId { get; set; }
    public string CaptainUsername { get; set; } = string.Empty;
    public List<TeamMemberDto> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class TeamMemberDto
{
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public bool IsCaptain { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class InvitationDto
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string VideogameName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
