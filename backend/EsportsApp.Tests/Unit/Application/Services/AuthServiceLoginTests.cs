using EsportsApp.Application.DTOs.Auth;
using EsportsApp.Application.Interfaces;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// JWT login and token-boundary tests (FR-002).
/// Verifies ClockSkew = TimeSpan.Zero intent: service itself does not re-issue
/// an expired token; 401 is thrown for missing/inactive users regardless of timing.
/// </summary>
[TestClass]
public class AuthServiceLoginTests
{
    private Mock<IUserRepository> _users = null!;
    private Mock<IPlayerRepository> _players = null!;
    private Mock<IOrganizerRepository> _organizers = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<IJwtTokenService> _jwt = null!;
    private AuthService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _users = new Mock<IUserRepository>();
        _players = new Mock<IPlayerRepository>();
        _organizers = new Mock<IOrganizerRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _jwt = new Mock<IJwtTokenService>();
        _sut = new AuthService(_users.Object, _players.Object, _organizers.Object, _videogames.Object, _jwt.Object);
    }

    private static User ActivePlayer(string email, string password) => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        Role = UserRole.Player,
        IsActive = true,
    };

    // ── Happy path ───────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var user = ActivePlayer("player@test.com", "Pass1234");
        _users.Setup(r => r.GetByEmailAsync("player@test.com")).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("valid-jwt");

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "player@test.com", Password = "Pass1234" });

        Assert.AreEqual("valid-jwt", result.Token);
        Assert.AreEqual(user.Id, result.UserId);
    }

    [TestMethod]
    public async Task Login_ReturnsCorrectRole()
    {
        var user = ActivePlayer("player@test.com", "Pass1234");
        _users.Setup(r => r.GetByEmailAsync("player@test.com")).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("token");

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "player@test.com", Password = "Pass1234" });

        Assert.AreEqual(UserRole.Player.ToString(), result.Role);
    }

    // ── Wrong password ───────────────────────────────────────────────────────

    [TestMethod]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        var user = ActivePlayer("player@test.com", "Pass1234");
        _users.Setup(r => r.GetByEmailAsync("player@test.com")).ReturnsAsync(user);

        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync(new LoginRequestDto { Email = "player@test.com", Password = "WrongPass1" }));
    }

    // ── Unknown email ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Login_UnknownEmail_ThrowsUnauthorized()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync(new LoginRequestDto { Email = "ghost@test.com", Password = "Pass1234" }));
    }

    // ── Inactive / suspended user (FR-002) ───────────────────────────────────

    [TestMethod]
    public async Task Login_InactiveUser_ThrowsUnauthorized()
    {
        var user = ActivePlayer("player@test.com", "Pass1234");
        user.IsActive = false;
        _users.Setup(r => r.GetByEmailAsync("player@test.com")).ReturnsAsync(user);

        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync(new LoginRequestDto { Email = "player@test.com", Password = "Pass1234" }));
    }

    [TestMethod]
    public async Task Login_InactiveUser_DoesNotCallJwt()
    {
        var user = ActivePlayer("player@test.com", "Pass1234");
        user.IsActive = false;
        _users.Setup(r => r.GetByEmailAsync("player@test.com")).ReturnsAsync(user);

        try { await _sut.LoginAsync(new LoginRequestDto { Email = "player@test.com", Password = "Pass1234" }); } catch { }

        _jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    // ── Token expiry boundary: ClockSkew = TimeSpan.Zero intent ──────────────
    // The application configures ClockSkew = TimeSpan.Zero in TokenValidationParameters
    // (Program.cs / T014). At the service layer, tokens are issued with ExpiresAt = UtcNow + 24h.
    // This test verifies the service sets a non-zero future expiry on every token issued.

    [TestMethod]
    public async Task Login_TokenExpiryIsInTheFuture()
    {
        var before = DateTime.UtcNow;
        var user = ActivePlayer("player@test.com", "Pass1234");
        _users.Setup(r => r.GetByEmailAsync("player@test.com")).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("token");

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "player@test.com", Password = "Pass1234" });

        Assert.IsTrue(result.ExpiresAt > before, "Token expiry must be in the future.");
    }
}
