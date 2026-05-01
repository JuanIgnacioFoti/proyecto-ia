using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;

namespace EsportsApp.Application.Scoring;

public interface IScoringStrategy
{
    (int win, int draw, int loss) GetPoints();
}

public class StandardScoringStrategy : IScoringStrategy
{
    public (int win, int draw, int loss) GetPoints() => (3, 1, 0);
}

public class WinnerTakesAllScoringStrategy : IScoringStrategy
{
    public (int win, int draw, int loss) GetPoints() => (3, 0, 0);
}

public class CustomScoringStrategy : IScoringStrategy
{
    private readonly int _win, _draw, _loss;
    public CustomScoringStrategy(int win, int draw, int loss) => (_win, _draw, _loss) = (win, draw, loss);
    public (int win, int draw, int loss) GetPoints() => (_win, _draw, _loss);
}

public static class ScoringStrategyFactory
{
    public static IScoringStrategy Create(ScoringSystem scoringSystem) =>
        scoringSystem.Type switch
        {
            ScoringSystemType.Standard => new StandardScoringStrategy(),
            ScoringSystemType.WinnerTakesAll => new WinnerTakesAllScoringStrategy(),
            ScoringSystemType.Custom => new CustomScoringStrategy(
                scoringSystem.WinPoints, scoringSystem.DrawPoints, scoringSystem.LossPoints),
            _ => throw new InvalidOperationException($"Unknown scoring type: {scoringSystem.Type}")
        };
}
