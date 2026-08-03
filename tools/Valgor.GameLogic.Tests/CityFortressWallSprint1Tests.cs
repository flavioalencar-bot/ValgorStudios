using Valgor.City.Buildings;
using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityFortressWallSprint1Tests
{
    [Fact]
    public void FortressWall_KitConfig_IsComplete()
    {
        Assert.True(CityFortressWallConfig.Enabled);
        Assert.Equal(12, CityFortressWallConfig.ModuleCount);
        Assert.Equal(CityFortressWallConfig.ModuleCount, CityFortressWallConfig.ModuleIds.Length);
        Assert.Contains("WallStraight4m", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallStraight8m", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallCorner45", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallCorner90", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallTowerSmall", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallTowerMedium", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallTowerLarge", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallGate", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallWalk", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallStairs", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallEnd", CityFortressWallConfig.ModuleIds);
        Assert.Contains("WallBastion", CityFortressWallConfig.ModuleIds);
        Assert.Equal(4, CityFortressWallConfig.LodLevels);
        Assert.Equal(32, CityFortressWallConfig.RingSegments);
        Assert.Equal(4, CityFortressWallConfig.TowerCycle.Length);
    }

    [Fact]
    public void FortressWall_PreservesSingleLogicalWallId()
    {
        Assert.True(BuildingIdMigration.IsCanonical("wall"));
        Assert.Equal(17, BuildingIdMigration.CanonicalIds.Length);
        Assert.False(BuildingIdMigration.IsCanonical("WallStraight4m"));
        Assert.False(BuildingIdMigration.IsCanonical("WallTowerLarge"));
        Assert.False(BuildingIdMigration.IsCanonical("WallBastion"));
        Assert.False(BuildingIdMigration.IsCanonical("CityFortifications"));
    }

    [Fact]
    public void FortressWall_PreservesCastleAndDragonAnchors()
    {
        var castle = CityCapitalLayoutConfig.GetPosition("castle");
        var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
        var wall = CityCapitalLayoutConfig.GetPosition("wall");
        Assert.Equal(0f, castle.X, 3);
        Assert.Equal(0f, castle.Z, 3);
        Assert.Equal(25.5f, tower.Z, 3);
        Assert.True(wall.Z < -16f);
        Assert.Equal(20.5f, CityCapitalLayoutConfig.WallRadius, 3);
    }
}
