using System;
using UnityEngine.UIElements;

namespace Valgor.WorldMap.Filters
{
    /// <summary>
    /// Painel UI Toolkit de filtros do mapa mundial.
    /// </summary>
    public sealed class WorldMapFilterPanel
    {
        private readonly WorldMapFilterService _filters;
        private readonly VisualElement _root;
        private Toggle _cities = null!;
        private Toggle _villages = null!;
        private Toggle _resources = null!;
        private Toggle _creatures = null!;
        private Toggle _dragons = null!;
        private Toggle _landmarks = null!;
        private Toggle _occupied = null!;
        private Toggle _available = null!;
        private bool _suppress;

        public WorldMapFilterPanel(WorldMapFilterService filters, VisualElement parent)
        {
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _root = new VisualElement();
            _root.style.position = Position.Absolute;
            _root.style.right = 18;
            _root.style.top = 70;
            _root.style.width = 200;
            _root.style.paddingLeft = 10;
            _root.style.paddingRight = 10;
            _root.style.paddingTop = 8;
            _root.style.paddingBottom = 8;
            _root.style.backgroundColor = new UnityEngine.Color(0.04f, 0.08f, 0.06f, 0.92f);
            parent.Add(_root);

            var title = new Label("Filtros");
            title.style.color = UnityEngine.Color.white;
            title.style.fontSize = 15;
            title.style.marginBottom = 4;
            _root.Add(title);

            _cities = AddToggle("Cidades", value => _filters.SetShowCities(value));
            _villages = AddToggle("Vilarejos", value => _filters.SetShowVillages(value));
            _resources = AddToggle("Recursos", value => _filters.SetShowResources(value));
            _creatures = AddToggle("Criaturas", value => _filters.SetShowCreatures(value));
            _dragons = AddToggle("Dragões", value => _filters.SetShowDragons(value));
            _landmarks = AddToggle("Marcos", value => _filters.SetShowLandmarks(value));
            _occupied = AddToggle("Ocupados", value => _filters.SetShowOccupied(value));
            _available = AddToggle("Disponíveis", value => _filters.SetShowAvailable(value));

            var clear = new Button(() => _filters.ClearFilters()) { text = "Limpar filtros" };
            clear.style.marginTop = 8;
            clear.style.paddingLeft = 8;
            clear.style.paddingRight = 8;
            clear.style.paddingTop = 5;
            clear.style.paddingBottom = 5;
            _root.Add(clear);

            _filters.Changed += SyncFromState;
            SyncFromState();
        }

        public VisualElement Root => _root;

        public void SyncFromState()
        {
            _suppress = true;
            var state = _filters.State;
            _cities.SetValueWithoutNotify(state.ShowCities);
            _villages.SetValueWithoutNotify(state.ShowVillages);
            _resources.SetValueWithoutNotify(state.ShowResources);
            _creatures.SetValueWithoutNotify(state.ShowCreatures);
            _dragons.SetValueWithoutNotify(state.ShowDragons);
            _landmarks.SetValueWithoutNotify(state.ShowLandmarks);
            _occupied.SetValueWithoutNotify(state.ShowOccupied);
            _available.SetValueWithoutNotify(state.ShowAvailable);
            _suppress = false;
        }

        private Toggle AddToggle(string label, Action<bool> onChanged)
        {
            var toggle = new Toggle(label);
            toggle.style.marginTop = 2;
            toggle.style.color = UnityEngine.Color.white;
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (_suppress)
                {
                    return;
                }

                onChanged(evt.newValue);
            });
            _root.Add(toggle);
            return toggle;
        }
    }
}
