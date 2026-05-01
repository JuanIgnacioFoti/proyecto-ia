using System.ComponentModel.DataAnnotations;

namespace EsportsApp.Application.DTOs.Users;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    // Player fields
    public string? Username { get; set; }
    public string? RealName { get; set; }
    public Guid? MainVideogameId { get; set; }
    public string? MainVideogameName { get; set; }
    // Organizer fields
    public string? OrganizationName { get; set; }
}

public class UpdatePlayerProfileRequestDto
{
    [Required] public string RealName { get; set; } = string.Empty;
    [EmailAddress] public string? Email { get; set; }
    public string? CurrentPassword { get; set; }
    [MinLength(8)] public string? NewPassword { get; set; }
}

public class UpdateOrganizerProfileRequestDto
{
    [Required, MinLength(3), MaxLength(60)]
    public string OrganizationName { get; set; } = string.Empty;

    public string? CurrentPassword { get; set; }
    [MinLength(8)] public string? NewPassword { get; set; }
}

public class UserSummaryDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? DisplayName { get; set; }
}
