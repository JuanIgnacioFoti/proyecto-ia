using EsportsApp.Application.DTOs.Registrations;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// TR-003: Tournament registration eligibility unit tests (US4, FR-023).
/// </summary>
[TestClass]
public class TournamentRegistrationServiceTests
{
    private Mock<ITournamentRepository> _tournaments = null!;
    private Mock<ITeamRepository> _teams = null!;
    private Mock<IPlayerRepository> _players = null!;
    private Mock<ITournamentRegistrationRepository> _registrations = null!;
    private Mock<IStandingRepository> _standings = null!;
    private TournamentRegistrationService _sut = null!;

    private static readonly Guid GameId = Guid.NewGuid();
    private static readonly Guid CaptainId = Guid.NewGuid();
    private static readonly Guid TournamentId = Guid.NewGuid();
    private static readonly Guid TeamId = Guid.NewGuid();

    private Tournament OpenTournament() => new()
    {
        Id = TournamentId,
        VideogameId = GameId,
        Status = TournamentStatus.Open,
        MaxTeams = 8,
        MinMembersPerTeam = 2,
    };

    private Team TeamWithMembers(int count) => new()
    {
        Id = TeamId,
        CaptainId = CaptainId,
        VideogameId = GameId,
        Members = Enumerable.Range(0, count)
            .Select(_ => new TeamMember { PlayerId = Guid.NewGuid() })
            .ToList(),
    };

    [TestInitialize]
    public void Setup()
    {
        _tournaments = new Mock<ITournamentRepository>();
        _teams = new Mock<ITeamRepository>();
        _players = new Mock<IPlayerRepository>();
        _registrations = new Mock<ITournamentRegistrationRepository>();
        _standings = new Mock<IStandingRepository>();
        _sut = new TournamentRegistrationService(
            _tournaments.Object, _teams.Object, _players.Object,
            _registrations.Object, _standings.Object);

        _registrations.Setup(r => r.AddAsync(It.IsAny<TournamentRegistration>())).Returns(Task.CompletedTask);
        _standings.Setup(r => r.AddAsync(It.IsAny<Standing>())).Returns(Task.CompletedTask);
        _registrations.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
    }

    // ── Happy path ───────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Register_EligibleTeam_ReturnsActiveRegistration()
    {
        var tournament = OpenTournament();
        var team = TeamWithMembers(3);

        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _teams.Setup(r => r.GetByIdWithMembersAsync(TeamId)).ReturnsAsync(team);
        _registrations.Setup(r => r.GetByTournamentAndTeamAsync(TournamentId, TeamId)).ReturnsAsync((TournamentRegistration?)null);
        _registrations.Setup(r => r.GetActiveCountByTournamentAsync(TournamentId)).ReturnsAsync(3);
        _standings.Setup(r => r.GetByTournamentAndTeamAsync(TournamentId, TeamId)).ReturnsAsync((Standing?)null);

        var result = await _sut.RegisterAsync(CaptainId, TournamentId, new RegisterTeamRequestDto { TeamId = TeamId });

        Assert.AreEqual(RegistrationStatus.Active.ToString(), result.Status);
    }

    // ── Tournament not open ───────────────────────────────────────────────────

    [TestMethod]
    public async Task Register_DraftTournament_ThrowsUnprocessable()
    {
        var tournament = OpenTournament();
        tournament.Status = TournamentStatus.Draft;
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            _sut.RegisterAsync(CaptainId, TournamentId, new RegisterTeamRequestDto { TeamId = TeamId }));
    }

    [TestMethod]
    public async Task Register_InProgressTournament_ThrowsUnprocessable()
    {
        var tournament = OpenTournament();
        tournament.Status = TournamentStatus.InProgress;
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            _sut.RegisterAsync(CaptainId, TournamentId, new RegisterTeamRequestDto { TeamId = TeamId }));
    }

    // ── Non-captain cannot register ───────────────────────────────────────────

    [TestMethod]
    public async Task Register_NonCaptain_ThrowsForbidden()
    {
        var tournament = OpenTournament();
        var team = TeamWithMembers(3);
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _teams.Setup(r => r.GetByIdWithMembersAsync(TeamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
            _sut.RegisterAsync(Guid.NewGuid(), TournamentId, new RegisterTeamRequestDto { TeamId = TeamId }));
    }

    // ── Game mismatch ─────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Register_GameMismatch_ThrowsValidation()
    {
        var tournament = OpenTournament();
        var team = TeamWithMembers(3);
        team.VideogameId = Guid.NewGuid(); // different game

        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _teams.Setup(r => r.GetByIdWithMembersAsync(TeamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _sut.RegisterAsync(CaptainId, TournamentId, new RegisterTeamRequestDto { TeamId = TeamId }));
    }

    // ── Insufficient members ──────────────────────────────────────────────────

    [TestMethod]
    public async Task Register_InsufficientMembers_ThrowsValidation()
    {
        var tournament = OpenTournament();
        tournament.MinMembersPerTeam = 5;
        var team = TeamWithMembers(3); // only 3 members, need 5

        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _teams.Setup(r => r.GetByIdWithMembersAsync(TeamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _sut.RegisterAsync(CaptainId, TournamentId, new RegisterTeamRequestDto { TeamId = TeamId }));
    }

    // ── Duplicate active registration (FR-023) ────────────────────────────────

    [TestMethod]
    public async Task Register_AlreadyRegistered_ThrowsConflict()
    {
        var tournament = OpenTournament();
        var team = TeamWithMembers(3);
        var existing = new TournamentRegistration
        {
            TournamentId = TournamentId,
            TeamId = TeamId,
            Status = RegistrationStatus.Active,
        };

        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _teams.Setup(r => r.GetByIdWithMembersAsync(TeamId)).ReturnsAsync(team);
        _registrations.Setup(r => r.GetByTournamentAndTeamAsync(TournamentId, TeamId)).ReturnsAsync(existing);

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.RegisterAsync(CaptainId, TournamentId, new RegisterTeamRequestDto { TeamId = TeamId }));
    }

    // ── Tournament at capacity ────────────────────────────────────────────────

    [TestMethod]
    public async Task Register_AtCapacity_ThrowsUnprocessable()
    {
        var tournament = OpenTournament();
        tournament.MaxTeams = 2;
        var team = TeamWithMembers(3);

        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _teams.Setup(r => r.GetByIdWithMembersAsync(TeamId)).ReturnsAsync(team);
        _registrations.Setup(r => r.GetByTournamentAndTeamAsync(TournamentId, TeamId)).ReturnsAsync((TournamentRegistration?)null);
        _registrations.Setup(r => r.GetActiveCountByTournamentAsync(TournamentId)).ReturnsAsync(2); // full

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            _sut.RegisterAsync(CaptainId, TournamentId, new RegisterTeamRequestDto { TeamId = TeamId }));
    }

    // ── Withdrawal ────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Withdraw_ActiveRegistration_SetsWithdrawn()
    {
        var team = new Team { Id = TeamId, CaptainId = CaptainId };
        var reg = new TournamentRegistration { TournamentId = TournamentId, TeamId = TeamId, Status = RegistrationStatus.Active };
        var tournament = OpenTournament();
        var standing = new Standing { TournamentId = TournamentId, TeamId = TeamId };

        _teams.Setup(r => r.GetByIdAsync(TeamId)).ReturnsAsync(team);
        _registrations.Setup(r => r.GetByTournamentAndTeamAsync(TournamentId, TeamId)).ReturnsAsync(reg);
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);
        _standings.Setup(r => r.GetByTournamentAndTeamAsync(TournamentId, TeamId)).ReturnsAsync(standing);
        _standings.Setup(r => r.DeleteAsync(standing)).Returns(Task.CompletedTask);
        _registrations.Setup(r => r.UpdateAsync(reg)).Returns(Task.CompletedTask);
        _registrations.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.WithdrawAsync(CaptainId, TournamentId, TeamId);

        Assert.AreEqual(RegistrationStatus.Withdrawn, reg.Status);
    }

    [TestMethod]
    public async Task Withdraw_InProgressTournament_ThrowsUnprocessable()
    {
        var team = new Team { Id = TeamId, CaptainId = CaptainId };
        var reg = new TournamentRegistration { TournamentId = TournamentId, TeamId = TeamId, Status = RegistrationStatus.Active };
        var tournament = OpenTournament();
        tournament.Status = TournamentStatus.InProgress;

        _teams.Setup(r => r.GetByIdAsync(TeamId)).ReturnsAsync(team);
        _registrations.Setup(r => r.GetByTournamentAndTeamAsync(TournamentId, TeamId)).ReturnsAsync(reg);
        _tournaments.Setup(r => r.GetByIdAsync(TournamentId)).ReturnsAsync(tournament);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            _sut.WithdrawAsync(CaptainId, TournamentId, TeamId));
    }

    [TestMethod]
    public async Task Withdraw_NonCaptain_ThrowsForbidden()
    {
        var team = new Team { Id = TeamId, CaptainId = CaptainId };
        _teams.Setup(r => r.GetByIdAsync(TeamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
            _sut.WithdrawAsync(Guid.NewGuid(), TournamentId, TeamId));
    }
}
