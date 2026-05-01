using System.Security.Claims;
using EsportsApp.Application.DTOs.Matches;
using EsportsApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsportsApp.API.Controllers;

[ApiController]
[Route("api/tournaments/{tournamentId:guid}/matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matches;

    public MatchesController(IMatchService matches) => _matches = matches;

    [HttpGet]
    public async Task<IActionResult> GetMatches(Guid tournamentId)
    {
        var list = await _matches.GetByTournamentAsync(tournamentId);
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Record(Guid tournamentId, [FromBody] RecordMatchRequestDto request)
    {
        var organizerId = GetUserId();
        var match = await _matches.RecordAsync(organizerId, tournamentId, request);
        return StatusCode(201, match);
    }

    [HttpPut("{matchId:guid}")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Correct(Guid tournamentId, Guid matchId, [FromBody] RecordMatchRequestDto request)
    {
        var organizerId = GetUserId();
        var match = await _matches.CorrectAsync(organizerId, tournamentId, matchId, request);
        return Ok(match);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);
}
