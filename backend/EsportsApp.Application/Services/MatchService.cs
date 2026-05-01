using EsportsApp.Application.DTOs.Matches;
using EsportsApp.Application.DTOs.Standings;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Application.Scoring;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;

namespace EsportsApp.Application.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _matches;
    private readonly ITournamentRepository _tournaments;
    private readonly IStandingRepository _standings;
    private readonly ITournamentRegistrationRepository _registrations;
    private readonly IMatchRealtimeNotifier _notifier;

    public MatchService(IMatchRepository matches, ITournamentRepository tournaments,
        IStandingRepository standings, ITournamentRegistrationRepository registrations,
        IMatchRealtimeNotifier notifier)
    {
        _matches = matches;
        _tournaments = tournaments;
        _standings = standings;
        _registrations = registrations;
        _notifier = notifier;
    }

    public async Task<MatchDto> RecordAsync(Guid organizerId, Guid tournamentId, RecordMatchRequestDto request)
    {
        var tournament = await _tournaments.GetByIdAsync(tournamentId)
            ?? throw new NotFoundException("Tournament not found.");

        if (tournament.OrganizerId != organizerId)
            throw new ForbiddenException("Not the tournament organizer.");

        if (tournament.Status != TournamentStatus.InProgress)
            throw new UnprocessableEntityException("Match recording requires tournament to be InProgress.");

        if (request.HomeTeamId == request.AwayTeamId)
            throw new ValidationException("awayTeamId", "Home and away teams must be different.");

        if (await _matches.DuplicateExistsAsync(tournamentId, request.HomeTeamId, request.AwayTeamId, request.PlayedAt))
            throw new ConflictException("A match between these teams at this time already exists.");

        var match = new Match
        {
            TournamentId = tournamentId,
            HomeTeamId = request.HomeTeamId,
            AwayTeamId = request.AwayTeamId,
            HomeScore = request.HomeScore,
            AwayScore = request.AwayScore,
            PlayedAt = request.PlayedAt,
        };

        await _matches.AddAsync(match);
        await _matches.SaveChangesAsync();

        await RecalculateStandingsAsync(tournament);

        var dto = await GetMatchDtoAsync(match);
        await _notifier.NotifyStandingsUpdatedAsync(tournamentId);

        return dto;
    }

    public async Task<MatchDto> CorrectAsync(Guid organizerId, Guid tournamentId, Guid matchId, RecordMatchRequestDto request)
    {
        var tournament = await _tournaments.GetByIdAsync(tournamentId)
            ?? throw new NotFoundException("Tournament not found.");

        if (tournament.OrganizerId != organizerId)
            throw new ForbiddenException("Not the tournament organizer.");

        if (tournament.Status != TournamentStatus.InProgress)
            throw new UnprocessableEntityException("Match correction requires tournament to be InProgress.");

        var match = await _matches.GetByIdAsync(matchId)
            ?? throw new NotFoundException("Match not found.");

        if (match.TournamentId != tournamentId)
            throw new NotFoundException("Match not found in this tournament.");

        // Check for duplicate excluding current match
        var allMatches = await _matches.GetByTournamentAsync(tournamentId);
        bool hasDuplicate = allMatches.Any(m =>
            m.Id != matchId
            && m.PlayedAt == request.PlayedAt
            && ((m.HomeTeamId == request.HomeTeamId && m.AwayTeamId == request.AwayTeamId)
                || (m.HomeTeamId == request.AwayTeamId && m.AwayTeamId == request.HomeTeamId)));

        if (hasDuplicate)
            throw new ConflictException("A match between these teams at this time already exists.");

        match.HomeTeamId = request.HomeTeamId;
        match.AwayTeamId = request.AwayTeamId;
        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        match.PlayedAt = request.PlayedAt;

        await _matches.UpdateAsync(match);
        await _matches.SaveChangesAsync();

        await RecalculateStandingsAsync(tournament);

        var dto = await GetMatchDtoAsync(match);
        await _notifier.NotifyStandingsUpdatedAsync(tournamentId);

        return dto;
    }

    public async Task<IEnumerable<MatchDto>> GetByTournamentAsync(Guid tournamentId)
    {
        var matches = await _matches.GetByTournamentAsync(tournamentId);
        return matches.Select(m => new MatchDto
        {
            Id = m.Id,
            TournamentId = m.TournamentId,
            HomeTeamId = m.HomeTeamId,
            HomeTeamName = m.HomeTeam?.Name ?? string.Empty,
            AwayTeamId = m.AwayTeamId,
            AwayTeamName = m.AwayTeam?.Name ?? string.Empty,
            HomeScore = m.HomeScore,
            AwayScore = m.AwayScore,
            PlayedAt = m.PlayedAt,
            RecordedAt = m.RecordedAt,
        });
    }

    private async Task RecalculateStandingsAsync(Tournament tournament)
    {
        var strategy = ScoringStrategyFactory.Create(tournament.ScoringSystem);
        var (winPts, drawPts, lossPts) = strategy.GetPoints();

        var allMatches = await _matches.GetByTournamentAsync(tournament.Id);
        var activeRegs = await _registrations.GetActiveByTournamentAsync(tournament.Id);

        // Reset all standings
        foreach (var reg in activeRegs)
        {
            var standing = await _standings.GetByTournamentAndTeamAsync(tournament.Id, reg.TeamId);
            if (standing == null)
            {
                standing = new Standing { TournamentId = tournament.Id, TeamId = reg.TeamId };
                await _standings.AddAsync(standing);
            }
            standing.Points = 0; standing.MatchesPlayed = 0; standing.Wins = 0; standing.Draws = 0; standing.Losses = 0;
            await _standings.UpdateAsync(standing);
        }

        await _standings.SaveChangesAsync();

        // Replay all matches
        foreach (var match in allMatches)
        {
            var homeStanding = await _standings.GetByTournamentAndTeamAsync(tournament.Id, match.HomeTeamId);
            var awayStanding = await _standings.GetByTournamentAndTeamAsync(tournament.Id, match.AwayTeamId);

            if (homeStanding == null || awayStanding == null) continue;

            homeStanding.MatchesPlayed++;
            awayStanding.MatchesPlayed++;

            if (match.HomeScore > match.AwayScore)
            {
                homeStanding.Wins++; homeStanding.Points += winPts;
                awayStanding.Losses++; awayStanding.Points += lossPts;
            }
            else if (match.HomeScore < match.AwayScore)
            {
                awayStanding.Wins++; awayStanding.Points += winPts;
                homeStanding.Losses++; homeStanding.Points += lossPts;
            }
            else
            {
                homeStanding.Draws++; homeStanding.Points += drawPts;
                awayStanding.Draws++; awayStanding.Points += drawPts;
            }

            await _standings.UpdateAsync(homeStanding);
            await _standings.UpdateAsync(awayStanding);
        }

        await _standings.SaveChangesAsync();
    }

    private async Task<MatchDto> GetMatchDtoAsync(Match match)
    {
        var loaded = await _matches.GetByIdAsync(match.Id) ?? match;
        return new MatchDto
        {
            Id = loaded.Id,
            TournamentId = loaded.TournamentId,
            HomeTeamId = loaded.HomeTeamId,
            HomeTeamName = loaded.HomeTeam?.Name ?? string.Empty,
            AwayTeamId = loaded.AwayTeamId,
            AwayTeamName = loaded.AwayTeam?.Name ?? string.Empty,
            HomeScore = loaded.HomeScore,
            AwayScore = loaded.AwayScore,
            PlayedAt = loaded.PlayedAt,
            RecordedAt = loaded.RecordedAt,
        };
    }
}
