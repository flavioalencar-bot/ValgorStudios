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
        private const string EnergyPrefsPrefix = "valgor.worldmap.energy.v1";

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
            _city.BuildingChanged += () =>
            {
                RefreshSelection();
                RefreshResources();
            };
            // Não chamar ForceApply a partir deste handler — evita reentrada Production.Changed.
            _city.Economy.Production.Changed += (_, __) =>
            {
                if (_selectedPanel != null && _selectedPanel.style.display == DisplayStyle.Flex)
                {
                    RefreshSelection();
                }
            };
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
            BetaUiPanels.ApplyTo(_document);
        }

        private void Build()
        {
            var root = _document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;
            root.pickingMode = PickingMode.Ignore;

            _resources = new Label();
            _resources.style.position = Position.Absolute;
            _resources.style.left = 12;
            _resources.style.right = 12;
            _resources.style.top = 8;
            _resources.style.height = 44;
            _resources.style.paddingLeft = 14;
            _resources.style.paddingRight = 14;
            _resources.style.paddingTop = 10;
            _resources.style.paddingBottom = 10;
            _resources.style.backgroundColor = new Color(0.08f, 0.09f, 0.1f, 0.88f);
            _resources.style.color = BetaVisualTheme.TextPrimary;
            _resources.style.borderBottomWidth = 2;
            _resources.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _resources.style.unityTextAlign = TextAnchor.MiddleLeft;
            _resources.style.fontSize = 13;
            _resources.pickingMode = PickingMode.Ignore;
            root.Add(_resources);

            _selectedPanel = new VisualElement();
            _selectedPanel.style.position = Position.Absolute;
            _selectedPanel.style.left = 16;
            _selectedPanel.style.bottom = 78;
            _selectedPanel.style.paddingLeft = 14;
            _selectedPanel.style.paddingRight = 14;
            _selectedPanel.style.paddingTop = 12;
            _selectedPanel.style.paddingBottom = 12;
            _selectedPanel.style.backgroundColor = new Color(0.1f, 0.11f, 0.12f, 0.94f);
            _selectedPanel.style.borderTopWidth = 2;
            _selectedPanel.style.borderBottomWidth = 2;
            _selectedPanel.style.borderLeftWidth = 2;
            _selectedPanel.style.borderRightWidth = 2;
            _selectedPanel.style.borderTopColor = BetaVisualTheme.AgedGold;
            _selectedPanel.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _selectedPanel.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _selectedPanel.style.borderRightColor = BetaVisualTheme.AgedGold;
            _selectedPanel.style.width = 340;
            _selectedText = new Label();
            _selectedText.style.whiteSpace = WhiteSpace.Normal;
            _selectedText.style.color = BetaVisualTheme.TextPrimary;
            _selectedText.style.fontSize = 13;
            _selectedPanel.Add(_selectedText);
            _collectButton = CreateButton("Coletar", () =>
            {
                var amount = _city.CollectSelected();
                _feedback.text = amount > 0
                    ? $"+{amount} coletado!"
                    : "Nada para coletar agora.";
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
            AttachJourneyGuide(root);
        }

        private void AttachJourneyGuide(VisualElement root)
        {
            BetaJourneyGuide.AttachOrRefresh(root, () =>
            {
                var step = LocalPlayerProfile.TutorialStep;
                switch (step)
                {
                    case LocalPlayerProfile.TutorialSteps.OpenHeroes:
                        _city.Persist();
                        BetaJourneyGuide.NotifyHeroesOpened();
                        StartCoroutine(GameBootstrap.Game.Navigator.GoToHeroes());
                        break;
                    case LocalPlayerProfile.TutorialSteps.DragonTower:
                        BetaFocusHints.RequestDragonTower();
                        _city.TrySelectByDefinitionId(BetaFocusHints.DragonTowerBuildingId);
                        BetaJourneyGuide.NotifyDragonTowerFocused();
                        RefreshSelection();
                        BetaJourneyGuide.AttachOrRefresh(root, null);
                        break;
                    case LocalPlayerProfile.TutorialSteps.OpenMap:
                        _city.Persist();
                        BetaJourneyGuide.NotifyWorldMapOpened();
                        StartCoroutine(GameBootstrap.Game.Navigator.GoToWorldMap());
                        break;
                }
            });
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
            button.style.fontSize = 13;
            return button;
        }

        private void OnUpgrade()
        {
            var building = _city.Selection.Selected;
            if (building == null)
            {
                return;
            }

            var definition = _city.GetDefinition(building);
            if (_city.TryUpgradeSelected())
            {
                var duration = definition.GetUpgradeDuration(building.Level);
                _feedback.text = building.State == BuildingState.Upgrading
                    ? $"{definition.DisplayName}: melhoria iniciada ({(int)duration.TotalSeconds}s)"
                    : $"{definition.DisplayName} → Nv.{building.Level}";
            }
            else
            {
                _feedback.text = _city.GetUpgradeBlockReason(building, definition) ?? "Não foi possível melhorar.";
            }

            RefreshSelection();
            RefreshResources();
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
            var name = LocalPlayerProfile.HasProfile
                ? LocalPlayerProfile.DisplayName
                : "Visitante";
            var level = _city.GetCastleLevel();
            var energy = ReadEnergyDisplay();
            _resources.text =
                $"{name}  ·  Nv.{level}  ·  " +
                $"Ouro {w.Get(ResourceType.Gold)}  ·  " +
                $"Comida {w.Get(ResourceType.Food)}  ·  " +
                $"Madeira {w.Get(ResourceType.Wood)}  ·  " +
                $"Pedra {w.Get(ResourceType.Stone)}  ·  " +
                $"Ferro {w.Get(ResourceType.Iron)}  ·  " +
                $"Essência {w.Get(ResourceType.DragonEssence)}  ·  " +
                $"Energia {energy}";
        }

        private static string ReadEnergyDisplay()
        {
            if (!PlayerPrefs.HasKey(EnergyPrefsPrefix + ".current"))
            {
                return "100/100";
            }

            var current = PlayerPrefs.GetInt(EnergyPrefsPrefix + ".current", 100);
            var max = Mathf.Max(1, PlayerPrefs.GetInt(EnergyPrefsPrefix + ".max", 100));
            return $"{current}/{max}";
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
            var builder = new StringBuilder();
            builder.AppendLine(definition.DisplayName);
            builder.AppendLine($"Nível {building.Level}/{definition.MaxLevel}");
            if (building.State == BuildingState.Upgrading && building.UpgradeCompletesAtUtc.HasValue)
            {
                var remaining = building.UpgradeCompletesAtUtc.Value - _city.Economy.Clock.UtcNow;
                if (remaining < TimeSpan.Zero)
                {
                    remaining = TimeSpan.Zero;
                }

                builder.AppendLine($"Melhorando → Nv.{building.Level + 1} ({(int)remaining.TotalSeconds}s)");
            }
            else
            {
                builder.AppendLine($"Estado: {FriendlyState(building.State)}");
            }

            var block = _city.GetUpgradeBlockReason(building, definition);
            if (!string.IsNullOrEmpty(block))
            {
                builder.AppendLine(block);
            }

            builder.AppendLine(BuildProductionBlock(building));

            var isTower = string.Equals(building.DefinitionId, "dragon-tower", StringComparison.Ordinal);
            if (isTower && _dragons != null)
            {
                builder.AppendLine($"Ninho: {_dragons.RoostOccupantCount}/{_dragons.RoostCapacity}");
            }

            _selectedText.text = builder.ToString();
            _upgradeButton.SetEnabled(_city.CanUpgrade(building, definition));
            var canCollect = _city.Economy.Production.TryGetState(building.DefinitionId, out var state) && state.HasCollectable;
            _collectButton.SetEnabled(canCollect);
            _collectButton.style.display = ProductionCatalog.TryGet(building.DefinitionId, out _)
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            _feedButton.style.display = isTower ? DisplayStyle.Flex : DisplayStyle.None;
            _hatchButton.style.display = isTower ? DisplayStyle.Flex : DisplayStyle.None;
            _evolveButton.style.display = isTower ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static string FriendlyState(BuildingState state) => state switch
        {
            BuildingState.Ready => "Pronto",
            BuildingState.Available => "Disponível",
            BuildingState.Locked => "Bloqueado",
            BuildingState.Upgrading => "Melhorando",
            _ => state.ToString()
        };

        private string BuildProductionBlock(BuildingInstance building)
        {
            if (!ProductionCatalog.TryGet(building.DefinitionId, out var productionDef))
            {
                return string.Empty;
            }

            // Leitura apenas — ForceApply aqui reentra em Production.Changed → StackOverflow.
            var rate = _city.Economy.Production.GetRatePerHour(building);
            var capacity = _city.Economy.Production.GetCapacity(building);
            _city.Economy.Production.TryGetState(building.DefinitionId, out var state);
            var accumulated = state?.Accumulated ?? 0;
            return $"Produção: {rate:0.#}/h · Acumulado {accumulated}/{capacity} ({productionDef.Resource})";
        }
    }
}
