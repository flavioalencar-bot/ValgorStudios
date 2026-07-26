using Valgor.City.Data;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.WorldMap.Camera;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Energy;
using Valgor.WorldMap.Filters;
using Valgor.WorldMap.Locate;
using Valgor.WorldMap.Marches;
using Valgor.WorldMap.Simulation;
using Valgor.WorldMap.Territory;
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

        var arrives = session.Marches.Active!.ArrivalAt;
        _clock.UtcNow = arrives.AddSeconds(-1);
        session.Marches.Advance(_clock.UtcNow);
        Assert.Equal(MarchState.Marching, session.Marches.Active.State);

        _clock.UtcNow = arrives;
        session.Marches.Advance(_clock.UtcNow);
        Assert.Equal(MarchState.Arrived, session.Marches.Active.State);
    }

    [Fact]
    public void Collect_AddsToWallet_AndDepletesNode()
    {
        var session = CreateSession();
        var wallet = new ResourceWallet();
        wallet.Add(ResourceType.Wood, 10);
        session.BindWallet(wallet);

        ArriveAt("forest-wood", session);
        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.TryCollectSelected(wallet, out _, out _));
        Assert.Equal(MarchState.Gathering, session.Marches.Active!.State);

        // 800 max / 200 per hour at level 1 => 4 hours to deplete
        _clock.UtcNow = _clock.UtcNow.AddHours(4);
        session.Tick();
        Assert.Equal(0, session.GetNode("forest-wood").RemainingAmount);
        Assert.Equal(ResourceNodeState.Respawning, session.GetNode("forest-wood").ResourceState);
        Assert.Equal(800, session.Marches.Active.ResourceLoad);

        Assert.True(session.TryReturnMarch(out _));
        _clock.UtcNow = session.Marches.Active!.ReturnAt!.Value;
        session.Tick();
        Assert.Equal(810, wallet.Get(ResourceType.Wood));
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
        Assert.Equal(MarchState.Returning, session.Marches.Active!.State);

        _clock.UtcNow = session.Marches.Active.ReturnAt!.Value;
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
            EnergyPersistenceKey = "valgor.worldmap.tests.persist.energy",
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };
        var repository = new LocalWorldMapRepository(settings.PersistenceKey);
        var energyRepo = new EnergyPersistenceRepository(settings.EnergyPersistenceKey);

        var first = new WorldMapSession(settings, _clock, heroes, repository, energyRepository: energyRepo);
        first.LoadOrInitialize();
        ArriveAt("coast-gold", first);
        first.Persist();

        Assert.Equal(MarchState.Arrived, first.Marches.Active!.State);

        var second = new WorldMapSession(settings, _clock, heroes, repository, energyRepository: energyRepo);
        second.LoadOrInitialize();
        Assert.Equal(MarchState.Arrived, second.Marches.Active!.State);
        Assert.Equal(first.Marches.Active.Id, second.Marches.Active.Id);
        Assert.Equal(first.Energy, second.Energy);
    }

    [Fact]
    public void Reconnection_DoesNotRewindMarch()
    {
        var session = CreateSession();
        ArriveAt("forest-wood", session);

        _clock.UtcNow = session.Marches.Active!.ArrivalAt.AddHours(-2);
        session.Marches.Advance(_clock.UtcNow);
        Assert.Equal(MarchState.Arrived, session.Marches.Active.State);
    }

    [Fact]
    public void Diamonds_HaveNoWorldResourceNode()
    {
        Assert.DoesNotContain(
            WorldNodeCatalog.All.Values.OfType<WorldResourceNode>(),
            node => node.Resource == ResourceType.Diamonds);
    }

    [Fact]
    public void CreatureDefinition_HasRequiredFields()
    {
        var creature = WorldCreatureCatalog.Get("forest-wolf");
        Assert.Equal("forest-wolf", creature.Id);
        Assert.Equal(WorldCreatureType.Beast, creature.Type);
        Assert.Equal(2, creature.Level);
        Assert.True(creature.RecommendedPower > 0);
        Assert.True(creature.EnergyCost > 0);
        Assert.NotEmpty(creature.Rewards.Entries);
        Assert.True(creature.RespawnDuration > TimeSpan.Zero);
        Assert.Equal("forest", creature.RegionId);
        Assert.Equal(-8f, creature.X);
        Assert.Equal(5f, creature.Z);
    }

    [Fact]
    public void DifficultyResolver_Bands_AreDeterministic()
    {
        Assert.Equal(CreatureDifficultyBand.Trivial, CreatureDifficultyResolver.Resolve(150, 100));
        Assert.Equal(CreatureDifficultyBand.Fair, CreatureDifficultyResolver.Resolve(100, 100));
        Assert.Equal(CreatureDifficultyBand.Impossible, CreatureDifficultyResolver.Resolve(50, 100));
        Assert.False(CreatureDifficultyResolver.CanDefeatProvisional(50, 100));
    }

    [Fact]
    public void Encounter_EngageResolve_GrantsRewards_AndRespawns()
    {
        var session = CreateSession();
        var wallet = new ResourceWallet();
        ArriveAt("forest-wolf", session);
        session.Selection.Select(session.GetNode("forest-wolf"));

        var energyBefore = session.Energy;
        Assert.True(session.TryEngageSelectedCreature(out _));
        Assert.Equal(WorldCreatureState.Engaged, session.Creatures["forest-wolf"].State);
        Assert.True(session.Energy < energyBefore);

        Assert.True(session.TryResolveSelectedCreature(wallet, out _, out var band));
        Assert.NotEqual(CreatureDifficultyBand.Impossible, band);
        Assert.True(wallet.Get(ResourceType.Food) > 0);
        Assert.Equal(WorldCreatureState.Respawning, session.Creatures["forest-wolf"].State);

        var respawnAt = session.Creatures["forest-wolf"].RespawnAtUtc!.Value;
        _clock.UtcNow = respawnAt;
        session.Tick();
        Assert.Equal(WorldCreatureState.Available, session.Creatures["forest-wolf"].State);
    }

    [Fact]
    public void Encounter_RequiresMarchAtCreature()
    {
        var session = CreateSession();
        session.Selection.Select(session.GetNode("forest-wolf"));
        Assert.False(session.TryEngageSelectedCreature(out var error));
        Assert.Contains("marcha", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LockedCreature_CannotEngage()
    {
        var session = CreateSession();
        session.Selection.Select(session.GetNode("desert-scorpion"));
        Assert.False(session.Encounters.CanEngage("desert-scorpion", session.Energy, out var error));
        Assert.Contains("bloqueada", error, StringComparison.OrdinalIgnoreCase);
        Assert.False(session.TryEngageSelectedCreature(out _));
    }

    private WorldMapSession CreateSession()
    {
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests." + Guid.NewGuid().ToString("N"),
            EnergyPersistenceKey = "valgor.worldmap.energy.tests." + Guid.NewGuid().ToString("N"),
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };
        return new WorldMapSession(
            settings,
            _clock,
            new ProvisionalHeroesGateway(),
            new LocalWorldMapRepository(settings.PersistenceKey));
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

public sealed class WorldMapEnergyTests
{
    private readonly FixedWorldMapClock _clock = new(new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void EnergyRegen_IsProportionalToElapsedTime()
    {
        var session = CreateSession(startingEnergy: 50, maxEnergy: 100, intervalSec: 60, regenAmount: 1);
        _clock.UtcNow = _clock.UtcNow.AddSeconds(180);
        session.EnergyRegen.ApplyUntil(_clock.UtcNow);
        Assert.Equal(53, session.Energy);
    }

    [Fact]
    public void EnergyRegen_ClampsAtMax()
    {
        var session = CreateSession(startingEnergy: 98, maxEnergy: 100, intervalSec: 60, regenAmount: 5);
        _clock.UtcNow = _clock.UtcNow.AddSeconds(120);
        session.EnergyRegen.ApplyUntil(_clock.UtcNow);
        Assert.Equal(100, session.Energy);
    }

    [Fact]
    public void EnergySpend_RejectsWhenInsufficient()
    {
        var session = CreateSession(startingEnergy: 5, maxEnergy: 100, intervalSec: 60, regenAmount: 1);
        Assert.False(session.EnergyWallet.TrySpend(8, out var error));
        Assert.Contains("insuficiente", error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(5, session.Energy);
    }

    [Fact]
    public void Reconnection_DoesNotDuplicateEnergyRegen()
    {
        var session = CreateSession(startingEnergy: 40, maxEnergy: 100, intervalSec: 60, regenAmount: 1);
        _clock.UtcNow = _clock.UtcNow.AddSeconds(120);
        session.EnergyRegen.ApplyUntil(_clock.UtcNow);
        Assert.Equal(42, session.Energy);

        session.EnergyRegen.ApplyUntil(_clock.UtcNow);
        Assert.Equal(42, session.Energy);
    }

    [Fact]
    public void EnergyPersistence_SurvivesReload_AndAppliesOfflineRegen()
    {
        var heroes = new ProvisionalHeroesGateway();
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests.energy." + Guid.NewGuid().ToString("N"),
            EnergyPersistenceKey = "valgor.worldmap.tests.energy.repo." + Guid.NewGuid().ToString("N"),
            StartingEnergy = 50,
            MaxEnergy = 100,
            EnergyRegenIntervalSec = 60,
            EnergyRegenAmount = 1,
            MarchTickIntervalSeconds = 0
        };
        var mapRepo = new LocalWorldMapRepository(settings.PersistenceKey);
        var energyRepo = new EnergyPersistenceRepository(settings.EnergyPersistenceKey);

        var first = new WorldMapSession(settings, _clock, heroes, mapRepo, energyRepository: energyRepo);
        first.LoadOrInitialize();
        Assert.True(first.EnergyWallet.TrySpend(20, out _));
        Assert.Equal(30, first.Energy);
        first.Persist();

        _clock.UtcNow = _clock.UtcNow.AddSeconds(180);
        var second = new WorldMapSession(settings, _clock, heroes, mapRepo, energyRepository: energyRepo);
        second.LoadOrInitialize();
        Assert.Equal(33, second.Energy);
    }

    [Fact]
    public void EnergyCostResolver_UsesCreatureDefinition()
    {
        var settings = new EnergySettings { MarchDispatchCost = 3 };
        var resolver = new EnergyCostResolver(settings);
        var creature = WorldCreatureCatalog.Get("forest-wolf");
        Assert.Equal(creature.EnergyCost, resolver.ResolveCreature(creature.Id));
        Assert.Equal(creature.EnergyCost, resolver.Resolve(EnergyActionKind.EngageCreature, creature.Id));
        Assert.Equal(3, resolver.ResolveMarchDispatch());
    }

    [Fact]
    public void MarchDispatch_SpendsConfiguredEnergyCost()
    {
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests.dispatch." + Guid.NewGuid().ToString("N"),
            EnergyPersistenceKey = "valgor.worldmap.tests.dispatch.energy." + Guid.NewGuid().ToString("N"),
            StartingEnergy = 20,
            MaxEnergy = 100,
            MarchDispatchEnergyCost = 5,
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };
        var session = new WorldMapSession(
            settings,
            _clock,
            new ProvisionalHeroesGateway(),
            new LocalWorldMapRepository(settings.PersistenceKey));
        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.TryDispatchToSelected(out _));
        Assert.Equal(15, session.Energy);
    }

    private WorldMapSession CreateSession(int startingEnergy, int maxEnergy, double intervalSec, int regenAmount)
    {
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests.energy." + Guid.NewGuid().ToString("N"),
            EnergyPersistenceKey = "valgor.worldmap.tests.energy.key." + Guid.NewGuid().ToString("N"),
            StartingEnergy = startingEnergy,
            MaxEnergy = maxEnergy,
            EnergyRegenIntervalSec = intervalSec,
            EnergyRegenAmount = regenAmount,
            MarchTickIntervalSeconds = 0
        };
        return new WorldMapSession(
            settings,
            _clock,
            new ProvisionalHeroesGateway(),
            new LocalWorldMapRepository(settings.PersistenceKey));
    }
}

public sealed class WorldMapFilterLocateTerritoryTests
{
    private readonly FixedWorldMapClock _clock = new(new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void Filters_HideKinds_WithoutDestroyingNodeState()
    {
        var session = CreateSession();
        var before = session.GetNode("forest-wood").RemainingAmount;
        Assert.True(session.IsNodeVisible("forest-wood"));

        session.Filters.SetShowResources(false);
        Assert.False(session.IsNodeVisible("forest-wood"));
        Assert.True(session.IsNodeVisible("forest-wolf"));
        Assert.Equal(before, session.GetNode("forest-wood").RemainingAmount);
        Assert.Equal(WorldNodeStatus.Available, session.GetNode("forest-wood").Status);
    }

    [Fact]
    public void Filters_CombineKindAndAvailability()
    {
        var session = CreateSession();
        var node = session.GetNode("forest-wood");
        node.Status = WorldNodeStatus.Occupied;
        node.OccupiedByMarchId = "march-1";

        session.Filters.SetShowOccupied(false);
        Assert.False(session.IsNodeVisible("forest-wood"));

        session.Filters.SetShowOccupied(true);
        session.Filters.SetShowAvailable(false);
        Assert.True(session.IsNodeVisible("forest-wood"));
        Assert.False(session.IsNodeVisible("home-city"));
    }

    [Fact]
    public void Filters_ClearRestoresDefaults()
    {
        var session = CreateSession();
        session.Filters.SetShowCreatures(false);
        session.Filters.SetShowDragons(false);
        session.Filters.ClearFilters();
        Assert.True(session.Filters.State.ShowCreatures);
        Assert.True(session.Filters.State.ShowDragons);
        Assert.True(session.IsNodeVisible("forest-wolf"));
    }

    [Fact]
    public void Filters_PersistAcrossReload()
    {
        var heroes = new ProvisionalHeroesGateway();
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests.filters." + Guid.NewGuid().ToString("N"),
            FilterPersistenceKey = "valgor.worldmap.tests.filters.repo." + Guid.NewGuid().ToString("N"),
            EnergyPersistenceKey = "valgor.worldmap.tests.filters.energy." + Guid.NewGuid().ToString("N"),
            MarchTickIntervalSeconds = 0
        };
        var mapRepo = new LocalWorldMapRepository(settings.PersistenceKey);
        var filterRepo = new WorldMapFilterPersistenceRepository(settings.FilterPersistenceKey);

        var first = new WorldMapSession(settings, _clock, heroes, mapRepo, filterRepository: filterRepo);
        first.LoadOrInitialize();
        first.Filters.SetShowLandmarks(false);
        first.Persist();

        var second = new WorldMapSession(settings, _clock, heroes, mapRepo, filterRepository: filterRepo);
        second.LoadOrInitialize();
        Assert.False(second.Filters.State.ShowLandmarks);
        Assert.False(second.IsNodeVisible("forest-stone"));
    }

    [Fact]
    public void Locator_FindsHomeMarchSelectedCreatureAndResource()
    {
        var session = CreateSession();
        Assert.True(session.Locator.TryLocatePlayerHome(out var home, out _));
        Assert.Equal(WorldMapLocationKind.PlayerHome, home.Kind);
        Assert.Equal(session.Settings.PlayerHomeNodeId, home.Id);

        Assert.False(session.Locator.TryLocateActiveMarch(out _, out var noMarch));
        Assert.Contains("marcha", noMarch, StringComparison.OrdinalIgnoreCase);

        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.Locator.TryLocateSelectedNode(out var selected, out _));
        Assert.Equal("forest-wood", selected.Id);

        Assert.True(session.Locator.TryLocateCreature("forest-wolf", out var creature, out _));
        Assert.Equal(WorldMapLocationKind.Creature, creature.Kind);
        Assert.True(session.Locator.TryLocateResource("forest-wood", out var resource, out _));
        Assert.Equal(WorldMapLocationKind.Resource, resource.Kind);

        var focus = session.Locator.CreateFocusRequest(home, zoomOverride: 11f);
        Assert.Equal(home.X, focus.X);
        Assert.Equal(home.Z, focus.Z);
        Assert.Equal(11f, focus.OrthographicSize);
    }

    [Fact]
    public void CameraBounds_ClampKeepsFocusInsideMap()
    {
        var bounds = new WorldMapBounds();
        var clamped = bounds.ClampPosition(new MapPosition(100f, 30f, -100f));
        Assert.Equal(bounds.MaxX, clamped.X);
        Assert.Equal(bounds.MinZ, clamped.Z);
        Assert.Equal(30f, clamped.Y);
    }

    [Fact]
    public void Territory_CatalogAndColors_CoverAllStates()
    {
        Assert.True(WorldTerritoryCatalog.All.Count >= 6);
        Assert.True(WorldTerritoryCatalog.TryGetByRegion("forest", out var forest));
        Assert.Equal(WorldTerritoryState.Owned, forest.DefaultState);

        foreach (WorldTerritoryState state in Enum.GetValues(typeof(WorldTerritoryState)))
        {
            var color = WorldTerritoryColorResolver.Resolve(state);
            Assert.InRange(color.A, 0.1f, 1f);
        }

        var session = CreateSession();
        Assert.True(session.TryGetTerritoryByRegion("portal", out var runtime));
        Assert.Equal(WorldTerritoryState.Contested, runtime.State);
    }

    private WorldMapSession CreateSession()
    {
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests.flt." + Guid.NewGuid().ToString("N"),
            FilterPersistenceKey = "valgor.worldmap.tests.flt.filters." + Guid.NewGuid().ToString("N"),
            EnergyPersistenceKey = "valgor.worldmap.tests.flt.energy." + Guid.NewGuid().ToString("N"),
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };
        return new WorldMapSession(
            settings,
            _clock,
            new ProvisionalHeroesGateway(),
            new LocalWorldMapRepository(settings.PersistenceKey));
    }
}

public sealed class WorldMapRestorePatchTests
{
    private readonly FixedWorldMapClock _clock = new(new DateTime(2026, 7, 26, 18, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void CameraPersistence_RestoresSavedPose_AndClamps()
    {
        var repo = new WorldCameraStateRepository("cam-test-" + Guid.NewGuid().ToString("N"));
        var service = new WorldCameraPersistenceService(repo, new WorldMapBounds(-22, 22, -18, 22));
        service.SavePose(100f, 30f, -90f, 16f);

        var restored = service.ResolveForRestore();
        Assert.True(restored.HasSavedPose);
        Assert.Equal(22f, restored.X);
        Assert.Equal(-18f, restored.Z);
        Assert.Equal(16f, restored.OrthographicSize);
    }

    [Fact]
    public void CameraPersistence_UsesDefaultWhenEmpty()
    {
        var repo = new WorldCameraStateRepository("cam-empty-" + Guid.NewGuid().ToString("N"));
        var service = new WorldCameraPersistenceService(repo, defaultZoom: 12f);
        var restored = service.ResolveForRestore();
        Assert.False(restored.HasSavedPose);
        Assert.Equal(12f, restored.OrthographicSize);
    }

    [Fact]
    public void Selection_RestoresById_AfterNodeReload()
    {
        var heroes = new ProvisionalHeroesGateway();
        var settings = NewSettings();
        var repo = new LocalWorldMapRepository(settings.PersistenceKey);
        var first = new WorldMapSession(settings, _clock, heroes, repo);
        first.LoadOrInitialize();
        first.Selection.Select(first.GetNode("forest-wood"));
        first.Persist();

        var second = new WorldMapSession(settings, _clock, heroes, repo);
        second.LoadOrInitialize();
        Assert.Equal("forest-wood", second.Selection.SelectedNodeId);
        Assert.Same(second.GetNode("forest-wood"), second.Selection.Selected);
    }

    [Fact]
    public void Selection_ClearsWhenNodeLocked()
    {
        var session = CreateSession();
        session.Selection.RestoreFromId("desert-stone", session.Nodes);
        Assert.Null(session.Selection.Selected);
        Assert.Null(session.Selection.SelectedNodeId);
    }

    [Fact]
    public void GlobalCoordinator_AdvancesMarch_WithoutWorldMapScene()
    {
        var session = CreateSession();
        var coordinator = new WorldSimulationCoordinator();
        coordinator.Bind(session);
        var tick = new GlobalMarchTickService(coordinator);

        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.TryDispatchToSelected(out _));
        var arrival = session.Marches.Active!.ArrivalAt;
        _clock.UtcNow = arrival;
        tick.Tick();
        Assert.Equal(MarchState.Arrived, session.Marches.Active.State);
    }

    [Fact]
    public void RewardDeposit_IsIdempotent_AndCommitsWallet()
    {
        var session = CreateSession();
        var wallet = new ResourceWallet();
        var walletPersists = 0;
        session.BindWallet(wallet, () => walletPersists++);

        session.Selection.Select(session.GetNode("forest-wood"));
        Assert.True(session.TryDispatchToSelected(out _));
        _clock.UtcNow = session.Marches.Active!.ArrivalAt;
        session.Tick();
        Assert.True(session.TryCollectSelected(wallet, out _, out _));
        _clock.UtcNow = _clock.UtcNow.AddHours(4);
        session.Tick();
        Assert.True(session.TryReturnMarch(out _));
        _clock.UtcNow = session.Marches.Active!.ReturnAt!.Value;
        session.Tick();

        var wood = wallet.Get(ResourceType.Wood);
        Assert.True(wood > 0);
        Assert.True(walletPersists >= 1);
        Assert.NotNull(session.Marches.LastCompleted);
        Assert.True(session.Marches.LastCompleted!.RewardsDelivered);
        Assert.True(session.Marches.LastCompleted.IsCommitted);
        Assert.False(string.IsNullOrEmpty(session.Marches.LastCompleted.RewardDeliveryId));

        var before = wood;
        var beforePersists = walletPersists;
        session.Tick();
        Assert.Equal(before, wallet.Get(ResourceType.Wood));
        Assert.Equal(beforePersists, walletPersists);
    }

    [Fact]
    public void RewardDeposit_SurvivesReload_WithoutDuplication()
    {
        var heroes = new ProvisionalHeroesGateway();
        var settings = NewSettings();
        var mapRepo = new LocalWorldMapRepository(settings.PersistenceKey);
        var wallet = new ResourceWallet();
        var first = new WorldMapSession(settings, _clock, heroes, mapRepo);
        first.BindWallet(wallet, () => { });
        first.LoadOrInitialize();
        first.Selection.Select(first.GetNode("forest-wood"));
        Assert.True(first.TryDispatchToSelected(out _));
        _clock.UtcNow = first.Marches.Active!.ArrivalAt;
        first.Tick();
        Assert.True(first.TryCollectSelected(wallet, out _, out _));
        _clock.UtcNow = _clock.UtcNow.AddHours(4);
        first.Tick();
        Assert.True(first.TryReturnMarch(out _));
        _clock.UtcNow = first.Marches.Active!.ReturnAt!.Value;
        first.Tick();
        var wood = wallet.Get(ResourceType.Wood);

        var second = new WorldMapSession(settings, _clock, heroes, mapRepo);
        second.BindWallet(wallet, () => { });
        second.LoadOrInitialize();
        Assert.Equal(wood, wallet.Get(ResourceType.Wood));
    }

    private WorldMapSession CreateSession()
    {
        var settings = NewSettings();
        return new WorldMapSession(
            settings,
            _clock,
            new ProvisionalHeroesGateway(),
            new LocalWorldMapRepository(settings.PersistenceKey));
    }

    private static WorldMapSettings NewSettings() =>
        new()
        {
            PersistenceKey = "valgor.worldmap.tests.restore." + Guid.NewGuid().ToString("N"),
            FilterPersistenceKey = "valgor.worldmap.tests.restore.filters." + Guid.NewGuid().ToString("N"),
            EnergyPersistenceKey = "valgor.worldmap.tests.restore.energy." + Guid.NewGuid().ToString("N"),
            CameraPersistenceKey = "valgor.worldmap.tests.restore.camera." + Guid.NewGuid().ToString("N"),
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };
}
