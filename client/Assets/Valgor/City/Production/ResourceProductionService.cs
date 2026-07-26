using System;
using System.Collections.Generic;
using Valgor.City.Data;

namespace Valgor.City.Production
{
    public sealed class ResourceProductionService
    {
        private readonly IGameClock _clock;
        private readonly ProductionSettings _settings;
        private readonly Dictionary<string, BuildingProductionState> _states = new();
        private readonly Dictionary<string, BuildingInstance> _buildings = new();

        public ResourceProductionService(IGameClock clock, ProductionSettings settings)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public event EventHandler<ProductionChangedEvent>? Changed;

        public IReadOnlyDictionary<string, BuildingProductionState> States => _states;

        public void RegisterBuilding(BuildingInstance building)
        {
            if (!ProductionCatalog.TryGet(building.DefinitionId, out _))
            {
                return;
            }

            _buildings[building.DefinitionId] = building;
            if (!_states.ContainsKey(building.DefinitionId))
            {
                _states[building.DefinitionId] = new BuildingProductionState(building.DefinitionId, _clock.UtcNow);
            }
        }

        public void RestoreState(BuildingProductionState state)
        {
            _states[state.BuildingDefinitionId] = new BuildingProductionState(state.BuildingDefinitionId, state.LastUpdatedUtc)
            {
                Accumulated = state.Accumulated
            };
        }

        public BuildingProductionState GetState(string buildingDefinitionId) => _states[buildingDefinitionId];

        public bool TryGetState(string buildingDefinitionId, out BuildingProductionState state) =>
            _states.TryGetValue(buildingDefinitionId, out state!);

        public double GetRatePerHour(BuildingInstance building)
        {
            if (!CanProduce(building) || !ProductionCatalog.TryGet(building.DefinitionId, out var definition))
            {
                return 0;
            }

            return definition.GetRatePerHour(building.Level);
        }

        public long GetCapacity(BuildingInstance building)
        {
            if (!ProductionCatalog.TryGet(building.DefinitionId, out var definition))
            {
                return 0;
            }

            return definition.GetCapacity(building.Level);
        }

        public static bool CanProduce(BuildingInstance building) =>
            building.State == BuildingState.Ready
            && building.Level > 0
            && ProductionCatalog.TryGet(building.DefinitionId, out _);

        /// <summary>
        /// Aplica produção desde LastUpdatedUtc até now. Idempotente para o mesmo instante.
        /// </summary>
        public void ApplyUntil(DateTime nowUtc)
        {
            foreach (var pair in _buildings)
            {
                ApplyBuilding(pair.Value, nowUtc);
            }
        }

        public void ApplyAll() => ApplyUntil(_clock.UtcNow);

        public void OnBuildingUpgraded(BuildingInstance building)
        {
            if (!_states.ContainsKey(building.DefinitionId) && ProductionCatalog.TryGet(building.DefinitionId, out _))
            {
                RegisterBuilding(building);
            }

            ApplyBuilding(building, _clock.UtcNow);
        }

        private void ApplyBuilding(BuildingInstance building, DateTime nowUtc)
        {
            if (!ProductionCatalog.TryGet(building.DefinitionId, out var definition))
            {
                return;
            }

            if (!_states.TryGetValue(building.DefinitionId, out var state))
            {
                state = new BuildingProductionState(building.DefinitionId, nowUtc);
                _states[building.DefinitionId] = state;
            }

            if (!CanProduce(building))
            {
                state.LastUpdatedUtc = nowUtc;
                return;
            }

            var capacity = definition.GetCapacity(building.Level);
            var rate = definition.GetRatePerHour(building.Level);
            var produced = OfflineProductionCalculator.CalculateProduced(
                rate,
                state.Accumulated,
                capacity,
                state.LastUpdatedUtc,
                nowUtc,
                _settings.MaxOfflineDuration);

            if (produced > 0)
            {
                state.Accumulated += produced;
            }

            // Avança o relógio mesmo sem produção (cheio / sem taxa) — evita duplicar no reconnect.
            state.LastUpdatedUtc = nowUtc;
            RaiseChanged(building.DefinitionId, definition.Resource, state.Accumulated, capacity);
        }

        public void NotifyCollected(string buildingDefinitionId, ResourceType resource, long capacity)
        {
            RaiseChanged(buildingDefinitionId, resource, 0, capacity);
        }

        private void RaiseChanged(string buildingDefinitionId, ResourceType resource, long accumulated, long capacity)
        {
            Changed?.Invoke(this, new ProductionChangedEvent(buildingDefinitionId, resource, accumulated, capacity));
        }
    }
}
