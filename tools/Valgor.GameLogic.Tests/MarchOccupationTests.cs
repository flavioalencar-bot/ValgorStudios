using Valgor.City.Data;
using Valgor.Core.Modules;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Marches;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class MarchOccupationTests
{
    private readonly FixedWorldMapClock _clock = new(new DateTime(2026, 7, 26, 15, 0, 0, DateTimeKind.Utc));
    private readonly MarchStateMachine _fsm = new();

    [Theory]
    [InlineData(MarchState.Preparing, MarchState.Marching)]
    [InlineData(MarchState.Preparing, MarchState.Cancelled)]
    [InlineData(MarchState.Marching, MarchState.Arrived)]
    [InlineData(MarchState.Marching, MarchState.Cancelled)]
    [InlineData(MarchState.Arrived, MarchState.Gathering)]
    [InlineData(MarchState.Arrived, MarchState.Returning)]
    [InlineData(MarchState.Arrived, MarchState.Cancelled)]
    [InlineData(MarchState.Gathering, MarchState.Returning)]
    [InlineData(MarchState.Gathering, MarchState.Cancelled)]
    [InlineData(MarchState.Returning, MarchState.Completed)]
    public void ValidTransitions_AreAllowed(MarchState from, MarchState to) =>
        Assert.True(_fsm.CanTransition(from, to));

    [Theory]
    [InlineData(MarchState.Preparing, MarchState.Arrived)]
    [InlineData(MarchState.Preparing, MarchState.Completed)]
    [InlineData(MarchState.Marching, MarchState.Gathering)]
    [InlineData(MarchState.Marching, MarchState.Completed)]
    [InlineData(MarchState.Arrived, MarchState.Marching)]
    [InlineData(MarchState.Returning, MarchState.Cancelled)]
    [InlineData(MarchState.Completed, MarchState.Marching)]
    [InlineData(MarchState.Cancelled, MarchState.Arrived)]
    public void InvalidTransitions_AreRejected(MarchState from, MarchState to) =>
        Assert.False(_fsm.CanTransition(from, to));

    [Fact]
    public void OccupiedResourceNode_RejectsSecondMarch()
    {
        var session = CreateSession();
        ArriveAt("forest-wood", session);
        Assert.Equal(session.Marches.Active!.MarchId, session.GetNode("forest-wood").OccupiedByMarchId);

        // Simula ocupação por outra marcha enquanto a ativa ainda está no nó.
        session.GetNode("coast-gold").OccupiedByMarchId = "foreign-march";
        session.Selection.Select(session.GetNode("coast-gold"));
        // Ainda há marcha ativa — rejeita. Libera a ativa via cancel para testar ocupação estrangeira.
        Assert.True(session.TryCancelMarch(out _));
        Assert.Null(session.GetNode("forest-wood").OccupiedByMarchId);

        session.Selection.Select(session.GetNode("coast-gold"));
        Assert.False(session.TryDispatchToSelected(out var error));
        Assert.Contains("ocupado", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Node_IsReleased_OnReturn()
    {
        var session = CreateSession();
        ArriveAt("forest-wood", session);
        Assert.NotNull(session.GetNode("forest-wood").OccupiedByMarchId);

        Assert.True(session.TryReturnMarch(out _));
        Assert.Null(session.GetNode("forest-wood").OccupiedByMarchId);
    }

    [Fact]
    public void Node_IsReleased_OnCancel()
    {
        var session = CreateSession();
        ArriveAt("mount-iron", session);
        Assert.NotNull(session.GetNode("mount-iron").OccupiedByMarchId);

        Assert.True(session.TryCancelMarch(out _));
        Assert.Null(session.GetNode("mount-iron").OccupiedByMarchId);
        Assert.Null(session.Marches.Active);
    }

    [Fact]
    public void SceneSwap_PreservesMarch_ViaRepository()
    {
        var heroes = new ProvisionalHeroesGateway();
        var settings = CreateSettings();
        var repository = new LocalWorldMapRepository(settings.PersistenceKey);
        var first = new WorldMapSession(settings, _clock, heroes, repository);
        first.LoadOrInitialize();
        ArriveAt("forest-wood", first);
        var marchId = first.Marches.Active!.MarchId;
        var arrival = first.Marches.Active.ArrivalAt;
        first.Persist();

        var second = new WorldMapSession(settings, _clock, heroes, repository);
        second.LoadOrInitialize();
        Assert.Equal(marchId, second.Marches.Active!.MarchId);
        Assert.Equal(arrival, second.Marches.Active.ArrivalAt);
        Assert.Equal(MarchState.Arrived, second.Marches.Active.State);
    }

    [Fact]
    public void Reconnection_PreservesTimestamps()
    {
        var session = CreateSession();
        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.TryDispatchToSelected(out _));
        var departure = session.Marches.Active!.DepartureAt;
        var arrival = session.Marches.Active.ArrivalAt;

        var repo = session.Marches.Repository;
        var snap = repo.Load();
        Assert.NotNull(snap);
        Assert.Equal(departure, snap!.March!.DepartureAt);
        Assert.Equal(arrival, snap.March.ArrivalAt);

        session.Marches.Restore(snap.March, snap.LastAdvanceUtc);
        Assert.Equal(departure, session.Marches.Active!.DepartureAt);
        Assert.Equal(arrival, session.Marches.Active.ArrivalAt);
    }

    [Fact]
    public void CompletedMarch_DoesNotDuplicateReward()
    {
        var session = CreateSession();
        var wallet = new ResourceWallet();
        ArriveAt("forest-wood", session);
        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.TryCollectSelected(wallet, out _, out var first));
        Assert.True(first > 0);
        Assert.True(session.Marches.Active!.RewardsDelivered);

        Assert.False(session.TryCollectSelected(wallet, out var error, out var second));
        Assert.Equal(0, second);
        Assert.False(string.IsNullOrEmpty(error));
        Assert.Equal(first, wallet.Get(ResourceType.Wood));
    }

    [Fact]
    public void TravelCalculation_IsTimestampBased_NotFps()
    {
        var settings = new WorldMapSettings { MarchSpeedUnitsPerHour = 10f };
        var travel = new MarchTravelCalculator(settings);
        var from = WorldNodeCatalog.Get("home-city");
        var to = WorldNodeCatalog.Get("forest-wood");
        var duration = travel.Calculate(from, to);
        var departure = _clock.UtcNow;
        var arrival = travel.EstimateArrival(departure, from, to);
        Assert.Equal(departure.Add(duration), arrival);
        // Mesmo cálculo independentemente de "frames".
        Assert.Equal(arrival, travel.EstimateArrival(departure, from, to));
    }

    [Fact]
    public void March_CannotOccupyTwoNodes()
    {
        var occupation = new WorldNodeOccupationService();
        var march = new MarchOrder(
            "m1",
            "p1",
            "home-city",
            "forest-wood",
            "team",
            _clock.UtcNow,
            _clock.UtcNow.AddHours(1),
            MarchState.Arrived,
            8f,
            1000,
            WorldNodeKind.Resource);
        var wood = new WorldNodeInstance("forest-wood", WorldNodeStatus.Available, 10);
        var gold = new WorldNodeInstance("coast-gold", WorldNodeStatus.Available, 10);

        Assert.True(occupation.TryOccupy(wood, march, out _));
        Assert.False(occupation.TryOccupy(gold, march, out var error));
        Assert.Contains("dois nós", error, StringComparison.OrdinalIgnoreCase);
        Assert.Null(gold.OccupiedByMarchId);
    }

    [Fact]
    public void Cancel_Rejected_WhenReturning()
    {
        var session = CreateSession();
        ArriveAt("forest-wood", session);
        Assert.True(session.TryReturnMarch(out _));
        Assert.False(session.TryCancelMarch(out var error));
        Assert.Contains("não permitido", error, StringComparison.OrdinalIgnoreCase);
    }

    private WorldMapSession CreateSession()
    {
        var settings = CreateSettings();
        return new WorldMapSession(settings, _clock, new ProvisionalHeroesGateway(), new LocalWorldMapRepository(settings.PersistenceKey));
    }

    private static WorldMapSettings CreateSettings() =>
        new()
        {
            PersistenceKey = "valgor.worldmap.march." + Guid.NewGuid().ToString("N"),
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };

    private void ArriveAt(string nodeId, WorldMapSession session)
    {
        session.Selection.Select(session.GetNode(nodeId));
        Assert.True(session.TryDispatchToSelected(out _));
        _clock.UtcNow = session.Marches.Active!.ArrivalAt;
        session.Marches.Advance(_clock.UtcNow);
        Assert.Equal(MarchState.Arrived, session.Marches.Active.State);
    }
}
