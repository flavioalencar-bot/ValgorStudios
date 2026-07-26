using System;
using System.Collections.Generic;
using UnityEngine;
using Valgor.City.Buildings;
using Valgor.City.Data;

namespace Valgor.City.Core
{
    public sealed class CityController
    {
        private readonly ResourceWallet _wallet;
        private readonly Dictionary<BuildingInstance, BuildingDefinition> _definitions = new();
        private readonly Dictionary<BuildingInstance, BuildingView> _views = new();

        public CityController(ResourceWallet wallet, BuildingSelectionService selection)
        {
            _wallet = wallet;
            Selection = selection;
            Selection.SelectionChanged += OnSelectionChanged;
        }

        public BuildingSelectionService Selection { get; }
        public event Action? BuildingChanged;

        public void Add(BuildingSlot slot, BuildingInstance instance, BuildingDefinition definition, BuildingView view)
        {
            slot.Initialize(slot.SlotId, definition.Id, instance);
            _definitions.Add(instance, definition);
            _views.Add(instance, view);
            view.Clicked += _ => Selection.Select(instance);
        }

        public BuildingDefinition GetDefinition(BuildingInstance instance) => _definitions[instance];

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
            BuildingChanged?.Invoke();
            return true;
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
