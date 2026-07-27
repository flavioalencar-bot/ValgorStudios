using System;
using UnityEngine.UIElements;

namespace Valgor.WorldMap.Filters
{
    /// <summary>
    /// Painel de filtros recolhível do mapa mundial.
    /// </summary>
    public sealed class WorldMapFilterPanel
    {
        private readonly WorldMapFilterService _filters;
        private readonly VisualElement _root;
        private readonly VisualElement _body;
        private Toggle _cities = null!;
        private Toggle _villages = null!;
        private Toggle _resources = null!;
        private Toggle _creatures = null!;
        private Toggle _dragons = null!;
        private Toggle _landmarks = null!;
        private Toggle _occupied = null!;
        private Toggle _available = null!;
        private bool _suppress;
        private bool _expanded;

        public WorldMapFilterPanel(WorldMapFilterService filters, VisualElement parent)
        {
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _root = new VisualElement();
            _root.style.position = Position.Absolute;
            _root.style.right = 14;
            _root.style.top = 56;
            _root.style.width = 188;
            _root.style.paddingLeft = 8;
            _root.style.paddingRight = 8;
            _root.style.paddingTop = 6;
            _root.style.paddingBottom = 6;
            _root.style.backgroundColor = new UnityEngine.Color(0.08f, 0.1f, 0.12f, 0.92f);
            parent.Add(_root);

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            _root.Add(header);

            var title = new Label("Filtros");
            title.style.color = UnityEngine.Color.white;
            title.style.fontSize = 13;
            header.Add(title);

            var toggle = new Button { text = "Abrir" };
            toggle.style.paddingLeft = 8;
            toggle.style.paddingRight = 8;
            toggle.style.paddingTop = 3;
            toggle.style.paddingBottom = 3;
            toggle.style.fontSize = 11;
            header.Add(toggle);

            _body = new VisualElement();
            _body.style.display = DisplayStyle.None;
            _root.Add(_body);

            _cities = AddToggle("Cidades", value => _filters.SetShowCities(value));
            _villages = AddToggle("Vilarejos", value => _filters.SetShowVillages(value));
            _resources = AddToggle("Recursos", value => _filters.SetShowResources(value));
            _creatures = AddToggle("Criaturas", value => _filters.SetShowCreatures(value));
            _dragons = AddToggle("Dragões", value => _filters.SetShowDragons(value));
            _landmarks = AddToggle("Marcos", value => _filters.SetShowLandmarks(value));
            _occupied = AddToggle("Ocupados", value => _filters.SetShowOccupied(value));
            _available = AddToggle("Disponíveis", value => _filters.SetShowAvailable(value));

            var clear = new Button(() => _filters.ClearFilters()) { text = "Limpar" };
            clear.style.marginTop = 6;
            clear.style.paddingLeft = 8;
            clear.style.paddingRight = 8;
            clear.style.paddingTop = 4;
            clear.style.paddingBottom = 4;
            clear.style.fontSize = 11;
            _body.Add(clear);

            toggle.clicked += () =>
            {
                _expanded = !_expanded;
                _body.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;
                toggle.text = _expanded ? "Fechar" : "Abrir";
            };

            _filters.Changed += SyncFromState;
            SyncFromState();
        }

        public VisualElement Root => _root;

        public void Expand()
        {
            _expanded = true;
            _body.style.display = DisplayStyle.Flex;
        }

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
            toggle.style.fontSize = 11;
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (_suppress)
                {
                    return;
                }

                onChanged(evt.newValue);
            });
            _body.Add(toggle);
            return toggle;
        }
    }
}
