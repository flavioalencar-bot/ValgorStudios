using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.Production;
using Valgor.Core.Modules;

namespace Valgor.City.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class CityHudController : MonoBehaviour
    {
        private UIDocument _document = null!;
        private CityController _city = null!;
        private Label _resources = null!;
        private VisualElement _selectedPanel = null!;
        private Label _selectedText = null!;
        private Button _upgradeButton = null!;
        private Button _collectButton = null!;

        public void Initialize(CityController city)
        {
            _city = city;
            _document = GetComponent<UIDocument>();
            EnsurePanelSettings();
            Build();
            _city.Economy.Wallet.Changed += OnResourceChanged;
            _city.Selection.SelectionChanged += _ => RefreshSelection();
            _city.BuildingChanged += RefreshSelection;
            _city.Economy.Production.Changed += (_, __) => RefreshSelection();
        }

        private void OnDestroy()
        {
            if (_city?.Economy?.Wallet != null)
            {
                _city.Economy.Wallet.Changed -= OnResourceChanged;
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
            _resources.pickingMode = PickingMode.Ignore;
            root.Add(_resources);

            var actions = new VisualElement();
            actions.style.position = Position.Absolute;
            actions.style.right = 18;
            actions.style.top = 18;
            root.Add(actions);
            actions.Add(CreateButton("Mapa Mundial", () =>
            {
                _city.Persist();
                StartCoroutine(GameBootstrap.Game.Navigator.GoToWorldMap());
            }));

            if (GameBootstrap.Services != null &&
                GameBootstrap.Services.TryGet<IHeroesGateway>(out var heroes) &&
                heroes.IsAvailable)
            {
                actions.Add(CreateButton("Heróis", () => Debug.Log("Integração de heróis disponível.")));
            }

            actions.Add(CreateButton("Debug: Main Menu", () =>
            {
                _city.Persist();
                StartCoroutine(GameBootstrap.Game.Navigator.GoToMainMenu());
            }));

            _selectedPanel = new VisualElement();
            _selectedPanel.style.position = Position.Absolute;
            _selectedPanel.style.left = 18;
            _selectedPanel.style.bottom = 18;
            _selectedPanel.style.paddingLeft = 14;
            _selectedPanel.style.paddingRight = 14;
            _selectedPanel.style.paddingTop = 12;
            _selectedPanel.style.paddingBottom = 12;
            _selectedPanel.style.backgroundColor = new Color(0.04f, 0.07f, 0.12f, 0.92f);
            _selectedPanel.style.width = 340;
            _selectedText = new Label();
            _selectedText.style.whiteSpace = WhiteSpace.Normal;
            _selectedText.style.color = Color.white;
            _selectedPanel.Add(_selectedText);
            _collectButton = CreateButton("Coletar", () =>
            {
                _city.CollectSelected();
                RefreshResources();
                RefreshSelection();
            });
            _selectedPanel.Add(_collectButton);
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
            var w = _city.Economy.Wallet;
            _resources.text =
                $"Ouro: {w.Get(ResourceType.Gold)}   Comida: {w.Get(ResourceType.Food)}   Madeira: {w.Get(ResourceType.Wood)}   " +
                $"Pedra: {w.Get(ResourceType.Stone)}   Ferro: {w.Get(ResourceType.Iron)}   Essência: {w.Get(ResourceType.DragonEssence)}   Diamantes: {w.Get(ResourceType.Diamonds)}";
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
            var productionBlock = BuildProductionBlock(building);
            _selectedText.text =
                $"{definition.DisplayName}\nNível {building.Level}/{definition.MaxLevel}\nEstado: {building.State}\n" +
                $"Custo upgrade: {definition.GetUpgradeCost(ResourceType.Gold, building.Level)} ouro\n{productionBlock}";
            _upgradeButton.SetEnabled(building.CanUpgrade(definition));
            var canCollect = _city.Economy.Production.TryGetState(building.DefinitionId, out var state) && state.HasCollectable;
            _collectButton.SetEnabled(canCollect);
            _collectButton.style.display = ProductionCatalog.TryGet(building.DefinitionId, out _)
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private string BuildProductionBlock(BuildingInstance building)
        {
            if (!ProductionCatalog.TryGet(building.DefinitionId, out var productionDef))
            {
                return "Sem produção passiva";
            }

            _city.Economy.Tick.ForceApply();
            var rate = _city.Economy.Production.GetRatePerHour(building);
            var capacity = _city.Economy.Production.GetCapacity(building);
            _city.Economy.Production.TryGetState(building.DefinitionId, out var state);
            var accumulated = state?.Accumulated ?? 0;
            var eta = OfflineProductionCalculator.EstimateTimeToFill(rate, accumulated, capacity);
            var etaText = eta == null
                ? "—"
                : eta.Value <= TimeSpan.Zero
                    ? "cheio"
                    : $"{eta.Value.TotalHours:0.0} h";

            return
                $"Recurso: {productionDef.Resource}\n" +
                $"Produção: {rate:0.#}/h\n" +
                $"Acumulado: {accumulated}/{capacity}\n" +
                $"Tempo para lotar: {etaText}";
        }
    }
}
