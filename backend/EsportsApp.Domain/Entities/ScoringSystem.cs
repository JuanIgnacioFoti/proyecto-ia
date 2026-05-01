using EsportsApp.Domain.Enums;

namespace EsportsApp.Domain.Entities;

public class ScoringSystem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TournamentId { get; set; }
    public ScoringSystemType Type { get; set; }
    public int WinPoints { get; set; }
    public int DrawPoints { get; set; }
    public int LossPoints { get; set; }

    public Tournament Tournament { get; set; } = null!;
}
