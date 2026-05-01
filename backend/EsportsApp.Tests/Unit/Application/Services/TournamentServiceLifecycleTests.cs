using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// TR-002: Tournament lifecycle transitions and delete-ownership unit tests.
/// </summary>
[TestClass]
public class TournamentServiceLifecycleTests
{
    private Mock<ITournamentRepository> _tournaments = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<ITournamentRegistrationRepository> _registrations = null!;
    private TournamentService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _tournaments = new Mock<ITournamentRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _registrations = new Mock<ITournamentRegistrationRepository>();
        _sut = new TournamentService(_tournaments.Object, _videogames.Object, _registrations.Object);

        _tournaments.Setup(r => r.UpdateAsync(It.IsAny<Tournament>())).Returns(Task.CompletedTask);
        _tournaments.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _tournaments.Setup(r => r.DeleteAsync(It.IsAny<Tournament>())).Returns(Task.CompletedTask);
    }

    // ── Status transitions ────────────────────────────────────────────────────

    [TestMethod]
    public async Task AdvanceStatus_Draft_BecomesOpen()
    {
        var orgId = Guid.NewGuid();
        var t = new Tournament { OrganizerId = orgId, Status = TournamentStatus.Draft };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await _sut.AdvanceStatusAsync(orgId, t.Id);

        Assert.AreEqual(TournamentStatus.Open, t.Status);
    }

    [TestMethod]
    public async Task AdvanceStatus_Open_BecomesInProgress()
    {
        var orgId = Guid.NewGuid();
        var t = new Tournament { OrganizerId = orgId, Status = TournamentStatus.Open };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await _sut.AdvanceStatusAsync(orgId, t.Id);

        Assert.AreEqual(TournamentStatus.InProgress, t.Status);
    }

    [TestMethod]
    public async Task AdvanceStatus_InProgress_BecomesCompleted()
    {
        var orgId = Guid.NewGuid();
        var t = new Tournament { OrganizerId = orgId, Status = TournamentStatus.InProgress };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await _sut.AdvanceStatusAsync(orgId, t.Id);

        Assert.AreEqual(TournamentStatus.Completed, t.Status);
    }

    [TestMethod]
    public async Task AdvanceStatus_Completed_ThrowsUnprocessable()
    {
        var orgId = Guid.NewGuid();
        var t = new Tournament { OrganizerId = orgId, Status = TournamentStatus.Completed };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() => _sut.AdvanceStatusAsync(orgId, t.Id));
    }

    [TestMethod]
    public async Task AdvanceStatus_Suspended_ThrowsUnprocessable()
    {
        var orgId = Guid.NewGuid();
        var t = new Tournament { OrganizerId = orgId, Status = TournamentStatus.Suspended };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() => _sut.AdvanceStatusAsync(orgId, t.Id));
    }

    // ── Ownership guard ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task AdvanceStatus_WrongOrganizer_ThrowsForbidden()
    {
        var t = new Tournament { OrganizerId = Guid.NewGuid(), Status = TournamentStatus.Draft };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() => _sut.AdvanceStatusAsync(Guid.NewGuid(), t.Id));
    }

    [TestMethod]
    public async Task AdvanceStatus_TournamentNotFound_ThrowsNotFound()
    {
        _tournaments.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Tournament?)null);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _sut.AdvanceStatusAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Delete_DraftTournament_Succeeds()
    {
        var orgId = Guid.NewGuid();
        var t = new Tournament { OrganizerId = orgId, Status = TournamentStatus.Draft };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await _sut.DeleteAsync(orgId, t.Id); // should not throw

        _tournaments.Verify(r => r.DeleteAsync(t), Times.Once);
    }

    [TestMethod]
    public async Task Delete_OpenTournament_Succeeds()
    {
        var orgId = Guid.NewGuid();
        var t = new Tournament { OrganizerId = orgId, Status = TournamentStatus.Open };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await _sut.DeleteAsync(orgId, t.Id);

        _tournaments.Verify(r => r.DeleteAsync(t), Times.Once);
    }

    [TestMethod]
    public async Task Delete_InProgressTournament_ThrowsForbidden()
    {
        var orgId = Guid.NewGuid();
        var t = new Tournament { OrganizerId = orgId, Status = TournamentStatus.InProgress };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() => _sut.DeleteAsync(orgId, t.Id));
    }

    [TestMethod]
    public async Task Delete_CompletedTournament_ThrowsForbidden()
    {
        var orgId = Guid.NewGuid();
        var t = new Tournament { OrganizerId = orgId, Status = TournamentStatus.Completed };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() => _sut.DeleteAsync(orgId, t.Id));
    }

    [TestMethod]
    public async Task Delete_WrongOrganizer_ThrowsForbidden()
    {
        var t = new Tournament { OrganizerId = Guid.NewGuid(), Status = TournamentStatus.Draft };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() => _sut.DeleteAsync(Guid.NewGuid(), t.Id));
    }

    // ── Suspend / Reinstate ───────────────────────────────────────────────────

    [TestMethod]
    public async Task Suspend_OpenTournament_SetsSuspendedAndSavesPreStatus()
    {
        var t = new Tournament { OrganizerId = Guid.NewGuid(), Status = TournamentStatus.Open };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await _sut.SuspendAsync(Guid.NewGuid(), t.Id);

        Assert.AreEqual(TournamentStatus.Suspended, t.Status);
        Assert.AreEqual(TournamentStatus.Open, t.PreSuspensionStatus);
    }

    [TestMethod]
    public async Task Suspend_CompletedTournament_ThrowsUnprocessable()
    {
        var t = new Tournament { Status = TournamentStatus.Completed };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() => _sut.SuspendAsync(Guid.NewGuid(), t.Id));
    }

    [TestMethod]
    public async Task Suspend_AlreadySuspended_ThrowsConflict()
    {
        var t = new Tournament { Status = TournamentStatus.Suspended };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.SuspendAsync(Guid.NewGuid(), t.Id));
    }

    [TestMethod]
    public async Task Reinstate_SuspendedTournament_RestoresPreSuspensionStatus()
    {
        var t = new Tournament
        {
            Status = TournamentStatus.Suspended,
            PreSuspensionStatus = TournamentStatus.InProgress,
        };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await _sut.ReinstateAsync(Guid.NewGuid(), t.Id);

        Assert.AreEqual(TournamentStatus.InProgress, t.Status);
        Assert.IsNull(t.PreSuspensionStatus);
    }

    [TestMethod]
    public async Task Reinstate_NotSuspended_ThrowsUnprocessable()
    {
        var t = new Tournament { Status = TournamentStatus.Open };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() => _sut.ReinstateAsync(Guid.NewGuid(), t.Id));
    }
}
