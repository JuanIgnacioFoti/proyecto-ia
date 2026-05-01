namespace EsportsApp.Application.Interfaces;

public interface IMatchRealtimeNotifier
{
    Task NotifyStandingsUpdatedAsync(Guid tournamentId);
}
