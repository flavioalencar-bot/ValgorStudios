using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityArtAssetCatalogTests
{
    [Fact]
    public void Pipeline_IsEnabled_AndPackIdIsCoreV1()
    {
        Assert.True(CityArtPipelineConfig.Enabled);
        Assert.Equal("CapitalArtPack_Core_v1", CityArtPipelineConfig.PackCoreId);
        Assert.Equal(4, CityArtPipelineConfig.LodLevels);
        Assert.Equal("ArtHF", CityArtPipelineConfig.HighFidelityChildName);
    }

    [Fact]
    public void DefaultBuildingSlots_CoverAll17OfficialIds()
    {
        var slots = CityArtAssetCatalogLogic.CreateDefaultBuildingSlots();
        Assert.Equal(17, slots.Count);
        Assert.Contains(slots, s => s.BuildingId == "castle" && s.UsesExistingCastleLoader);
        Assert.Contains(slots, s => s.BuildingId == "dragon-tower");
        Assert.All(slots, s =>
        {
            Assert.True(s.SlotReady);
            Assert.False(s.ContentAvailable);
            Assert.StartsWith("building.", s.AssetId);
        });
    }

    [Fact]
    public void CorePackSlots_MatchManifestMinimum()
    {
        var slots = CityArtAssetCatalogLogic.CreateCorePackSlots();
        Assert.Equal(CityArtPipelineConfig.CorePackAssetIds.Length, slots.Count);
        Assert.Contains(slots, s => s.AssetId == "architecture.castle");
        Assert.Contains(slots, s => s.AssetId == "materials.water");
        Assert.Contains(slots, s => s.AssetId == "environment.mountain-distant");
        Assert.All(slots, s => Assert.False(s.ContentAvailable));
    }

    [Fact]
    public void NamingConvention_RequiresValCapitalPrefix()
    {
        Assert.True(CityArtPipelineConfig.MatchesNamingConvention("VAL_Capital_Architecture_Castle_Tier06"));
        Assert.False(CityArtPipelineConfig.MatchesNamingConvention("Castle_Proxy"));
        Assert.Equal(
            "VAL_Capital_Architecture_Castle_Tier06",
            CityArtPipelineConfig.PrefabName("Architecture", "Castle", "Tier06"));
    }
}

public sealed class CityArtFallbackTests
{
    [Fact]
    public void MissingContent_ResolvesToProceduralFallback()
    {
        var slots = CityArtAssetCatalogLogic.CreateDefaultBuildingSlots();
        var farm = CityArtAssetCatalogLogic.FindByBuildingId(slots, "farm");
        Assert.NotNull(farm);
        var result = CityArtAssetCatalogLogic.Resolve(
            farm, "farm", highLoaded: false, mediumLoaded: false, lowLoaded: false, qualityRank: 1);
        Assert.Equal(CityArtResolveMode.ProceduralFallback, result.Mode);
        Assert.True(result.UsedFallback);
    }

    [Fact]
    public void AvailableHigh_PreferredOnHighQuality()
    {
        var slots = CityArtAssetCatalogLogic.CreateDefaultBuildingSlots();
        var farm = CityArtAssetCatalogLogic.FindByBuildingId(slots, "farm");
        var result = CityArtAssetCatalogLogic.Resolve(
            farm, "farm", highLoaded: true, mediumLoaded: true, lowLoaded: true, qualityRank: 2);
        Assert.Equal(CityArtResolveMode.HighFidelity, result.Mode);
        Assert.False(result.UsedFallback);
    }

    [Fact]
    public void Castle_UsesExistingLoaderMode()
    {
        var slots = CityArtAssetCatalogLogic.CreateDefaultBuildingSlots();
        var castle = CityArtAssetCatalogLogic.FindByBuildingId(slots, "castle");
        var result = CityArtAssetCatalogLogic.Resolve(
            castle, "castle", highLoaded: false, mediumLoaded: false, lowLoaded: false, qualityRank: 2);
        Assert.Equal(CityArtResolveMode.ExistingCastleLoader, result.Mode);
        Assert.False(result.UsedFallback);
    }
}

public sealed class CityArtReplacementTests
{
    [Fact]
    public void Resolve_DoesNotChangeBuildingId()
    {
        var slots = CityArtAssetCatalogLogic.CreateDefaultBuildingSlots();
        var tower = CityArtAssetCatalogLogic.FindByBuildingId(slots, "dragon-tower");
        var result = CityArtAssetCatalogLogic.Resolve(
            tower, "dragon-tower", highLoaded: true, mediumLoaded: false, lowLoaded: false, qualityRank: 1);
        Assert.Equal("dragon-tower", result.BuildingId);
        Assert.Equal(CityArtPipelineConfig.AssetIdForBuilding("dragon-tower"), result.AssetId);
    }

    [Fact]
    public void MediumQuality_UsesMediumWhenHighMissing()
    {
        var slots = CityArtAssetCatalogLogic.CreateDefaultBuildingSlots();
        var market = CityArtAssetCatalogLogic.FindByBuildingId(slots, "market");
        var result = CityArtAssetCatalogLogic.Resolve(
            market, "market", highLoaded: false, mediumLoaded: true, lowLoaded: true, qualityRank: 1);
        Assert.Equal(CityArtResolveMode.Medium, result.Mode);
        Assert.False(result.UsedFallback);
    }
}

public sealed class CityArtPrefabValidationTests
{
    [Fact]
    public void ForbiddenScripts_IncludeGameplayTypes()
    {
        Assert.True(CityArtPipelineConfig.IsForbiddenScript("BuildingView"));
        Assert.True(CityArtPipelineConfig.IsForbiddenScript("CityController"));
        Assert.False(CityArtPipelineConfig.IsForbiddenScript("LODGroup"));
    }

    [Fact]
    public void DescriptorValidation_RequiresAssetId()
    {
        var issues = CityArtPipelineConfig.ValidateDescriptorRules(new CityArtAssetDescriptor());
        Assert.Contains(issues, i => i.Contains("AssetId"));
    }

    [Fact]
    public void DescriptorValidation_AcceptsReadySlot()
    {
        var d = new CityArtAssetDescriptor { AssetId = "building.farm", SlotReady = true };
        Assert.Empty(CityArtPipelineConfig.ValidateDescriptorRules(d));
    }
}
