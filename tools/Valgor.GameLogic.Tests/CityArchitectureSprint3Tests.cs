using Valgor.City.Buildings;
using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityArchitectureSprint3Tests
{
    [Fact]
    public void Sprint3_Config_IsComplete()
    {
        Assert.True(CityArchitectureSprint3Config.Enabled);
        Assert.True(CityArchitectureSprint3Config.SuppressPrototypeHousingRings);
        Assert.True(CityArchitectureSprint3Config.SiteIntegrationEnabled);
        Assert.True(CityArchitectureSprint3Config.ThematicDressingEnabled);
        Assert.Equal(17, CityArchitectureSprint3Config.OfficialBuildings.Length);
        Assert.Equal(17, CityArchitectureSprint3Config.ArchitectureReady.Length);
        Assert.Equal(5, CityArchitectureSprint3Config.Stages.Length);
        Assert.Contains("proxies-oficiais", CityArchitectureSprint3Config.Stages);
        Assert.Contains("eliminar-casas-coloridas", CityArchitectureSprint3Config.Stages);
    }

    [Fact]
    public void Sprint3_CoversAllCanonicalBuildingIds()
    {
        Assert.Equal(17, BuildingIdMigration.CanonicalIds.Length);
        foreach (var id in BuildingIdMigration.CanonicalIds)
        {
            Assert.Contains(id, CityArchitectureSprint3Config.OfficialBuildings);
            Assert.Contains(id, CityArchitectureSprint3Config.ArchitectureReady);
        }
    }

    [Fact]
    public void Sprint3_DoesNotAlterLogicAnchors()
    {
        Assert.False(BuildingIdMigration.IsCanonical("ArchSprint3Dressing"));
        Assert.False(BuildingIdMigration.IsCanonical("ArchitectureSprint3"));
        Assert.True(BuildingIdMigration.IsCanonical("market"));
        Assert.True(BuildingIdMigration.IsCanonical("foundry"));
        Assert.True(BuildingIdMigration.IsCanonical("hero-hall"));

        var castle = CityCapitalLayoutConfig.GetPosition("castle");
        var market = CityCapitalLayoutConfig.GetPosition("market");
        var mine = CityCapitalLayoutConfig.GetPosition("mine");
        Assert.Equal(0f, castle.X, 3);
        Assert.Equal(0f, castle.Z, 3);
        Assert.Equal(-8.6f, market.X, 3);
        Assert.Equal(-6.4f, market.Z, 3);
        Assert.Equal(-15.0f, mine.X, 3);
        Assert.Equal(11.8f, mine.Z, 3);
    }

    [Fact]
    public void Sprint3_ArtPass3_DropsVernacularFromBlockout()
    {
        Assert.DoesNotContain("habitacao-vernacular", CityArtPass3Config.RemainingBlockout);
        Assert.Contains("market", CityArtPass3Config.HighFidelityAssets);
        Assert.Contains("mine", CityArtPass3Config.HighFidelityAssets);
        Assert.Contains("residence", CityArtPass3Config.HighFidelityAssets);
        Assert.True(CityArtPass3Config.HighFidelityAssets.Length >= 17);
    }
}
