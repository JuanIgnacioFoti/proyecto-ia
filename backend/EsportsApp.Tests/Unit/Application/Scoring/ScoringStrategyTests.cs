using EsportsApp.Application.Scoring;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;

namespace EsportsApp.Tests.Unit.Application.Scoring;

/// <summary>
/// Scoring strategy calculation tests for Standard, WinnerTakesAll, and Custom systems (US6, FR-032).
/// </summary>
[TestClass]
public class ScoringStrategyTests
{
    // ── Standard: 3-1-0 ──────────────────────────────────────────────────────

    [TestMethod]
    public void Standard_Win_Returns3Points()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.Standard });
        var (win, _, _) = strategy.GetPoints();
        Assert.AreEqual(3, win);
    }

    [TestMethod]
    public void Standard_Draw_Returns1Point()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.Standard });
        var (_, draw, _) = strategy.GetPoints();
        Assert.AreEqual(1, draw);
    }

    [TestMethod]
    public void Standard_Loss_Returns0Points()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.Standard });
        var (_, _, loss) = strategy.GetPoints();
        Assert.AreEqual(0, loss);
    }

    // ── WinnerTakesAll: 3-0-0 ────────────────────────────────────────────────

    [TestMethod]
    public void WTA_Win_Returns3Points()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.WinnerTakesAll });
        var (win, _, _) = strategy.GetPoints();
        Assert.AreEqual(3, win);
    }

    [TestMethod]
    public void WTA_Draw_Returns0Points()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.WinnerTakesAll });
        var (_, draw, _) = strategy.GetPoints();
        Assert.AreEqual(0, draw);
    }

    [TestMethod]
    public void WTA_Loss_Returns0Points()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.WinnerTakesAll });
        var (_, _, loss) = strategy.GetPoints();
        Assert.AreEqual(0, loss);
    }

    // ── WTA: draw is treated same as loss (draws have no value) ──────────────

    [TestMethod]
    public void WTA_DrawEqualsLoss_BothZero()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.WinnerTakesAll });
        var (_, draw, loss) = strategy.GetPoints();
        Assert.AreEqual(draw, loss);
    }

    // ── Custom: uses configured values ───────────────────────────────────────

    [TestMethod]
    public void Custom_UsesConfiguredWinPoints()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.Custom, WinPoints = 5, DrawPoints = 2, LossPoints = 1 });
        var (win, _, _) = strategy.GetPoints();
        Assert.AreEqual(5, win);
    }

    [TestMethod]
    public void Custom_UsesConfiguredDrawPoints()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.Custom, WinPoints = 5, DrawPoints = 2, LossPoints = 1 });
        var (_, draw, _) = strategy.GetPoints();
        Assert.AreEqual(2, draw);
    }

    [TestMethod]
    public void Custom_UsesConfiguredLossPoints()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.Custom, WinPoints = 5, DrawPoints = 2, LossPoints = 1 });
        var (_, _, loss) = strategy.GetPoints();
        Assert.AreEqual(1, loss);
    }

    [TestMethod]
    public void Custom_ZeroLossPoints_IsAllowed()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.Custom, WinPoints = 3, DrawPoints = 1, LossPoints = 0 });
        var (win, draw, loss) = strategy.GetPoints();
        Assert.AreEqual(3, win);
        Assert.AreEqual(1, draw);
        Assert.AreEqual(0, loss);
    }

    [TestMethod]
    public void Custom_AllZero_IsAllowed()
    {
        var strategy = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.Custom, WinPoints = 0, DrawPoints = 0, LossPoints = 0 });
        var (win, draw, loss) = strategy.GetPoints();
        Assert.AreEqual(0, win);
        Assert.AreEqual(0, draw);
        Assert.AreEqual(0, loss);
    }

    // ── Factory creates distinct instances ───────────────────────────────────

    [TestMethod]
    public void Factory_StandardAndWTA_ReturnDifferentDrawValues()
    {
        var standard = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.Standard });
        var wta = ScoringStrategyFactory.Create(new ScoringSystem { Type = ScoringSystemType.WinnerTakesAll });

        var (_, stdDraw, _) = standard.GetPoints();
        var (_, wtaDraw, _) = wta.GetPoints();

        Assert.AreNotEqual(stdDraw, wtaDraw);
    }
}
