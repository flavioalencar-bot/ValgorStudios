using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.WorldMap.Creatures;

namespace Valgor.WorldMap.Raids
{
    public enum CityRaidState
    {
        Available = 0,
        Engaged = 1,
        Raided = 2,
        Recovering = 3
    }

    public sealed class CityRaidRewardTable
    {
        public CityRaidRewardTable(string id, IReadOnlyList<CreatureRewardEntry> entries)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Entries = entries ?? Array.Empty<CreatureRewardEntry>();
        }

        public string Id { get; }
        public IReadOnlyList<CreatureRewardEntry> Entries { get; }

        public void GrantTo(ResourceWallet wallet)
        {
            foreach (var entry in Entries)
            {
                wallet.Add(entry.Resource, entry.Amount);
            }
        }
    }

    public sealed class CityRaidDefinition
    {
        public CityRaidDefinition(
            string cityNodeId,
            string displayName,
            int defensePower,
            int energyCost,
            TimeSpan recoverDuration,
            CityRaidRewardTable rewards,
            int minRecommendedPower,
            NpcCityFortification? fortification = null,
            NpcDefenderFormation? defenders = null,
            ResourceBundle? availableResources = null,
            ResourceBundle? minimumReserve = null,
            long marchCapacityHint = 500)
        {
            CityNodeId = cityNodeId ?? throw new ArgumentNullException(nameof(cityNodeId));
            DisplayName = displayName ?? cityNodeId;
            DefensePower = defensePower;
            EnergyCost = energyCost;
            RecoverDuration = recoverDuration;
            Rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
            MinRecommendedPower = minRecommendedPower;
            Fortification = fortification ?? new NpcCityFortification();
            Defenders = defenders ?? new NpcDefenderFormation { Power = defensePower, TroopCount = 800 };
            AvailableResources = availableResources ?? new ResourceBundle();
            MinimumReserve = minimumReserve ?? new ResourceBundle();
            MarchCapacityHint = Math.Max(0, marchCapacityHint);
        }

        public string CityNodeId { get; }
        public string DisplayName { get; }
        /// <summary>Compat: poder efetivo legado (defensores).</summary>
        public int DefensePower { get; }
        public int EnergyCost { get; }
        public TimeSpan RecoverDuration { get; }
        public CityRaidRewardTable Rewards { get; }
        public int MinRecommendedPower { get; }
        public NpcCityFortification Fortification { get; }
        public NpcDefenderFormation Defenders { get; }
        public ResourceBundle AvailableResources { get; }
        public ResourceBundle MinimumReserve { get; }
        public long MarchCapacityHint { get; }

        public string DescribeInspection()
        {
            var f = Fortification;
            var lootable = WarehouseProtectionResolver.Resolve(
                AvailableResources,
                f.ProtectedResources,
                MinimumReserve).LootableResources;
            return
                $"{DisplayName} · poder recomendado {MinRecommendedPower}\n" +
                $"Muralha Nv.{f.WallLevel} · durabilidade {f.WallCurrentDurability}/{f.WallMaxDurability}\n" +
                $"Bônus defesa {f.WallDefenseBonus:P0} · redução dano {f.WallDamageReduction:P0}\n" +
                $"Hospital Nv.{f.HospitalLevel} · capacidade {f.HospitalCapacity}\n" +
                $"Armazém Nv.{f.WarehouseLevel} · proteção ouro {f.ProtectedResources.Get(ResourceType.Gold)}\n" +
                $"Defensores {Defenders.Label}: poder {Defenders.Power} · tropas {Defenders.TroopCount}\n" +
                $"Saqueável (estimado): ouro {lootable.Get(ResourceType.Gold)}, comida {lootable.Get(ResourceType.Food)}";
        }
    }

    public sealed class CityRaidInstance
    {
        public CityRaidInstance(string cityNodeId, CityRaidState state = CityRaidState.Available)
        {
            CityNodeId = cityNodeId ?? throw new ArgumentNullException(nameof(cityNodeId));
            State = state;
        }

        public string CityNodeId { get; }
        public CityRaidState State { get; set; }
        public string? EngagedMarchId { get; set; }
        public DateTime? RecoverAtUtc { get; set; }

        public long WallCurrentDurability { get; set; }
        public string? ActiveBattleId { get; set; }
        public string? ActiveLootTransactionId { get; set; }

        public CityRaidInstance Clone() =>
            new(CityNodeId, State)
            {
                EngagedMarchId = EngagedMarchId,
                RecoverAtUtc = RecoverAtUtc,
                WallCurrentDurability = WallCurrentDurability,
                ActiveBattleId = ActiveBattleId,
                ActiveLootTransactionId = ActiveLootTransactionId
            };
    }

    /// <summary>Relatório de batalha de raid/defesa (Fase 5).</summary>
    public sealed class CityBattleReport
    {
        public DateTime CreatedUtc { get; set; }
        public string TargetCityId { get; set; } = string.Empty;
        public string TargetDisplayName { get; set; } = string.Empty;
        public bool Victory { get; set; }
        public bool IsDefense { get; set; }
        public int AttackerPower { get; set; }
        public int DefensePower { get; set; }
        public CreatureDifficultyBand Band { get; set; }
        public string DragonRoleLabel { get; set; } = "Support";
        public int TroopWoundedPercent { get; set; }
        public bool LootGranted { get; set; }
        public string DragonSummary { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string BattleId { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        public long WallDurabilityAfter { get; set; }
        public bool DominatorWrathUsed { get; set; }
        public string DefenseEventId { get; set; } = string.Empty;
    }

    public static class CityRaidCatalog
    {
        private static readonly Dictionary<string, CityRaidRewardTable> Rewards = new()
        {
            ["ash-loot"] = new("ash-loot", new[]
            {
                new CreatureRewardEntry(ResourceType.Gold, 180),
                new CreatureRewardEntry(ResourceType.Food, 90),
                new CreatureRewardEntry(ResourceType.Iron, 40)
            }),
            ["ruin-loot"] = new("ruin-loot", new[]
            {
                new CreatureRewardEntry(ResourceType.Gold, 260),
                new CreatureRewardEntry(ResourceType.Stone, 120),
                new CreatureRewardEntry(ResourceType.DragonEssence, 8)
            })
        };

        private static readonly Dictionary<string, CityRaidDefinition> Raids = new()
        {
            ["ash-keep"] = new(
                "ash-keep",
                "Fortaleza de Cinzas",
                defensePower: 110,
                energyCost: 16,
                recoverDuration: TimeSpan.FromHours(3),
                rewards: Rewards["ash-loot"],
                minRecommendedPower: 100,
                fortification: new NpcCityFortification
                {
                    WallLevel = 10,
                    WallMaxDurability = 5000,
                    WallCurrentDurability = 5000,
                    WallDefenseBonus = 0.15m,
                    WallDamageReduction = 0.08m,
                    HospitalLevel = 8,
                    HospitalCapacity = 3000,
                    WarehouseLevel = 8,
                    ProtectedResources = ResourceBundle.From(
                        (ResourceType.Food, 2000),
                        (ResourceType.Wood, 2000),
                        (ResourceType.Stone, 1500),
                        (ResourceType.Iron, 800),
                        (ResourceType.Gold, 2500))
                },
                defenders: new NpcDefenderFormation
                {
                    Power = 110,
                    TroopCount = 900,
                    Label = "Guarnição das Cinzas"
                },
                availableResources: ResourceBundle.From(
                    (ResourceType.Food, 8000),
                    (ResourceType.Wood, 7000),
                    (ResourceType.Stone, 5000),
                    (ResourceType.Iron, 3000),
                    (ResourceType.Gold, 9000),
                    (ResourceType.Diamonds, 50)),
                minimumReserve: ResourceBundle.From(
                    (ResourceType.Food, 500),
                    (ResourceType.Gold, 300)),
                marchCapacityHint: 600),
            ["ruin-citadel"] = new(
                "ruin-citadel",
                "Cidadela Ruinosa",
                defensePower: 160,
                energyCost: 22,
                recoverDuration: TimeSpan.FromHours(5),
                rewards: Rewards["ruin-loot"],
                minRecommendedPower: 150,
                fortification: new NpcCityFortification
                {
                    WallLevel = 14,
                    WallMaxDurability = 8000,
                    WallCurrentDurability = 8000,
                    WallDefenseBonus = 0.20m,
                    WallDamageReduction = 0.12m,
                    HospitalLevel = 11,
                    HospitalCapacity = 4500,
                    WarehouseLevel = 11,
                    ProtectedResources = ResourceBundle.From(
                        (ResourceType.Food, 3500),
                        (ResourceType.Wood, 3000),
                        (ResourceType.Stone, 2800),
                        (ResourceType.Iron, 1600),
                        (ResourceType.Gold, 4000))
                },
                defenders: new NpcDefenderFormation
                {
                    Power = 160,
                    TroopCount = 1200,
                    Label = "Sentinelas Ruinosas"
                },
                availableResources: ResourceBundle.From(
                    (ResourceType.Food, 12000),
                    (ResourceType.Wood, 10000),
                    (ResourceType.Stone, 9000),
                    (ResourceType.Iron, 6000),
                    (ResourceType.Gold, 14000),
                    (ResourceType.Diamonds, 80)),
                minimumReserve: ResourceBundle.From(
                    (ResourceType.Food, 800),
                    (ResourceType.Gold, 500)),
                marchCapacityHint: 800)
        };

        public static IReadOnlyDictionary<string, CityRaidDefinition> All => Raids;

        public static bool TryGet(string cityNodeId, out CityRaidDefinition definition) =>
            Raids.TryGetValue(cityNodeId, out definition!);
    }
}
