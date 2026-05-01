using EsportsApp.Application.DTOs.Teams;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Application.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teams;
    private readonly IPlayerRepository _players;
    private readonly IUserRepository _users;
    private readonly IVideogameRepository _videogames;
    private readonly ITeamInvitationRepository _invitations;

    public TeamService(ITeamRepository teams, IPlayerRepository players, IUserRepository users,
        IVideogameRepository videogames, ITeamInvitationRepository invitations)
    {
        _teams = teams;
        _players = players;
        _users = users;
        _videogames = videogames;
        _invitations = invitations;
    }

    public async Task<TeamDetailDto> GetByIdAsync(Guid teamId)
    {
        var team = await _teams.GetByIdWithMembersAsync(teamId)
            ?? throw new NotFoundException("Team not found.");
        return MapToDetail(team);
    }

    public async Task<TeamDetailDto> CreateAsync(Guid captainPlayerId, CreateTeamRequestDto request)
    {
        var player = await _players.GetByIdAsync(captainPlayerId)
            ?? throw new NotFoundException("Player not found.");

        if (!await _videogames.ExistsAsync(request.VideogameId))
            throw new ValidationException("videogameId", "Videogame not found.");

        // Captain cannot lead more than one team per game
        var existingTeams = await _teams.GetByPlayerAsync(captainPlayerId);
        if (existingTeams.Any(t => t.VideogameId == request.VideogameId && t.CaptainId == captainPlayerId))
            throw new ConflictException("You are already captain of a team for this game.");

        // Player cannot be member of more than one team per game
        if (existingTeams.Any(t => t.VideogameId == request.VideogameId))
            throw new ConflictException("You are already a member of a team for this game.");

        if (await _teams.NameExistsForGameAsync(request.Name, request.VideogameId))
            throw new ConflictException("Team name already in use for this game.");

        var team = new Team
        {
            Name = request.Name,
            Description = request.Description,
            VideogameId = request.VideogameId,
            CaptainId = captainPlayerId,
        };

        // Auto-add captain as member
        team.Members.Add(new TeamMember { TeamId = team.Id, PlayerId = captainPlayerId });

        await _teams.AddAsync(team);
        await _teams.SaveChangesAsync();

        return await GetByIdAsync(team.Id);
    }

    public async Task<TeamDetailDto> UpdateAsync(Guid captainPlayerId, Guid teamId, UpdateTeamRequestDto request)
    {
        var team = await _teams.GetByIdAsync(teamId)
            ?? throw new NotFoundException("Team not found.");

        if (team.CaptainId != captainPlayerId)
            throw new ForbiddenException("Only the team captain can edit this team.");

        if (request.Name != null && request.Name != team.Name)
        {
            if (await _teams.NameExistsForGameAsync(request.Name, team.VideogameId))
                throw new ConflictException("Team name already in use for this game.");
            team.Name = request.Name;
        }

        if (request.Description != null) team.Description = request.Description;

        await _teams.UpdateAsync(team);
        await _teams.SaveChangesAsync();

        return await GetByIdAsync(team.Id);
    }

    public async Task InvitePlayerAsync(Guid captainPlayerId, Guid teamId, InvitePlayerRequestDto request)
    {
        var team = await _teams.GetByIdWithMembersAsync(teamId)
            ?? throw new NotFoundException("Team not found.");

        if (team.CaptainId != captainPlayerId)
            throw new ForbiddenException("Only the team captain can invite players.");

        Player? invited = null;
        if (!string.IsNullOrWhiteSpace(request.Username))
            invited = await _players.GetByUsernameAsync(request.Username);
        else if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var user = await _users.GetByEmailAsync(request.Email);
            if (user?.Player != null) invited = await _players.GetByIdAsync(user.Id);
        }

        if (invited == null)
            throw new NotFoundException("Player not found.");

        if (team.Members.Any(m => m.PlayerId == invited.Id))
            throw new ConflictException("Player is already a member of this team.");

        // FR-019 / US3-Scenario3: reject invitation if player already belongs to a team for this videogame
        var invitedTeams = await _teams.GetByPlayerAsync(invited.Id);
        if (invitedTeams.Any(t => t.VideogameId == team.VideogameId))
            throw new ConflictException("Player is already a member of a team for this game.");

        if (await _invitations.PendingInvitationExistsAsync(teamId, invited.Id))
            throw new ConflictException("A pending invitation already exists for this player.");

        var invitation = new TeamInvitation
        {
            TeamId = teamId,
            InvitedPlayerId = invited.Id,
        };

        await _invitations.AddAsync(invitation);
        await _invitations.SaveChangesAsync();
    }

    public async Task RespondToInvitationAsync(Guid playerId, Guid invitationId, bool accept)
    {
        var invitation = await _invitations.GetByIdAsync(invitationId)
            ?? throw new NotFoundException("Invitation not found.");

        if (invitation.InvitedPlayerId != playerId)
            throw new ForbiddenException("Cannot respond to this invitation.");

        if (invitation.EffectiveStatus != InvitationStatus.Pending)
            throw new UnprocessableEntityException("Invitation is no longer pending.");

        if (accept)
        {
            // Check membership constraints
            var existingTeams = await _teams.GetByPlayerAsync(playerId);
            var team = await _teams.GetByIdAsync(invitation.TeamId)
                ?? throw new NotFoundException("Team not found.");

            if (existingTeams.Any(t => t.VideogameId == team.VideogameId))
                throw new ConflictException("Already a member of a team for this game.");

            await _teams.AddMemberAsync(new TeamMember { TeamId = team.Id, PlayerId = playerId });
            invitation.Status = InvitationStatus.Accepted;
        }
        else
        {
            invitation.Status = InvitationStatus.Declined;
        }

        // Re-verify the invitation is still pending immediately before persisting.
        // Protects against a race condition where a concurrent request responded
        // between the check above and this point.
        var currentStatus = await _invitations.GetCurrentStatusAsync(invitationId);
        if (currentStatus == null)
            throw new NotFoundException("Invitation no longer exists.");
        if (currentStatus != InvitationStatus.Pending)
            throw new ConflictException("The invitation was already responded to by a concurrent request.");

        await _invitations.UpdateAsync(invitation);
        try
        {
            await _invitations.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // The invitation row was deleted or already modified by a concurrent
            // request between our last status check and this save.
            throw new ConflictException("The invitation was already responded to or no longer exists.");
        }
    }

    public async Task RemoveMemberAsync(Guid captainPlayerId, Guid teamId, Guid memberId)
    {
        var team = await _teams.GetByIdWithMembersAsync(teamId)
            ?? throw new NotFoundException("Team not found.");

        if (team.CaptainId != captainPlayerId)
            throw new ForbiddenException("Only the captain can remove members.");

        if (memberId == captainPlayerId)
            throw new UnprocessableEntityException("Captain cannot remove themselves. Transfer captaincy first.");

        var member = team.Members.FirstOrDefault(m => m.PlayerId == memberId)
            ?? throw new NotFoundException("Member not found in this team.");

        team.Members.Remove(member);
        await _teams.UpdateAsync(team);
        await _teams.SaveChangesAsync();
    }

    public async Task TransferCaptaincyAsync(Guid currentCaptainId, Guid teamId, Guid newCaptainId)
    {
        var team = await _teams.GetByIdWithMembersAsync(teamId)
            ?? throw new NotFoundException("Team not found.");

        if (team.CaptainId != currentCaptainId)
            throw new ForbiddenException("Only the current captain can transfer captaincy.");

        if (!team.Members.Any(m => m.PlayerId == newCaptainId))
            throw new ValidationException("newCaptainId", "New captain must be a current team member.");

        team.CaptainId = newCaptainId;
        await _teams.UpdateAsync(team);
        await _teams.SaveChangesAsync();
    }

    public async Task<IEnumerable<InvitationDto>> GetMyInvitationsAsync(Guid playerId)
    {
        var invitations = await _invitations.GetPendingByPlayerAsync(playerId);
        return invitations.Select(i => new InvitationDto
        {
            Id = i.Id,
            TeamId = i.TeamId,
            TeamName = i.Team?.Name ?? string.Empty,
            VideogameName = i.Team?.Videogame?.Name ?? string.Empty,
            Status = i.EffectiveStatus.ToString(),
            CreatedAt = i.CreatedAt,
            ExpiresAt = i.CreatedAt.AddDays(7),
        });
    }

    public async Task<TeamDetailDto?> GetMyTeamAsync(Guid playerId)
    {
        var teams = await _teams.GetByPlayerAsync(playerId);
        var team = teams.FirstOrDefault();
        if (team == null) return null;
        var full = await _teams.GetByIdWithMembersAsync(team.Id);
        return full == null ? null : MapToDetail(full);
    }

    private static TeamDetailDto MapToDetail(Team team) => new()
    {
        Id = team.Id,
        Name = team.Name,
        Description = team.Description,
        VideogameId = team.VideogameId,
        VideogameName = team.Videogame?.Name ?? string.Empty,
        CaptainId = team.CaptainId,
        CaptainUsername = team.Captain?.Username ?? string.Empty,
        Members = team.Members.Select(m => new TeamMemberDto
        {
            PlayerId = m.PlayerId,
            Username = m.Player?.Username ?? string.Empty,
            RealName = m.Player?.RealName ?? string.Empty,
            IsCaptain = m.PlayerId == team.CaptainId,
            JoinedAt = m.JoinedAt,
        }).ToList(),
        CreatedAt = team.CreatedAt,
    };
}
