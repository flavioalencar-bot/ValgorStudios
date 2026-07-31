using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Raids;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class Phase5RaidResolverTests
{
    private sealed class FixedClock : IWorldMapClock
    {
        public DateTime UtcNow { get; set; } = new(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public void Wall_BonusActive_ThenRemovedAtZero()
    {
        var withHp = WallCombatResolver.Resolve(new WallCombatInput
        {
            WallLevel = 10,
            MaxDurability = 1000,
            CurrentDurability = 1000,
            DefenseBonus = 0.15m,
            DamageReduction = 0.10m,
            IncomingWallDamage = 100
        });
        Assert.True(withHp.BonusActive);
        Assert.Equal(10, withHp.WallLevel);
        Assert.True(withHp.CurrentDurabilityAfter < withHp.CurrentDurabilityBefore);

        var zeroed = WallCombatResolver.Resolve(new WallCombatInput
        {
            WallLevel = 10,
            MaxDurability = 1000,
            CurrentDurability = 50,
            DefenseBonus = 0.15m,
            DamageReduction = 0m,
            IncomingWallDamage = 500
        });
        Assert.Equal(0, zeroed.CurrentDurabilityAfter);
        Assert.False(zeroed.BonusActive);
        Assert.Equal(10, zeroed.WallLevel);
    }

    [Fact]
    public void Wall_Repair_Idempotent_AndDoesNotExceedMax()
    {
        var processed = new ProcessedOperationRegistry();
        var job = new WallRepairJob
        {
            WallRepairId = "repair-1",
            DurabilityToRestore = 99999
        };
        var next = WallCombatResolver.ApplyRepair(800, 1000, job, processed, out var error);
        Assert.Equal(string.Empty, error);
        Assert.Equal(1000, next);
        Assert.True(job.Completed);

        var again = WallCombatResolver.ApplyRepair(500, 1000, job, processed, out error);
        Assert.Contains("já aplicado", error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(500, again);
    }

    [Fact]
    public void Hospital_EightyTwenty_AndOverflowBecomesLoss()
    {
        var full = CasualtyResolver.Resolve(100, freeHospitalCapacity: 1000);
        Assert.Equal(80, full.Wounded);
        Assert.Equal(20, full.Lost);
        Assert.Equal(100, full.Wounded + full.Lost);

        var none = CasualtyResolver.Resolve(100, freeHospitalCapacity: 0);
        Assert.Equal(0, none.Wounded);
        Assert.Equal(100, none.Lost);
        Assert.Equal(80, none.HospitalOverflow);

        var partial = CasualtyResolver.Resolve(100, freeHospitalCapacity: 30);
        Assert.Equal(30, partial.Wounded);
        Assert.Equal(70, partial.Lost);
    }

    [Fact]
    public void Warehouse_ProtectsAndNeverLootsDiamonds()
    {
        var current = ResourceBundle.From(
            (ResourceType.Gold, 1000),
            (ResourceType.Food, 100),
            (ResourceType.Diamonds, 50));
        var protectedRes = ResourceBundle.From((ResourceType.Gold, 400), (ResourceType.Food, 200));
        var reserve = ResourceBundle.From((ResourceType.Gold, 100));
        var result = WarehouseProtectionResolver.Resolve(current, protectedRes, reserve);
        Assert.Equal(500, result.LootableResources.Get(ResourceType.Gold));
        Assert.Equal(0, result.LootableResources.Get(ResourceType.Food));
        Assert.Equal(0, result.LootableResources.Get(ResourceType.Diamonds));
        Assert.True(WarehouseProtectionResolver.IsNeverLootable(ResourceType.Diamonds));
    }

    [Fact]
    public void Loot_VictoryCap20_AndDefeatZero_AndIdempotent()
    {
        var lootable = ResourceBundle.From((ResourceType.Gold, 1000), (ResourceType.Food, 500));
        var processed = new ProcessedOperationRegistry();
        var win = RaidLootCalculator.Calculate(BattleOutcome.Victory, lootable, 100, "loot-a", processed);
        Assert.True(win.CapacityUsed > 0);
        Assert.True(win.CapacityUsed <= 100);
        Assert.True(win.LootedResources.Get(ResourceType.Gold) <= 200);

        var lose = RaidLootCalculator.Calculate(BattleOutcome.Defeat, lootable, 100, "loot-b");
        Assert.Equal(0, lose.CapacityUsed);

        RaidLootCalculator.MarkApplied(win, processed);
        var again = RaidLootCalculator.Calculate(BattleOutcome.Victory, lootable, 100, "loot-a", processed);
        Assert.True(again.Applied);
    }

    [Fact]
    public void BattleResolver_AssaultWithWrath_DamagesWallMore()
    {
        var support = CityRaidBattleResolver.Resolve(BaseInput(DragonBattleRole.Support, wrath: false));
        var assault = CityRaidBattleResolver.Resolve(BaseInput(DragonBattleRole.Assault, wrath: true));
        Assert.True(assault.WallDamage >= support.WallDamage);
        Assert.True(assault.DominatorWrathUsed);
        Assert.False(support.DominatorWrathUsed);
    }

    [Fact]
    public void Application_IsIdempotent_ByBattleId()
    {
        var processed = new ProcessedOperationRegistry();
        var service = new RaidResultApplicationService(processed);
        var wallet = new ResourceWallet();
        var result = CityRaidBattleResolver.Resolve(BaseInput(DragonBattleRole.Support, wrath: false));
        result.Loot.LootedResources.Set(ResourceType.Gold, 10);
        result.Loot.CapacityUsed = 10;
        result.Outcome = BattleOutcome.Victory;

        var first = service.Apply(new RaidApplicationRequest
        {
            RaidId = "ash-keep",
            MarchId = "m1",
            ReportId = result.BattleId + ":report",
            Result = result,
            AttackerWallet = wallet
        });
        Assert.True(first.Applied);
        var gold = wallet.Get(ResourceType.Gold);

        var second = service.Apply(new RaidApplicationRequest
        {
            RaidId = "ash-keep",
            MarchId = "m1",
            ReportId = result.BattleId + ":report",
            Result = result,
            AttackerWallet = wallet
        });
        Assert.True(second.AlreadyApplied);
        Assert.Equal(gold, wallet.Get(ResourceType.Gold));
    }

    [Fact]
    public void DefenseOffline_ProcessesOnce()
    {
        var clock = new FixedClock();
        var session = WorldMapSession.Create(new ProvisionalHeroesGateway(), clock);
        session.BindCityDefenseStats(10, 8, 8);
        session.ScheduleNpcInvasion("def-1", attackerPower: 40, scheduledAtUtc: clock.UtcNow);
        var n = session.ProcessPendingDefenses(out _);
        Assert.True(n >= 1);
        Assert.NotNull(session.LastBattleReport);
        Assert.True(session.LastBattleReport!.IsDefense);
        var n2 = session.ProcessPendingDefenses(out _);
        Assert.Equal(0, n2);
    }

    [Fact]
    public void DefenseOffline_Reload_DoesNotDuplicate()
    {
        var clock = new FixedClock();
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests.def." + Guid.NewGuid().ToString("N"),
            EnergyPersistenceKey = "valgor.energy.tests.def." + Guid.NewGuid().ToString("N"),
            FilterPersistenceKey = "valgor.filter.tests.def." + Guid.NewGuid().ToString("N"),
            CameraPersistenceKey = "valgor.camera.tests.def." + Guid.NewGuid().ToString("N"),
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };
        var repo = new LocalWorldMapRepository(settings.PersistenceKey);
        var first = new WorldMapSession(settings, clock, new ProvisionalHeroesGateway(), repo);
        first.LoadOrInitialize();
        first.BindCityDefenseStats(12, 8, 8);
        first.ScheduleNpcInvasion("def-reload", attackerPower: 35, scheduledAtUtc: clock.UtcNow);
        Assert.True(first.ProcessPendingDefenses(out _) >= 1);
        var reportId = first.LastBattleReport!.ReportId;
        Assert.Contains("def-reload", first.ProcessedOperations.DefenseEventIds);

        var second = new WorldMapSession(settings, clock, new ProvisionalHeroesGateway(), repo);
        second.LoadOrInitialize();
        Assert.Contains("def-reload", second.ProcessedOperations.DefenseEventIds);
        Assert.Equal(0, second.ProcessPendingDefenses(out _));
        Assert.Equal(reportId, second.LastBattleReport?.ReportId);
    }

    [Fact]
    public void ScenarioB_AssaultWrath_RuinCitadel()
    {
        var input = BaseInput(DragonBattleRole.Assault, wrath: true);
        input.BattleId = "ruin-wrath-1";
        input.Defender = new FormationSnapshot { Power = 160, TroopCount = 1200 };
        input.Wall = new WallSnapshot
        {
            WallLevel = 14,
            MaxDurability = 8000,
            CurrentDurability = 8000,
            DefenseBonus = 0.20m,
            DamageReduction = 0.12m
        };
        var once = CityRaidBattleResolver.Resolve(input);
        Assert.True(once.DominatorWrathUsed);
        Assert.True(once.WallDamage > 0);

        input.Dragon.DominatorWrathAlreadyUsedInBattle = true;
        var twice = CityRaidBattleResolver.Resolve(input);
        Assert.False(twice.DominatorWrathUsed);
        Assert.True(twice.WallDamage <= once.WallDamage);
    }

    [Fact]
    public void ScenarioC_Defeat_NoLoot()
    {
        var input = BaseInput(DragonBattleRole.Support, wrath: false);
        input.Attacker = new FormationSnapshot { Power = 20, TroopCount = 200 };
        input.Dragon.SupportPower = 0;
        var result = CityRaidBattleResolver.Resolve(input);
        Assert.Equal(BattleOutcome.Defeat, result.Outcome);
        Assert.Equal(0, result.Loot.CapacityUsed);
    }

    [Fact]
    public void ScenarioD_HospitalFull_OverflowLost()
    {
        var result = CasualtyResolver.Resolve(1000, freeHospitalCapacity: 0);
        Assert.Equal(0, result.Wounded);
        Assert.Equal(1000, result.Lost);
        Assert.Equal(800, result.HospitalOverflow);
    }

    [Fact]
    public void E2E_Logic_RaidThenDefenseIdempotent()
    {
        var clock = new FixedClock();
        var settings = new WorldMapSettings
        {
            PersistenceKey = "valgor.worldmap.tests.e2e." + Guid.NewGuid().ToString("N"),
            EnergyPersistenceKey = "valgor.energy.tests.e2e." + Guid.NewGuid().ToString("N"),
            FilterPersistenceKey = "valgor.filter.tests.e2e." + Guid.NewGuid().ToString("N"),
            CameraPersistenceKey = "valgor.camera.tests.e2e." + Guid.NewGuid().ToString("N"),
            MarchSpeedUnitsPerHour = 10f,
            MarchTickIntervalSeconds = 0
        };
        var repo = new LocalWorldMapRepository(settings.PersistenceKey);
        var session = new WorldMapSession(settings, clock, new ProvisionalHeroesGateway(), repo);
        session.LoadOrInitialize();
        session.BindCityDefenseStats(10, 8, 8);
        var wallet = new ResourceWallet();
        ArriveAt("ash-keep", session, clock);
        session.Selection.Select(session.GetNode("ash-keep"));
        Assert.Contains("Muralha", session.DescribeSelectedRaidInspection(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Formação", session.DescribeRaidPreparation(), StringComparison.OrdinalIgnoreCase);
        Assert.True(session.TryEngageSelectedCityRaid(out var err), err);
        Assert.True(session.TryResolveSelectedCityRaid(wallet, out err, out _), err);
        Assert.True(wallet.Get(ResourceType.Gold) > 0);
        Assert.True(session.LastBattleReport!.Victory);
        var battleId = session.LastBattleReport.BattleId;

        session.ScheduleNpcInvasion("e2e-def", attackerPower: 40, scheduledAtUtc: clock.UtcNow);
        Assert.True(session.ProcessPendingDefenses(out _) >= 1);

        var reloaded = new WorldMapSession(settings, clock, new ProvisionalHeroesGateway(), repo);
        reloaded.LoadOrInitialize();
        Assert.Contains(battleId, reloaded.ProcessedOperations.BattleIds);
        Assert.Contains("e2e-def", reloaded.ProcessedOperations.DefenseEventIds);
        Assert.Equal(0, reloaded.ProcessPendingDefenses(out _));
    }

    [Fact]
    public void CityRaid_AshKeep_UsesFortificationCatalog()
    {
        Assert.True(CityRaidCatalog.TryGet("ash-keep", out var ash));
        Assert.Equal(110, ash.DefensePower);
        Assert.Equal(10, ash.Fortification.WallLevel);
        Assert.True(ash.Fortification.HospitalCapacity > 0);
        Assert.Contains("Muralha", ash.DescribeInspection(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Integration_AshKeep_Victory_PersistReloadMarkers()
    {
        var clock = new FixedClock();
        var session = WorldMapSession.Create(new ProvisionalHeroesGateway(), clock);
        var wallet = new ResourceWallet();
        ArriveAt("ash-keep", session, clock);
        session.Selection.Select(session.GetNode("ash-keep"));
        Assert.True(session.TryEngageSelectedCityRaid(out var err), err);
        Assert.True(session.TryResolveSelectedCityRaid(wallet, out err, out _), err);
        Assert.NotNull(session.LastBattleReport);
        Assert.False(string.IsNullOrEmpty(session.LastBattleReport!.BattleId));
        Assert.Contains(session.LastBattleReport.BattleId, session.ProcessedOperations.BattleIds);
        // Segunda resolução sem novo engage deve falhar (sem raid engajado).
        Assert.False(session.TryResolveSelectedCityRaid(wallet, out _, out _));
    }

    [Fact]
    public void Migration_V7_To_V8_PreservesDragonProgress()
    {
        var repo = new MemoryRepo();
        var wallet = new FakeDragonWallet { Food = 50_000, Essence = 5_000 };
        var settings = new DragonSettings
        {
            HatchDurationHours = 1,
            JuvenileDurationHours = 1,
            CareRequiredForHatch = 2,
            CareFoodCost = 50,
            CareExtendsHatchHours = 0
        };
        var first = new DragonService(settings, repo, () => DateTime.UtcNow);
        first.BindWallet(wallet);
        first.LoadOrInitialize();
        first.SyncCastleLevel(20);
        Assert.True(first.TryAcceptEggMission(out _));
        Assert.True(first.TryConquerEgg(out _));
        Assert.True(first.TryBeginIncubation(out _));
        Assert.True(first.TryCareIncubation(out _));
        Assert.True(first.TryCareIncubation(out _));
        Assert.True(first.TryGet("dragon-ember-1", out var egg));
        egg.StateEndsAtUtc = DateTime.UtcNow.AddSeconds(-1);
        first.Tick();
        Assert.True(first.TryGet("dragon-ember-1", out var ember));
        ember.DragonLevel = 21;
        ember.Experience = 123;
        ember.BondedHeroId = "HERO_VORTEX_000";
        ember.IsMounted = true;
        ember.BattleRole = DragonBattleRole.Assault;
        first.Persist();

        var second = new DragonService(settings, repo, () => DateTime.UtcNow);
        second.BindWallet(wallet);
        second.LoadOrInitialize();
        Assert.Equal("valgor.dragons.v8", second.PersistenceKey);
        Assert.True(second.TryGet("dragon-ember-1", out var loaded));
        Assert.Equal(21, loaded.DragonLevel);
        Assert.Equal(123, loaded.Experience);
        Assert.Equal("HERO_VORTEX_000", loaded.BondedHeroId);
        Assert.True(loaded.IsMounted);
        Assert.Equal(DragonBattleRole.Assault, loaded.BattleRole);
    }

    private static CityRaidBattleInput BaseInput(DragonBattleRole role, bool wrath) =>
        new()
        {
            BattleId = "battle-" + role + (wrath ? "-wrath" : ""),
            IsCityRaid = true,
            Attacker = new FormationSnapshot { Power = 200, TroopCount = 1000 },
            Defender = new FormationSnapshot { Power = 110, TroopCount = 900 },
            Wall = new WallSnapshot
            {
                WallLevel = 10,
                MaxDurability = 5000,
                CurrentDurability = 5000,
                DefenseBonus = 0.15m,
                DamageReduction = 0.08m
            },
            Dragon = new DragonBattleSnapshot
            {
                DragonId = "dragon-ember-1",
                DragonLevel = 21,
                Role = role,
                SupportPower = 40,
                DominatorWrathUnlocked = true,
                DominatorWrathRequested = wrath
            },
            HospitalFreeCapacity = 3000,
            DefenderResources = ResourceBundle.From((ResourceType.Gold, 9000), (ResourceType.Food, 8000)),
            WarehouseProtection = ResourceBundle.From((ResourceType.Gold, 2500), (ResourceType.Food, 2000)),
            MinimumReserve = ResourceBundle.From((ResourceType.Gold, 300)),
            MarchCapacity = 600,
            LootTransactionId = "loot-" + role
        };

    private static void ArriveAt(string nodeId, WorldMapSession session, FixedClock clock)
    {
        session.Selection.Select(session.GetNode(nodeId));
        Assert.True(session.TryDispatchToSelected(out var error), error);
        Assert.NotNull(session.Marches.Active);
        clock.UtcNow = session.Marches.Active!.ArrivalAt;
        session.Marches.Advance(clock.UtcNow);
        Assert.Equal(Valgor.WorldMap.Marches.MarchState.Arrived, session.Marches.Active.State);
    }

    private sealed class FakeDragonWallet : IDragonResourceWallet
    {
        public long Food { get; set; }
        public long Essence { get; set; }
        public long Diamonds { get; set; } = 10_000;
        public long GetFood() => Food;
        public long GetDragonEssence() => Essence;
        public long GetDiamonds() => Diamonds;
        public bool TrySpendFood(long amount)
        {
            if (Food < amount) return false;
            Food -= amount;
            return true;
        }
        public bool TrySpendDragonEssence(long amount)
        {
            if (Essence < amount) return false;
            Essence -= amount;
            return true;
        }
        public bool TrySpendDiamonds(long amount)
        {
            if (Diamonds < amount) return false;
            Diamonds -= amount;
            return true;
        }
        public void AddFood(long amount) => Food += amount;
        public void AddDragonEssence(long amount) => Essence += amount;
    }

    private sealed class MemoryRepo : IDragonRepository
    {
        private DragonSnapshot? _snap;
        public DragonSnapshot? Load() => _snap;
        public void Save(DragonSnapshot snapshot) => _snap = snapshot;
    }
}
