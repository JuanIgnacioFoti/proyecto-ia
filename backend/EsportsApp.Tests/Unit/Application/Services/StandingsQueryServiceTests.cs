using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// Standings visibility and status-gating unit tests (US5, FR-030).
/// Standings are only accessible for InProgress or Completed tournaments.
/// </summary>
[TestClass]
public class StandingsQueryServiceTests
{
    private Mock<IStandingRepository> _standings = null!;
    private Mock<ITournamentRepository> _tournaments = null!;
    private StandingsService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _standings = new Mock<IStandingRepository>();
        _tournaments = new Mock<ITournamentRepository>();
        _sut = new StandingsService(_standings.Object, _tournaments.Object);
    }

    private static Standing MakeStanding(Guid tournamentId, string teamName, int points, int played, int wins, int draws, int losses) =>
        new()
        {
            TournamentId = tournamentId,
            TeamId = Guid.NewGuid(),
            Team = new Team { Name = teamName },
            Points = points,
            MatchesPlayed = played,
            Wins = wins,
            Draws = draws,
            Losses = losses,
        };

    // ── Availability gates ────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetStandings_DraftTournament_ThrowsConflict()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Draft };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.GetByTournamentAsync(t.Id));
    }

    [TestMethod]
    public async Task GetStandings_OpenTournament_ThrowsConflict()
    {
        var t = new Tournament { Id = Guid.NewGuid(), Status = TournamentStatus.Open };
        _tournaments.Setup(r => r.GetByIdAsync(t.Id)).ReturnsAsync(t);

        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.GetByTournamentAsync(t.Id));
    }

    [TestMethod]
    public async Task GetStandings_InProgressTournament_ReturnsStandings()
    {
        var tid = Guid.NewGuid();
        var t = new Tournament { Id = tid, Status = TournamentStatus.InProgress };
        _tournaments.Setup(r => r.GetByIdAsync(tid)).ReturnsAsync(t);
        _standings.Setup(r => r.GetByTournamentAsync(tid)).ReturnsAsync(new List<Standing>
        {
            MakeStanding(tid, "TeamA", 9, 3, 3, 0, 0),
        });

        var result = await _sut.GetByTournamentAsync(tid);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
    }

    [TestMethod]
    public async Task GetStandings_CompletedTournament_ReturnsStandings()
    {
        var tid = Guid.NewGuid();
        var t = new Tournament { Id = tid, Status = TournamentStatus.Completed };
        _tournaments.Setup(r => r.GetByIdAsync(tid)).ReturnsAsync(t);
        _standings.Setup(r => r.GetByTournamentAsync(tid)).ReturnsAsync(new List<Standing>());

        var result = await _sut.GetByTournamentAsync(tid);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetStandings_TournamentNotFound_ThrowsNotFound()
    {
        _tournaments.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Tournament?)null);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _sut.GetByTournamentAsync(Guid.NewGuid()));
    }

    // ── Standard competition ranking (1224 algorithm, FR-031) ─────────────────

    [TestMethod]
    public async Task GetStandings_Ranking_UsesStandardCompetitionRanking()
    {
        // Three teams: A=9pts, B=6pts, C=6pts
        // Expected ranks: A=1, B=2, C=2 (tied, next rank would be 4 not 3)
        var tid = Guid.NewGuid();
        var t = new Tournament { Id = tid, Status = TournamentStatus.InProgress };
        _tournaments.Setup(r => r.GetByIdAsync(tid)).ReturnsAsync(t);
        _standings.Setup(r => r.GetByTournamentAsync(tid)).ReturnsAsync(new List<Standing>
        {
            MakeStanding(tid, "TeamA", 9, 3, 3, 0, 0),
            MakeStanding(tid, "TeamB", 6, 3, 2, 0, 1),
            MakeStanding(tid, "TeamC", 6, 3, 2, 0, 1),
        });

        var result = (await _sut.GetByTournamentAsync(tid)).ToList();

        Assert.AreEqual(1, result[0].Rank); // TeamA
        Assert.AreEqual(2, result[1].Rank); // TeamB (tied)
        Assert.AreEqual(2, result[2].Rank); // TeamC (tied)
    }

    [TestMethod]
    public async Task GetStandings_Ranking_SortsByPointsDescThenNameAsc()
    {
        var tid = Guid.NewGuid();
        var t = new Tournament { Id = tid, Status = TournamentStatus.InProgress };
        _tournaments.Setup(r => r.GetByIdAsync(tid)).ReturnsAsync(t);
        _standings.Setup(r => r.GetByTournamentAsync(tid)).ReturnsAsync(new List<Standing>
        {
            MakeStanding(tid, "Zebra", 6, 2, 2, 0, 0),
            MakeStanding(tid, "Alpha", 6, 2, 2, 0, 0), // same points — Alpha should be first alphabetically
        });

        var result = (await _sut.GetByTournamentAsync(tid)).ToList();

        Assert.AreEqual("Alpha", result[0].TeamName);
        Assert.AreEqual("Zebra", result[1].TeamName);
        Assert.AreEqual(1, result[0].Rank); // both share rank 1 (same points)
        Assert.AreEqual(1, result[1].Rank);
    }
}
