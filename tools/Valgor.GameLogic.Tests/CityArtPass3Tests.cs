using Valgor.City.Buildings;
using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityArtPass3Tests
{
    [Fact]
    public void ArtPass3_Config_IsComplete()
    {
        Assert.True(CityArtPass3Config.Enabled);
        Assert.True(CityArtPass3Config.ClosedWallEnabled);
        Assert.True(CityArtPass3Config.VernacularHousingEnabled);
        Assert.Equal(28, CityArtPass3Config.WallSegments);
        Assert.Equal(4, CityArtPass3Config.CornerTowerCount);
        Assert.Equal(10, CityArtPass3Config.Stages.Length);
        Assert.Contains("muralha-fechada", CityArtPass3Config.Stages);
        Assert.Contains("wall-gate", CityArtPass3Config.HighFidelityAssets);
        Assert.Contains("habitacao-vernacular", CityArtPass3Config.RemainingBlockout);
        Assert.Equal(12, CityArtPass3Config.StaticNpcMarkerCount);
    }

    [Fact]
    public void ArtPass3_DoesNotAlterCanonicalBuildingIds()
    {
        foreach (var id in BuildingIdMigration.CanonicalIds)
        {
            Assert.True(BuildingIdMigration.IsCanonical(id));
        }

        Assert.False(BuildingIdMigration.IsCanonical("ArtPass3"));
        Assert.False(BuildingIdMigration.IsCanonical("VernacularHousing"));
        Assert.False(BuildingIdMigration.IsCanonical("GateWingL"));
        Assert.False(BuildingIdMigration.IsCanonical("CornerTower_0"));
        Assert.Equal(17, BuildingIdMigration.CanonicalIds.Length);
    }

    [Fact]
    public void ArtPass3_PreservesCriticalLayoutAnchors()
    {
        var castle = CityCapitalLayoutConfig.GetPosition("castle");
        var wall = CityCapitalLayoutConfig.GetPosition("wall");
        var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
        Assert.Equal(0f, castle.X, 3);
        Assert.Equal(0f, castle.Z, 3);
        Assert.Equal(0f, wall.X, 3);
        Assert.True(wall.Z < -16f);
        Assert.Equal(0f, tower.X, 3);
        Assert.Equal(25.5f, tower.Z, 3);
        Assert.Equal(20.5f, CityCapitalLayoutConfig.WallRadius, 3);
    }
}
