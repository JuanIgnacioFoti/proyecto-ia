using EsportsApp.Application.DTOs.Matches;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;
using Match = EsportsApp.Domain.Entities.Match;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// TR-004: Match record/correction and duplicate detection unit tests (US6, FR-020, FR-021).
/// </summary>
[TestClass]
public class MatchServiceTests
{
    private Mock<IMatchRepository> _matches = null!;
    private Mock<ITournamentRepository> _tournaments = null!;
    private Mock<IStandingRepository> _standings = null!;
    private Mock<ITournamentRegistrationRepository> _registrations = null!;
    private Mock<IMatchRealtimeNotifier> _notifier = null!;
    private MatchService _sut = null!;

    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly Guid TournamentId = Guid.NewGuid();
    private static readonly Guid HomeTeamId = Guid.NewGuid();
    private static readonly Guid AwayTeamId = Guid.NewGuid();

    private Tournament InProgressTournament() => new()
    {
        Id = TournamentId,
        OrganizerId = OrgId,
        Status = TournamentStatus.InProgress,
        ScoringSystem = new ScoringSystem { Type = ScoringSystemType.Standard, WinPoints = 3, DrawPoints = 1, LossPoints = 0 },
    };

    [TestInitialize]
    public void Setup()
    {
        _matches = new Mock<IMatchRepository>();
        _tournaments = new Mock<ITournamentRepository>();
        _standings = new Mock<IStandingRepository>();
        _registrations = new Mock<ITournamentRegistrationRepository>();
        _notifier = new Mock<IMatchRealtimeNotifier>();
        _sut = new MatchService(_matches.Object, _tournaments.Object, _standings.Object, _registrations.Object, _notifier.Object);

        _matches.Setup(r => r.AddAsync(It.IsAny<Match>())).Returns(Task.CompletedTask);
        _matches.Setup(r => r.UpdateAsync(It.IsAny<Match>())).Returns(Task.CompletedTask);
        _matches.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _standings.Setup(r => r.AddAsync(It.IsAny<Standing>())).Returns(Task.CompletedTask);
        _standings.Setup(r => r.UpdateAsync(It.IsAny<Standing>())).Returns(Task.CompletedTask);
        _standings.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _registrations.Setup(r => r.GetActiveByTournamentAsync(It.IsAny<Guid>())).ReturnsAsync(new List<TournamentRegistration>());
        _matches.Setup(r => r.GetByTournamentAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Match>());
        _notifier.Setup(n => n.NotifyStandingsUpdatedAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
    }

    private void SetupMatchReturnForGetById(Match match)
    {
        _matches.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);
    }

    // ── Record: happy path ────────────────────────────────────────────────────

    [TestMethod]
    public async Task Record_ValidMatch_ReturnsMatchDto()
    {
        var tournament = InProgressTournament();
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _matches.Setup(r => r.DuplicateExistsAsync(TournamentId, HomeTeamId, AwayTeamId, It.IsAny<DateTime>())).ReturnsAsync(false);

        Match? addedMatch = null;
        _matches.Setup(r => r.AddAsync(It.IsAny<Match>()))
                .Callback<Match>(m => addedMatch = m)
                .Returns(Task.CompletedTask);
        _matches.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => addedMatch!);

        var result = await _sut.RecordAsync(OrgId, TournamentId, new RecordMatchRequestDto
        {
            HomeTeamId = HomeTeamId,
            AwayTeamId = AwayTeamId,
            HomeScore = 2,
            AwayScore = 1,
            PlayedAt = DateTime.UtcNow.AddHours(-1),
        });

        Assert.IsNotNull(result);
        Assert.AreEqual(HomeTeamId, result.HomeTeamId);
    }

    [TestMethod]
    public async Task Record_ValidMatch_NotifiesSignalR()
    {
        var tournament = InProgressTournament();
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _matches.Setup(r => r.DuplicateExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>())).ReturnsAsync(false);

        Match? addedMatch = null;
        _matches.Setup(r => r.AddAsync(It.IsAny<Match>()))
                .Callback<Match>(m => addedMatch = m)
                .Returns(Task.CompletedTask);
        _matches.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => addedMatch!);

        await _sut.RecordAsync(OrgId, TournamentId, new RecordMatchRequestDto
        {
            HomeTeamId = HomeTeamId,
            AwayTeamId = AwayTeamId,
            HomeScore = 1,
            AwayScore = 0,
            PlayedAt = DateTime.UtcNow.AddHours(-1),
        });

        _notifier.Verify(n => n.NotifyStandingsUpdatedAsync(TournamentId), Times.Once);
    }

    // ── Record: ownership guard ───────────────────────────────────────────────

    [TestMethod]
    public async Task Record_WrongOrganizer_ThrowsForbidden()
    {
        var tournament = InProgressTournament();
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
            _sut.RecordAsync(Guid.NewGuid(), TournamentId, new RecordMatchRequestDto
            {
                HomeTeamId = HomeTeamId,
                AwayTeamId = AwayTeamId,
                HomeScore = 1,
                AwayScore = 0,
                PlayedAt = DateTime.UtcNow,
            }));
    }

    // ── Record: tournament not InProgress ─────────────────────────────────────

    [TestMethod]
    public async Task Record_OpenTournament_ThrowsUnprocessable()
    {
        var tournament = InProgressTournament();
        tournament.Status = TournamentStatus.Open;
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            _sut.RecordAsync(OrgId, TournamentId, new RecordMatchRequestDto
            {
                HomeTeamId = HomeTeamId,
                AwayTeamId = AwayTeamId,
                HomeScore = 1,
                AwayScore = 0,
                PlayedAt = DateTime.UtcNow,
            }));
    }

    // ── Record: same-team match ───────────────────────────────────────────────

    [TestMethod]
    public async Task Record_SameTeamBothSides_ThrowsValidation()
    {
        var tournament = InProgressTournament();
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);

        await Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _sut.RecordAsync(OrgId, TournamentId, new RecordMatchRequestDto
            {
                HomeTeamId = HomeTeamId,
                AwayTeamId = HomeTeamId, // same
                HomeScore = 1,
                AwayScore = 0,
                PlayedAt = DateTime.UtcNow,
            }));
    }

    // ── Record: duplicate match detection (FR-020, FR-021) ───────────────────

    [TestMethod]
    public async Task Record_DuplicateMatch_ThrowsConflict()
    {
        var tournament = InProgressTournament();
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _matches.Setup(r => r.DuplicateExistsAsync(TournamentId, HomeTeamId, AwayTeamId, It.IsAny<DateTime>())).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.RecordAsync(OrgId, TournamentId, new RecordMatchRequestDto
            {
                HomeTeamId = HomeTeamId,
                AwayTeamId = AwayTeamId,
                HomeScore = 2,
                AwayScore = 0,
                PlayedAt = DateTime.UtcNow.AddHours(-1),
            }));
    }

    // ── Correct: happy path ───────────────────────────────────────────────────

    [TestMethod]
    public async Task Correct_ValidRequest_UpdatesMatchAndNotifies()
    {
        var matchId = Guid.NewGuid();
        var match = new Match
        {
            Id = matchId,
            TournamentId = TournamentId,
            HomeTeamId = HomeTeamId,
            AwayTeamId = AwayTeamId,
            HomeScore = 1,
            AwayScore = 0,
        };
        var tournament = InProgressTournament();
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _matches.Setup(r => r.GetByIdAsync(matchId)).ReturnsAsync(match);
        _matches.Setup(r => r.GetByTournamentAsync(TournamentId)).ReturnsAsync(new List<Match> { match });

        await _sut.CorrectAsync(OrgId, TournamentId, matchId, new RecordMatchRequestDto
        {
            HomeTeamId = HomeTeamId,
            AwayTeamId = AwayTeamId,
            HomeScore = 2,
            AwayScore = 1,
            PlayedAt = DateTime.UtcNow.AddHours(-2),
        });

        Assert.AreEqual(2, match.HomeScore);
        Assert.AreEqual(1, match.AwayScore);
        _notifier.Verify(n => n.NotifyStandingsUpdatedAsync(TournamentId), Times.Once);
    }

    // ── Correct: ownership guard ──────────────────────────────────────────────

    [TestMethod]
    public async Task Correct_WrongOrganizer_ThrowsForbidden()
    {
        var tournament = InProgressTournament();
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
            _sut.CorrectAsync(Guid.NewGuid(), TournamentId, Guid.NewGuid(), new RecordMatchRequestDto
            {
                HomeTeamId = HomeTeamId,
                AwayTeamId = AwayTeamId,
                HomeScore = 0,
                AwayScore = 0,
                PlayedAt = DateTime.UtcNow,
            }));
    }

    // ── Correct: match not in this tournament ─────────────────────────────────

    [TestMethod]
    public async Task Correct_MatchBelongsToOtherTournament_ThrowsNotFound()
    {
        var matchId = Guid.NewGuid();
        var match = new Match { Id = matchId, TournamentId = Guid.NewGuid() }; // different tournament
        var tournament = InProgressTournament();
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _matches.Setup(r => r.GetByIdAsync(matchId)).ReturnsAsync(match);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            _sut.CorrectAsync(OrgId, TournamentId, matchId, new RecordMatchRequestDto
            {
                HomeTeamId = HomeTeamId,
                AwayTeamId = AwayTeamId,
                HomeScore = 1,
                AwayScore = 1,
                PlayedAt = DateTime.UtcNow,
            }));
    }

    // ── Correct: duplicate detection excludes the match being corrected ───────

    [TestMethod]
    public async Task Correct_DuplicateExcludesCurrentMatch_DoesNotThrow()
    {
        var matchId = Guid.NewGuid();
        var playedAt = DateTime.UtcNow.AddHours(-1);
        var match = new Match
        {
            Id = matchId,
            TournamentId = TournamentId,
            HomeTeamId = HomeTeamId,
            AwayTeamId = AwayTeamId,
            HomeScore = 1,
            AwayScore = 0,
            PlayedAt = playedAt,
        };
        var tournament = InProgressTournament();
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _matches.Setup(r => r.GetByIdAsync(matchId)).ReturnsAsync(match);
        // Only the same match is in tournament — no duplicate other than self
        _matches.Setup(r => r.GetByTournamentAsync(TournamentId)).ReturnsAsync(new List<Match> { match });

        // Should not throw despite same teams/time being sent
        await _sut.CorrectAsync(OrgId, TournamentId, matchId, new RecordMatchRequestDto
        {
            HomeTeamId = HomeTeamId,
            AwayTeamId = AwayTeamId,
            HomeScore = 2,
            AwayScore = 0,
            PlayedAt = playedAt,
        });
    }
}
