using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// AdminService suspend/reinstate unit tests (FR-008, FR-012).
/// Tests: suspend moves non-Completed tournaments to Suspended,
/// reinstate restores user active status, guard prevents suspending another admin.
/// </summary>
[TestClass]
public class AdminServiceTests
{
    private Mock<IUserRepository> _users = null!;
    private Mock<ITournamentRepository> _tournaments = null!;
    private AdminService _sut = null!;

    private static readonly Guid AdminId = Guid.NewGuid();
    private static readonly Guid TargetId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _users = new Mock<IUserRepository>();
        _tournaments = new Mock<ITournamentRepository>();
        _sut = new AdminService(_users.Object, _tournaments.Object);

        _users.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _users.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _tournaments.Setup(r => r.UpdateAsync(It.IsAny<Tournament>())).Returns(Task.CompletedTask);
    }

    // ── Suspend: happy path ───────────────────────────────────────────────────

    [TestMethod]
    public async Task SuspendUser_ActivePlayer_SetsInactive()
    {
        var user = new User { Id = TargetId, Role = UserRole.Player, IsActive = true };
        _users.Setup(r => r.GetByIdAsync(TargetId)).ReturnsAsync(user);
        _tournaments.Setup(r => r.GetByOrganizerAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Tournament>());

        await _sut.SuspendUserAsync(AdminId, TargetId);

        Assert.IsFalse(user.IsActive);
    }

    [TestMethod]
    public async Task SuspendUser_OrganizerWithOpenTournament_SuspendsTournament()
    {
        var orgId = Guid.NewGuid();
        var organizer = new Organizer { Id = orgId };
        var user = new User { Id = TargetId, Role = UserRole.Organizer, IsActive = true, Organizer = organizer };
        user.Organizer.Id = orgId;

        var tournament = new Tournament { Id = Guid.NewGuid(), OrganizerId = orgId, Status = TournamentStatus.Open };

        _users.Setup(r => r.GetByIdAsync(TargetId)).ReturnsAsync(user);
        _tournaments.Setup(r => r.GetByOrganizerAsync(orgId)).ReturnsAsync(new List<Tournament> { tournament });

        await _sut.SuspendUserAsync(AdminId, TargetId);

        Assert.AreEqual(TournamentStatus.Suspended, tournament.Status);
        Assert.AreEqual(TournamentStatus.Open, tournament.PreSuspensionStatus);
    }

    [TestMethod]
    public async Task SuspendUser_OrganizerWithCompletedTournament_DoesNotSuspendTournament()
    {
        var orgId = Guid.NewGuid();
        var organizer = new Organizer { Id = orgId };
        var user = new User { Id = TargetId, Role = UserRole.Organizer, IsActive = true, Organizer = organizer };

        var tournament = new Tournament { Id = Guid.NewGuid(), OrganizerId = orgId, Status = TournamentStatus.Completed };

        _users.Setup(r => r.GetByIdAsync(TargetId)).ReturnsAsync(user);
        _tournaments.Setup(r => r.GetByOrganizerAsync(orgId)).ReturnsAsync(new List<Tournament> { tournament });

        await _sut.SuspendUserAsync(AdminId, TargetId);

        // Completed tournaments must not change status
        Assert.AreEqual(TournamentStatus.Completed, tournament.Status);
    }

    [TestMethod]
    public async Task SuspendUser_OrganizerWithAlreadySuspendedTournament_DoesNotDouble()
    {
        var orgId = Guid.NewGuid();
        var organizer = new Organizer { Id = orgId };
        var user = new User { Id = TargetId, Role = UserRole.Organizer, IsActive = true, Organizer = organizer };

        var tournament = new Tournament
        {
            Id = Guid.NewGuid(),
            OrganizerId = orgId,
            Status = TournamentStatus.Suspended,
            PreSuspensionStatus = TournamentStatus.Open,
        };

        _users.Setup(r => r.GetByIdAsync(TargetId)).ReturnsAsync(user);
        _tournaments.Setup(r => r.GetByOrganizerAsync(orgId)).ReturnsAsync(new List<Tournament> { tournament });

        await _sut.SuspendUserAsync(AdminId, TargetId);

        // Status unchanged; PreSuspensionStatus must remain the same (Open)
        Assert.AreEqual(TournamentStatus.Suspended, tournament.Status);
        Assert.AreEqual(TournamentStatus.Open, tournament.PreSuspensionStatus);
    }

    // ── Suspend: cannot suspend another admin (FR-008) ────────────────────────

    [TestMethod]
    public async Task SuspendUser_TargetIsAdmin_ThrowsForbidden()
    {
        var adminTarget = new User { Id = TargetId, Role = UserRole.Admin, IsActive = true };
        _users.Setup(r => r.GetByIdAsync(TargetId)).ReturnsAsync(adminTarget);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() => _sut.SuspendUserAsync(AdminId, TargetId));
    }

    [TestMethod]
    public async Task SuspendUser_UserNotFound_ThrowsNotFound()
    {
        _users.Setup(r => r.GetByIdAsync(TargetId)).ReturnsAsync((User?)null);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _sut.SuspendUserAsync(AdminId, TargetId));
    }

    // ── Reinstate: happy path ─────────────────────────────────────────────────

    [TestMethod]
    public async Task ReinstateUser_InactiveUser_SetsActive()
    {
        var user = new User { Id = TargetId, Role = UserRole.Player, IsActive = false };
        _users.Setup(r => r.GetByIdAsync(TargetId)).ReturnsAsync(user);

        await _sut.ReinstateUserAsync(AdminId, TargetId);

        Assert.IsTrue(user.IsActive);
    }

    [TestMethod]
    public async Task ReinstateUser_UserNotFound_ThrowsNotFound()
    {
        _users.Setup(r => r.GetByIdAsync(TargetId)).ReturnsAsync((User?)null);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _sut.ReinstateUserAsync(AdminId, TargetId));
    }
}
