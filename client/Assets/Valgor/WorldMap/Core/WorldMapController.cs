using System;
using System.Collections.Generic;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Nodes;

namespace Valgor.WorldMap.Core
{
    public sealed class WorldMapController
    {
        private readonly WorldMapSession _session;
        private readonly Dictionary<string, WorldNodeView> _nodeViews = new();
        private readonly Dictionary<RegionInstance, RegionDefinition> _regionDefinitions = new();
        private readonly Dictionary<RegionInstance, RegionNodeView> _regionViews = new();
        private readonly List<RegionInstance> _regions = new();

        public WorldMapController(WorldMapSession session)
        {
            _session = session;
            _session.Selection.SelectionChanged += OnNodeSelectionChanged;
            _session.RegionSelection.SelectionChanged += OnRegionSelectionChanged;
            _session.Changed += () => Changed?.Invoke();
            _session.Marches.Changed += (_, _) => Changed?.Invoke();
        }

        public WorldMapSession Session => _session;
        public IReadOnlyList<RegionInstance> Regions => _regions;
        public event Action? Changed;

        public void AddRegion(RegionInstance instance, RegionDefinition definition, RegionNodeView view)
        {
            _regions.Add(instance);
            _regionDefinitions[instance] = definition;
            _regionViews[instance] = view;
            view.Clicked += _ => _session.RegionSelection.Select(instance);
        }

        public void AddNode(WorldNodeInstance instance, WorldMapNodeDefinition definition, WorldNodeView view)
        {
            _nodeViews[instance.DefinitionId] = view;
            view.Clicked += _ => _session.Selection.Select(instance);
        }

        public RegionDefinition GetRegionDefinition(RegionInstance instance) => _regionDefinitions[instance];

        public void Tick() => _session.Tick();

        public void Persist() => _session.Persist();

        private void OnNodeSelectionChanged(WorldNodeInstance? selected)
        {
            foreach (var pair in _nodeViews)
            {
                pair.Value.SetSelected(string.Equals(pair.Key, selected?.DefinitionId, StringComparison.Ordinal));
            }

            Changed?.Invoke();
        }

        private void OnRegionSelectionChanged(RegionInstance? selected)
        {
            foreach (var pair in _regionViews)
            {
                pair.Value.SetSelected(ReferenceEquals(pair.Key, selected));
            }

            Changed?.Invoke();
        }
    }
}
