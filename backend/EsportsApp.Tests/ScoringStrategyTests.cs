using EsportsApp.Application.Scoring;
using EsportsApp.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EsportsApp.Tests;

[TestClass]
public class ScoringStrategyTests
{
    [TestMethod]
    public void StandardStrategy_WinnerGets3()
    {
        var strategy = ScoringStrategyFactory.Create(new EsportsApp.Domain.Entities.ScoringSystem { Type = ScoringSystemType.Standard });
        var (win, draw, loss) = strategy.GetPoints();
        Assert.AreEqual(3, win);
        Assert.AreEqual(1, draw);
        Assert.AreEqual(0, loss);
    }

    [TestMethod]
    public void WinnerTakesAllStrategy_WinnerGets3DrawGets0()
    {
        var strategy = ScoringStrategyFactory.Create(new EsportsApp.Domain.Entities.ScoringSystem { Type = ScoringSystemType.WinnerTakesAll });
        var (win, draw, loss) = strategy.GetPoints();
        Assert.AreEqual(3, win);
        Assert.AreEqual(0, draw);
        Assert.AreEqual(0, loss);
    }

    [TestMethod]
    public void CustomStrategy_UsesConfiguredPoints()
    {
        var strategy = ScoringStrategyFactory.Create(new EsportsApp.Domain.Entities.ScoringSystem
        {
            Type = ScoringSystemType.Custom,
            WinPoints = 5, DrawPoints = 2, LossPoints = 1
        });
        var (win, draw, loss) = strategy.GetPoints();
        Assert.AreEqual(5, win);
        Assert.AreEqual(2, draw);
        Assert.AreEqual(1, loss);
    }
}
