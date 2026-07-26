using System;
using Valgor.City.Data;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Marches;

namespace Valgor.WorldMap.Creatures
{
    /// <summary>
    /// Encontros provisórios com criaturas do mapa. Sem combate visual / PvP.
    /// </summary>
    public sealed class CreatureEncounterService
    {
        private readonly Func<string, WorldCreatureDefinition?> _resolveDefinition;
        private readonly Func<string, WorldCreatureInstance?> _resolveInstance;
        private readonly Func<MarchOrder?> _activeMarch;

        public CreatureEncounterService(
            Func<string, WorldCreatureDefinition?> resolveDefinition,
            Func<string, WorldCreatureInstance?> resolveInstance,
            Func<MarchOrder?> activeMarch)
        {
            _resolveDefinition = resolveDefinition;
            _resolveInstance = resolveInstance;
            _activeMarch = activeMarch;
        }

        public event Action? Changed;

        public bool CanEngage(string creatureId, int availableEnergy, out string error)
        {
            if (!TryGet(creatureId, out var definition, out var instance))
            {
                error = "Criatura não encontrada.";
                return false;
            }

            if (definition.StartsLocked)
            {
                error = "Criatura bloqueada.";
                return false;
            }

            if (instance.State != WorldCreatureState.Available)
            {
                error = $"Criatura indisponível ({instance.State}).";
                return false;
            }

            var march = _activeMarch();
            if (march == null ||
                march.State != MarchState.Arrived ||
                !string.Equals(march.TargetNodeId, creatureId, StringComparison.Ordinal))
            {
                error = "A marcha precisa estar no nó da criatura.";
                return false;
            }

            if (availableEnergy < definition.EnergyCost)
            {
                error = "Energia insuficiente.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public bool TryEngage(string creatureId, ref int availableEnergy, out string error)
        {
            if (!CanEngage(creatureId, availableEnergy, out error))
            {
                return false;
            }

            var definition = _resolveDefinition(creatureId)!;
            var instance = _resolveInstance(creatureId)!;
            availableEnergy -= definition.EnergyCost;
            instance.State = WorldCreatureState.Engaged;
            instance.EngagedMarchId = _activeMarch()!.Id;
            instance.RespawnAtUtc = null;
            Changed?.Invoke();
            error = string.Empty;
            return true;
        }

        public bool TryResolveProvisional(
            string creatureId,
            int attackerPower,
            ResourceWallet wallet,
            DateTime utcNow,
            out string error,
            out CreatureDifficultyBand band)
        {
            band = CreatureDifficultyBand.Impossible;
            if (!TryGet(creatureId, out var definition, out var instance))
            {
                error = "Criatura não encontrada.";
                return false;
            }

            if (instance.State != WorldCreatureState.Engaged)
            {
                error = "Nenhum encontro ativo.";
                return false;
            }

            band = CreatureDifficultyResolver.Resolve(attackerPower, definition.RecommendedPower);
            if (!CreatureDifficultyResolver.CanDefeatProvisional(attackerPower, definition.RecommendedPower))
            {
                instance.State = WorldCreatureState.Available;
                instance.EngagedMarchId = null;
                Changed?.Invoke();
                error = "Poder insuficiente para o encontro provisório.";
                return false;
            }

            definition.Rewards.GrantTo(wallet);
            instance.State = WorldCreatureState.Defeated;
            instance.EngagedMarchId = null;
            StartRespawn(instance, definition, utcNow);
            error = string.Empty;
            return true;
        }

        public void AdvanceInstance(WorldCreatureInstance instance, WorldCreatureDefinition definition, DateTime utcNow)
        {
            if (instance.State == WorldCreatureState.Defeated)
            {
                StartRespawn(instance, definition, utcNow);
            }

            if (instance.State == WorldCreatureState.Respawning &&
                instance.RespawnAtUtc.HasValue &&
                utcNow >= instance.RespawnAtUtc.Value)
            {
                instance.State = WorldCreatureState.Available;
                instance.RespawnAtUtc = null;
                Changed?.Invoke();
            }
        }

        private void StartRespawn(WorldCreatureInstance instance, WorldCreatureDefinition definition, DateTime utcNow)
        {
            instance.State = WorldCreatureState.Respawning;
            instance.RespawnAtUtc = utcNow.Add(definition.RespawnDuration);
            Changed?.Invoke();
        }

        private bool TryGet(string creatureId, out WorldCreatureDefinition definition, out WorldCreatureInstance instance)
        {
            definition = null!;
            instance = null!;
            var def = _resolveDefinition(creatureId);
            var inst = _resolveInstance(creatureId);
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
