using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.Core.Modules;

namespace Valgor.City.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class CityHudController : MonoBehaviour
    {
        private UIDocument _document = null!;
        private ResourceWallet _wallet = null!;
        private CityController _city = null!;
        private Label _resources = null!;
        private VisualElement _selectedPanel = null!;
        private Label _selectedText = null!;
        private Button _upgradeButton = null!;

        public void Initialize(ResourceWallet wallet, CityController city)
        {
            _wallet = wallet;
            _city = city;
            _document = GetComponent<UIDocument>();
            EnsurePanelSettings();
            Build();
            _wallet.Changed += OnResourceChanged;
            _city.Selection.SelectionChanged += _ => RefreshSelection();
            _city.BuildingChanged += RefreshSelection;
        }

        private void OnDestroy()
        {
            if (_wallet != null)
            {
                _wallet.Changed -= OnResourceChanged;
            }
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

            _resources = new Label();
            _resources.style.position = Position.Absolute;
            _resources.style.left = 18;
            _resources.style.top = 18;
            _resources.style.paddingLeft = 12;
            _resources.style.paddingRight = 12;
            _resources.style.paddingTop = 8;
            _resources.style.paddingBottom = 8;
            _resources.style.backgroundColor = new Color(0.04f, 0.07f, 0.12f, 0.88f);
            _resources.style.color = Color.white;
            root.Add(_resources);

            var actions = new VisualElement();
            actions.style.position = Position.Absolute;
            actions.style.right = 18;
            actions.style.top = 18;
            root.Add(actions);
            actions.Add(CreateButton("Mapa Mundial", () => StartCoroutine(GameBootstrap.Game.Navigator.GoToWorldMap())));

            if (GameBootstrap.Services != null && GameBootstrap.Services.TryGet<IHeroesGateway>(out var heroes) && heroes.IsAvailable)
            {
                actions.Add(CreateButton("Heróis", () => Debug.Log("Integração de heróis disponível.")));
            }

            actions.Add(CreateButton("Debug: Main Menu", () => StartCoroutine(GameBootstrap.Game.Navigator.GoToMainMenu())));

            _selectedPanel = new VisualElement();
            _selectedPanel.style.position = Position.Absolute;
            _selectedPanel.style.left = 18;
            _selectedPanel.style.bottom = 18;
            _selectedPanel.style.paddingLeft = 14;
            _selectedPanel.style.paddingRight = 14;
            _selectedPanel.style.paddingTop = 12;
            _selectedPanel.style.paddingBottom = 12;
            _selectedPanel.style.backgroundColor = new Color(0.04f, 0.07f, 0.12f, 0.92f);
            _selectedPanel.style.width = 310;
            _selectedText = new Label();
            _selectedText.style.whiteSpace = WhiteSpace.Normal;
            _selectedText.style.color = Color.white;
            _selectedPanel.Add(_selectedText);
            _upgradeButton = CreateButton("Melhorar", OnUpgrade);
            _selectedPanel.Add(_upgradeButton);
            _selectedPanel.Add(CreateButton("Fechar", () => _city.Selection.Deselect()));
            root.Add(_selectedPanel);
            RefreshResources();
            RefreshSelection();
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

        private void OnUpgrade()
        {
            _city.TryUpgradeSelected();
            RefreshSelection();
        }

        private void OnResourceChanged(object? sender, ResourceChangedEvent args) => RefreshResources();

        private void RefreshResources()
        {
            _resources.text = $"Ouro: {_wallet.Get(ResourceType.Gold)}   Comida: {_wallet.Get(ResourceType.Food)}   Madeira: {_wallet.Get(ResourceType.Wood)}   Pedra: {_wallet.Get(ResourceType.Stone)}   Ferro: {_wallet.Get(ResourceType.Iron)}   Essência: {_wallet.Get(ResourceType.DragonEssence)}   Diamantes: {_wallet.Get(ResourceType.Diamonds)}";
        }

        private void RefreshSelection()
        {
            var building = _city.Selection.Selected;
            _selectedPanel.style.display = building == null ? DisplayStyle.None : DisplayStyle.Flex;
            if (building == null)
            {
                return;
            }

            var definition = _city.GetDefinition(building);
            _selectedText.text = $"{definition.DisplayName}\nNível {building.Level}/{definition.MaxLevel}\nEstado: {building.State}\nCusto: {definition.GetUpgradeCost(ResourceType.Gold, building.Level)} ouro";
            _upgradeButton.SetEnabled(building.CanUpgrade(definition));
        }
    }
}
