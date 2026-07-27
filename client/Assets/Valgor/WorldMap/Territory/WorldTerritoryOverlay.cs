using UnityEngine;
using Valgor.City.Visual;
using Valgor.WorldMap.Nodes;

namespace Valgor.WorldMap.Territory
{
    /// <summary>
    /// Overlay visual de território sobre o disco da região.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public sealed class WorldTerritoryOverlay : MonoBehaviour
    {
        private Renderer _renderer = null!;
        private RegionNodeView? _regionView;

        public string TerritoryId { get; private set; } = string.Empty;
        public WorldTerritoryState State { get; private set; } = WorldTerritoryState.Neutral;

        public void Initialize(string territoryId, WorldTerritoryState state, RegionNodeView? regionView = null)
        {
            TerritoryId = territoryId;
            State = state;
            _regionView = regionView;
            _renderer = GetComponent<Renderer>();
            Apply();
        }

        public void SetState(WorldTerritoryState state)
        {
            State = state;
            Apply();
        }

        public void Apply()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            var color = WorldTerritoryColorResolver.Resolve(State);
            // Opaco tintado — URP Lit sem alpha pipeline evita “branco lavado”.
            CityVisualMaterials.Apply(_renderer, new Color(color.R, color.G, color.B, 1f));
        }
    }
}
