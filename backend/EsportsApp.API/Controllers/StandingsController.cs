using EsportsApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EsportsApp.API.Controllers;

[ApiController]
[Route("api/tournaments/{tournamentId:guid}/standings")]
public class StandingsController : ControllerBase
{
    private readonly IStandingsService _standings;

    public StandingsController(IStandingsService standings) => _standings = standings;

    [HttpGet]
    public async Task<IActionResult> GetStandings(Guid tournamentId)
    {
        var list = await _standings.GetByTournamentAsync(tournamentId);
        return Ok(list);
    }
}
