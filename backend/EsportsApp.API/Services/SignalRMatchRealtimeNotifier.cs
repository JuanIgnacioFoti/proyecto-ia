using EsportsApp.Application.Interfaces;
using EsportsApp.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EsportsApp.API.Services;

public class SignalRMatchRealtimeNotifier : IMatchRealtimeNotifier
{
    private readonly IHubContext<TournamentHub> _hub;
    public SignalRMatchRealtimeNotifier(IHubContext<TournamentHub> hub) => _hub = hub;

    public async Task NotifyStandingsUpdatedAsync(Guid tournamentId)
    {
        await _hub.Clients.Group(tournamentId.ToString()).SendAsync("StandingsUpdated", tournamentId);
    }
}
