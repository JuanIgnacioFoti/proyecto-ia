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
/// TR-001: Organizer registration validation unit tests (FR-036).
/// </summary>
[TestClass]
public class OrganizerServiceRegistrationTests
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

        // Happy-path defaults
        _users.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _organizers.Setup(r => r.OrganizationNameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _users.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _organizers.Setup(r => r.AddAsync(It.IsAny<Organizer>())).Returns(Task.CompletedTask);
        _users.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("test-token");
    }

    // ── Happy path ───────────────────────────────────────────────────────────

    [TestMethod]
    public async Task RegisterOrganizer_ValidRequest_ReturnsAuthResult()
    {
        var req = new RegisterOrganizerRequestDto
        {
            Email = "org@test.com",
            Password = "Pass1234",
            OrganizationName = "Team Alpha",
        };

        var result = await _sut.RegisterOrganizerAsync(req);

        Assert.AreEqual("org@test.com", result.Email);
        Assert.AreEqual(UserRole.Organizer.ToString(), result.Role);
        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    public async Task RegisterOrganizer_SetsRoleToOrganizer()
    {
        User? capturedUser = null;
        _users.Setup(r => r.AddAsync(It.IsAny<User>()))
              .Callback<User>(u => capturedUser = u)
              .Returns(Task.CompletedTask);

        var req = new RegisterOrganizerRequestDto
        {
            Email = "org@test.com",
            Password = "Pass1234",
            OrganizationName = "Team Alpha",
        };

        await _sut.RegisterOrganizerAsync(req);

        Assert.IsNotNull(capturedUser);
        Assert.AreEqual(UserRole.Organizer, capturedUser!.Role);
    }

    // ── Duplicate email ──────────────────────────────────────────────────────

    [TestMethod]
    public async Task RegisterOrganizer_DuplicateEmail_ThrowsConflict()
    {
        _users.Setup(r => r.EmailExistsAsync("dup@test.com")).ReturnsAsync(true);
        var req = new RegisterOrganizerRequestDto { Email = "dup@test.com", Password = "Pass1234", OrganizationName = "Org1" };
        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.RegisterOrganizerAsync(req));
    }

    // ── Duplicate organization name (FR-036) ─────────────────────────────────

    [TestMethod]
    public async Task RegisterOrganizer_DuplicateOrgName_ThrowsConflict()
    {
        _organizers.Setup(r => r.OrganizationNameExistsAsync("TakenOrg")).ReturnsAsync(true);
        var req = new RegisterOrganizerRequestDto { Email = "new@test.com", Password = "Pass1234", OrganizationName = "TakenOrg" };
        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.RegisterOrganizerAsync(req));
    }

    // ── Password complexity (FR-036) ─────────────────────────────────────────

    [TestMethod]
    public async Task RegisterOrganizer_WeakPassword_ThrowsValidation()
    {
        var req = new RegisterOrganizerRequestDto { Email = "new@test.com", Password = "password", OrganizationName = "Org1" };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterOrganizerAsync(req));
    }

    [TestMethod]
    public async Task RegisterOrganizer_PasswordNoUppercase_ThrowsValidation()
    {
        var req = new RegisterOrganizerRequestDto { Email = "new@test.com", Password = "pass1234", OrganizationName = "Org1" };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterOrganizerAsync(req));
    }

    [TestMethod]
    public async Task RegisterOrganizer_PasswordNoDigit_ThrowsValidation()
    {
        var req = new RegisterOrganizerRequestDto { Email = "new@test.com", Password = "Password", OrganizationName = "Org1" };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterOrganizerAsync(req));
    }

    [TestMethod]
    public async Task RegisterOrganizer_PasswordTooShort_ThrowsValidation()
    {
        var req = new RegisterOrganizerRequestDto { Email = "new@test.com", Password = "Ab1", OrganizationName = "Org1" };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterOrganizerAsync(req));
    }
}
