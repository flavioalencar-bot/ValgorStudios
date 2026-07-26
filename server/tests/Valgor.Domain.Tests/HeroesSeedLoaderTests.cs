using Valgor.Domain.Heroes;
using Valgor.Infrastructure.Persistence.Seed;

namespace Valgor.Domain.Tests;

public sealed class HeroesSeedLoaderTests
{
    [Fact]
    public void Seed_Contains_Exactly_Eleven_Heroes()
    {
        var bundle = HeroesSeedLoader.LoadFromEmbeddedOrFile();
        Assert.Equal(11, bundle.Heroes.Count);
        Assert.Contains(bundle.Heroes, h => h.Id == "HERO_VORTEX_000");
        Assert.Contains(bundle.Heroes, h => h.Id == "HERO_VESPERA_010");
    }

    [Fact]
    public void Pending_Names_Remain_Valid_Through_Internal_Ids()
    {
        var bundle = HeroesSeedLoader.LoadFromEmbeddedOrFile();
        var pending = bundle.Heroes.Where(h => h.Name == "A definir").ToArray();

        Assert.Equal(3, pending.Length);
        Assert.All(pending, hero =>
        {
            Assert.False(string.IsNullOrWhiteSpace(hero.Id));
            Assert.StartsWith("HERO_", hero.Id);
            Assert.Equal(hero.Title, hero.DisplayName);
        });

        Assert.Contains(pending, h => h.Id == "HERO_CONSORTE_002");
        Assert.Contains(pending, h => h.Id == "HERO_SOMBRA_003");
        Assert.Contains(pending, h => h.Id == "HERO_ABISMO_007");
    }

    [Fact]
    public void Team_Bonuses_Match_Approved_Multipliers()
    {
        var bundle = HeroesSeedLoader.LoadFromEmbeddedOrFile();
        Assert.Contains(bundle.TeamBonuses, b => b.SameFactionCount == 3 && b.OtherFactionCount == 0 && b.TotalTroopAttackMultiplier == 1.05m);
        Assert.Contains(bundle.TeamBonuses, b => b.SameFactionCount == 3 && b.OtherFactionCount == 2 && b.TotalTroopAttackMultiplier == 1.07m);
        Assert.Contains(bundle.TeamBonuses, b => b.SameFactionCount == 4 && b.OtherFactionCount == 0 && b.TotalTroopAttackMultiplier == 1.10m);
        Assert.Contains(bundle.TeamBonuses, b => b.SameFactionCount == 5 && b.OtherFactionCount == 0 && b.TotalTroopAttackMultiplier == 1.15m);
    }
}
