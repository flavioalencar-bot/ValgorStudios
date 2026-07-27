using System;
using System.Collections.Generic;
using UnityEngine;
using Valgor.WorldMap.Camera;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Locate;
using Valgor.WorldMap.Nodes;
using Valgor.WorldMap.Territory;
using Valgor.WorldMap.Visual;

namespace Valgor.WorldMap.Core
{
    public sealed class WorldMapController
    {
        private readonly WorldMapSession _session;
        private readonly Dictionary<string, WorldNodeView> _nodeViews = new();
        private readonly Dictionary<RegionInstance, RegionDefinition> _regionDefinitions = new();
        private readonly Dictionary<RegionInstance, RegionNodeView> _regionViews = new();
        private readonly Dictionary<string, WorldTerritoryOverlay> _territoryOverlays = new();
        private readonly List<RegionInstance> _regions = new();
        private WorldMapCameraController? _camera;
        private MarchArmyView? _marchArmy;

        public WorldMapController(WorldMapSession session)
        {
            _session = session;
            _session.Selection.SelectionChanged += OnNodeSelectionChanged;
            _session.RegionSelection.SelectionChanged += OnRegionSelectionChanged;
            _session.Changed += () =>
            {
                ApplyNodeVisibility();
                Changed?.Invoke();
            };
            _session.Marches.Changed += (_, _) =>
            {
                ApplyNodeVisibility();
                SyncMarchArmy();
                Changed?.Invoke();
            };
            _session.Filters.Changed += () =>
            {
                ApplyNodeVisibility();
                Changed?.Invoke();
            };
        }

        public WorldMapSession Session => _session;
        public IReadOnlyList<RegionInstance> Regions => _regions;
        public event Action? Changed;

        public void BindCamera(WorldMapCameraController camera)
        {
            _camera = camera;
            _camera.BindPersistence(_session.CameraPersistence);
        }

        public void BindMarchArmy(MarchArmyView army)
        {
            _marchArmy = army;
            SyncMarchArmy();
        }

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
            ApplyNodeVisibility(instance.DefinitionId);
        }

        public void AddTerritoryOverlay(string territoryId, WorldTerritoryOverlay overlay)
        {
            _territoryOverlays[territoryId] = overlay;
            if (_session.TryGetTerritory(territoryId, out var runtime))
            {
                overlay.SetState(runtime.State);
            }
        }

        public RegionDefinition GetRegionDefinition(RegionInstance instance) => _regionDefinitions[instance];

        public void Tick()
        {
            SyncMarchArmy();
            Changed?.Invoke();
        }

        public void Persist()
        {
            _camera?.PersistCurrentPose();
            _session.Persist();
        }

        public void RestoreSelectionVisuals()
        {
            OnNodeSelectionChanged(_session.Selection.Selected);
        }

        public void ApplyNodeVisibility()
        {
            foreach (var pair in _nodeViews)
            {
                ApplyNodeVisibility(pair.Key);
            }
        }

        public bool TryFocus(WorldCameraFocusRequest request)
        {
            if (_camera == null || request == null)
            {
                return false;
            }

            _camera.FocusOn(request.X, request.Z, request.OrthographicSize);
            return true;
        }

        public bool TryLocatePlayerHome(out string error)
        {
            if (!_session.Locator.TryLocatePlayerHome(out var target, out error))
            {
                return false;
            }

            _session.Selection.Select(_session.GetNode(target.Id));
            return TryFocus(_session.Locator.CreateFocusRequest(target, _session.Settings.LocateHomeZoom));
        }

        public bool TryLocateActiveMarch(out string error)
        {
            if (!_session.Locator.TryLocateActiveMarch(out var target, out error))
            {
                return false;
            }

            _session.Selection.Select(_session.GetNode(target.Id));
            if (_marchArmy != null && _marchArmy.gameObject.activeInHierarchy)
            {
                return TryFocus(new WorldCameraFocusRequest(
                    _marchArmy.transform.position.x,
                    _marchArmy.transform.position.z,
                    _session.Settings.LocateHomeZoom));
            }

            return TryFocus(_session.Locator.CreateFocusRequest(target));
        }

        public bool TryLocateSelectedNode(out string error)
        {
            if (!_session.Locator.TryLocateSelectedNode(out var target, out error))
            {
                return false;
            }

            return TryFocus(_session.Locator.CreateFocusRequest(target));
        }

        public bool TryLocateCreature(string creatureNodeId, out string error)
        {
            if (!_session.Locator.TryLocateCreature(creatureNodeId, out var target, out error))
            {
                return false;
            }

            _session.Selection.Select(_session.GetNode(target.Id));
            return TryFocus(_session.Locator.CreateFocusRequest(target));
        }

        public bool TryLocateResource(string resourceNodeId, out string error)
        {
            if (!_session.Locator.TryLocateResource(resourceNodeId, out var target, out error))
            {
                return false;
            }

            _session.Selection.Select(_session.GetNode(target.Id));
            return TryFocus(_session.Locator.CreateFocusRequest(target));
        }

        private void SyncMarchArmy()
        {
            _marchArmy?.Sync(
                _session.Marches.Active,
                id =>
                {
                    var definition = _session.GetDefinition(id);
                    return new Vector3(definition.X, 0f, definition.Z);
                },
                _session.Clock.UtcNow,
                _session.DescribeHeroFormation());
        }

        private void ApplyNodeVisibility(string nodeId)
        {
            if (!_nodeViews.TryGetValue(nodeId, out var view) || view == null)
            {
                return;
            }

            var visible = _session.IsNodeVisible(nodeId);
            view.gameObject.SetActive(visible);
            if (visible)
            {
                view.RefreshVisual();
            }
        }

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
