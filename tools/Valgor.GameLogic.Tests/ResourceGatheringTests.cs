using Valgor.City.Data;
using Valgor.Core.Modules;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Marches;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class ResourceGatheringTests
{
    private readonly FixedWorldMapClock _clock = new(new DateTime(2026, 7, 26, 18, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void WorldResourceNode_HasRequiredFields()
    {
        var node = (WorldResourceNode)WorldNodeCatalog.Get("forest-wood");
        Assert.Equal(ResourceType.Wood, node.ResourceType);
        Assert.Equal(800, node.MaxAmount);
        Assert.Equal(1, node.Level);
        Assert.Equal(200, node.GatherRatePerHour);
        Assert.True(node.RespawnDuration > TimeSpan.Zero);

        var instance = new WorldNodeInstance(node.Id, WorldNodeStatus.Available, node.MaxAmount);
        Assert.Equal(800, instance.RemainingAmount);
        Assert.Equal(ResourceNodeState.Available, instance.ResourceState);
        Assert.Null(instance.RespawnAt);
        Assert.Null(instance.OccupiedByMarchId);
    }

    [Fact]
    public void Gather_IsProportionalToTime_AndRespectsCapacity()
    {
        var session = CreateSession();
        var wallet = new ResourceWallet();
        session.BindWallet(wallet);
        // Capacidade pequena para parar antes de esgotar o nó
        session.Settings.DefaultMarchCapacity = 100;

        // Need remake session with capacity - settings are read at dispatch
        session = CreateSession(capacity: 100);
        session.BindWallet(wallet);

        ArriveAt("forest-wood", session);
        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.TryCollectSelected(wallet, out _, out _));

        _clock.UtcNow = _clock.UtcNow.AddHours(0.25); // 200/h * 0.25 = 50
        session.Tick();
        Assert.Equal(50, session.Marches.Active!.ResourceLoad);
        Assert.Equal(750, session.GetNode("forest-wood").RemainingAmount);

        _clock.UtcNow = _clock.UtcNow.AddHours(1); // would gather 200 but room only 50
        session.Tick();
        Assert.Equal(100, session.Marches.Active.ResourceLoad);
        Assert.Equal(700, session.GetNode("forest-wood").RemainingAmount);
        Assert.Equal(ResourceNodeState.Occupied, session.GetNode("forest-wood").ResourceState);
    }

    [Fact]
    public void Gather_StopsWhenNodeDepletes_AndRespawns()
    {
        var session = CreateSession(capacity: 10_000);
        ArriveAt("coast-gold", session);
        session.Selection.Select(session.GetNode("coast-gold"));
        Assert.True(session.TryCollectSelected(null, out _, out _));

        // coast-gold: 350 max, 120/h => ~2.92h
        _clock.UtcNow = _clock.UtcNow.AddHours(3);
        session.Tick();
        var node = session.GetNode("coast-gold");
        Assert.Equal(0, node.RemainingAmount);
        Assert.Equal(ResourceNodeState.Respawning, node.ResourceState);
        Assert.NotNull(node.RespawnAt);
        Assert.Equal(350, session.Marches.Active!.ResourceLoad);

        _clock.UtcNow = node.RespawnAt!.Value;
        session.Tick();
        Assert.Equal(350, node.RemainingAmount);
        Assert.Equal(ResourceNodeState.Available, node.ResourceState);
        Assert.Null(node.RespawnAt);
    }

    [Fact]
    public void Diamonds_CannotBeResourceNode()
    {
        Assert.Throws<ArgumentException>(() =>
            new WorldResourceNode(
                "x",
                "forest",
                "X",
                "x",
                WorldNodeStatus.Available,
                0,
                0,
                ResourceType.Diamonds,
                10,
                1,
                1,
                TimeSpan.FromHours(1)));
    }

    private WorldMapSession CreateSession(long capacity = 10_000)
    {
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.gather." + Guid.NewGuid().ToString("N"),
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0,
            DefaultMarchCapacity = capacity
        };
        return new WorldMapSession(settings, _clock, new ProvisionalHeroesGateway(), new LocalWorldMapRepository(settings.PersistenceKey));
    }

    private void ArriveAt(string nodeId, WorldMapSession session)
    {
        session.Selection.Select(session.GetNode(nodeId));
        Assert.True(session.TryDispatchToSelected(out _));
        _clock.UtcNow = session.Marches.Active!.ArrivalAt;
        session.Marches.Advance(_clock.UtcNow);
        Assert.Equal(MarchState.Arrived, session.Marches.Active.State);
    }
}
