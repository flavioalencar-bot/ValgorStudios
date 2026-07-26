using System;
using System.Collections.Generic;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Nodes;

namespace Valgor.WorldMap.Core
{
    public sealed class WorldMapController
    {
        private readonly Dictionary<RegionInstance, RegionDefinition> _definitions = new();
        private readonly Dictionary<RegionInstance, RegionNodeView> _views = new();
        private readonly List<RegionInstance> _regions = new();

        public WorldMapController(RegionSelectionService selection)
        {
            Selection = selection;
            Selection.SelectionChanged += OnSelectionChanged;
        }

        public RegionSelectionService Selection { get; }
        public IReadOnlyList<RegionInstance> Regions => _regions;
        public event Action? Changed;

        public void Add(RegionInstance instance, RegionDefinition definition, RegionNodeView view)
        {
            _regions.Add(instance);
            _definitions[instance] = definition;
            _views[instance] = view;
            view.Clicked += _ => Selection.Select(instance);
        }

        public RegionDefinition GetDefinition(RegionInstance instance) => _definitions[instance];

        private void OnSelectionChanged(RegionInstance? selected)
        {
            foreach (var pair in _views)
            {
                pair.Value.SetSelected(ReferenceEquals(pair.Key, selected));
            }

            Changed?.Invoke();
        }
    }
}
