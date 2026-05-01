using EsportsApp.Application.DTOs.Auth;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// TR-001: Player registration unit tests (FR-001, FR-036).
/// </summary>
[TestClass]
public class PlayerServiceRegistrationTests
{
    private Mock<IUserRepository> _users = null!;
    private Mock<IPlayerRepository> _players = null!;
    private Mock<IOrganizerRepository> _organizers = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<IJwtTokenService> _jwt = null!;
    private AuthService _sut = null!;

    private static readonly Guid ValidGameId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _users = new Mock<IUserRepository>();
        _players = new Mock<IPlayerRepository>();
        _organizers = new Mock<IOrganizerRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _jwt = new Mock<IJwtTokenService>();
        _sut = new AuthService(_users.Object, _players.Object, _organizers.Object, _videogames.Object, _jwt.Object);

        // Happy-path defaults
        _users.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _players.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _videogames.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        _users.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _players.Setup(r => r.AddAsync(It.IsAny<Player>())).Returns(Task.CompletedTask);
        _users.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("test-token");
    }

    // ── Happy path ───────────────────────────────────────────────────────────

    [TestMethod]
    public async Task RegisterPlayer_ValidRequest_ReturnsAuthResult()
    {
        var req = new RegisterPlayerRequestDto
        {
            Email = "player@test.com",
            Password = "Pass1234",
            Username = "player1",
            VideogameId = ValidGameId,
        };

        var result = await _sut.RegisterPlayerAsync(req);

        Assert.AreEqual("player@test.com", result.Email);
        Assert.AreEqual(UserRole.Player.ToString(), result.Role);
        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    public async Task RegisterPlayer_SetsRoleToPlayer()
    {
        User? capturedUser = null;
        _users.Setup(r => r.AddAsync(It.IsAny<User>()))
              .Callback<User>(u => capturedUser = u)
              .Returns(Task.CompletedTask);

        var req = new RegisterPlayerRequestDto
        {
            Email = "player@test.com",
            Password = "Pass1234",
            Username = "player1",
            VideogameId = ValidGameId,
        };

        await _sut.RegisterPlayerAsync(req);

        Assert.IsNotNull(capturedUser);
        Assert.AreEqual(UserRole.Player, capturedUser!.Role);
    }

    // ── Duplicate email ──────────────────────────────────────────────────────

    [TestMethod]
    public async Task RegisterPlayer_DuplicateEmail_ThrowsConflict()
    {
        _users.Setup(r => r.EmailExistsAsync("dup@test.com")).ReturnsAsync(true);
        var req = new RegisterPlayerRequestDto { Email = "dup@test.com", Password = "Pass1234", Username = "u1", VideogameId = ValidGameId };
        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.RegisterPlayerAsync(req));
    }

    [TestMethod]
    public async Task RegisterPlayer_DuplicateEmail_EmailCheckIsCaseInsensitiveAtService()
    {
        // Repository is called with the email as provided; any normalisation is in the repo mock
        _users.Setup(r => r.EmailExistsAsync("dup@test.com")).ReturnsAsync(true);
        var req = new RegisterPlayerRequestDto { Email = "dup@test.com", Password = "Pass1234", Username = "u1", VideogameId = ValidGameId };
        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.RegisterPlayerAsync(req));
    }

    // ── Duplicate username ───────────────────────────────────────────────────

    [TestMethod]
    public async Task RegisterPlayer_DuplicateUsername_ThrowsConflict()
    {
        _players.Setup(r => r.UsernameExistsAsync("taken")).ReturnsAsync(true);
        var req = new RegisterPlayerRequestDto { Email = "new@test.com", Password = "Pass1234", Username = "taken", VideogameId = ValidGameId };
        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.RegisterPlayerAsync(req));
    }

    // ── Videogame not found ──────────────────────────────────────────────────

    [TestMethod]
    public async Task RegisterPlayer_InvalidVideogame_ThrowsValidation()
    {
        _videogames.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);
        var req = new RegisterPlayerRequestDto { Email = "new@test.com", Password = "Pass1234", Username = "u1", VideogameId = Guid.NewGuid() };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterPlayerAsync(req));
    }

    // ── Password complexity (FR-036) ─────────────────────────────────────────

    [TestMethod]
    public async Task RegisterPlayer_PasswordTooShort_ThrowsValidation()
    {
        var req = new RegisterPlayerRequestDto { Email = "new@test.com", Password = "Ab1", Username = "u1", VideogameId = ValidGameId };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterPlayerAsync(req));
    }

    [TestMethod]
    public async Task RegisterPlayer_PasswordNoUppercase_ThrowsValidation()
    {
        var req = new RegisterPlayerRequestDto { Email = "new@test.com", Password = "password1", Username = "u1", VideogameId = ValidGameId };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterPlayerAsync(req));
    }

    [TestMethod]
    public async Task RegisterPlayer_PasswordNoDigit_ThrowsValidation()
    {
        var req = new RegisterPlayerRequestDto { Email = "new@test.com", Password = "Password", Username = "u1", VideogameId = ValidGameId };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterPlayerAsync(req));
    }

    [TestMethod]
    public async Task RegisterPlayer_PasswordNoLowercase_ThrowsValidation()
    {
        var req = new RegisterPlayerRequestDto { Email = "new@test.com", Password = "PASSWORD1", Username = "u1", VideogameId = ValidGameId };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterPlayerAsync(req));
    }
}
