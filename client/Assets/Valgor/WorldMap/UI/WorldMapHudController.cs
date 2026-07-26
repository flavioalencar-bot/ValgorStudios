using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class WorldMapHudController : MonoBehaviour
    {
        private UIDocument _document = null!;
        private WorldMapController _map = null!;
        private Label _title = null!;
        private VisualElement _panel = null!;
        private Label _details = null!;

        public void Initialize(WorldMapController map)
        {
            _map = map;
            _document = GetComponent<UIDocument>();
            EnsurePanelSettings();
            Build();
            _map.Selection.SelectionChanged += _ => Refresh();
            _map.Changed += Refresh;
        }

        private void EnsurePanelSettings()
        {
            if (_document.panelSettings != null)
            {
                return;
            }

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            _document.panelSettings = settings;
        }

        private void Build()
        {
            var root = _document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;
            root.pickingMode = PickingMode.Ignore;

            _title = new Label("World Map");
            _title.style.position = Position.Absolute;
            _title.style.left = 18;
            _title.style.top = 18;
            _title.style.fontSize = 28;
            _title.style.color = Color.white;
            root.Add(_title);

            var actions = new VisualElement();
            actions.style.position = Position.Absolute;
            actions.style.right = 18;
            actions.style.top = 18;
            root.Add(actions);
            actions.Add(CreateButton("Voltar para a Cidade", () =>
                StartCoroutine(GameBootstrap.Game.Navigator.GoToCity())));

            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.left = 18;
            _panel.style.bottom = 18;
            _panel.style.width = 340;
            _panel.style.paddingLeft = 14;
            _panel.style.paddingRight = 14;
            _panel.style.paddingTop = 12;
            _panel.style.paddingBottom = 12;
            _panel.style.backgroundColor = new Color(0.04f, 0.08f, 0.06f, 0.92f);
            _details = new Label();
            _details.style.color = Color.white;
            _details.style.whiteSpace = WhiteSpace.Normal;
            _panel.Add(_details);
            _panel.Add(CreateButton("Fechar", () => _map.Selection.Deselect()));
            root.Add(_panel);
            Refresh();
        }

        private Button CreateButton(string text, System.Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginTop = 8;
            button.style.paddingLeft = 12;
            button.style.paddingRight = 12;
            button.style.paddingTop = 7;
            button.style.paddingBottom = 7;
            return button;
        }

        private void Refresh()
        {
            var selected = _map.Selection.Selected;
            _panel.style.display = selected == null ? DisplayStyle.None : DisplayStyle.Flex;
            if (selected == null)
            {
                return;
            }

            var definition = _map.GetDefinition(selected);
            _details.text =
                $"{definition.DisplayName}\nStatus: {selected.Status}\n{definition.Description}\n\nExploração detalhada nas próximas sprints.";
        }
    }
}
