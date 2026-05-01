using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// UserService unit tests covering Player own-profile view (FR-009),
/// Organizer allowed-field update rules (FR-037: org name uniqueness, length, password complexity).
/// </summary>
[TestClass]
public class UserServiceTests
{
    private Mock<IUserRepository> _users = null!;
    private Mock<IPlayerRepository> _players = null!;
    private Mock<IOrganizerRepository> _organizers = null!;
    private UserService _sut = null!;

    private static readonly Guid PlayerId = Guid.NewGuid();
    private static readonly Guid OrgUserId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _users = new Mock<IUserRepository>();
        _players = new Mock<IPlayerRepository>();
        _organizers = new Mock<IOrganizerRepository>();
        _sut = new UserService(_users.Object, _players.Object, _organizers.Object);

        _players.Setup(r => r.UpdateAsync(It.IsAny<Player>())).Returns(Task.CompletedTask);
        _players.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _organizers.Setup(r => r.UpdateAsync(It.IsAny<Organizer>())).Returns(Task.CompletedTask);
        _organizers.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _users.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _users.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
    }

    // ── GetProfile: Player (FR-009) ───────────────────────────────────────────

    [TestMethod]
    public async Task GetProfile_Player_ReturnsPlayerFields()
    {
        var gameId = Guid.NewGuid();
        var user = new User
        {
            Id = PlayerId,
            Email = "player@test.com",
            Role = UserRole.Player,
            IsActive = true,
            Player = new Player { Id = PlayerId, Username = "pro_player", RealName = "Alice", MainVideogameId = gameId },
        };
        _users.Setup(r => r.GetByIdAsync(PlayerId)).ReturnsAsync(user);

        var result = await _sut.GetProfileAsync(PlayerId);

        Assert.AreEqual("player@test.com", result.Email);
        Assert.AreEqual("pro_player", result.Username);
        Assert.AreEqual(UserRole.Player.ToString(), result.Role);
    }

    [TestMethod]
    public async Task GetProfile_UserNotFound_ThrowsNotFound()
    {
        _users.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _sut.GetProfileAsync(Guid.NewGuid()));
    }

    [TestMethod]
    public async Task GetProfile_Organizer_ReturnsOrgName()
    {
        var user = new User
        {
            Id = OrgUserId,
            Email = "org@test.com",
            Role = UserRole.Organizer,
            IsActive = true,
            Organizer = new Organizer { Id = OrgUserId, OrganizationName = "Super Org" },
        };
        _users.Setup(r => r.GetByIdAsync(OrgUserId)).ReturnsAsync(user);

        var result = await _sut.GetProfileAsync(OrgUserId);

        Assert.AreEqual("Super Org", result.OrganizationName);
    }

    // ── UpdateOrganizerProfile: name uniqueness (FR-037) ─────────────────────

    [TestMethod]
    public async Task UpdateOrganizerProfile_DuplicateOrgName_ThrowsConflict()
    {
        var organizer = new Organizer { Id = OrgUserId, OrganizationName = "OldOrg" };
        _organizers.Setup(r => r.GetByIdAsync(OrgUserId)).ReturnsAsync(organizer);
        _organizers.Setup(r => r.OrganizationNameExistsAsync("TakenOrg")).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.UpdateOrganizerProfileAsync(OrgUserId, new EsportsApp.Application.DTOs.Users.UpdateOrganizerProfileRequestDto
            {
                OrganizationName = "TakenOrg",
            }));
    }

    [TestMethod]
    public async Task UpdateOrganizerProfile_SameOrgNameUnchanged_DoesNotThrow()
    {
        var organizer = new Organizer { Id = OrgUserId, OrganizationName = "SameOrg" };
        _organizers.Setup(r => r.GetByIdAsync(OrgUserId)).ReturnsAsync(organizer);
        // No conflict check triggered when name is unchanged (service skips uniqueness check)
        _organizers.Setup(r => r.OrganizationNameExistsAsync("SameOrg")).ReturnsAsync(true); // even if it exists

        await _sut.UpdateOrganizerProfileAsync(OrgUserId, new EsportsApp.Application.DTOs.Users.UpdateOrganizerProfileRequestDto
        {
            OrganizationName = "SameOrg",
        });
        // Should not throw
    }

    // ── UpdateOrganizerProfile: password complexity (FR-037) ─────────────────

    [TestMethod]
    public async Task UpdateOrganizerProfile_WeakNewPassword_ThrowsValidation()
    {
        var orgHash = BCrypt.Net.BCrypt.HashPassword("OldPass1");
        var organizer = new Organizer { Id = OrgUserId, OrganizationName = "Org" };
        var user = new User { Id = OrgUserId, PasswordHash = orgHash };
        _organizers.Setup(r => r.GetByIdAsync(OrgUserId)).ReturnsAsync(organizer);
        _organizers.Setup(r => r.OrganizationNameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _users.Setup(r => r.GetByIdAsync(OrgUserId)).ReturnsAsync(user);

        await Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _sut.UpdateOrganizerProfileAsync(OrgUserId, new EsportsApp.Application.DTOs.Users.UpdateOrganizerProfileRequestDto
            {
                OrganizationName = "Org",
                CurrentPassword = "OldPass1",
                NewPassword = "simple", // no uppercase, no digit
            }));
    }

    [TestMethod]
    public async Task UpdateOrganizerProfile_WrongCurrentPassword_ThrowsValidation()
    {
        var orgHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1");
        var organizer = new Organizer { Id = OrgUserId, OrganizationName = "Org" };
        var user = new User { Id = OrgUserId, PasswordHash = orgHash };
        _organizers.Setup(r => r.GetByIdAsync(OrgUserId)).ReturnsAsync(organizer);
        _organizers.Setup(r => r.OrganizationNameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _users.Setup(r => r.GetByIdAsync(OrgUserId)).ReturnsAsync(user);

        await Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _sut.UpdateOrganizerProfileAsync(OrgUserId, new EsportsApp.Application.DTOs.Users.UpdateOrganizerProfileRequestDto
            {
                OrganizationName = "Org",
                CurrentPassword = "WrongPass1",
                NewPassword = "NewValid1",
            }));
    }

    // ── UpdatePlayerProfile: email uniqueness ─────────────────────────────────

    [TestMethod]
    public async Task UpdatePlayerProfile_DuplicateEmail_ThrowsConflict()
    {
        var player = new Player { Id = PlayerId };
        var user = new User { Id = PlayerId, Email = "old@test.com", PasswordHash = "hash" };
        _players.Setup(r => r.GetByIdAsync(PlayerId)).ReturnsAsync(player);
        _users.Setup(r => r.GetByIdAsync(PlayerId)).ReturnsAsync(user);
        _users.Setup(r => r.EmailExistsAsync("taken@test.com")).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.UpdatePlayerProfileAsync(PlayerId, new EsportsApp.Application.DTOs.Users.UpdatePlayerProfileRequestDto
            {
                Email = "taken@test.com",
            }));
    }

    [TestMethod]
    public async Task UpdatePlayerProfile_WeakNewPassword_ThrowsValidation()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("CurrentPass1");
        var player = new Player { Id = PlayerId };
        var user = new User { Id = PlayerId, Email = "player@test.com", PasswordHash = hash };
        _players.Setup(r => r.GetByIdAsync(PlayerId)).ReturnsAsync(player);
        _users.Setup(r => r.GetByIdAsync(PlayerId)).ReturnsAsync(user);

        await Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _sut.UpdatePlayerProfileAsync(PlayerId, new EsportsApp.Application.DTOs.Users.UpdatePlayerProfileRequestDto
            {
                CurrentPassword = "CurrentPass1",
                NewPassword = "weak",
            }));
    }
}
