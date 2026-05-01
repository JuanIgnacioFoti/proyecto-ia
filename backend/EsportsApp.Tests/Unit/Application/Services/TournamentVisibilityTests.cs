using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// T091: Suspended tournament public-listing visibility tests (FR-012, US5).
/// Verifies:
/// - Suspended tournaments appear in GET /tournaments with status=Suspended
/// - Suspended status prevents organizer from advancing the lifecycle
/// - Suspended status prevents new team registrations
/// </summary>
[TestClass]
public class TournamentVisibilityTests
{
    private Mock<ITournamentRepository> _tournaments = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<ITournamentRegistrationRepository> _registrations = null!;
    private TournamentService _tournamentSvc = null!;

    private static readonly Guid OrgId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _tournaments = new Mock<ITournamentRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _registrations = new Mock<ITournamentRegistrationRepository>();
        _tournamentSvc = new TournamentService(_tournaments.Object, _videogames.Object, _registrations.Object);

        _registrations.Setup(r => r.GetActiveCountByTournamentAsync(It.IsAny<Guid>())).ReturnsAsync(0);
        _tournaments.Setup(r => r.UpdateAsync(It.IsAny<Tournament>())).Returns(Task.CompletedTask);
        _tournaments.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
    }

    // ── Public listing includes Suspended with correct status badge ───────────

    [TestMethod]
    public async Task GetPublicList_SuspendedTournament_IncludedWithSuspendedStatus()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Suspended, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetPublicListAsync()).ReturnsAsync(new List<Tournament> { t });

        var result = await _tournamentSvc.GetPublicListAsync();

        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Suspended", result.First().Status);
    }

    [TestMethod]
    public async Task GetById_SuspendedTournament_ReturnsSuspendedStatus()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Suspended, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        var result = await _tournamentSvc.GetByIdAsync(t.Id, null);

        Assert.AreEqual("Suspended", result.Status);
    }

    // ── Organizer cannot advance a Suspended tournament ───────────────────────

    [TestMethod]
    public async Task AdvanceStatus_SuspendedTournament_ThrowsUnprocessable()
    {
        var t = new Tournament { Id = Guid.NewGuid(), OrganizerId = OrgId, Status = TournamentStatus.Suspended };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            _tournamentSvc.AdvanceStatusAsync(OrgId, t.Id));
    }

    // ── Registration service cannot register in a Suspended tournament ─────────

    [TestMethod]
    public async Task Registration_SuspendedTournament_ThrowsUnprocessable()
    {
        // Arrange a TournamentRegistrationService with the suspended tournament
        var teamsRepo = new Mock<ITeamRepository>();
        var playersRepo = new Mock<IPlayerRepository>();
        var regsRepo = new Mock<ITournamentRegistrationRepository>();
        var standingsRepo = new Mock<IStandingRepository>();

        var regSvc = new TournamentRegistrationService(
            _tournaments.Object, teamsRepo.Object, playersRepo.Object,
            regsRepo.Object, standingsRepo.Object);

        var tournamentId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var captainId = Guid.NewGuid();

        var t = new Tournament { Id = tournamentId, Status = TournamentStatus.Suspended, MaxTeams = 8, MinMembersPerTeam = 2 };
        _tournaments.Setup(r => r.GetByIdAsync(tournamentId)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            regSvc.RegisterAsync(captainId, tournamentId,
                new EsportsApp.Application.DTOs.Registrations.RegisterTeamRequestDto { TeamId = teamId }));
    }
}
