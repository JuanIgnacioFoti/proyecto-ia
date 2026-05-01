using System.ComponentModel.DataAnnotations;

namespace EsportsApp.Application.DTOs.Auth;

public class RegisterPlayerRequestDto
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string RealName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
    [Required] public Guid VideogameId { get; set; }
}

public class RegisterOrganizerRequestDto
{
    [Required, MinLength(3), MaxLength(60)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required, EmailAddress] public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class AuthResultDto
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

// Keep for backward-compat, but not used by controller anymore
public class RegisterPlayerResultDto { public Guid UserId { get; set; } }
public class RegisterOrganizerResultDto { public Guid UserId { get; set; } }
public class LoginResultDto { public string Token { get; set; } = string.Empty; public DateTime ExpiresAt { get; set; } }
