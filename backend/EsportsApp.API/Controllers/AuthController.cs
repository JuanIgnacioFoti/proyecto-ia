using EsportsApp.Application.DTOs.Auth;
using EsportsApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EsportsApp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register/player")]
    public async Task<IActionResult> RegisterPlayer([FromBody] RegisterPlayerRequestDto request)
    {
        var result = await _auth.RegisterPlayerAsync(request);
        return StatusCode(201, result);
    }

    [HttpPost("register/organizer")]
    public async Task<IActionResult> RegisterOrganizer([FromBody] RegisterOrganizerRequestDto request)
    {
        var result = await _auth.RegisterOrganizerAsync(request);
        return StatusCode(201, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _auth.LoginAsync(request);
        return Ok(result);
    }
}
