using System.Security.Claims;
using EsportsApp.Application.DTOs.Users;
using EsportsApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsportsApp.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserId();
        var profile = await _users.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPatch("me/player")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> UpdatePlayerProfile([FromBody] UpdatePlayerProfileRequestDto request)
    {
        var userId = GetUserId();
        await _users.UpdatePlayerProfileAsync(userId, request);
        return NoContent();
    }

    [HttpPatch("me/organizer")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> UpdateOrganizerProfile([FromBody] UpdateOrganizerProfileRequestDto request)
    {
        var userId = GetUserId();
        await _users.UpdateOrganizerProfileAsync(userId, request);
        return NoContent();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("No user ID in token."));
}
