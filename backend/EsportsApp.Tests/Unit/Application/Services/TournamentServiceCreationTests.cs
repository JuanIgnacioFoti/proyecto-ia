using EsportsApp.Application.DTOs.Tournaments;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// TR-002: Tournament creation validation unit tests.
/// </summary>
[TestClass]
public class TournamentServiceCreationTests
{
    private Mock<ITournamentRepository> _tournaments = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<ITournamentRegistrationRepository> _registrations = null!;
    private TournamentService _sut = null!;

    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly Guid GameId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _tournaments = new Mock<ITournamentRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _registrations = new Mock<ITournamentRegistrationRepository>();
        _sut = new TournamentService(_tournaments.Object, _videogames.Object, _registrations.Object);

        _videogames.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        _tournaments.Setup(r => r.AddAsync(It.IsAny<Tournament>())).Returns(Task.CompletedTask);
        _tournaments.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _registrations.Setup(r => r.GetActiveCountByTournamentAsync(It.IsAny<Guid>())).ReturnsAsync(0);
    }

    private static CreateTournamentRequestDto ValidRequest() => new()
    {
        Name = "Summer Cup",
        VideogameId = GameId,
        StartDate = DateTime.UtcNow.AddDays(7),
        EstimatedEndDate = DateTime.UtcNow.AddDays(14),
        MaxTeams = 8,
        MinMembersPerTeam = 5,
        ScoringSystem = new ScoringSystemRequestDto { Type = "Standard" },
    };

    // ── Happy path ───────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Create_ValidRequest_StartsAsDraft()
    {
        Tournament? saved = null;
        _tournaments.Setup(r => r.AddAsync(It.IsAny<Tournament>()))
                    .Callback<Tournament>(t => saved = t)
                    .Returns(Task.CompletedTask);
        _tournaments.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Guid id) => saved!);

        await _sut.CreateAsync(OrgId, ValidRequest());

        Assert.IsNotNull(saved);
        Assert.AreEqual(TournamentStatus.Draft, saved!.Status);
    }

    [TestMethod]
    public async Task Create_ValidRequest_AssignsOrganizerId()
    {
        Tournament? saved = null;
        _tournaments.Setup(r => r.AddAsync(It.IsAny<Tournament>()))
                    .Callback<Tournament>(t => saved = t)
                    .Returns(Task.CompletedTask);
        _tournaments.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Guid id) => saved!);

        await _sut.CreateAsync(OrgId, ValidRequest());

        Assert.AreEqual(OrgId, saved!.OrganizerId);
    }

    // ── Videogame not found ──────────────────────────────────────────────────

    [TestMethod]
    public async Task Create_InvalidVideogame_ThrowsValidation()
    {
        _videogames.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.CreateAsync(OrgId, ValidRequest()));
    }

    // ── Start date in the past ────────────────────────────────────────────────

    [TestMethod]
    public async Task Create_StartDateInPast_ThrowsValidation()
    {
        var req = ValidRequest();
        req.StartDate = DateTime.UtcNow.AddDays(-1);
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.CreateAsync(OrgId, req));
    }

    [TestMethod]
    public async Task Create_StartDateIsNow_ThrowsValidation()
    {
        var req = ValidRequest();
        req.StartDate = DateTime.UtcNow; // at exact boundary — should still fail
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.CreateAsync(OrgId, req));
    }

    // ── End date before start date ────────────────────────────────────────────

    [TestMethod]
    public async Task Create_EndDateBeforeStartDate_ThrowsValidation()
    {
        var req = ValidRequest();
        req.EstimatedEndDate = req.StartDate.AddDays(-1);
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.CreateAsync(OrgId, req));
    }

    [TestMethod]
    public async Task Create_EndDateEqualToStartDate_ThrowsValidation()
    {
        var req = ValidRequest();
        req.EstimatedEndDate = req.StartDate;
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.CreateAsync(OrgId, req));
    }

    // ── Invalid scoring type ──────────────────────────────────────────────────

    [TestMethod]
    public async Task Create_InvalidScoringType_ThrowsValidation()
    {
        var req = ValidRequest();
        req.ScoringSystem = new ScoringSystemRequestDto { Type = "Unknown" };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.CreateAsync(OrgId, req));
    }
}
