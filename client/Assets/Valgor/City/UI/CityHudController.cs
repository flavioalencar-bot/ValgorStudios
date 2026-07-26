using System;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.Production;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.UI;

namespace Valgor.City.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class CityHudController : MonoBehaviour
    {
        private UIDocument _document = null!;
        private CityController _city = null!;
        private IDragonGateway? _dragons;
        private Label _resources = null!;
        private VisualElement _selectedPanel = null!;
        private Label _selectedText = null!;
        private Button _upgradeButton = null!;
        private Button _collectButton = null!;
        private Button _feedButton = null!;
        private Button _hatchButton = null!;
        private Button _evolveButton = null!;
        private Label _feedback = null!;

        public void Initialize(CityController city, IDragonGateway? dragons = null)
        {
            _city = city;
            _dragons = dragons ?? city.Dragons;
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
            _resources.style.top = 64;
            _resources.style.paddingLeft = 12;
            _resources.style.paddingRight = 12;
            _resources.style.paddingTop = 8;
            _resources.style.paddingBottom = 8;
            _resources.style.backgroundColor = BetaVisualTheme.BackgroundPanel;
            _resources.style.color = BetaVisualTheme.TextPrimary;
            _resources.style.borderLeftWidth = 2;
            _resources.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _resources.pickingMode = PickingMode.Ignore;
            root.Add(_resources);

            var actions = new VisualElement();
            actions.style.position = Position.Absolute;
            actions.style.right = 18;
            actions.style.top = 64;
            root.Add(actions);
            actions.Add(CreateButton("Mapa Mundial", () =>
            {
                _city.Persist();
                StartCoroutine(GameBootstrap.Game.Navigator.GoToWorldMap());
            }));
            actions.Add(CreateButton("Heróis", () =>
            {
                _city.Persist();
                StartCoroutine(GameBootstrap.Game.Navigator.GoToHeroes());
            }));
            actions.Add(CreateButton("Torre dos Dragões", () =>
            {
                _city.TrySelectByDefinitionId(BetaFocusHints.DragonTowerBuildingId);
                RefreshSelection();
            }));
            actions.Add(CreateButton("Menu Principal", () =>
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
            _selectedPanel.style.backgroundColor = BetaVisualTheme.BackgroundPanel;
            _selectedPanel.style.borderTopWidth = 2;
            _selectedPanel.style.borderBottomWidth = 2;
            _selectedPanel.style.borderLeftWidth = 2;
            _selectedPanel.style.borderRightWidth = 2;
            _selectedPanel.style.borderTopColor = BetaVisualTheme.AgedGold;
            _selectedPanel.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _selectedPanel.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _selectedPanel.style.borderRightColor = BetaVisualTheme.AgedGold;
            _selectedPanel.style.width = 380;
            _selectedText = new Label();
            _selectedText.style.whiteSpace = WhiteSpace.Normal;
            _selectedText.style.color = BetaVisualTheme.TextPrimary;
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
            _feedButton = CreateButton("Alimentar dragão", OnFeed);
            _selectedPanel.Add(_feedButton);
            _hatchButton = CreateButton("Chocar ovo", OnHatch);
            _selectedPanel.Add(_hatchButton);
            _evolveButton = CreateButton("Evoluir dragão", OnEvolve);
            _selectedPanel.Add(_evolveButton);
            _feedback = new Label();
            _feedback.style.color = BetaVisualTheme.AgedGoldBright;
            _feedback.style.marginTop = 6;
            _feedback.style.whiteSpace = WhiteSpace.Normal;
            _selectedPanel.Add(_feedback);
            _selectedPanel.Add(CreateButton("Fechar", () => _city.Selection.Deselect()));
            root.Add(_selectedPanel);
            RefreshResources();
            RefreshSelection();
        }

        private Button CreateButton(string text, Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginTop = 8;
            button.style.paddingLeft = 12;
            button.style.paddingRight = 12;
            button.style.paddingTop = 8;
            button.style.paddingBottom = 8;
            button.style.backgroundColor = BetaVisualTheme.ButtonFace;
            button.style.color = BetaVisualTheme.TextPrimary;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = BetaVisualTheme.ButtonBorder;
            button.style.borderBottomColor = BetaVisualTheme.ButtonBorder;
            button.style.borderLeftColor = BetaVisualTheme.ButtonBorder;
            button.style.borderRightColor = BetaVisualTheme.ButtonBorder;
            button.style.fontSize = 14;
            return button;
        }

        private void OnUpgrade()
        {
            _city.TryUpgradeSelected();
            RefreshSelection();
        }

        private void OnFeed()
        {
            if (_dragons == null)
            {
                return;
            }

            foreach (var status in _dragons.GetDragonStatuses())
            {
                if (status.StateLabel is "HUNGRY" or "RESTING" or "READY" or "JUVENILE")
                {
                    _feedback.text = _dragons.TryFeed(status.DragonId, out var error)
                        ? "Dragão alimentado."
                        : error;
                    RefreshResources();
                    RefreshSelection();
                    return;
                }
            }

            _feedback.text = "Nenhum dragão disponível para alimentar.";
        }

        private void OnHatch()
        {
            if (_dragons == null)
            {
                return;
            }

            _feedback.text = _dragons.TryUnlockAndHatch("ash-drake", out var error)
                ? "Incubação iniciada."
                : error;
            RefreshSelection();
        }

        private void OnEvolve()
        {
            if (_dragons == null)
            {
                return;
            }

            foreach (var status in _dragons.GetDragonStatuses())
            {
                if (_dragons.TryEvolve(status.DragonId, out var error))
                {
                    _feedback.text = $"{status.DisplayName} evoluiu!";
                    RefreshSelection();
                    return;
                }

                if (!string.Equals(error, "Esta espécie não possui evolução.", StringComparison.Ordinal) &&
                    !string.Equals(error, "Evolução indisponível neste estado.", StringComparison.Ordinal))
                {
                    _feedback.text = error;
                    return;
                }
            }

            _feedback.text = "Nenhum dragão elegível para evoluir.";
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
            var builder = new StringBuilder();
            builder.AppendLine($"{definition.DisplayName}  ·  PLACEHOLDER");
            builder.AppendLine($"Nível {building.Level}/{definition.MaxLevel}");
            builder.AppendLine($"Estado: {building.State}");
            builder.AppendLine($"Custo upgrade: {definition.GetUpgradeCost(ResourceType.Gold, building.Level)} ouro");
            builder.AppendLine(productionBlock);

            var isTower = string.Equals(building.DefinitionId, "dragon-tower", StringComparison.Ordinal);
            if (isTower && _dragons != null)
            {
                builder.AppendLine($"Ninho: {_dragons.RoostOccupantCount}/{_dragons.RoostCapacity}");
                builder.AppendLine("Modelos de dragão: PLACEHOLDER visual");
                foreach (var status in _dragons.GetDragonStatuses())
                {
                    builder.AppendLine(
                        $"• {status.DisplayName}: {status.StateLabel} | {status.GrowthStageLabel} | " +
                        $"fome {status.Hunger}/{status.MaxHunger} | stamina {status.Stamina} | vínculo {status.BondLevel}");
                }
            }

            _selectedText.text = builder.ToString();
            _upgradeButton.SetEnabled(building.CanUpgrade(definition));
            var canCollect = _city.Economy.Production.TryGetState(building.DefinitionId, out var state) && state.HasCollectable;
            _collectButton.SetEnabled(canCollect);
            _collectButton.style.display = ProductionCatalog.TryGet(building.DefinitionId, out _)
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            _feedButton.style.display = isTower ? DisplayStyle.Flex : DisplayStyle.None;
            _hatchButton.style.display = isTower ? DisplayStyle.Flex : DisplayStyle.None;
            _evolveButton.style.display = isTower ? DisplayStyle.Flex : DisplayStyle.None;
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
