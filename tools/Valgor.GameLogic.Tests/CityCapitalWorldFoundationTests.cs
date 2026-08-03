using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

[Collection("CityCapitalLayoutRuntime")]
public sealed class CityCapitalWorldFoundationTests : IDisposable
{
    public CityCapitalWorldFoundationTests()
    {
        CityCapitalLayoutRuntime.ResetForTests();
        CityAmbientQualitySettings.Set(CityAmbientQuality.Medium);
    }

    public void Dispose()
    {
        CityCapitalLayoutRuntime.ResetForTests();
        CityAmbientQualitySettings.Set(CityAmbientQuality.Medium);
    }

    [Fact]
    public void Kit_HasAllRequiredAssets()
    {
        Assert.Equal(CityCapitalWorldConfig.KitId, CityTerrainAssetCatalog.KitId);
        Assert.Empty(CityTerrainAssetCatalog.ValidateKitComplete());
        Assert.True(CityTerrainAssetCatalog.Count >= 40);
    }

    [Fact]
    public void WorldConfig_ReservedAreas_CoverOfficialBuildings()
    {
        var areas = CityCapitalWorldConfig.ReservedBuildingAreas();
        Assert.True(areas.Count >= 17);
        Assert.Empty(CityCapitalWorldConfig.ValidateInvariants());
    }

    [Fact]
    public void WorldConfig_RiversDoNotCrossBuildingSlots_V1()
    {
        CityCapitalLayoutRuntime.UseOfficialV1();
        Assert.Empty(CityCapitalWorldConfig.ValidateInvariants());
    }

    [Fact]
    public void WorldConfig_RiversDoNotCrossBuildingSlots_V2Preview()
    {
        CityCapitalLayoutRuntime.UsePreviewV2();
        Assert.Empty(CityCapitalWorldConfig.ValidateInvariants());
        var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
        Assert.True(tower.X >= 8f);
    }

    [Fact]
    public void QualityBudgets_AreValid()
    {
        Assert.True(CityAmbientQualitySettings.IsValid(CityAmbientQuality.Low));
        Assert.True(CityAmbientQualitySettings.IsValid(CityAmbientQuality.Medium));
        Assert.True(CityAmbientQualitySettings.IsValid(CityAmbientQuality.High));
        Assert.True(CityCapitalWorldConfig.VegetationDensity(CityAmbientQuality.Low) <
                    CityCapitalWorldConfig.VegetationDensity(CityAmbientQuality.High));
        Assert.False(CityCapitalWorldConfig.AllowDistantMountainShadows(CityAmbientQuality.Medium));
        Assert.True(CityCapitalWorldConfig.AllowWaterParticles(CityAmbientQuality.Medium));
    }

    [Fact]
    public void AutoReject_RespectsTriangleBudget()
    {
        Assert.True(CityTerrainAssetCatalog.TryGet("Mountain_A", out var mountain));
        Assert.True(CityTerrainAssetCatalog.AutoReject(mountain, mountain.MaxTris * 2f, 1, out var reason));
        Assert.Contains("tris", reason, StringComparison.OrdinalIgnoreCase);
        Assert.False(CityTerrainAssetCatalog.AutoReject(mountain, mountain.MaxTris * 0.5f, 2, out _));
    }

    [Fact]
    public void FallbackStatus_IsDefaultUntilTripoImport()
    {
        Assert.True(CityTerrainAssetCatalog.TryGet("Cliff_DragonTower", out var cliff));
        Assert.Equal(CityTerrainAssetStatus.Fallback, cliff.Status);
        Assert.False(cliff.HasHighFidelity);
    }

    [Fact]
    public void HorizonAndWater_HavePlacements()
    {
        Assert.NotEmpty(CityCapitalWorldConfig.HorizonMountains());
        Assert.NotEmpty(CityCapitalWorldConfig.RiverSegments());
        Assert.NotEmpty(CityCapitalWorldConfig.WaterfallsAndBridges());
        Assert.NotEmpty(CityCapitalWorldConfig.NearCliffsAndPlateaus());
        Assert.NotEmpty(CityCapitalWorldConfig.RockScatter(CityAmbientQuality.Medium));
    }
}
