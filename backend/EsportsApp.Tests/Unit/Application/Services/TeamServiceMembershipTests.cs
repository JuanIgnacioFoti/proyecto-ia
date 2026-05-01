using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// Team membership management and captaincy transfer rule tests (US3).
/// </summary>
[TestClass]
public class TeamServiceMembershipTests
{
    private Mock<ITeamRepository> _teams = null!;
    private Mock<IPlayerRepository> _players = null!;
    private Mock<IUserRepository> _users = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<ITeamInvitationRepository> _invitations = null!;
    private TeamService _sut = null!;

    private static readonly Guid CaptainId = Guid.NewGuid();
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly Guid GameId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _teams = new Mock<ITeamRepository>();
        _players = new Mock<IPlayerRepository>();
        _users = new Mock<IUserRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _invitations = new Mock<ITeamInvitationRepository>();
        _sut = new TeamService(_teams.Object, _players.Object, _users.Object, _videogames.Object, _invitations.Object);

        _teams.Setup(r => r.UpdateAsync(It.IsAny<Team>())).Returns(Task.CompletedTask);
        _teams.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
    }

    private Team TeamWithMembers(Guid teamId) => new()
    {
        Id = teamId,
        CaptainId = CaptainId,
        VideogameId = GameId,
        Members = new List<TeamMember>
        {
            new() { TeamId = teamId, PlayerId = CaptainId },
            new() { TeamId = teamId, PlayerId = MemberId },
        },
    };

    // ── Remove member: happy path ─────────────────────────────────────────────

    [TestMethod]
    public async Task RemoveMember_ValidRequest_RemovesMember()
    {
        var teamId = Guid.NewGuid();
        var team = TeamWithMembers(teamId);
        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);

        await _sut.RemoveMemberAsync(CaptainId, teamId, MemberId);

        Assert.IsFalse(team.Members.Any(m => m.PlayerId == MemberId));
    }

    // ── Remove member: non-captain cannot remove ───────────────────────────────

    [TestMethod]
    public async Task RemoveMember_NonCaptain_ThrowsForbidden()
    {
        var teamId = Guid.NewGuid();
        var team = TeamWithMembers(teamId);
        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
            _sut.RemoveMemberAsync(Guid.NewGuid(), teamId, MemberId));
    }

    // ── Remove member: captain cannot remove themselves ───────────────────────

    [TestMethod]
    public async Task RemoveMember_CaptainRemovesSelf_ThrowsUnprocessable()
    {
        var teamId = Guid.NewGuid();
        var team = TeamWithMembers(teamId);
        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
            _sut.RemoveMemberAsync(CaptainId, teamId, CaptainId));
    }

    // ── Remove member: member not found ──────────────────────────────────────

    [TestMethod]
    public async Task RemoveMember_MemberNotInTeam_ThrowsNotFound()
    {
        var teamId = Guid.NewGuid();
        var team = TeamWithMembers(teamId);
        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            _sut.RemoveMemberAsync(CaptainId, teamId, Guid.NewGuid())); // random unknown id
    }

    // ── Transfer captaincy: happy path ────────────────────────────────────────

    [TestMethod]
    public async Task TransferCaptaincy_ValidRequest_UpdatesCaptain()
    {
        var teamId = Guid.NewGuid();
        var team = TeamWithMembers(teamId);
        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);

        await _sut.TransferCaptaincyAsync(CaptainId, teamId, MemberId);

        Assert.AreEqual(MemberId, team.CaptainId);
    }

    // ── Transfer captaincy: non-captain cannot transfer ───────────────────────

    [TestMethod]
    public async Task TransferCaptaincy_NonCaptain_ThrowsForbidden()
    {
        var teamId = Guid.NewGuid();
        var team = TeamWithMembers(teamId);
        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
            _sut.TransferCaptaincyAsync(Guid.NewGuid(), teamId, MemberId));
    }

    // ── Transfer captaincy: new captain must be a member ─────────────────────

    [TestMethod]
    public async Task TransferCaptaincy_NonMemberTarget_ThrowsValidation()
    {
        var teamId = Guid.NewGuid();
        var team = TeamWithMembers(teamId);
        _teams.Setup(r => r.GetByIdWithMembersAsync(teamId)).ReturnsAsync(team);

        await Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _sut.TransferCaptaincyAsync(CaptainId, teamId, Guid.NewGuid())); // not in team
    }

    // ── Transfer captaincy: team not found ───────────────────────────────────

    [TestMethod]
    public async Task TransferCaptaincy_TeamNotFound_ThrowsNotFound()
    {
        _teams.Setup(r => r.GetByIdWithMembersAsync(It.IsAny<Guid>())).ReturnsAsync((Team?)null);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            _sut.TransferCaptaincyAsync(CaptainId, Guid.NewGuid(), MemberId));
    }
}
