using System;
using Valgor.City.Data;
using Valgor.Dragons.Data;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Marches;

namespace Valgor.WorldMap.Raids
{
    /// <summary>
    /// Raid PvE a cidades NPC (Fase 5 / D030). Não é PvP multiplayer.
    /// </summary>
    public sealed class CityRaidEncounterService
    {
        private readonly Func<string, CityRaidDefinition?> _resolveDefinition;
        private readonly Func<string, CityRaidInstance?> _resolveInstance;
        private readonly Func<string, WorldMapNodeDefinition?> _resolveNode;
        private readonly Func<MarchOrder?> _activeMarch;

        public CityRaidEncounterService(
            Func<string, CityRaidDefinition?> resolveDefinition,
            Func<string, CityRaidInstance?> resolveInstance,
            Func<string, WorldMapNodeDefinition?> resolveNode,
            Func<MarchOrder?> activeMarch)
        {
            _resolveDefinition = resolveDefinition;
            _resolveInstance = resolveInstance;
            _resolveNode = resolveNode;
            _activeMarch = activeMarch;
        }

        public event Action? Changed;

        public bool CanRaid(string cityNodeId, int availableEnergy, out string error)
        {
            if (!TryGet(cityNodeId, out var definition, out var instance))
            {
                error = "Cidade não é alvo de raid.";
                return false;
            }

            var node = _resolveNode(cityNodeId);
            if (node is WorldCityNode city && city.IsPlayerHome)
            {
                error = "Não é possível atacar a própria cidade.";
                return false;
            }

            if (instance.State != CityRaidState.Available)
            {
                error = $"Cidade indisponível ({instance.State}).";
                return false;
            }

            var march = _activeMarch();
            if (march == null ||
                march.State != MarchState.Arrived ||
                !string.Equals(march.TargetNodeId, cityNodeId, StringComparison.Ordinal))
            {
                error = "A marcha precisa estar no nó da cidade alvo.";
                return false;
            }

            if (availableEnergy < definition.EnergyCost)
            {
                error = "Energia insuficiente para o raid.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public bool TryEngage(string cityNodeId, ref int availableEnergy, out string error)
        {
            if (!CanRaid(cityNodeId, availableEnergy, out error))
            {
                return false;
            }

            var definition = _resolveDefinition(cityNodeId)!;
            var instance = _resolveInstance(cityNodeId)!;
            availableEnergy -= definition.EnergyCost;
            instance.State = CityRaidState.Engaged;
            instance.EngagedMarchId = _activeMarch()!.Id;
            instance.RecoverAtUtc = null;
            if (instance.WallCurrentDurability <= 0)
            {
                instance.WallCurrentDurability = definition.Fortification.WallCurrentDurability;
            }

            instance.ActiveBattleId = $"raid:{cityNodeId}:{instance.EngagedMarchId}:{DateTime.UtcNow.Ticks}";
            instance.ActiveLootTransactionId = instance.ActiveBattleId + ":loot";
            Changed?.Invoke();
            error = string.Empty;
            return true;
        }

        public bool TryResolve(
            string cityNodeId,
            int attackerPower,
            ResourceWallet wallet,
            DateTime utcNow,
            out string error,
            out CreatureDifficultyBand band,
            out bool victory,
            out string report) =>
            TryResolveDetailed(
                cityNodeId,
                attackerPower,
                troopCount: 1000,
                dragon: new DragonBattleSnapshot
                {
                    Role = DragonBattleRole.Support,
                    SupportPower = Math.Max(0, attackerPower / 10)
                },
                wallet,
                utcNow,
                out error,
                out band,
                out victory,
                out report,
                out _);

        public bool TryResolveDetailed(
            string cityNodeId,
            int attackerFormationPower,
            int troopCount,
            DragonBattleSnapshot dragon,
            ResourceWallet wallet,
            DateTime utcNow,
            out string error,
            out CreatureDifficultyBand band,
            out bool victory,
            out string report,
            out CityRaidBattleResult? battleResult)
        {
            band = CreatureDifficultyBand.Impossible;
            victory = false;
            report = string.Empty;
            battleResult = null;
            if (!TryGet(cityNodeId, out var definition, out var instance))
            {
                error = "Cidade não é alvo de raid.";
                return false;
            }

            if (instance.State != CityRaidState.Engaged)
            {
                error = "Nenhum raid ativo nesta cidade.";
                return false;
            }

            var fort = definition.Fortification;
            var wallCurrent = instance.WallCurrentDurability > 0
                ? instance.WallCurrentDurability
                : fort.WallCurrentDurability;
            var battleId = instance.ActiveBattleId ?? $"raid:{cityNodeId}:{utcNow.Ticks}";
            var lootId = instance.ActiveLootTransactionId ?? battleId + ":loot";

            battleResult = CityRaidBattleResolver.Resolve(new CityRaidBattleInput
            {
                BattleId = battleId,
                IsCityRaid = true,
                IsDefense = false,
                Attacker = new FormationSnapshot
                {
                    Power = Math.Max(1, attackerFormationPower),
                    TroopCount = Math.Max(1, troopCount),
                    Label = "Formação atacante"
                },
                Defender = new FormationSnapshot
                {
                    Power = definition.Defenders.Power,
                    TroopCount = definition.Defenders.TroopCount,
                    Label = definition.Defenders.Label
                },
                Wall = new WallSnapshot
                {
                    WallLevel = fort.WallLevel,
                    MaxDurability = fort.WallMaxDurability,
                    CurrentDurability = wallCurrent,
                    DefenseBonus = fort.WallDefenseBonus,
                    DamageReduction = fort.WallDamageReduction
                },
                Dragon = dragon ?? new DragonBattleSnapshot(),
                HospitalFreeCapacity = HospitalCapacityResolver.CapacityFromCatalog(fort.HospitalCapacity, 0),
                DefenderResources = definition.AvailableResources.Clone(),
                WarehouseProtection = fort.ProtectedResources.Clone(),
                MinimumReserve = definition.MinimumReserve.Clone(),
                MarchCapacity = definition.MarchCapacityHint,
                LootTransactionId = lootId
            });

            band = CreatureDifficultyResolver.Resolve(
                Math.Max(1, battleResult.AttackerPowerFinal),
                Math.Max(1, battleResult.DefenderPowerFinal));
            victory = battleResult.Outcome == BattleOutcome.Victory;
            instance.WallCurrentDurability = battleResult.WallDurabilityAfter;

            if (!victory)
            {
                instance.State = CityRaidState.Available;
                instance.EngagedMarchId = null;
                instance.ActiveBattleId = null;
                instance.ActiveLootTransactionId = null;
                report = battleResult.Summary;
                Changed?.Invoke();
                error = battleResult.Outcome == BattleOutcome.Draw
                    ? "Empate — sem saque."
                    : "Defesa da cidade repeliu o ataque.";
                return false;
            }

            // Saque de armazém (capacidade) + tabela de recompensa do encontro (catálogo).
            foreach (var pair in battleResult.Loot.LootedResources.AsDictionary())
            {
                if (pair.Value > 0)
                {
                    wallet.Add(pair.Key, pair.Value);
                }
            }

            definition.Rewards.GrantTo(wallet);

            instance.State = CityRaidState.Raided;
            instance.EngagedMarchId = null;
            instance.ActiveBattleId = null;
            instance.ActiveLootTransactionId = null;
            StartRecover(instance, definition, utcNow);
            report = battleResult.Summary + " Saque concedido.";
            error = string.Empty;
            Changed?.Invoke();
            return true;
        }

        public void AdvanceInstance(CityRaidInstance instance, CityRaidDefinition definition, DateTime utcNow)
        {
            if (instance.State == CityRaidState.Raided)
            {
                StartRecover(instance, definition, utcNow);
            }

            if (instance.State == CityRaidState.Recovering &&
                instance.RecoverAtUtc.HasValue &&
                utcNow >= instance.RecoverAtUtc.Value)
            {
                instance.State = CityRaidState.Available;
                instance.RecoverAtUtc = null;
                // Restaura durabilidade parcial ao reabrir (catálogo).
                instance.WallCurrentDurability = definition.Fortification.WallMaxDurability;
                Changed?.Invoke();
            }
        }

        private void StartRecover(CityRaidInstance instance, CityRaidDefinition definition, DateTime utcNow)
        {
            instance.State = CityRaidState.Recovering;
            instance.RecoverAtUtc = utcNow.Add(definition.RecoverDuration);
            Changed?.Invoke();
        }

        private bool TryGet(string cityNodeId, out CityRaidDefinition definition, out CityRaidInstance instance)
        {
            definition = null!;
            instance = null!;
            var def = _resolveDefinition(cityNodeId);
            var inst = _resolveInstance(cityNodeId);
            if (def == null || inst == null)
            {
                return false;
            }

            definition = def;
            instance = inst;
            return true;
        }
    }
}
