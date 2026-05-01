using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// Team invitation accept/decline and expiry unit tests (US3).
/// </summary>
[TestClass]
public class TeamServiceInvitationTests
{
    private Mock<ITeamRepository> _teams = null!;
    private Mock<IPlayerRepository> _players = null!;
    private Mock<IUserRepository> _users = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<ITeamInvitationRepository> _invitations = null!;
    private TeamService _sut = null!;

    private static readonly Guid GameId = Guid.NewGuid();
    private static readonly Guid CaptainId = Guid.NewGuid();
    private static readonly Guid InvitedId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _teams = new Mock<ITeamRepository>();
        _players = new Mock<IPlayerRepository>();
        _users = new Mock<IUserRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _invitations = new Mock<ITeamInvitationRepository>();
        _sut = new TeamService(_teams.Object, _players.Object, _users.Object, _videogames.Object, _invitations.Object);
    }

    // ── Invite: happy path ────────────────────────────────────────────────────

    [TestMethod]
    public async Task Invite_ValidRequest_AddsInvitation()
    {
        var teamId = Guid.NewGuid();
        var team = new Team
        {
            Id = teamId,
            CaptainId = CaptainId,
            VideogameId = GameId,
            Members = new List<TeamMember> { new() { PlayerId = CaptainId } },
        };
        var invited = new Player { Id = InvitedId };

        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);
        _players.Setup(r => r.GetByUsernameAsync("invitedUser")).ReturnsAsync(invited);
        _teams.Setup(r => r.GetByPlayerAsync(InvitedId)).ReturnsAsync(new List<Team>());
        _invitations.Setup(r => r.PendingInvitationExistsAsync(teamId, InvitedId)).ReturnsAsync(false);
        _invitations.Setup(r => r.AddAsync(It.IsAny<TeamInvitation>())).Returns(Task.CompletedTask);
        _invitations.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.InvitePlayerAsync(CaptainId, teamId, new EsportsApp.Application.DTOs.Teams.InvitePlayerRequestDto
        {
            Username = "invitedUser",
        });

        _invitations.Verify(r => r.AddAsync(It.Is<TeamInvitation>(i => i.InvitedPlayerId == InvitedId && i.TeamId == teamId)), Times.Once);
    }

    // ── Invite: non-captain cannot invite ─────────────────────────────────────

    [TestMethod]
    public async Task Invite_NonCaptain_ThrowsForbidden()
    {
        var teamId = Guid.NewGuid();
        var team = new Team { Id = teamId, CaptainId = CaptainId, VideogameId = GameId, Members = new List<TeamMember>() };
        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
            _sut.InvitePlayerAsync(Guid.NewGuid(), teamId, new EsportsApp.Application.DTOs.Teams.InvitePlayerRequestDto { Username = "anyone" }));
    }

    // ── Invite: already a member ──────────────────────────────────────────────

    [TestMethod]
    public async Task Invite_AlreadyMember_ThrowsConflict()
    {
        var teamId = Guid.NewGuid();
        var team = new Team
        {
            Id = teamId,
            CaptainId = CaptainId,
            VideogameId = GameId,
            Members = new List<TeamMember>
            {
                new() { PlayerId = CaptainId },
                new() { PlayerId = InvitedId }, // already member
            },
        };
        var invited = new Player { Id = InvitedId };

        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);
        _players.Setup(r => r.GetByUsernameAsync("invitedUser")).ReturnsAsync(invited);

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.InvitePlayerAsync(CaptainId, teamId, new EsportsApp.Application.DTOs.Teams.InvitePlayerRequestDto { Username = "invitedUser" }));
    }

    // ── Invite: player already in a team for the same game (FR-019) ───────────

    [TestMethod]
    public async Task Invite_PlayerAlreadyInTeamForGame_ThrowsConflict()
    {
        var teamId = Guid.NewGuid();
        var team = new Team
        {
            Id = teamId,
            CaptainId = CaptainId,
            VideogameId = GameId,
            Members = new List<TeamMember> { new() { PlayerId = CaptainId } },
        };
        var invited = new Player { Id = InvitedId };
        var conflictTeam = new Team { CaptainId = Guid.NewGuid(), VideogameId = GameId }; // same game

        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);
        _players.Setup(r => r.GetByUsernameAsync("invitedUser")).ReturnsAsync(invited);
        _teams.Setup(r => r.GetByPlayerAsync(InvitedId)).ReturnsAsync(new List<Team> { conflictTeam });

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.InvitePlayerAsync(CaptainId, teamId, new EsportsApp.Application.DTOs.Teams.InvitePlayerRequestDto { Username = "invitedUser" }));
    }

    // ── Invite: duplicate pending invitation ──────────────────────────────────

    [TestMethod]
    public async Task Invite_DuplicatePendingInvitation_ThrowsConflict()
    {
        var teamId = Guid.NewGuid();
        var team = new Team
        {
            Id = teamId,
            CaptainId = CaptainId,
            VideogameId = GameId,
            Members = new List<TeamMember> { new() { PlayerId = CaptainId } },
        };
        var invited = new Player { Id = InvitedId };

        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);
        _players.Setup(r => r.GetByUsernameAsync("invitedUser")).ReturnsAsync(invited);
        _teams.Setup(r => r.GetByPlayerAsync(InvitedId)).ReturnsAsync(new List<Team>());
        _invitations.Setup(r => r.PendingInvitationExistsAsync(teamId, InvitedId)).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.InvitePlayerAsync(CaptainId, teamId, new EsportsApp.Application.DTOs.Teams.InvitePlayerRequestDto { Username = "invitedUser" }));
    }

    // ── Invite: player not found by username ──────────────────────────────────

    [TestMethod]
    public async Task Invite_PlayerNotFound_ThrowsNotFound()
    {
        var teamId = Guid.NewGuid();
        var team = new Team { Id = teamId, CaptainId = CaptainId, VideogameId = GameId, Members = new List<TeamMember> { new() { PlayerId = CaptainId } } };

        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);
        _players.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((Player?)null);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            _sut.InvitePlayerAsync(CaptainId, teamId, new EsportsApp.Application.DTOs.Teams.InvitePlayerRequestDto { Username = "ghost" }));
    }

    // ── Respond: accept happy path ────────────────────────────────────────────

    [TestMethod]
    public async Task Respond_Accept_AddsMemberAndSetsAccepted()
    {
        var invitationId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var invitation = new TeamInvitation
        {
            Id = invitationId,
            TeamId = teamId,
            InvitedPlayerId = InvitedId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
        var team = new Team { Id = teamId, VideogameId = GameId };

        _invitations.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);
        _teams.Setup(r => r.GetByPlayerAsync(InvitedId)).ReturnsAsync(new List<Team>());
        _teams.Setup(r => r.GetByIdAsync(teamId)).ReturnsAsync(team);
        _teams.Setup(r => r.AddMemberAsync(It.IsAny<TeamMember>())).Returns(Task.CompletedTask);
        _invitations.Setup(r => r.GetCurrentStatusAsync(invitationId)).ReturnsAsync(InvitationStatus.Pending);
        _invitations.Setup(r => r.UpdateAsync(invitation)).Returns(Task.CompletedTask);
        _invitations.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.RespondToInvitationAsync(InvitedId, invitationId, accept: true);

        Assert.AreEqual(InvitationStatus.Accepted, invitation.Status);
        _teams.Verify(r => r.AddMemberAsync(It.Is<TeamMember>(m => m.PlayerId == InvitedId)), Times.Once);
    }

    // ── Respond: decline ──────────────────────────────────────────────────────

    [TestMethod]
    public async Task Respond_Decline_SetsDeclined()
    {
        var invitationId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var invitation = new TeamInvitation
        {
            Id = invitationId,
            TeamId = teamId,
            InvitedPlayerId = InvitedId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _invitations.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);
        _invitations.Setup(r => r.GetCurrentStatusAsync(invitationId)).ReturnsAsync(InvitationStatus.Pending);
        _invitations.Setup(r => r.UpdateAsync(invitation)).Returns(Task.CompletedTask);
        _invitations.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.RespondToInvitationAsync(InvitedId, invitationId, accept: false);

        Assert.AreEqual(InvitationStatus.Declined, invitation.Status);
    }

    // ── Respond: wrong player ─────────────────────────────────────────────────

    [TestMethod]
    public async Task Respond_WrongPlayer_ThrowsForbidden()
    {
        var invitationId = Guid.NewGuid();
        var invitation = new TeamInvitation
        {
            Id = invitationId,
            TeamId = Guid.NewGuid(),
            InvitedPlayerId = InvitedId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
        _invitations.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
            _sut.RespondToInvitationAsync(Guid.NewGuid(), invitationId, true));
    }

    // ── Respond: already responded (not pending) ──────────────────────────────

    [TestMethod]
    public async Task Respond_AlreadyAccepted_ThrowsUnprocessable()
    {
        var invitationId = Guid.NewGuid();
        var invitation = new TeamInvitation
        {
            Id = invitationId,
            TeamId = Guid.NewGuid(),
            InvitedPlayerId = InvitedId,
            Status = InvitationStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
        };
        _invitations.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            _sut.RespondToInvitationAsync(InvitedId, invitationId, true));
    }

    // ── Respond: expired invitation ───────────────────────────────────────────

    [TestMethod]
    public async Task Respond_ExpiredInvitation_ThrowsUnprocessable()
    {
        var invitationId = Guid.NewGuid();
        var invitation = new TeamInvitation
        {
            Id = invitationId,
            TeamId = Guid.NewGuid(),
            InvitedPlayerId = InvitedId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddDays(-8), // expired (7-day window)
        };
        _invitations.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);

        // EffectiveStatus will be Expired due to IsExpired property on entity
        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            _sut.RespondToInvitationAsync(InvitedId, invitationId, true));
    }
}
