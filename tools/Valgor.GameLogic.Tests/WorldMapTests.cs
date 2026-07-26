using Valgor.City.Data;
using Valgor.Core;
using Valgor.Core.Modules;
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

public sealed class WorldMapInteractionTests
{
    private readonly FixedWorldMapClock _clock = new(new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void NodeCatalog_ContainsAllNodeKinds()
    {
        Assert.Contains(WorldNodeCatalog.All.Values, n => n is WorldCityNode);
        Assert.Contains(WorldNodeCatalog.All.Values, n => n is WorldVillageNode);
        Assert.Contains(WorldNodeCatalog.All.Values, n => n is WorldResourceNode);
        Assert.Contains(WorldNodeCatalog.All.Values, n => n is WorldCreatureNode);
        Assert.Contains(WorldNodeCatalog.All.Values, n => n is WorldDragonNode);
        Assert.Contains(WorldNodeCatalog.All.Values, n => n is WorldLandmarkNode);
    }

    [Fact]
    public void TravelTime_IsProportionalToDistance()
    {
        var settings = new WorldMapSettings { MarchSpeedUnitsPerHour = 10f };
        var shortTrip = TravelTimeCalculator.Calculate(0, 0, 10, 0, settings);
        var longTrip = TravelTimeCalculator.Calculate(0, 0, 20, 0, settings);
        Assert.Equal(1.0, shortTrip.TotalHours, 5);
        Assert.Equal(2.0, longTrip.TotalHours, 5);
        Assert.True(longTrip > shortTrip);
    }

    [Fact]
    public void March_AdvancesByTimestamp_NotFrames()
    {
        var session = CreateSession();
        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.TryDispatchToSelected(out _));

        var arrives = session.Marches.Active!.ArrivesAtUtc;
        _clock.UtcNow = arrives.AddSeconds(-1);
        session.Marches.Advance(_clock.UtcNow);
        Assert.Equal(MarchPhase.TravelingOutbound, session.Marches.Active.Phase);

        _clock.UtcNow = arrives;
        session.Marches.Advance(_clock.UtcNow);
        Assert.Equal(MarchPhase.Arrived, session.Marches.Active.Phase);
    }

    [Fact]
    public void Collect_AddsToWallet_AndDepletesNode()
    {
        var session = CreateSession();
        var wallet = new ResourceWallet();
        wallet.Add(ResourceType.Wood, 10);

        ArriveAt("forest-wood", session);
        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.TryCollectSelected(wallet, out _, out var collected));
        Assert.Equal(800, collected);
        Assert.Equal(810, wallet.Get(ResourceType.Wood));
        Assert.Equal(0, session.GetNode("forest-wood").RemainingAmount);
        Assert.Equal(WorldNodeStatus.Depleted, session.GetNode("forest-wood").Status);
    }

    [Fact]
    public void Collect_OnlyWhenMarchArrivedAtResource()
    {
        var session = CreateSession();
        var wallet = new ResourceWallet();
        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.False(session.TryCollectSelected(wallet, out _, out _));
        Assert.Equal(0, wallet.Get(ResourceType.Wood));
    }

    [Fact]
    public void Wallet_NeverGoesNegative_OnCollectPath()
    {
        var wallet = new ResourceWallet();
        Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Add(ResourceType.Gold, -1));
        Assert.False(wallet.TrySpend(ResourceType.Gold, 1));
        Assert.Equal(0, wallet.Get(ResourceType.Gold));
    }

    [Fact]
    public void LockedNode_CannotDispatch()
    {
        var session = CreateSession();
        session.Selection.Select(session.GetNode("desert-stone"));
        Assert.False(session.TryDispatchToSelected(out var error));
        Assert.Contains("bloqueado", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReturnMarch_CompletesAtHome()
    {
        var session = CreateSession();
        ArriveAt("forest-wood", session);
        Assert.True(session.TryReturnMarch(out _));
        Assert.Equal(MarchPhase.Returning, session.Marches.Active!.Phase);

        _clock.UtcNow = session.Marches.Active.ArrivesAtUtc;
        session.Marches.Advance(_clock.UtcNow);
        Assert.Null(session.Marches.Active);
    }

    [Fact]
    public void Persistence_SurvivesReload_WithoutDuplicatingArrival()
    {
        var heroes = new ProvisionalHeroesGateway();
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests.persist",
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };
        var repository = new LocalWorldMapRepository(settings.PersistenceKey);

        var first = new WorldMapSession(settings, _clock, heroes, repository);
        first.LoadOrInitialize();
        ArriveAt("coast-gold", first);
        first.Persist();

        Assert.Equal(MarchPhase.Arrived, first.Marches.Active!.Phase);

        var second = new WorldMapSession(settings, _clock, heroes, repository);
        second.LoadOrInitialize();
        Assert.Equal(MarchPhase.Arrived, second.Marches.Active!.Phase);
        Assert.Equal(first.Marches.Active.Id, second.Marches.Active.Id);
    }

    [Fact]
    public void Reconnection_DoesNotRewindMarch()
    {
        var session = CreateSession();
        ArriveAt("forest-wood", session);

        _clock.UtcNow = session.Marches.Active!.ArrivesAtUtc.AddHours(-2);
        session.Marches.Advance(_clock.UtcNow);
        Assert.Equal(MarchPhase.Arrived, session.Marches.Active.Phase);
    }

    [Fact]
    public void Diamonds_HaveNoWorldResourceNode()
    {
        Assert.DoesNotContain(
            WorldNodeCatalog.All.Values.OfType<WorldResourceNode>(),
            node => node.Resource == ResourceType.Diamonds);
    }

    private WorldMapSession CreateSession()
    {
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests." + Guid.NewGuid().ToString("N"),
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };
        return new WorldMapSession(settings, _clock, new ProvisionalHeroesGateway(), new LocalWorldMapRepository(settings.PersistenceKey));
    }

    private void ArriveAt(string nodeId, WorldMapSession session)
    {
        session.Selection.Select(session.GetNode(nodeId));
        Assert.True(session.TryDispatchToSelected(out _));
        _clock.UtcNow = session.Marches.Active!.ArrivesAtUtc;
        session.Marches.Advance(_clock.UtcNow);
        Assert.Equal(MarchPhase.Arrived, session.Marches.Active.Phase);
    }
}
