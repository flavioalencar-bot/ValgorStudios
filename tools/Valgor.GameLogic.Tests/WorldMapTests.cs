using Valgor.Core;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Data;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class WorldMapFoundationTests
{
    [Fact]
    public void Catalog_ContainsExpectedRegions()
    {
        Assert.True(WorldMapCatalog.All.ContainsKey("forest"));
        Assert.Equal(RegionStatus.Locked, WorldMapCatalog.Get("portal").DefaultStatus);
    }

    [Fact]
    public void Selection_AndDeselection_Work()
    {
        var selection = new RegionSelectionService();
        var region = new RegionInstance("forest", RegionStatus.Available);
        RegionInstance? last = region;
        selection.SelectionChanged += value => last = value;

        selection.Select(region);
        Assert.Same(region, selection.Selected);
        selection.Deselect();
        Assert.Null(selection.Selected);
        Assert.Null(last);
    }

    [Fact]
    public void WorldMap_SceneId_IsStable()
    {
        Assert.Equal("WorldMap", SceneIds.WorldMap);
    }
}
