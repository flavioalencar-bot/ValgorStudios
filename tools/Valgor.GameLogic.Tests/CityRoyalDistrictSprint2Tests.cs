using Valgor.City.Buildings;
using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityRoyalDistrictSprint2Tests
{
    [Fact]
    public void RoyalDistrict_Config_IsComplete()
    {
        Assert.True(CityRoyalDistrictConfig.Enabled);
        Assert.Equal("RoyalDistrict", CityRoyalDistrictConfig.RootName);
        Assert.Equal(10, CityRoyalDistrictConfig.Features.Length);
        Assert.Contains("praca-imperial", CityRoyalDistrictConfig.Features);
        Assert.Contains("fonte-monumental", CityRoyalDistrictConfig.Features);
        Assert.Contains("residencias-nobres", CityRoyalDistrictConfig.Features);
        Assert.Equal(8, CityRoyalDistrictConfig.FountainSides);
        Assert.Equal(8, CityRoyalDistrictConfig.NobleResidenceCount);
        Assert.True(CityRoyalDistrictConfig.VernacularExclusionRadius >= 7f);
    }

    [Fact]
    public void RoyalDistrict_DoesNotAlterLogicAnchors()
    {
        Assert.Equal(17, BuildingIdMigration.CanonicalIds.Length);
        Assert.True(BuildingIdMigration.IsCanonical("castle"));
        Assert.False(BuildingIdMigration.IsCanonical("RoyalDistrict"));
        Assert.False(BuildingIdMigration.IsCanonical("MonumentalFountain"));
        Assert.False(BuildingIdMigration.IsCanonical("NobleResidences"));

        var castle = CityCapitalLayoutConfig.GetPosition("castle");
        var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
        var wall = CityCapitalLayoutConfig.GetPosition("wall");
        Assert.Equal(0f, castle.X, 3);
        Assert.Equal(0f, castle.Z, 3);
        Assert.Equal(25.5f, tower.Z, 3);
        Assert.True(wall.Z < -16f);
        Assert.Equal(-3.2f, CityCapitalEnvironmentConfig.PlazaCenterZ, 3);
    }
}
