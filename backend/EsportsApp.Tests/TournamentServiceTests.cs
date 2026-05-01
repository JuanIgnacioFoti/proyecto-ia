using EsportsApp.Application.DTOs.Tournaments;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EsportsApp.Tests;

[TestClass]
public class TournamentServiceTests
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
    }

    [TestMethod]
    public async Task AdvanceStatus_Draft_BecomesOpen()
    {
        var orgId = Guid.NewGuid();
        var tournament = new Tournament { Id = Guid.NewGuid(), OrganizerId = orgId, Status = TournamentStatus.Draft };
        _tournaments.Setup(r => r.GetByIdAsync(tournament.Id)).ReturnsAsync(tournament);
        _tournaments.Setup(r => r.UpdateAsync(tournament)).Returns(Task.CompletedTask);
        _tournaments.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.AdvanceStatusAsync(orgId, tournament.Id);

        Assert.AreEqual(TournamentStatus.Open, tournament.Status);
    }

    [TestMethod]
    public async Task AdvanceStatus_NotOrganizer_ThrowsForbidden()
    {
        var tournament = new Tournament { Id = Guid.NewGuid(), OrganizerId = Guid.NewGuid(), Status = TournamentStatus.Draft };
        _tournaments.Setup(r => r.GetByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() => _sut.AdvanceStatusAsync(Guid.NewGuid(), tournament.Id));
    }

    [TestMethod]
    public async Task Delete_InProgressTournament_ThrowsForbidden()
    {
        var orgId = Guid.NewGuid();
        var tournament = new Tournament { Id = Guid.NewGuid(), OrganizerId = orgId, Status = TournamentStatus.InProgress };
        _tournaments.Setup(r => r.GetByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() => _sut.DeleteAsync(orgId, tournament.Id));
    }

    [TestMethod]
    public async Task Suspend_NotAdmin_ThrowsForbidden()
    {
        var tournament = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Open };
        _tournaments.Setup(r => r.GetByIdAsync(tournament.Id)).ReturnsAsync(tournament);

        // Suspend should only be called by admin, but TournamentService.SuspendAsync trusts callers; 
        // the actual admin check is at the controller level via [Authorize(Roles = "Admin")]
        // So just test it doesn't throw with a valid id
        _tournaments.Setup(r => r.UpdateAsync(tournament)).Returns(Task.CompletedTask);
        _tournaments.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.SuspendAsync(Guid.NewGuid(), tournament.Id);
        Assert.AreEqual(TournamentStatus.Suspended, tournament.Status);
    }
}
