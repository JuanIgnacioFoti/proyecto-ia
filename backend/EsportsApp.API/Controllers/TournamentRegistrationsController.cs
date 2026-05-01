using System.Security.Claims;
using EsportsApp.Application.DTOs.Registrations;
using EsportsApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsportsApp.API.Controllers;

[ApiController]
[Route("api/tournaments/{tournamentId:guid}/registrations")]
public class TournamentRegistrationsController : ControllerBase
{
    private readonly ITournamentRegistrationService _registrations;

    public TournamentRegistrationsController(ITournamentRegistrationService registrations) =>
        _registrations = registrations;

    [HttpGet]
    public async Task<IActionResult> GetRegistrations(Guid tournamentId)
    {
        var list = await _registrations.GetByTournamentAsync(tournamentId);
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> Register(Guid tournamentId, [FromBody] RegisterTeamRequestDto request)
    {
        var captainId = GetUserId();
        var result = await _registrations.RegisterAsync(captainId, tournamentId, request);
        return StatusCode(201, result);
    }

    [HttpDelete("{teamId:guid}")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> Withdraw(Guid tournamentId, Guid teamId)
    {
        var captainId = GetUserId();
        await _registrations.WithdrawAsync(captainId, tournamentId, teamId);
        return NoContent();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);
}
