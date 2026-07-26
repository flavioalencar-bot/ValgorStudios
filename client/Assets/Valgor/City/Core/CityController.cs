using System;
using System.Collections.Generic;
using Valgor.City.Buildings;
using Valgor.City.Data;
using Valgor.City.Production;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;

namespace Valgor.City.Core
{
    public sealed class CityController
    {
        private readonly ResourceWallet _wallet;
        private readonly CityEconomy _economy;
        private readonly Dictionary<BuildingInstance, BuildingDefinition> _definitions = new();
        private readonly Dictionary<BuildingInstance, BuildingView> _views = new();
        private readonly List<BuildingInstance> _buildings = new();
        private DragonService? _dragons;

        public CityController(CityEconomy economy, BuildingSelectionService selection)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _wallet = economy.Wallet;
            Selection = selection;
            Selection.SelectionChanged += OnSelectionChanged;
            _economy.Production.Changed += (_, __) =>
            {
                RefreshCollectableIndicators();
                BuildingChanged?.Invoke();
            };
        }

        public BuildingSelectionService Selection { get; }
        public CityEconomy Economy => _economy;
        public IDragonGateway? Dragons => _dragons;
        public IReadOnlyList<BuildingInstance> Buildings => _buildings;
        public event Action? BuildingChanged;

        public void BindDragons(DragonService dragons)
        {
            _dragons = dragons ?? throw new ArgumentNullException(nameof(dragons));
            _dragons.Changed += (_, __) => BuildingChanged?.Invoke();
        }

        public void Add(BuildingSlot slot, BuildingInstance instance, BuildingDefinition definition, BuildingView view)
        {
            slot.Initialize(slot.SlotId, definition.Id, instance);
            _definitions.Add(instance, definition);
            _views.Add(instance, view);
            _buildings.Add(instance);
            _economy.Production.RegisterBuilding(instance);
            view.Clicked += _ => Selection.Select(instance);
        }

        public BuildingDefinition GetDefinition(BuildingInstance instance) => _definitions[instance];

        public bool TrySelectByDefinitionId(string definitionId)
        {
            foreach (var building in _buildings)
            {
                if (string.Equals(building.DefinitionId, definitionId, StringComparison.Ordinal))
                {
                    Selection.Select(building);
                    return true;
                }
            }

            return false;
        }

        public bool TryUpgradeSelected()
        {
            var building = Selection.Selected;
            if (building == null || !_definitions.TryGetValue(building, out var definition) || !building.CanUpgrade(definition))
            {
                return false;
            }

            foreach (var cost in definition.BaseCosts)
            {
                var amount = definition.GetUpgradeCost(cost.Key, building.Level);
                if (_wallet.Get(cost.Key) < amount)
                {
                    return false;
                }
            }

            foreach (var cost in definition.BaseCosts)
            {
                _wallet.TrySpend(cost.Key, definition.GetUpgradeCost(cost.Key, building.Level));
            }

            building.CompleteUpgrade();
            _economy.Production.OnBuildingUpgraded(building);
            _economy.Persist(_buildings);
            BuildingChanged?.Invoke();
            return true;
        }

        public long CollectSelected()
        {
            var building = Selection.Selected;
            if (building == null)
            {
                return 0;
            }

            _economy.Tick.ForceApply();
            var amount = _economy.Collection.Collect(building);
            if (amount > 0)
            {
                _economy.Persist(_buildings);
                BuildingChanged?.Invoke();
                RefreshCollectableIndicators();
            }

            return amount;
        }

        public void Tick()
        {
            _economy.Tick.Update();
            _dragons?.Tick();
            RefreshCollectableIndicators();
        }

        public void Persist()
        {
            _economy.Persist(_buildings);
            _dragons?.Persist();
        }

        private void RefreshCollectableIndicators()
        {
            foreach (var pair in _views)
            {
                var has = _economy.Production.TryGetState(pair.Key.DefinitionId, out var state) && state.HasCollectable;
                pair.Value.SetCollectable(has);
            }
        }

        private void OnSelectionChanged(BuildingInstance? selected)
        {
            foreach (var pair in _views)
            {
                pair.Value.SetSelected(ReferenceEquals(pair.Key, selected));
            }
        }
    }
}
