using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// Public tournament listing filter tests (US5, FR-012, FR-015).
/// Verifies: Open/InProgress/Completed appear publicly, Draft does NOT appear
/// to non-owning users, Suspended appears with Suspended status (FR-012).
/// </summary>
[TestClass]
public class TournamentQueryServiceTests
{
    private Mock<ITournamentRepository> _tournaments = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<ITournamentRegistrationRepository> _registrations = null!;
    private TournamentService _sut = null!;

    private static readonly Guid OrgId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _tournaments = new Mock<ITournamentRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _registrations = new Mock<ITournamentRegistrationRepository>();
        _sut = new TournamentService(_tournaments.Object, _videogames.Object, _registrations.Object);

        _registrations.Setup(r => r.GetActiveCountByTournamentAsync(It.IsAny<Guid>())).ReturnsAsync(0);
    }

    // ── GetPublicList: repository returns pre-filtered results ────────────────

    [TestMethod]
    public async Task GetPublicList_IncludesOpenTournament()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Open, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetPublicListAsync()).ReturnsAsync(new List<Tournament> { t });

        var result = await _sut.GetPublicListAsync();

        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Open", result.First().Status);
    }

    [TestMethod]
    public async Task GetPublicList_IncludesInProgressTournament()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.InProgress, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetPublicListAsync()).ReturnsAsync(new List<Tournament> { t });

        var result = await _sut.GetPublicListAsync();

        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("InProgress", result.First().Status);
    }

    [TestMethod]
    public async Task GetPublicList_IncludesCompletedTournament()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Completed, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetPublicListAsync()).ReturnsAsync(new List<Tournament> { t });

        var result = await _sut.GetPublicListAsync();

        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Completed", result.First().Status);
    }

    [TestMethod]
    public async Task GetPublicList_IncludesSuspendedTournamentWithSuspendedStatus_FR012()
    {
        // FR-012: Suspended tournaments must appear in public listing with Suspended status
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Suspended, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetPublicListAsync()).ReturnsAsync(new List<Tournament> { t });

        var result = await _sut.GetPublicListAsync();

        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Suspended", result.First().Status);
    }

    [TestMethod]
    public async Task GetPublicList_ExcludesDraftTournament_RepositoryFilters()
    {
        // The repository GetPublicListAsync is responsible for excluding Draft tournaments.
        // When it returns an empty list, the service propagates an empty response.
        _tournaments.Setup(r => r.GetPublicListAsync()).ReturnsAsync(new List<Tournament>());

        var result = await _sut.GetPublicListAsync();

        Assert.AreEqual(0, result.Count());
    }

    // ── GetById: Draft only visible to owning organizer (FR-015) ─────────────

    [TestMethod]
    public async Task GetById_DraftTournament_VisibleToOwningOrganizer()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Draft, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);
        _registrations.Setup(r => r.GetActiveCountByTournamentAsync(t.Id)).ReturnsAsync(0);

        var result = await _sut.GetByIdAsync(t.Id, OrgId);

        Assert.IsNotNull(result);
        Assert.AreEqual("Draft", result.Status);
    }

    [TestMethod]
    public async Task GetById_DraftTournament_NotVisibleToOtherUser_FR015()
    {
        // FR-015: Draft tournaments must not appear to any user other than the owning organizer
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Draft, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        // Different user (including anonymous null)
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _sut.GetByIdAsync(t.Id, Guid.NewGuid()));
    }

    [TestMethod]
    public async Task GetById_DraftTournament_NotVisibleAnonymously_FR015()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Draft, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _sut.GetByIdAsync(t.Id, null));
    }

    [TestMethod]
    public async Task GetById_OpenTournament_VisibleToAnyone()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Open, OrganizerId = OrgId };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);
        _registrations.Setup(r => r.GetActiveCountByTournamentAsync(t.Id)).ReturnsAsync(0);

        var result = await _sut.GetByIdAsync(t.Id, null);

        Assert.IsNotNull(result);
    }
}
