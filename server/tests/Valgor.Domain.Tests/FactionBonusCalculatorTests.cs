using Valgor.Domain.Heroes;
using Valgor.Domain.Heroes.Services;

namespace Valgor.Domain.Tests;

public sealed class FactionBonusCalculatorTests
{
    private static FactionBonusCalculator Create() =>
        new(
        [
            new TeamBonusRule(3, 0, 1.05m),
            new TeamBonusRule(3, 2, 1.07m),
            new TeamBonusRule(4, 0, 1.10m),
            new TeamBonusRule(5, 0, 1.15m)
        ]);

    [Fact]
    public void Bonus_3_Same_Returns_1_05()
    {
        var result = Create().Calculate(
        [
            FactionIds.RosaDeSangue,
            FactionIds.RosaDeSangue,
            FactionIds.RosaDeSangue
        ]);
        Assert.Equal(1.05m, result.TotalTroopAttackMultiplier);
    }

    [Fact]
    public void Bonus_3_Plus_2_Returns_1_07()
    {
        var result = Create().Calculate(
        [
            FactionIds.RosaDeSangue,
            FactionIds.RosaDeSangue,
            FactionIds.RosaDeSangue,
            FactionIds.AsasDoAmanhecer,
            FactionIds.AsasDoAmanhecer
        ]);
        Assert.Equal(1.07m, result.TotalTroopAttackMultiplier);
    }

    [Fact]
    public void Bonus_4_Same_Returns_1_10()
    {
        var result = Create().Calculate(
        [
            FactionIds.GuardaDaOrdem,
            FactionIds.GuardaDaOrdem,
            FactionIds.GuardaDaOrdem,
            FactionIds.GuardaDaOrdem
        ]);
        Assert.Equal(1.10m, result.TotalTroopAttackMultiplier);
    }

    [Fact]
    public void Bonus_5_Same_Returns_1_15()
    {
        var result = Create().Calculate(
        [
            FactionIds.AsasDoAmanhecer,
            FactionIds.AsasDoAmanhecer,
            FactionIds.AsasDoAmanhecer,
            FactionIds.AsasDoAmanhecer,
            FactionIds.AsasDoAmanhecer
        ]);
        Assert.Equal(1.15m, result.TotalTroopAttackMultiplier);
    }
}
