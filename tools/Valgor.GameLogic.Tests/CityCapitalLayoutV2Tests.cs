using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

[Collection("CityCapitalLayoutRuntime")]
public sealed class CityCapitalLayoutV2Tests : IDisposable
{
    public CityCapitalLayoutV2Tests()
    {
        CityCapitalLayoutRuntime.ResetForTests();
    }

    public void Dispose() => CityCapitalLayoutRuntime.ResetForTests();

    [Fact]
    public void V1_RemainsDefault_AndPassesLegacyAxis()
    {
        Assert.Equal(CityCapitalLayoutRuntime.Version1, CityCapitalLayoutRuntime.ActiveVersion);
        Assert.False(CityCapitalLayoutRuntime.IsPreviewV2);
        Assert.Empty(CityCapitalLayoutConfig.ValidateBlueprintInvariants());
        var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
        Assert.Equal(0f, tower.X, 2);
    }

    [Fact]
    public void V2_Preview_PassesOrganicInvariants()
    {
        CityCapitalLayoutRuntime.UsePreviewV2();
        Assert.True(CityCapitalLayoutRuntime.IsPreviewV2);
        var issues = CityCapitalLayoutConfig.ValidateBlueprintInvariants();
        Assert.True(issues.Count == 0, string.Join("; ", issues));

        var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
        var academy = CityCapitalLayoutConfig.GetPosition("academy");
        var field = CityCapitalLayoutConfig.GetPosition("training-ground");
        var market = CityCapitalLayoutConfig.GetPosition("market");
        var arena = CityCapitalLayoutConfig.GetPosition("arena");
        var castle = CityCapitalLayoutConfig.GetPosition("castle");
        var wall = CityCapitalLayoutConfig.GetPosition("wall");

        Assert.True(tower.X >= 8f && tower.Z >= 12f);
        Assert.True(academy.X <= -8f);
        Assert.True(field.X <= -6f && field.Z <= -4f);
        Assert.True(Math.Abs(market.X) <= 4f && market.Z <= -6f);
        Assert.True(arena.X >= 8f);
        Assert.True(castle.Z >= 1.5f);
        Assert.True(wall.Z <= -16f);
    }

    [Fact]
    public void V2_HasExactlyOfficialBuildings()
    {
        Assert.Equal(17, CityCapitalLayoutV2.Entries.Count);
        Assert.Empty(CityCapitalLayoutV2.ValidateInvariants());
    }

    [Fact]
    public void Migration_IsBlocked_UntilPreviewApproved()
    {
        Assert.False(CityCapitalLayoutMigration.TryMigrate(out var detail));
        Assert.Contains("bloqueada", detail, StringComparison.OrdinalIgnoreCase);
    }
}

[CollectionDefinition("CityCapitalLayoutRuntime", DisableParallelization = true)]
public sealed class CityCapitalLayoutRuntimeCollection
{
}
