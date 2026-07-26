using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Data;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class DragonFoundationTests
{
    private sealed class FakeWallet : IDragonResourceWallet
    {
        public long Food { get; set; }
        public long Essence { get; set; }

        public long GetFood() => Food;
        public long GetDragonEssence() => Essence;

        public bool TrySpendFood(long amount)
        {
            if (Food < amount)
            {
                return false;
            }

            Food -= amount;
            return true;
        }

        public bool TrySpendDragonEssence(long amount)
        {
            if (Essence < amount)
            {
                return false;
            }

            Essence -= amount;
            return true;
        }
    }

    private sealed class MemoryDragonRepository : IDragonRepository
    {
        private DragonSnapshot? _snapshot;
        public DragonSnapshot? Load() => _snapshot;
        public void Save(DragonSnapshot snapshot) => _snapshot = snapshot;
    }

    private static DragonService CreateService(
        FakeWallet? wallet = null,
        DateTime? now = null,
        IDragonRepository? repository = null)
    {
        var clock = now ?? new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc);
        return DragonService.Create(
            wallet ?? new FakeWallet { Food = 1000, Essence = 100 },
            repository: repository ?? new MemoryDragonRepository(),
            utcNow: () => clock);
    }

    [Fact]
    public void Seed_StartsWithReadyAndLockedDragons()
    {
        var service = CreateService();
        Assert.Equal(1, service.GetReadyDragonCount());
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Ready, ember.State);
        Assert.True(service.TryGet("dragon-ash-1", out var ash));
        Assert.Equal(DragonState.Locked, ash.State);
        Assert.Equal(2, service.RoostOccupantCount);
    }

    [Fact]
    public void StateMachine_RejectsInvalidTransition()
    {
        var machine = new DragonStateMachine();
        var dragon = new DragonInstance("d1", "ember-whelp", DragonState.Locked, 0);
        Assert.False(machine.TryTransition(dragon, DragonState.Ready, out var error));
        Assert.Contains("inválida", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Feeding_SpendsResourcesAndCanRestoreReady()
    {
        var wallet = new FakeWallet { Food = 200, Essence = 10 };
        var repo = new MemoryDragonRepository();
        var now = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc);
        var settings = new DragonSettings();
        var service = new DragonService(settings, repo, () => now);
        service.BindWallet(wallet);
        service.LoadOrInitialize();

        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.State = DragonState.Hungry;
        ember.Hunger = 10;

        Assert.True(service.TryFeed(ember.InstanceId, out _));
        Assert.Equal(0, wallet.Food);
        Assert.Equal(0, wallet.Essence);
        Assert.True(ember.State is DragonState.Ready or DragonState.Resting);
    }

    [Fact]
    public void Hatch_StartsHatchingAndCompletesToResting()
    {
        var now = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc);
        var current = now;
        var settings = new DragonSettings { HatchDurationHours = 1 };
        var service = new DragonService(settings, new MemoryDragonRepository(), () => current);
        service.BindWallet(new FakeWallet { Food = 500, Essence = 50 });
        service.LoadOrInitialize();

        Assert.True(service.TryUnlockAndHatch("ash-drake", out _));
        Assert.True(service.TryGet("dragon-ash-1", out var ash));
        Assert.Equal(DragonState.Hatching, ash.State);

        current = now.AddHours(1.1);
        service.Tick();
        Assert.Equal(DragonState.Resting, ash.State);
    }

    [Fact]
    public void Deployment_DeployRecall_AndCombat()
    {
        var service = CreateService();
        Assert.True(service.TryDeployFirstReadyToMarch("march-1", out _));
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Flying, ember.State);
        Assert.Equal(80, service.GetProvisionalDragonPower());

        Assert.True(service.TryEnterCombatForMarch("march-1", out _));
        Assert.Equal(DragonState.Combat, ember.State);

        Assert.True(service.TryRecallFromMarch("march-1", out _));
        Assert.Equal(DragonState.Recovering, ember.State);
        Assert.Equal(0, service.GetReadyDragonCount());
    }

    [Fact]
    public void Repository_PersistsAcrossServiceInstances()
    {
        var repo = new MemoryDragonRepository();
        var first = CreateService(repository: repo);
        Assert.True(first.TryDeployFirstReadyToMarch("m1", out _));
        first.Persist();

        var second = DragonService.Create(
            new FakeWallet { Food = 100, Essence = 10 },
            repository: repo,
            utcNow: () => new DateTime(2026, 7, 26, 13, 0, 0, DateTimeKind.Utc));
        Assert.True(second.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Flying, ember.State);
        Assert.Equal("m1", ember.AssignedMarchId);
    }

    [Fact]
    public void CityWalletAdapter_SpendsCityResources()
    {
        var cityWallet = new ResourceWallet();
        cityWallet.Add(ResourceType.Food, 250);
        cityWallet.Add(ResourceType.DragonEssence, 20);
        var adapter = new CityDragonResourceWallet(cityWallet);

        Assert.True(adapter.TrySpendFood(200));
        Assert.True(adapter.TrySpendDragonEssence(10));
        Assert.Equal(50, cityWallet.Get(ResourceType.Food));
        Assert.Equal(10, cityWallet.Get(ResourceType.DragonEssence));
    }

    [Fact]
    public void WorldMap_Dispatch_AttachesReadyDragon()
    {
        var clock = new ManualWorldMapClock(new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc));
        var settings = new WorldMapSettings { MarchDispatchEnergyCost = 0 };
        var session = new WorldMapSession(
            settings,
            clock,
            new ProvisionalHeroesGateway(),
            new LocalWorldMapRepository("test.dragons.dispatch"));
        var dragons = CreateService();
        session.BindDragons(dragons);
        session.LoadOrInitialize();

        var target = session.Nodes.Values.First(n =>
            session.GetDefinition(n.DefinitionId) is WorldResourceNode &&
            n.Status != WorldNodeStatus.Locked);
        session.Selection.Select(target);

        Assert.True(session.TryDispatchToSelected(out var error), error);
        Assert.NotNull(session.Marches.Active);
        Assert.True(dragons.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Flying, ember.State);
        Assert.Equal(session.Marches.Active!.Id, ember.AssignedMarchId);
    }

    [Fact]
    public void WorldMap_Cancel_RecallsDragon()
    {
        var clock = new ManualWorldMapClock(new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc));
        var settings = new WorldMapSettings { MarchDispatchEnergyCost = 0 };
        var session = new WorldMapSession(
            settings,
            clock,
            new ProvisionalHeroesGateway(),
            new LocalWorldMapRepository("test.dragons.cancel"));
        var dragons = CreateService();
        session.BindDragons(dragons);
        session.LoadOrInitialize();

        var target = session.Nodes.Values.First(n =>
            session.GetDefinition(n.DefinitionId) is WorldResourceNode &&
            n.Status != WorldNodeStatus.Locked);
        session.Selection.Select(target);
        Assert.True(session.TryDispatchToSelected(out _));
        Assert.True(session.TryCancelMarch(out _));
        Assert.True(dragons.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Recovering, ember.State);
    }

    private sealed class ManualWorldMapClock : IWorldMapClock
    {
        public ManualWorldMapClock(DateTime utcNow) => UtcNow = utcNow;
        public DateTime UtcNow { get; set; }
    }
}
