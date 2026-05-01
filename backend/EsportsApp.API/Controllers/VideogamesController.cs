using EsportsApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EsportsApp.API.Controllers;

[ApiController]
[Route("api/videogames")]
public class VideogamesController : ControllerBase
{
    private readonly IVideogameService _videogames;

    public VideogamesController(IVideogameService videogames) => _videogames = videogames;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _videogames.GetAllAsync();
        return Ok(list);
    }
}
