using System.Linq;
using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityCapitalLayoutTests
{
    [Fact]
    public void Blueprint_HasExactly15LogicalBuildings()
    {
        Assert.Equal(15, CityCapitalLayoutConfig.All.Count);
        Assert.Empty(CityCapitalLayoutConfig.ValidateBlueprintInvariants());
    }

    [Fact]
    public void Blueprint_Axis_GateCastleTowerAligned()
    {
        var gate = CityCapitalLayoutConfig.GetPosition("wall");
        var castle = CityCapitalLayoutConfig.GetPosition("castle");
        var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");

        Assert.Equal(0f, castle.X, 2);
        Assert.Equal(0f, castle.Z, 2);
        Assert.True(gate.Z < castle.Z);
        Assert.True(castle.Z < tower.Z);
        Assert.True(tower.Y > 0.5f);
        Assert.True(System.Math.Abs(gate.X) < 0.5f);
        Assert.True(System.Math.Abs(tower.X) < 1.5f);
    }

    [Fact]
    public void Blueprint_Districts_MatchOfficialRoles()
    {
        Assert.Equal(CityDistrict.Royal, CityCapitalLayoutConfig.GetDistrict("castle"));
        Assert.Equal(CityDistrict.Dragon, CityCapitalLayoutConfig.GetDistrict("dragon-tower"));
        Assert.Equal(CityDistrict.Sacred, CityCapitalLayoutConfig.GetDistrict("hospital"));
        Assert.Equal(CityDistrict.Sacred, CityCapitalLayoutConfig.GetDistrict("temple"));
        Assert.Equal(CityDistrict.Military, CityCapitalLayoutConfig.GetDistrict("arena"));
        Assert.Equal(CityDistrict.Economic, CityCapitalLayoutConfig.GetDistrict("market"));
        Assert.Equal(CityDistrict.Industrial, CityCapitalLayoutConfig.GetDistrict("mine"));
        Assert.Equal(CityDistrict.Rural, CityCapitalLayoutConfig.GetDistrict("farm"));
        Assert.Equal(CityDistrict.Access, CityCapitalLayoutConfig.GetDistrict("wall"));
    }

    [Fact]
    public void EnvironmentTier_MapsCastleLevels()
    {
        Assert.Equal(1, CityCapitalLayoutConfig.ResolveEnvironmentTier(1));
        Assert.Equal(1, CityCapitalLayoutConfig.ResolveEnvironmentTier(5));
        Assert.Equal(2, CityCapitalLayoutConfig.ResolveEnvironmentTier(6));
        Assert.Equal(3, CityCapitalLayoutConfig.ResolveEnvironmentTier(15));
        Assert.Equal(4, CityCapitalLayoutConfig.ResolveEnvironmentTier(16));
        Assert.Equal(5, CityCapitalLayoutConfig.ResolveEnvironmentTier(25));
        Assert.Equal(6, CityCapitalLayoutConfig.ResolveEnvironmentTier(30));
    }

    [Fact]
    public void Blueprint_NoDuplicateBuildingIds()
    {
        var ids = CityCapitalLayoutConfig.All.Keys.ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void MainGate_IsSouthOfWallRadius()
    {
        Assert.True(CityCapitalLayoutConfig.MainGateZ < 0f);
        Assert.True(System.Math.Abs(CityCapitalLayoutConfig.MainGateZ) >= CityCapitalLayoutConfig.WallRadius - 0.01f);
    }
}
