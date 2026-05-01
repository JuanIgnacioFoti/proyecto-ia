using EsportsApp.Application.DTOs.Registrations;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;

namespace EsportsApp.Application.Services;

public class TournamentRegistrationService : ITournamentRegistrationService
{
    private readonly ITournamentRepository _tournaments;
    private readonly ITeamRepository _teams;
    private readonly IPlayerRepository _players;
    private readonly ITournamentRegistrationRepository _registrations;
    private readonly IStandingRepository _standings;

    public TournamentRegistrationService(ITournamentRepository tournaments, ITeamRepository teams,
        IPlayerRepository players, ITournamentRegistrationRepository registrations, IStandingRepository standings)
    {
        _tournaments = tournaments;
        _teams = teams;
        _players = players;
        _registrations = registrations;
        _standings = standings;
    }

    public async Task<RegistrationDto> RegisterAsync(Guid captainPlayerId, Guid tournamentId, RegisterTeamRequestDto request)
    {
        var tournament = await _tournaments.GetByIdAsync(tournamentId)
            ?? throw new NotFoundException("Tournament not found.");

        if (tournament.Status != TournamentStatus.Open)
            throw new UnprocessableEntityException("Tournament is not open for registration.");

        var team = await _teams.GetByIdWithMembersAsync(request.TeamId)
            ?? throw new NotFoundException("Team not found.");

        if (team.CaptainId != captainPlayerId)
            throw new ForbiddenException("Only the team captain can register the team.");

        if (team.VideogameId != tournament.VideogameId)
            throw new ValidationException("teamId", "Team's game does not match tournament's game.");

        if (team.Members.Count < tournament.MinMembersPerTeam)
            throw new ValidationException("teamId", $"Team needs at least {tournament.MinMembersPerTeam} members.");

        var existing = await _registrations.GetByTournamentAndTeamAsync(tournamentId, request.TeamId);
        if (existing != null && existing.Status == RegistrationStatus.Active)
            throw new ConflictException("Team is already registered in this tournament.");

        var activeCount = await _registrations.GetActiveCountByTournamentAsync(tournamentId);
        if (activeCount >= tournament.MaxTeams)
            throw new UnprocessableEntityException("Tournament is at maximum capacity.");

        if (existing != null)
        {
            // Re-register (was withdrawn)
            existing.Status = RegistrationStatus.Active;
            existing.RegisteredAt = DateTime.UtcNow;
            await _registrations.UpdateAsync(existing);

            // Restore standing
            var existingStanding = await _standings.GetByTournamentAndTeamAsync(tournamentId, request.TeamId);
            if (existingStanding == null)
            {
                await _standings.AddAsync(new Standing { TournamentId = tournamentId, TeamId = request.TeamId });
            }

            await _registrations.SaveChangesAsync();
            return MapToDto(existing, team.Name);
        }

        var registration = new TournamentRegistration
        {
            TournamentId = tournamentId,
            TeamId = request.TeamId,
        };

        await _registrations.AddAsync(registration);
        await _standings.AddAsync(new Standing { TournamentId = tournamentId, TeamId = request.TeamId });
        await _registrations.SaveChangesAsync();

        return MapToDto(registration, team.Name);
    }

    public async Task WithdrawAsync(Guid captainPlayerId, Guid tournamentId, Guid teamId)
    {
        var team = await _teams.GetByIdAsync(teamId)
            ?? throw new NotFoundException("Team not found.");

        if (team.CaptainId != captainPlayerId)
            throw new ForbiddenException("Only the team captain can withdraw.");

        var registration = await _registrations.GetByTournamentAndTeamAsync(tournamentId, teamId)
            ?? throw new NotFoundException("Registration not found.");

        if (registration.Status == RegistrationStatus.Withdrawn)
            throw new ConflictException("Team is not registered in this tournament.");

        var tournament = await _tournaments.GetByIdAsync(tournamentId)!;
        if (tournament!.Status == TournamentStatus.InProgress || tournament.Status == TournamentStatus.Completed)
            throw new UnprocessableEntityException("Cannot withdraw from an in-progress or completed tournament.");

        registration.Status = RegistrationStatus.Withdrawn;

        var standing = await _standings.GetByTournamentAndTeamAsync(tournamentId, teamId);
        if (standing != null) await _standings.DeleteAsync(standing);

        await _registrations.UpdateAsync(registration);
        await _registrations.SaveChangesAsync();
    }

    public async Task<IEnumerable<RegistrationDto>> GetByTournamentAsync(Guid tournamentId)
    {
        var registrations = await _registrations.GetActiveByTournamentAsync(tournamentId);
        return registrations.Select(r => MapToDto(r, r.Team?.Name ?? string.Empty));
    }

    private static RegistrationDto MapToDto(TournamentRegistration r, string teamName) => new()
    {
        Id = r.Id,
        TournamentId = r.TournamentId,
        TeamId = r.TeamId,
        TeamName = teamName,
        Status = r.Status.ToString(),
        RegisteredAt = r.RegisteredAt,
    };
}
