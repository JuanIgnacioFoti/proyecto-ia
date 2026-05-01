using System.Security.Claims;
using EsportsApp.Application.DTOs.Tournaments;
using EsportsApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsportsApp.API.Controllers;

[ApiController]
[Route("api/tournaments")]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentService _tournaments;

    public TournamentsController(ITournamentService tournaments) => _tournaments = tournaments;

    [HttpGet]
    public async Task<IActionResult> GetPublicList()
    {
        var list = await _tournaments.GetPublicListAsync();
        return Ok(list);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetMyTournaments()
    {
        var organizerId = GetUserId();
        var list = await _tournaments.GetMyTournamentsAsync(organizerId);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        Guid? userId = TryGetUserId();
        var t = await _tournaments.GetByIdAsync(id, userId);
        return Ok(t);
    }

    [HttpPost]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Create([FromBody] CreateTournamentRequestDto request)
    {
        var organizerId = GetUserId();
        var t = await _tournaments.CreateAsync(organizerId, request);
        return StatusCode(201, t);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTournamentRequestDto request)
    {
        var organizerId = GetUserId();
        var t = await _tournaments.UpdateAsync(organizerId, id, request);
        return Ok(t);
    }

    [HttpPost("{id:guid}/advance")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> AdvanceStatus(Guid id)
    {
        var organizerId = GetUserId();
        await _tournaments.AdvanceStatusAsync(organizerId, id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organizerId = GetUserId();
        await _tournaments.DeleteAsync(organizerId, id);
        return NoContent();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var adminId = GetUserId();
        await _tournaments.SuspendAsync(adminId, id);
        return NoContent();
    }

    [HttpPost("{id:guid}/reinstate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reinstate(Guid id)
    {
        var adminId = GetUserId();
        await _tournaments.ReinstateAsync(adminId, id);
        return NoContent();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    private Guid? TryGetUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return val != null ? Guid.Parse(val) : null;
    }
}
