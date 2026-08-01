using System.Linq;
using Valgor.City.Buildings;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

/// <summary>
/// Seleção lógica dos 17 oficiais (raycast/collider exigem player Unity).
/// </summary>
public sealed class CitySelectionTests
{
    [Fact]
    public void OfficialIds_AreExactly17UniqueCanonical()
    {
        Assert.Equal(17, BuildingIdMigration.CanonicalIds.Length);
        Assert.Equal(17, BuildingIdMigration.CanonicalIds.Distinct().Count());
        foreach (var id in BuildingIdMigration.CanonicalIds)
        {
            Assert.True(BuildingIdMigration.IsCanonical(id));
            Assert.True(BuildingCatalog.TryGet(id, out _));
            Assert.True(CityCapitalLayoutConfig.TryGet(id, out _));
        }
    }

    [Fact]
    public void SelectionService_ResolvesSingleBuildingPerSelect()
    {
        var service = new BuildingSelectionService();
        var wall = new BuildingInstance("wall", 1, BuildingState.Ready);
        var farm = new BuildingInstance("farm", 1, BuildingState.Ready);

        service.Select(wall);
        Assert.Same(wall, service.Selected);
        Assert.Equal("wall", service.Selected!.DefinitionId);

        service.Select(farm);
        Assert.Same(farm, service.Selected);
        Assert.Equal("farm", service.Selected!.DefinitionId);
    }

    [Fact]
    public void Wall_IsSingleLogicalBuilding()
    {
        Assert.Equal(1, BuildingIdMigration.CanonicalIds.Count(id => id == "wall"));
        Assert.True(CityCapitalLayoutConfig.TryGet("wall", out var entry));
        Assert.Equal("wall", entry.BuildingId);
    }

    [Fact]
    public void TrainingGround_IsDistinctFromAcademy()
    {
        Assert.NotEqual("academy", BuildingIdMigration.ToCanonical("laboratory"));
        Assert.Equal("training-ground", BuildingIdMigration.ToCanonical("laboratory"));
        Assert.NotEqual(
            CityCapitalLayoutConfig.GetPosition("training-ground"),
            CityCapitalLayoutConfig.GetPosition("academy"));
    }

    [Fact]
    public void HeroHall_IsDistinctFromCastle()
    {
        Assert.Equal("hero-hall", BuildingIdMigration.ToCanonical("institute"));
        Assert.NotEqual("castle", BuildingIdMigration.ToCanonical("institute"));
        Assert.True(CityCapitalLayoutConfig.GetPosition("hero-hall").X < 0f);
        Assert.Equal(0f, CityCapitalLayoutConfig.GetPosition("castle").X, 2);
    }

    [Fact]
    public void ResidenceAndFoundry_AreOfficialNotDecorativeAliases()
    {
        Assert.True(BuildingIdMigration.IsCanonical("residence"));
        Assert.True(BuildingIdMigration.IsCanonical("foundry"));
        Assert.Equal("Residência", BuildingCatalog.Get("residence").DisplayName);
        Assert.Equal("Fundição", BuildingCatalog.Get("foundry").DisplayName);
        Assert.Equal(CityDistrict.Economic, CityCapitalLayoutConfig.GetDistrict("residence"));
        Assert.Equal(CityDistrict.Industrial, CityCapitalLayoutConfig.GetDistrict("foundry"));
    }

    [Fact]
    public void LegacyAliases_DoNotCreateDuplicateCatalogEntries()
    {
        Assert.Equal(BuildingCatalog.Get("institute").Id, BuildingCatalog.Get("hero-hall").Id);
        Assert.Equal(BuildingCatalog.Get("laboratory").Id, BuildingCatalog.Get("training-ground").Id);
        Assert.Equal(17, BuildingCatalog.All.Count);
    }
}
