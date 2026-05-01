using EsportsApp.Application.DTOs.Standings;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;

namespace EsportsApp.Application.Services;

public class StandingsService : IStandingsService
{
    private readonly IStandingRepository _standings;
    private readonly ITournamentRepository _tournaments;

    public StandingsService(IStandingRepository standings, ITournamentRepository tournaments)
    {
        _standings = standings;
        _tournaments = tournaments;
    }

    public async Task<IEnumerable<StandingDto>> GetByTournamentAsync(Guid tournamentId)
    {
        var tournament = await _tournaments.GetByIdAsync(tournamentId)
            ?? throw new NotFoundException("Tournament not found.");

        if (tournament.Status == TournamentStatus.Draft || tournament.Status == TournamentStatus.Open)
            throw new ConflictException("Standings are only available for InProgress or Completed tournaments.");

        var standings = await _standings.GetByTournamentAsync(tournamentId);
        var sorted = standings.OrderByDescending(s => s.Points).ThenBy(s => s.Team.Name).ToList();

        int rank = 1, prev = -1, idx = 0;
        return sorted.Select(s =>
        {
            if (s.Points != prev) { rank = idx + 1; prev = s.Points; }
            idx++;
            return new StandingDto
            {
                Rank = rank,
                TeamId = s.TeamId,
                TeamName = s.Team?.Name ?? string.Empty,
                Points = s.Points,
                MatchesPlayed = s.MatchesPlayed,
                Wins = s.Wins,
                Draws = s.Draws,
                Losses = s.Losses,
            };
        });
    }
}
