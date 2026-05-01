using EsportsApp.Application.DTOs.Auth;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;

namespace EsportsApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPlayerRepository _players;
    private readonly IOrganizerRepository _organizers;
    private readonly IVideogameRepository _videogames;
    private readonly IJwtTokenService _jwt;

    public AuthService(IUserRepository users, IPlayerRepository players,
        IOrganizerRepository organizers, IVideogameRepository videogames, IJwtTokenService jwt)
    {
        _users = users;
        _players = players;
        _organizers = organizers;
        _videogames = videogames;
        _jwt = jwt;
    }

    public async Task<AuthResultDto> RegisterPlayerAsync(RegisterPlayerRequestDto request)
    {
        if (await _users.EmailExistsAsync(request.Email))
            throw new ConflictException("Email already in use.");

        if (await _players.UsernameExistsAsync(request.Username))
            throw new ConflictException("Username already in use.");

        if (!await _videogames.ExistsAsync(request.VideogameId))
            throw new ValidationException("videogameId", "Videogame not found.");

        if (!IsPasswordComplex(request.Password))
            throw new ValidationException("password", "Password must be at least 8 characters with uppercase, lowercase, and digit.");

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Player,
        };

        var player = new Player
        {
            Id = userId,
            Username = request.Username,
            RealName = request.RealName,
            MainVideogameId = request.VideogameId,
        };

        await _users.AddAsync(user);
        await _players.AddAsync(player);
        await _users.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);
        return new AuthResultDto { Token = token, UserId = userId, Role = user.Role.ToString(), Email = user.Email, ExpiresAt = DateTime.UtcNow.AddHours(24) };
    }

    public async Task<AuthResultDto> RegisterOrganizerAsync(RegisterOrganizerRequestDto request)
    {
        if (await _users.EmailExistsAsync(request.Email))
            throw new ConflictException("Email already in use.");

        if (await _organizers.OrganizationNameExistsAsync(request.OrganizationName))
            throw new ConflictException("Organization name already in use.");

        if (!IsPasswordComplex(request.Password))
            throw new ValidationException("password", "Password must be at least 8 characters with uppercase, lowercase, and digit.");

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Organizer,
        };

        var organizer = new Organizer
        {
            Id = userId,
            OrganizationName = request.OrganizationName,
        };

        await _users.AddAsync(user);
        await _organizers.AddAsync(organizer);
        await _users.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);
        return new AuthResultDto { Token = token, UserId = userId, Role = user.Role.ToString(), Email = user.Email, ExpiresAt = DateTime.UtcNow.AddHours(24) };
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _users.GetByEmailAsync(request.Email);
        if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _jwt.GenerateToken(user);

        return new AuthResultDto
        {
            Token = token,
            UserId = user.Id,
            Role = user.Role.ToString(),
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
        };
    }

    private static bool IsPasswordComplex(string password) =>
        password.Length >= 8
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit);
}
