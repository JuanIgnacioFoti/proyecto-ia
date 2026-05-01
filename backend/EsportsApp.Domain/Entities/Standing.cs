namespace EsportsApp.Domain.Entities;

public class Standing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TournamentId { get; set; }
    public Guid TeamId { get; set; }
    public int Points { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }

    // Optimistic concurrency token
    public byte[]? RowVersion { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
