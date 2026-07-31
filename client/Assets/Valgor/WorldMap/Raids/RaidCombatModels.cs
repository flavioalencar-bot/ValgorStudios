using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.Dragons.Data;

namespace Valgor.WorldMap.Raids
{
    public enum BattleOutcome
    {
        Victory = 0,
        Defeat = 1,
        Draw = 2
    }

    public enum DefenseEventStatus
    {
        Pending = 0,
        Resolving = 1,
        Completed = 2,
        Failed = 3
    }

    public enum RaidRuntimeStatus
    {
        Idle = 0,
        Marching = 1,
        Arrived = 2,
        Resolving = 3,
        Returning = 4,
        Completed = 5
    }

    /// <summary>Bundle de recursos tipado (lógica pura, sem UI/Save).</summary>
    public sealed class ResourceBundle
    {
        private readonly Dictionary<ResourceType, long> _amounts = new();

        public ResourceBundle()
        {
        }

        public ResourceBundle(IReadOnlyDictionary<ResourceType, long> source)
        {
            if (source == null)
            {
                return;
            }

            foreach (var pair in source)
            {
                _amounts[pair.Key] = Math.Max(0, pair.Value);
            }
        }

        public long Get(ResourceType type) =>
            _amounts.TryGetValue(type, out var v) ? v : 0;

        public void Set(ResourceType type, long amount) =>
            _amounts[type] = Math.Max(0, amount);

        public void Add(ResourceType type, long amount)
        {
            if (amount == 0)
            {
                return;
            }

            Set(type, Get(type) + amount);
        }

        public IReadOnlyDictionary<ResourceType, long> AsDictionary() =>
            new Dictionary<ResourceType, long>(_amounts);

        public ResourceBundle Clone() => new(_amounts);

        public long TotalAmount()
        {
            long sum = 0;
            foreach (var v in _amounts.Values)
            {
                sum += v;
            }

            return sum;
        }

        public static ResourceBundle From(params (ResourceType Type, long Amount)[] entries)
        {
            var bundle = new ResourceBundle();
            if (entries == null)
            {
                return bundle;
            }

            foreach (var (type, amount) in entries)
            {
                bundle.Set(type, amount);
            }

            return bundle;
        }
    }

    public sealed class NpcCityFortification
    {
        public int WallLevel { get; set; }
        public long WallMaxDurability { get; set; }
        public long WallCurrentDurability { get; set; }
        public decimal WallDefenseBonus { get; set; }
        public decimal WallDamageReduction { get; set; }
        public int HospitalLevel { get; set; }
        public int HospitalCapacity { get; set; }
        public int WarehouseLevel { get; set; }
        public ResourceBundle ProtectedResources { get; set; } = new();
    }

    public sealed class NpcDefenderFormation
    {
        public int Power { get; set; }
        public int TroopCount { get; set; }
        public string Label { get; set; } = "Guarnição NPC";
    }

    public sealed class FormationSnapshot
    {
        public int Power { get; set; }
        public int TroopCount { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public sealed class WallSnapshot
    {
        public int WallLevel { get; set; }
        public long MaxDurability { get; set; }
        public long CurrentDurability { get; set; }
        public decimal DefenseBonus { get; set; }
        public decimal DamageReduction { get; set; }
    }

    public sealed class DragonBattleSnapshot
    {
        public string DragonId { get; set; } = string.Empty;
        public int DragonLevel { get; set; }
        public DragonBattleRole Role { get; set; }
        public int SupportPower { get; set; }
        public int Energy { get; set; }
        public bool DominatorWrathUnlocked { get; set; }
        public bool DominatorWrathRequested { get; set; }
        public bool DominatorWrathAlreadyUsedInBattle { get; set; }
    }

    public sealed class WallCombatInput
    {
        public int WallLevel { get; set; }
        public long MaxDurability { get; set; }
        public long CurrentDurability { get; set; }
        public decimal DefenseBonus { get; set; }
        public decimal DamageReduction { get; set; }
        public long IncomingWallDamage { get; set; }
        public bool DominatorWrathActive { get; set; }
        public decimal DominatorWrathDamageBonus { get; set; }
        public decimal DominatorWrathDefenseIgnore { get; set; }
    }

    public sealed class WallCombatResult
    {
        public int WallLevel { get; set; }
        public long MaxDurability { get; set; }
        public long CurrentDurabilityBefore { get; set; }
        public long CurrentDurabilityAfter { get; set; }
        public long DamageApplied { get; set; }
        public decimal EffectiveDefenseBonus { get; set; }
        public bool BonusActive => CurrentDurabilityAfter > 0;
    }

    public sealed class WallRepairJob
    {
        public string WallRepairId { get; set; } = string.Empty;
        public long DurabilityToRestore { get; set; }
        public ResourceBundle Cost { get; set; } = new();
        public DateTime StartedAtUtc { get; set; }
        public DateTime CompleteAtUtc { get; set; }
        public bool Completed { get; set; }
    }

    public sealed class CasualtyResult
    {
        public int TotalCasualties { get; set; }
        public int Wounded { get; set; }
        public int Lost { get; set; }
        public int HospitalOverflow { get; set; }
        public int HospitalCapacityBefore { get; set; }
        public int HospitalCapacityAfter { get; set; }
    }

    public sealed class WarehouseProtectionResult
    {
        public ResourceBundle CurrentResources { get; set; } = new();
        public ResourceBundle ProtectedResources { get; set; } = new();
        public ResourceBundle MinimumReserve { get; set; } = new();
        public ResourceBundle LootableResources { get; set; } = new();
    }

    public sealed class RaidLootResult
    {
        public string LootTransactionId { get; set; } = string.Empty;
        public bool VictoryRequired { get; set; } = true;
        public long MarchCapacity { get; set; }
        public long CapacityUsed { get; set; }
        public ResourceBundle LootedResources { get; set; } = new();
        public bool Applied { get; set; }
    }

    public sealed class CityRaidBattleInput
    {
        public string BattleId { get; set; } = string.Empty;
        public FormationSnapshot Attacker { get; set; } = new();
        public FormationSnapshot Defender { get; set; } = new();
        public WallSnapshot Wall { get; set; } = new();
        public DragonBattleSnapshot Dragon { get; set; } = new();
        public int HospitalFreeCapacity { get; set; }
        public ResourceBundle DefenderResources { get; set; } = new();
        public ResourceBundle WarehouseProtection { get; set; } = new();
        public ResourceBundle MinimumReserve { get; set; } = new();
        public long MarchCapacity { get; set; }
        public string LootTransactionId { get; set; } = string.Empty;
        public bool IsCityRaid { get; set; } = true;
        public bool IsDefense { get; set; }
    }

    public sealed class CityRaidBattleResult
    {
        public string BattleId { get; set; } = string.Empty;
        public BattleOutcome Outcome { get; set; }
        public long WallDamage { get; set; }
        public long WallDurabilityBefore { get; set; }
        public long WallDurabilityAfter { get; set; }
        public CasualtyResult AttackerCasualties { get; set; } = new();
        public CasualtyResult DefenderCasualties { get; set; } = new();
        public RaidLootResult Loot { get; set; } = new();
        public int DragonEnergySpent { get; set; }
        public bool DominatorWrathUsed { get; set; }
        public int AttackerPowerFinal { get; set; }
        public int DefenderPowerFinal { get; set; }
        public string Summary { get; set; } = string.Empty;
    }

    public sealed class NpcAttackerSnapshot
    {
        public string AttackerId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Power { get; set; }
        public int TroopCount { get; set; }
    }

    public sealed class DefenseEvent
    {
        public string DefenseEventId { get; set; } = string.Empty;
        public string BattleId { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        public DateTime ScheduledAtUtc { get; set; }
        public DefenseEventStatus Status { get; set; }
        public NpcAttackerSnapshot Attacker { get; set; } = new();
        public bool ResultApplied { get; set; }
        public CityRaidBattleResult? Result { get; set; }
    }

    public sealed class ProcessedOperationRegistry
    {
        public HashSet<string> RaidIds { get; } = new(StringComparer.Ordinal);
        public HashSet<string> MarchIds { get; } = new(StringComparer.Ordinal);
        public HashSet<string> BattleIds { get; } = new(StringComparer.Ordinal);
        public HashSet<string> LootTransactionIds { get; } = new(StringComparer.Ordinal);
        public HashSet<string> ReportIds { get; } = new(StringComparer.Ordinal);
        public HashSet<string> WallRepairIds { get; } = new(StringComparer.Ordinal);
        public HashSet<string> DefenseEventIds { get; } = new(StringComparer.Ordinal);

        public ProcessedOperationRegistry Clone()
        {
            var clone = new ProcessedOperationRegistry();
            Copy(RaidIds, clone.RaidIds);
            Copy(MarchIds, clone.MarchIds);
            Copy(BattleIds, clone.BattleIds);
            Copy(LootTransactionIds, clone.LootTransactionIds);
            Copy(ReportIds, clone.ReportIds);
            Copy(WallRepairIds, clone.WallRepairIds);
            Copy(DefenseEventIds, clone.DefenseEventIds);
            return clone;
        }

        private static void Copy(HashSet<string> from, HashSet<string> to)
        {
            foreach (var id in from)
            {
                to.Add(id);
            }
        }
    }

    /// <summary>Constantes de catálogo (sem números mágicos nos resolvers).</summary>
    public static class RaidCombatCatalog
    {
        public static readonly ResourceType[] LootOrder =
        {
            ResourceType.Food,
            ResourceType.Wood,
            ResourceType.Stone,
            ResourceType.Iron,
            ResourceType.Gold
        };

        public static readonly HashSet<ResourceType> NeverLootable = new()
        {
            ResourceType.Diamonds,
            ResourceType.DragonEssence
        };

        public const decimal LootableFraction = 0.20m;
        public const decimal WoundedCandidateRatio = 0.80m;
        public const int DominatorWrathUnlockLevel = 21;
        public const decimal DominatorWrathDamageBonus = 0.35m;
        public const decimal DominatorWrathDefenseIgnore = 0.40m;
        public const int SupportEnergyCost = 8;
        public const int AssaultEnergyCost = 14;
        public const int GuardEnergyCost = 6;
        public const decimal AssaultWallDamageRatio = 0.45m;
        public const decimal SupportWallDamageRatio = 0.25m;
        public const decimal SupportFormationBonus = 0.08m;
        public const decimal AssaultFormationBonus = 0.18m;
        public const decimal GuardFormationBonus = 0.22m;
    }
}
