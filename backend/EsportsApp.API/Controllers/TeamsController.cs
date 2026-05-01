using System.Security.Claims;
using EsportsApp.Application.DTOs.Teams;
using EsportsApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsportsApp.API.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teams;

    public TeamsController(ITeamService teams) => _teams = teams;

    [HttpGet("my")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> GetMyTeam()
    {
        var playerId = GetUserId();
        var team = await _teams.GetMyTeamAsync(playerId);
        if (team == null) return NotFound();
        return Ok(team);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var team = await _teams.GetByIdAsync(id);
        return Ok(team);
    }

    [HttpPost]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequestDto request)
    {
        var playerId = GetUserId();
        var team = await _teams.CreateAsync(playerId, request);
        return StatusCode(201, team);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamRequestDto request)
    {
        var playerId = GetUserId();
        var team = await _teams.UpdateAsync(playerId, id, request);
        return Ok(team);
    }

    [HttpPost("{id:guid}/invitations")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> InvitePlayer(Guid id, [FromBody] InvitePlayerRequestDto request)
    {
        var playerId = GetUserId();
        await _teams.InvitePlayerAsync(playerId, id, request);
        return StatusCode(201);
    }

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId)
    {
        var playerId = GetUserId();
        await _teams.RemoveMemberAsync(playerId, id, memberId);
        return NoContent();
    }

    [HttpPost("{id:guid}/transfer-captaincy")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> TransferCaptaincy(Guid id, [FromBody] TransferCaptaincyRequestDto request)
    {
        var playerId = GetUserId();
        await _teams.TransferCaptaincyAsync(playerId, id, request.NewCaptainId);
        return NoContent();
    }

    [HttpGet("invitations/my")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> GetMyInvitations()
    {
        var playerId = GetUserId();
        var invitations = await _teams.GetMyInvitationsAsync(playerId);
        return Ok(invitations);
    }

    [HttpPost("invitations/{invitationId:guid}/respond")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> RespondToInvitation(Guid invitationId, [FromBody] RespondInvitationRequestDto request)
    {
        var playerId = GetUserId();
        await _teams.RespondToInvitationAsync(playerId, invitationId, request.Accept);
        return NoContent();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);
}

public class TransferCaptaincyRequestDto
{
    public Guid NewCaptainId { get; set; }
}

public class RespondInvitationRequestDto
{
    public bool Accept { get; set; }
}
