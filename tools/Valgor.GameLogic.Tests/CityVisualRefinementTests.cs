using Valgor.City.Buildings;
using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityVisualRefinementTests
{
    [Fact]
    public void OfficialBuildings_RemainExactly17()
    {
        Assert.Equal(17, BuildingIdMigration.OfficialCount);
        Assert.Equal(17, BuildingCatalog.All.Count);
        Assert.Equal(17, CityCapitalLayoutConfig.All.Count);
        Assert.Empty(CityCapitalLayoutConfig.ValidateBlueprintInvariants());
    }

    [Fact]
    public void LogicalPositions_PreservedForKeyLandmarks()
    {
        var castle = CityCapitalLayoutConfig.GetPosition("castle");
        Assert.Equal(0f, castle.X, 2);
        Assert.Equal(0f, castle.Z, 2);
        Assert.True(CityCapitalLayoutConfig.GetPosition("dragon-tower").Z > 14f);
        Assert.True(CityCapitalLayoutConfig.GetPosition("wall").Z < -16f);
        Assert.True(CityCapitalLayoutConfig.GetPosition("hero-hall").X < -2f);
        var field = CityCapitalLayoutConfig.GetPosition("training-ground");
        var academy = CityCapitalLayoutConfig.GetPosition("academy");
        Assert.False(System.Math.Abs(field.X - academy.X) < 0.01f && System.Math.Abs(field.Z - academy.Z) < 0.01f);
    }

    [Fact]
    public void EnvironmentConfig_BudgetsAreSane()
    {
        Assert.True(CityCapitalEnvironmentConfig.ImperialRoadWidth > 3f);
        Assert.InRange(CityCapitalEnvironmentConfig.VegetationCount(CityAmbientQuality.Medium, 1), 12, 48);
        Assert.True(
            CityCapitalEnvironmentConfig.VegetationCount(CityAmbientQuality.High, 6) >=
            CityCapitalEnvironmentConfig.VegetationCount(CityAmbientQuality.Medium, 1));
        Assert.True(CityCapitalEnvironmentConfig.TierVisualDensity(6) >
                    CityCapitalEnvironmentConfig.TierVisualDensity(1));
        Assert.Equal(0f, CityCapitalEnvironmentConfig.SmokeScale(CityAmbientQuality.Low));
        Assert.True(CityCapitalEnvironmentConfig.SmokeScale(CityAmbientQuality.High) >
                    CityCapitalEnvironmentConfig.SmokeScale(CityAmbientQuality.Medium));
    }

    [Fact]
    public void Decoration_AreNotCanonicalBuildingIds()
    {
        Assert.False(BuildingIdMigration.IsCanonical("CottageA"));
        Assert.False(BuildingIdMigration.IsCanonical("FountainBase"));
        Assert.False(BuildingIdMigration.IsCanonical("FoundrySmoke"));
        Assert.True(BuildingIdMigration.IsCanonical("residence"));
        Assert.True(BuildingIdMigration.IsCanonical("foundry"));
    }

    [Fact]
    public void Wall_RemainsSingleLogicalEntry()
    {
        Assert.Equal(1, System.Linq.Enumerable.Count(BuildingIdMigration.CanonicalIds, id => id == "wall"));
    }
}

public sealed class CityAmbientQualityTests
{
    [Fact]
    public void Quality_Gates_AreMonotonic()
    {
        CityAmbientQualitySettings.Set(CityAmbientQuality.Low);
        Assert.True(CityAmbientQualitySettings.AllowFlags);
        Assert.False(CityAmbientQualitySettings.AllowSmoke);
        Assert.False(CityAmbientQualitySettings.AllowBirds);

        CityAmbientQualitySettings.Set(CityAmbientQuality.Medium);
        Assert.True(CityAmbientQualitySettings.AllowSmoke);
        Assert.True(CityAmbientQualitySettings.AllowTorches);
        Assert.False(CityAmbientQualitySettings.AllowBirds);
        Assert.False(CityCapitalEnvironmentConfig.AllowHighExclusiveProps);

        CityAmbientQualitySettings.Set(CityAmbientQuality.High);
        Assert.True(CityAmbientQualitySettings.AllowBirds);
        Assert.True(CityCapitalEnvironmentConfig.AllowHighExclusiveProps);
        Assert.True(CityAmbientQualitySettings.AllowDragonExtraMotion);

        // Restaura default homologado.
        CityAmbientQualitySettings.Set(CityAmbientQuality.Medium);
        Assert.Equal(CityAmbientQuality.Medium, CityAmbientQualitySettings.Current);
    }

    [Fact]
    public void Quality_Values_AreValid()
    {
        Assert.True(CityAmbientQualitySettings.IsValid(CityAmbientQuality.Low));
        Assert.True(CityAmbientQualitySettings.IsValid(CityAmbientQuality.Medium));
        Assert.True(CityAmbientQualitySettings.IsValid(CityAmbientQuality.High));
    }
}

public sealed class CityTierEnvironmentTests
{
    [Fact]
    public void EnvironmentTier_MapsCastleBands()
    {
        Assert.Equal(1, CityCapitalLayoutConfig.ResolveEnvironmentTier(1));
        Assert.Equal(1, CityCapitalLayoutConfig.ResolveEnvironmentTier(5));
        Assert.Equal(3, CityCapitalLayoutConfig.ResolveEnvironmentTier(15));
        Assert.Equal(6, CityCapitalLayoutConfig.ResolveEnvironmentTier(30));
    }

    [Fact]
    public void TierDensity_IncreasesWithTier()
    {
        for (var t = 2; t <= 6; t++)
        {
            Assert.True(CityCapitalEnvironmentConfig.TierVisualDensity(t) >=
                        CityCapitalEnvironmentConfig.TierVisualDensity(t - 1));
        }
    }
}

public sealed class CityDecorationSelectionTests
{
    [Fact]
    public void CanonicalCatalog_DoesNotIncludeDecorNames()
    {
        foreach (var id in new[] { "MarketStall", "CottageA", "ImperialStatue", "LandingPad" })
        {
            Assert.False(BuildingCatalog.TryGet(id, out _));
        }
    }

    [Fact]
    public void SelectionAliases_StillResolveOnlyToOfficialBuildings()
    {
        Assert.Equal("hero-hall", BuildingIdMigration.ToCanonical("institute"));
        Assert.Equal("training-ground", BuildingIdMigration.ToCanonical("laboratory"));
        Assert.True(BuildingCatalog.TryGet("hero-hall", out var hall));
        Assert.Equal("Salão dos Heróis", hall.DisplayName);
    }
}
