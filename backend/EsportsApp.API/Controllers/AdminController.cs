using System.Security.Claims;
using EsportsApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsportsApp.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin) => _admin = admin;

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _admin.GetUsersAsync();
        return Ok(users);
    }

    [HttpPatch("users/{userId:guid}/role")]
    public async Task<IActionResult> SetRole(Guid userId, [FromBody] SetRoleRequestDto request)
    {
        var adminId = GetUserId();
        await _admin.SetRoleAsync(adminId, userId, request.Role);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid userId)
    {
        var adminId = GetUserId();
        await _admin.SuspendUserAsync(adminId, userId);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/reinstate")]
    public async Task<IActionResult> ReinstateUser(Guid userId)
    {
        var adminId = GetUserId();
        await _admin.ReinstateUserAsync(adminId, userId);
        return NoContent();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);
}

public class SetRoleRequestDto
{
    public string Role { get; set; } = string.Empty;
}
