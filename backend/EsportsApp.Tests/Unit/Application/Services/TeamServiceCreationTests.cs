using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Moq;

namespace EsportsApp.Tests.Unit.Application.Services;

/// <summary>
/// Team creation and captain constraint unit tests (US3).
/// </summary>
[TestClass]
public class TeamServiceCreationTests
{
    private Mock<ITeamRepository> _teams = null!;
    private Mock<IPlayerRepository> _players = null!;
    private Mock<IUserRepository> _users = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<ITeamInvitationRepository> _invitations = null!;
    private TeamService _sut = null!;

    private static readonly Guid GameId = Guid.NewGuid();
    private static readonly Guid CaptainId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _teams = new Mock<ITeamRepository>();
        _players = new Mock<IPlayerRepository>();
        _users = new Mock<IUserRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _invitations = new Mock<ITeamInvitationRepository>();
        _sut = new TeamService(_teams.Object, _players.Object, _users.Object, _videogames.Object, _invitations.Object);

        _players.Setup(r => r.GetByIdAsync(CaptainId)).ReturnsAsync(new Player { Id = CaptainId });
        _videogames.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        _teams.Setup(r => r.GetByPlayerAsync(CaptainId)).ReturnsAsync(new List<Team>());
        _teams.Setup(r => r.NameExistsForGameAsync(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(false);
        _teams.Setup(r => r.AddAsync(It.IsAny<Team>())).Returns(Task.CompletedTask);
        _teams.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // GetByIdWithMembersAsync called when returning the created team detail
        _teams.Setup(r => r.GetByIdWithMembersAsync(It.IsAny<Guid>()))
              .ReturnsAsync((Guid id) => new Team
              {
                  Id = id,
                  Name = "TestTeam",
                  VideogameId = GameId,
                  CaptainId = CaptainId,
                  Members = new List<TeamMember> { new TeamMember { PlayerId = CaptainId } },
              });
    }

    // ── Happy path ───────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Create_ValidRequest_ReturnTeamDetail()
    {
        var result = await _sut.CreateAsync(CaptainId, new EsportsApp.Application.DTOs.Teams.CreateTeamRequestDto
        {
            Name = "Dream Team",
            VideogameId = GameId,
        });

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Name);
    }

    // ── Videogame not found ──────────────────────────────────────────────────

    [TestMethod]
    public async Task Create_InvalidVideogame_ThrowsValidation()
    {
        _videogames.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);
        await Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _sut.CreateAsync(CaptainId, new EsportsApp.Application.DTOs.Teams.CreateTeamRequestDto
            {
                Name = "Team",
                VideogameId = Guid.NewGuid(),
            }));
    }

    // ── Player already captain for same game ──────────────────────────────────

    [TestMethod]
    public async Task Create_AlreadyCaptainForGame_ThrowsConflict()
    {
        var existingTeam = new Team { CaptainId = CaptainId, VideogameId = GameId };
        _teams.Setup(r => r.GetByPlayerAsync(CaptainId)).ReturnsAsync(new List<Team> { existingTeam });

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.CreateAsync(CaptainId, new EsportsApp.Application.DTOs.Teams.CreateTeamRequestDto
            {
                Name = "Another Team",
                VideogameId = GameId,
            }));
    }

    // ── Player already member of a team for same game ─────────────────────────

    [TestMethod]
    public async Task Create_AlreadyMemberForGame_ThrowsConflict()
    {
        // Player is a member but not captain
        var existingTeam = new Team { CaptainId = Guid.NewGuid(), VideogameId = GameId };
        _teams.Setup(r => r.GetByPlayerAsync(CaptainId)).ReturnsAsync(new List<Team> { existingTeam });

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.CreateAsync(CaptainId, new EsportsApp.Application.DTOs.Teams.CreateTeamRequestDto
            {
                Name = "New Team",
                VideogameId = GameId,
            }));
    }

    // ── Duplicate team name for same game ─────────────────────────────────────

    [TestMethod]
    public async Task Create_DuplicateTeamNameForGame_ThrowsConflict()
    {
        _teams.Setup(r => r.NameExistsForGameAsync("TakenName", GameId)).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ConflictException>(() =>
            _sut.CreateAsync(CaptainId, new EsportsApp.Application.DTOs.Teams.CreateTeamRequestDto
            {
                Name = "TakenName",
                VideogameId = GameId,
            }));
    }

    // ── Player not found ──────────────────────────────────────────────────────

    [TestMethod]
    public async Task Create_PlayerNotFound_ThrowsNotFound()
    {
        _players.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Player?)null);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), new EsportsApp.Application.DTOs.Teams.CreateTeamRequestDto
            {
                Name = "Team",
                VideogameId = GameId,
            }));
    }
}
