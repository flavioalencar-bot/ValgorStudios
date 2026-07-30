using System;
using System.Linq;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;
using Valgor.Dragons.Mount;
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
        public long Diamonds { get; set; } = 10_000;

        public long GetFood() => Food;
        public long GetDragonEssence() => Essence;
        public long GetDiamonds() => Diamonds;

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

        public bool TrySpendDiamonds(long amount)
        {
            if (Diamonds < amount)
            {
                return false;
            }

            Diamonds -= amount;
            return true;
        }
    }

    private sealed class MemoryDragonRepository : IDragonRepository
    {
        private DragonSnapshot? _snapshot;
        public DragonSnapshot? Load() => _snapshot;
        public void Save(DragonSnapshot snapshot) => _snapshot = snapshot;
    }

    private sealed class MutableClock
    {
        public DateTime UtcNow { get; set; }
    }

    private static DragonService CreateService(
        FakeWallet? wallet = null,
        MutableClock? clock = null,
        IDragonRepository? repository = null,
        DragonSettings? settings = null)
    {
        clock ??= new MutableClock
        {
            UtcNow = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc)
        };
        var resolved = settings ?? new DragonSettings();
        var service = new DragonService(resolved, repository ?? new MemoryDragonRepository(), () => clock.UtcNow);
        service.BindWallet(wallet ?? new FakeWallet { Food = 5000, Essence = 200 });
        service.LoadOrInitialize();
        return service;
    }

    /// <summary>Avança a jornada Fase 1 até Juvenile Nv.1 (e opcionalmente Ready).</summary>
    private static DragonService CreateBornService(
        out MutableClock clock,
        FakeWallet? wallet = null,
        bool advanceToReady = false,
        IDragonRepository? repository = null)
    {
        clock = new MutableClock
        {
            UtcNow = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc)
        };
        var settings = new DragonSettings
        {
            HatchDurationHours = 1,
            JuvenileDurationHours = 1,
            RestDurationHours = 1,
            CareRequiredForHatch = 2,
            CareFoodCost = 50,
            CareExtendsHatchHours = 0
        };
        var service = CreateService(wallet ?? new FakeWallet { Food = 5000, Essence = 200 }, clock, repository, settings);
        Assert.True(service.TryAcceptEggMission(out var err) == false);
        service.SyncCastleLevel(20);
        Assert.Equal(DragonEggJourneyPhase.Unlocked, service.EggJourneyPhase);
        Assert.True(service.TryAcceptEggMission(out err), err);
        Assert.True(service.TryConquerEgg(out err), err);
        Assert.True(service.TryBeginIncubation(out err), err);
        Assert.True(service.TryCareIncubation(out err), err);
        Assert.True(service.TryCareIncubation(out err), err);

        clock.UtcNow = clock.UtcNow.AddHours(1.1);
        service.Tick();
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Juvenile, ember.State);
        Assert.Equal(1, ember.DragonLevel);
        Assert.Equal(DragonEggJourneyPhase.Born, service.EggJourneyPhase);

        if (advanceToReady)
        {
            ember.Hunger = 100;
            clock.UtcNow = clock.UtcNow.AddHours(1.1);
            service.Tick();
            Assert.Equal(DragonState.Resting, ember.State);
            clock.UtcNow = clock.UtcNow.AddHours(1.1);
            service.Tick();
            Assert.Equal(DragonState.Ready, ember.State);
            ember.GrowthStage = DragonGrowthStage.Adult;
        }

        return service;
    }

    [Fact]
    public void Seed_StartsLockedEgg_Phase1()
    {
        var service = CreateService();
        Assert.Equal(0, service.GetReadyDragonCount());
        Assert.Equal(DragonEggJourneyPhase.Locked, service.EggJourneyPhase);
        Assert.False(service.IsDragonContentUnlocked);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Locked, ember.State);
        Assert.Equal(0, ember.DragonLevel);
        Assert.Equal(1, service.RoostOccupantCount);
        Assert.False(service.TryGet("dragon-ash-1", out _));
    }

    [Fact]
    public void Castle20_UnlocksEggContent()
    {
        var service = CreateService();
        service.SyncCastleLevel(19);
        Assert.Equal(DragonEggJourneyPhase.Locked, service.EggJourneyPhase);
        service.SyncCastleLevel(20);
        Assert.Equal(DragonEggJourneyPhase.Unlocked, service.EggJourneyPhase);
        Assert.True(service.IsDragonContentUnlocked);
        Assert.Contains("missão", service.DescribeEggJourney(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EggJourney_MissionConquerIncubateCare_BirthsLevel1()
    {
        CreateBornService(out _, advanceToReady: false);
    }

    [Fact]
    public void Hatch_RequiresCare_DoesNotBirthWithoutIt()
    {
        var clock = new MutableClock
        {
            UtcNow = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc)
        };
        var settings = new DragonSettings
        {
            HatchDurationHours = 1,
            CareRequiredForHatch = 3,
            CareFoodCost = 10
        };
        var service = CreateService(new FakeWallet { Food = 1000, Essence = 50 }, clock, settings: settings);
        service.SyncCastleLevel(20);
        Assert.True(service.TryAcceptEggMission(out _));
        Assert.True(service.TryConquerEgg(out _));
        Assert.True(service.TryBeginIncubation(out _));
        Assert.True(service.TryCareIncubation(out _)); // 1/3

        clock.UtcNow = clock.UtcNow.AddHours(2);
        service.Tick();
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Hatching, ember.State);

        Assert.True(service.TryCareIncubation(out _));
        Assert.True(service.TryCareIncubation(out _));
        clock.UtcNow = clock.UtcNow.AddHours(0.1);
        service.Tick();
        Assert.Equal(DragonState.Juvenile, ember.State);
        Assert.Equal(1, ember.DragonLevel);
    }

    [Fact]
    public void GrowthStages_AreAllRepresented()
    {
        var values = Enum.GetValues<DragonGrowthStage>();
        Assert.Contains(DragonGrowthStage.Egg, values);
        Assert.Contains(DragonGrowthStage.Hatchling, values);
        Assert.Contains(DragonGrowthStage.Juvenile, values);
        Assert.Contains(DragonGrowthStage.Adult, values);
        Assert.Contains(DragonGrowthStage.Elder, values);
        Assert.Contains(DragonGrowthStage.Ancient, values);
        Assert.Contains(DragonGrowthStage.Adolescent, values);
        Assert.Contains(DragonGrowthStage.YoungAdult, values);
        Assert.Equal(8, values.Length);
    }

    [Fact]
    public void Hatch_SetsHatchlingGrowthStage()
    {
        var service = CreateBornService(out _);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonGrowthStage.Hatchling, ember.GrowthStage);
    }

    [Fact]
    public void Feed_IncreasesBondAndGrowth()
    {
        var service = CreateBornService(out _, advanceToReady: true);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        var bondBefore = ember.BondPoints;
        var growthBefore = ember.GrowthPoints;
        Assert.True(service.TryFeed(ember.InstanceId, out var error), error);
        Assert.True(ember.BondPoints > bondBefore || ember.BondLevel > 0);
        Assert.True(ember.GrowthPoints > growthBefore || ember.GrowthStage >= DragonGrowthStage.Adult);
    }

    [Fact]
    public void Growth_AdvancesAdultToElder()
    {
        var clock = new MutableClock
        {
            UtcNow = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc)
        };
        var settings = new DragonSettings
        {
            HatchDurationHours = 1,
            JuvenileDurationHours = 1,
            RestDurationHours = 1,
            CareRequiredForHatch = 1,
            CareFoodCost = 10,
            AdultToElderPoints = 10,
            GrowthPointsPerFeed = 10
        };
        var wallet = new FakeWallet { Food = 5000, Essence = 200 };
        var service = CreateService(wallet, clock, settings: settings);
        service.SyncCastleLevel(20);
        Assert.True(service.TryAcceptEggMission(out _));
        Assert.True(service.TryConquerEgg(out _));
        Assert.True(service.TryBeginIncubation(out _));
        Assert.True(service.TryCareIncubation(out _));
        clock.UtcNow = clock.UtcNow.AddHours(1.1);
        service.Tick();
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.Hunger = 100;
        clock.UtcNow = clock.UtcNow.AddHours(1.1);
        service.Tick();
        clock.UtcNow = clock.UtcNow.AddHours(1.1);
        service.Tick();
        ember.GrowthStage = DragonGrowthStage.Adult;
        ember.GrowthPoints = 0;
        Assert.True(service.TryFeed(ember.InstanceId, out var error), error);
        Assert.Equal(DragonGrowthStage.Elder, ember.GrowthStage);
    }

    [Fact]
    public void Evolution_RequiresAdultAndBond()
    {
        var service = CreateBornService(out _, advanceToReady: true);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.BondLevel = 0;
        ember.BondPoints = 0;
        ember.GrowthStage = DragonGrowthStage.Adult;
        Assert.False(service.TryEvolve(ember.InstanceId, out _));
        ember.BondLevel = 2;
        Assert.True(service.TryEvolve(ember.InstanceId, out var error), error);
        Assert.Equal("ash-drake", ember.DefinitionId);
    }

    [Fact]
    public void OfficialStates_AreAllRepresented()
    {
        var values = Enum.GetValues<DragonState>();
        Assert.Contains(DragonState.Locked, values);
        Assert.Contains(DragonState.Egg, values);
        Assert.Contains(DragonState.Hatching, values);
        Assert.Contains(DragonState.Juvenile, values);
        Assert.Contains(DragonState.Ready, values);
        Assert.Contains(DragonState.Deployed, values);
        Assert.Contains(DragonState.Hungry, values);
        Assert.Contains(DragonState.Exhausted, values);
        Assert.Contains(DragonState.Injured, values);
        Assert.Contains(DragonState.Recovering, values);
        Assert.Contains(DragonState.Resting, values);
        Assert.Equal(11, values.Length);
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
        var service = CreateBornService(out _, wallet, advanceToReady: true);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.State = DragonState.Hungry;
        ember.Hunger = 10;
        wallet.Food = 200;
        wallet.Essence = 10;

        Assert.True(service.TryFeed(ember.InstanceId, out _));
        Assert.Equal(0, wallet.Food);
        Assert.Equal(0, wallet.Essence);
        Assert.True(ember.State is DragonState.Ready or DragonState.Resting);
    }

    [Fact]
    public void Hunger_DecaysReadyIntoHungry()
    {
        var clock = new MutableClock
        {
            UtcNow = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc)
        };
        var settings = new DragonSettings
        {
            HungerIntervalHours = 1,
            HungerDecayPerTick = 30,
            HungryThresholdRatio = 0.25,
            HatchDurationHours = 1,
            JuvenileDurationHours = 1,
            RestDurationHours = 1,
            CareRequiredForHatch = 1,
            CareFoodCost = 10
        };
        var service = CreateService(new FakeWallet { Food = 1000, Essence = 50 }, clock, settings: settings);
        service.SyncCastleLevel(20);
        Assert.True(service.TryAcceptEggMission(out _));
        Assert.True(service.TryConquerEgg(out _));
        Assert.True(service.TryBeginIncubation(out _));
        Assert.True(service.TryCareIncubation(out _));
        clock.UtcNow = clock.UtcNow.AddHours(1.1);
        service.Tick();
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.Hunger = 100;
        clock.UtcNow = clock.UtcNow.AddHours(1.1);
        service.Tick();
        clock.UtcNow = clock.UtcNow.AddHours(1.1);
        service.Tick();
        Assert.Equal(DragonState.Ready, ember.State);

        ember.Hunger = 40;
        ember.LastUpdatedUtc = clock.UtcNow;
        clock.UtcNow = clock.UtcNow.AddHours(2.1);
        service.Tick();
        Assert.Equal(DragonState.Hungry, ember.State);
        Assert.True(ember.Hunger <= 25);
    }

    [Fact]
    public void Rest_CompletesToReadyWhenHungerSufficient()
    {
        var service = CreateBornService(out var clock, advanceToReady: true);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        Assert.True(service.Recovery.TryBeginRest(ember, clock.UtcNow, out _));
        Assert.Equal(DragonState.Resting, ember.State);
        ember.Hunger = 100;
        clock.UtcNow = clock.UtcNow.AddHours(1.1);
        service.Tick();
        Assert.Equal(DragonState.Ready, ember.State);
    }

    [Fact]
    public void Recovery_ExhaustedToResting()
    {
        var service = CreateBornService(out var clock, advanceToReady: true);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.State = DragonState.Exhausted;
        Assert.True(service.TryStartRecovery(ember.InstanceId, out _));
        Assert.Equal(DragonState.Recovering, ember.State);
        clock.UtcNow = clock.UtcNow.AddHours(2.1);
        service.Tick();
        Assert.Equal(DragonState.Resting, ember.State);
    }

    [Fact]
    public void Deployment_DeployRecall_AndCombatStayDeployed()
    {
        var service = CreateBornService(out _, advanceToReady: true);
        Assert.True(service.TryDeployFirstReadyToMarch("march-1", out var error), error);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Deployed, ember.State);
        Assert.True(service.GetProvisionalDragonPower() > 0);

        Assert.True(service.TryEnterCombatForMarch("march-1", out _));
        Assert.Equal(DragonState.Deployed, ember.State);

        Assert.True(service.TryRecallFromMarch("march-1", out _));
        Assert.Equal(DragonState.Recovering, ember.State);
        Assert.Equal(0, service.GetReadyDragonCount());
    }

    [Fact]
    public void Repository_PersistsJourneyAcrossServiceInstances()
    {
        var repo = new MemoryDragonRepository();
        var first = CreateBornService(out _, repository: repo, advanceToReady: true);
        Assert.True(first.TryDeployFirstReadyToMarch("m1", out _));
        first.Persist();

        var clock = new MutableClock
        {
            UtcNow = new DateTime(2026, 7, 26, 18, 0, 0, DateTimeKind.Utc)
        };
        var second = new DragonService(new DragonSettings(), repo, () => clock.UtcNow);
        second.BindWallet(new FakeWallet { Food = 100, Essence = 10 });
        second.LoadOrInitialize();
        Assert.Equal(DragonEggJourneyPhase.Born, second.EggJourneyPhase);
        Assert.True(second.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Deployed, ember.State);
        Assert.Equal("m1", ember.AssignedMarchId);
        Assert.Equal(1, ember.DragonLevel);
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
        var dragons = CreateBornService(out _, advanceToReady: true);
        session.BindDragons(dragons);
        session.LoadOrInitialize();

        var target = session.Nodes.Values.First(n =>
            session.GetDefinition(n.DefinitionId) is WorldResourceNode &&
            n.Status != WorldNodeStatus.Locked);
        session.Selection.Select(target);

        Assert.True(session.TryDispatchToSelected(out var error), error);
        Assert.NotNull(session.Marches.Active);
        Assert.True(dragons.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonState.Deployed, ember.State);
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
        var dragons = CreateBornService(out _, advanceToReady: true);
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

    [Fact]
    public void Caps_CastleAndTowerLimitMaxLevel()
    {
        Assert.Equal(20, DragonProgressionRules.CapFromCastle(20));
        Assert.Equal(30, DragonProgressionRules.CapFromCastle(35));
        Assert.Equal(5, DragonProgressionRules.CapFromTower(1));
        Assert.Equal(30, DragonProgressionRules.CapFromTower(15));
        Assert.Equal(5, DragonProgressionRules.EffectiveMaxLevel(20, 1));
        Assert.Equal(20, DragonProgressionRules.EffectiveMaxLevel(20, 10));
        Assert.Equal(30, DragonProgressionRules.EffectiveMaxLevel(30, 15));
    }

    [Fact]
    public void RitualTargets_AreTierBreakpoints()
    {
        Assert.True(DragonProgressionRules.IsRitualTarget(6));
        Assert.True(DragonProgressionRules.IsRitualTarget(11));
        Assert.True(DragonProgressionRules.IsRitualTarget(16));
        Assert.True(DragonProgressionRules.IsRitualTarget(21));
        Assert.True(DragonProgressionRules.IsRitualTarget(26));
        Assert.False(DragonProgressionRules.IsRitualTarget(7));
        Assert.Equal(DragonGrowthStage.Juvenile, DragonProgressionRules.StageForLevel(6));
        Assert.Equal(DragonGrowthStage.Adolescent, DragonProgressionRules.StageForLevel(11));
        Assert.Equal(DragonGrowthStage.YoungAdult, DragonProgressionRules.StageForLevel(16));
        Assert.Equal(DragonGrowthStage.Adult, DragonProgressionRules.StageForLevel(21));
        Assert.Equal(DragonGrowthStage.Ancient, DragonProgressionRules.StageForLevel(26));
        Assert.Equal(DragonVisualStage.Ancestral, DragonProgressionRules.VisualStageForLevel(26));
        Assert.Equal("Ancestral", DragonProgressionRules.StageDisplayName(DragonGrowthStage.Ancient));
        Assert.Equal("Adolescente", DragonProgressionRules.StageDisplayName(DragonGrowthStage.Adolescent));
    }

    [Fact]
    public void VisualStages_MatchLevelBands()
    {
        Assert.Equal(DragonVisualStage.Egg, DragonProgressionRules.VisualStageForLevel(0));
        Assert.Equal(DragonVisualStage.Hatchling, DragonProgressionRules.VisualStageForLevel(5));
        Assert.Equal(DragonVisualStage.Young, DragonProgressionRules.VisualStageForLevel(6));
        Assert.Equal(DragonVisualStage.Adolescent, DragonProgressionRules.VisualStageForLevel(11));
        Assert.Equal(DragonVisualStage.YoungAdult, DragonProgressionRules.VisualStageForLevel(16));
        Assert.Equal(DragonVisualStage.Adult, DragonProgressionRules.VisualStageForLevel(21));
        Assert.Equal(DragonVisualStage.Ancestral, DragonProgressionRules.VisualStageForLevel(26));
        Assert.Equal(DragonVisualStage.Ancestral, DragonProgressionRules.VisualStageForLevel(30));
    }

    [Fact]
    public void Phase2_FeedGrantsXpAndLevelUpCompletes()
    {
        var wallet = new FakeWallet { Food = 50_000, Essence = 5_000, Diamonds = 100 };
        var service = CreateBornService(out var clock, wallet, advanceToReady: true);
        service.SyncBuildingLevels(30, 15);
        Assert.Equal(30, service.GetMaxAllowedDragonLevel());
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.Experience = DragonProgressionRules.ExperienceRequiredForLevel(1);
        ember.Energy = 100;
        ember.Health = 100;
        Assert.True(service.TryStartLevelUp(ember.InstanceId, out var error), error);
        Assert.True(ember.IsLevelingUp);
        Assert.Equal(2, ember.PendingLevel);
        clock.UtcNow = clock.UtcNow.AddHours(1);
        service.Tick();
        Assert.False(ember.IsLevelingUp);
        Assert.Equal(2, ember.DragonLevel);
        Assert.Equal(DragonGrowthStage.Hatchling, ember.GrowthStage);
    }

    [Fact]
    public void Phase2_RitualAt6_AndInstantAccelerate()
    {
        var wallet = new FakeWallet { Food = 50_000, Essence = 5_000, Diamonds = 100 };
        var service = CreateBornService(out var clock, wallet, advanceToReady: true);
        service.SyncBuildingLevels(30, 15);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.DragonLevel = 5;
        ember.Experience = DragonProgressionRules.ExperienceRequiredForLevel(5);
        ember.Energy = 100;
        ember.Health = 100;
        Assert.True(service.TryStartLevelUp(ember.InstanceId, out var error), error);
        Assert.Equal(6, ember.PendingLevel);
        Assert.Contains("Ritual", service.DescribeDragonProgression(ember.InstanceId), StringComparison.OrdinalIgnoreCase);
        Assert.True(service.TryInstantCompleteLevelUp(ember.InstanceId, out error), error);
        Assert.Equal(6, ember.DragonLevel);
        Assert.Equal(DragonGrowthStage.Juvenile, ember.GrowthStage);
        Assert.True(wallet.Diamonds < 100);
    }

    [Fact]
    public void Phase2_BlockedByTowerCap()
    {
        var service = CreateBornService(out _, advanceToReady: true);
        service.SyncBuildingLevels(30, 1); // max 5
        Assert.Equal(5, service.GetMaxAllowedDragonLevel());
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.DragonLevel = 5;
        ember.Experience = 9999;
        ember.Energy = 100;
        ember.Health = 100;
        Assert.False(service.TryStartLevelUp(ember.InstanceId, out var error));
        Assert.Contains("Limite", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase2_PersistsXpEnergyHealthAcrossReload()
    {
        var repo = new MemoryDragonRepository();
        var first = CreateBornService(out _, repository: repo, advanceToReady: true);
        first.SyncBuildingLevels(25, 10);
        Assert.True(first.TryGet("dragon-ember-1", out var ember));
        ember.DragonLevel = 4;
        ember.Experience = 33;
        ember.Energy = 77;
        ember.Health = 88;
        first.Persist();

        var second = new DragonService(new DragonSettings(), repo, () => DateTime.UtcNow);
        second.BindWallet(new FakeWallet { Food = 1000, Essence = 100 });
        second.LoadOrInitialize();
        Assert.True(second.TryGet("dragon-ember-1", out var loaded));
        Assert.Equal(4, loaded.DragonLevel);
        Assert.Equal(33, loaded.Experience);
        Assert.Equal(77, loaded.Energy);
        Assert.Equal(88, loaded.Health);
    }

    [Fact]
    public void Phase3_Abilities_UnlockAndEquip()
    {
        var service = CreateBornService(out _, advanceToReady: true);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        Assert.Equal(DragonAbilityId.EmberBreath, ember.AbilitySlot0);
        Assert.False(service.TrySetAbilitySlot(ember.InstanceId, 1, "scale-guard", out var err));
        Assert.Contains("Desbloqueia", err, StringComparison.OrdinalIgnoreCase);

        ember.DragonLevel = 6;
        Assert.True(service.TrySetAbilitySlot(ember.InstanceId, 1, "scale-guard", out err), err);
        Assert.Equal(DragonAbilityId.ScaleGuard, ember.AbilitySlot1);
        Assert.Contains("Escama", service.DescribeDragonAbilities(ember.InstanceId), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase3_CombatSupport_SpendsEnergyHealthAndCanInjure()
    {
        var service = CreateBornService(out _, advanceToReady: true);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.DragonLevel = 10;
        ember.Energy = 100;
        ember.Health = 100;
        ember.AbilitySlot0 = DragonAbilityId.EmberBreath;
        ember.AbilitySlot1 = DragonAbilityId.ScaleGuard;

        Assert.True(service.TryDeployToMarch(ember.InstanceId, "march-pve", out var error), error);
        Assert.True(service.TryEnterCombatForMarch("march-pve", out error), error);
        var powerBefore = service.GetSupportPowerForMarch("march-pve");
        Assert.True(powerBefore > 0);

        Assert.True(service.TryApplyCombatOutcomeForMarch(
            "march-pve",
            victory: true,
            difficultyBand: 2,
            out error,
            out var summary), error);
        Assert.Contains("Vitória", summary, StringComparison.OrdinalIgnoreCase);
        Assert.True(ember.Energy < 100);
        Assert.True(ember.Health <= 100);

        // Combate duro + derrota tende a ferir.
        ember.Energy = 100;
        ember.Health = 40;
        Assert.True(service.TryApplyCombatOutcomeForMarch(
            "march-pve",
            victory: false,
            difficultyBand: 3,
            out error,
            out _), error);
        Assert.True(ember.PendingCombatInjury || ember.Health < 40);

        Assert.True(service.TryRecallFromMarch("march-pve", out error), error);
        Assert.True(ember.State is DragonState.Recovering or DragonState.Injured or DragonState.Exhausted);
    }

    [Fact]
    public void Phase3_PersistsAbilityLoadout()
    {
        var repo = new MemoryDragonRepository();
        var first = CreateBornService(out _, repository: repo, advanceToReady: true);
        Assert.True(first.TryGet("dragon-ember-1", out var ember));
        ember.DragonLevel = 16;
        Assert.True(first.TrySetAbilitySlot(ember.InstanceId, 0, "ember-breath", out _));
        Assert.True(first.TrySetAbilitySlot(ember.InstanceId, 1, "scale-guard", out _));
        Assert.True(first.TrySetAbilitySlot(ember.InstanceId, 2, "ash-surge", out _));
        first.Persist();

        var second = new DragonService(new DragonSettings(), repo, () => DateTime.UtcNow);
        second.BindWallet(new FakeWallet { Food = 1000, Essence = 100 });
        second.LoadOrInitialize();
        Assert.True(second.TryGet("dragon-ember-1", out var loaded));
        Assert.Equal(DragonAbilityId.EmberBreath, loaded.AbilitySlot0);
        Assert.Equal(DragonAbilityId.ScaleGuard, loaded.AbilitySlot1);
        Assert.Equal(DragonAbilityId.AshSurge, loaded.AbilitySlot2);
    }

    [Fact]
    public void Phase4_MountBond_TrainEquipAndPowerBonus()
    {
        var wallet = new FakeWallet { Food = 50_000, Essence = 5_000, Diamonds = 100 };
        var service = CreateBornService(out _, wallet, advanceToReady: true);
        Assert.True(service.TryGet("dragon-ember-1", out var ember));
        ember.Energy = 100;
        ember.Health = 100;

        Assert.True(service.TryCreateMountBond(ember.InstanceId, "HERO_VORTEX_000", out var error), error);
        Assert.Equal("HERO_VORTEX_000", ember.BondedHeroId);
        Assert.True(ember.MountBondLevel >= 1);

        Assert.False(service.TryCreateMountBond(ember.InstanceId, "HERO_ELYRA_001", out error));
        Assert.Contains("outro herói", error, StringComparison.OrdinalIgnoreCase);

        Assert.True(service.TryTrainMountBond(ember.InstanceId, out error), error);
        Assert.True(ember.MountBondPoints > 0 || ember.MountBondLevel > 1);

        Assert.True(service.TryEquipMount(ember.InstanceId, out error), error);
        Assert.True(ember.IsMounted);

        var unmountedPower = 0;
        ember.IsMounted = false;
        unmountedPower = service.GetProvisionalDragonPower(); // 0 — not deployed
        ember.IsMounted = true;
        Assert.True(service.TryDeployToMarch(ember.InstanceId, "march-mount", out error), error);
        var mountedPower = service.GetSupportPowerForMarch("march-mount");
        ember.IsMounted = false;
        var supportUnmounted = service.Combat.ResolveSupportPower(ember, DragonCatalog.Get(ember.DefinitionId));
        ember.IsMounted = true;
        var supportMounted = service.Combat.ResolveSupportPower(ember, DragonCatalog.Get(ember.DefinitionId));
        Assert.True(supportMounted > supportUnmounted);
        Assert.True(mountedPower > 0);
        _ = unmountedPower;

        Assert.True(service.TryGetMarchDragonPresence(
            "march-mount",
            out var id,
            out var stage,
            out var mounted,
            out var hero));
        Assert.Equal(ember.InstanceId, id);
        Assert.True(mounted);
        Assert.Equal("HERO_VORTEX_000", hero);
        Assert.False(string.IsNullOrEmpty(stage));
    }

    [Fact]
    public void Phase4_MountCompatibility_RequiresLevel()
    {
        Assert.False(DragonMountCompatibility.IsCompatible("HERO_ELYRA_001", 1, out var error));
        Assert.Contains("Nv.6", error, StringComparison.OrdinalIgnoreCase);
        Assert.True(DragonMountCompatibility.IsCompatible("HERO_ELYRA_001", 6, out _));
        Assert.True(DragonMountCompatibility.IsCompatible("HERO_VORTEX_000", 1, out _));
    }

    [Fact]
    public void Phase4_PersistsMountBond()
    {
        var repo = new MemoryDragonRepository();
        var first = CreateBornService(out _, repository: repo, advanceToReady: true);
        Assert.True(first.TryGet("dragon-ember-1", out var ember));
        Assert.True(first.TryCreateMountBond(ember.InstanceId, "HERO_VORTEX_000", out _));
        Assert.True(first.TryEquipMount(ember.InstanceId, out _));
        ember.MountBondLevel = 3;
        ember.MountBondPoints = 7;
        first.Persist();

        var second = new DragonService(new DragonSettings(), repo, () => DateTime.UtcNow);
        second.BindWallet(new FakeWallet { Food = 1000, Essence = 100 });
        second.LoadOrInitialize();
        Assert.True(second.TryGet("dragon-ember-1", out var loaded));
        Assert.Equal("HERO_VORTEX_000", loaded.BondedHeroId);
        Assert.True(loaded.IsMounted);
        Assert.Equal(3, loaded.MountBondLevel);
        Assert.Equal(7, loaded.MountBondPoints);
    }

    private sealed class ManualWorldMapClock : IWorldMapClock
    {
        public ManualWorldMapClock(DateTime utcNow) => UtcNow = utcNow;
        public DateTime UtcNow { get; set; }
    }
}
