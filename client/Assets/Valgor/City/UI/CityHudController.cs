using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Core;
using Valgor.City.Data;
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
        private Label _resources = null!;
        private BuildingSelectionPresenter? _presenter;
        private IDragonGateway? _dragons;

        public BuildingSelectionPresenter? Presenter => _presenter;

        public void Initialize(CityController city, IDragonGateway? dragons = null)
        {
            _city = city;
            _dragons = dragons ?? city.Dragons;
            _document = GetComponent<UIDocument>();
            BetaUiPanels.ApplyTo(_document);
            Build();
            _city.Economy.Wallet.Changed += OnResourceChanged;
            _city.BuildingChanged += RefreshResources;
            _city.Economy.Production.Changed += (_, __) => _presenter?.RefreshCurrent();
        }

        private void OnDestroy()
        {
            if (_city?.Economy?.Wallet != null)
            {
                _city.Economy.Wallet.Changed -= OnResourceChanged;
            }

            _presenter?.Dispose();
        }

        private float _nextHudRefreshUnscaled;

        private void Update()
        {
            _presenter?.Tick();
            if (_city != null &&
                (_city.GetActiveConstructionCount() > 0 || Time.unscaledTime >= _nextHudRefreshUnscaled))
            {
                _nextHudRefreshUnscaled = Time.unscaledTime + 0.5f;
                RefreshResources();
            }
        }

        private void Build()
        {
            var root = _document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;
            root.pickingMode = PickingMode.Ignore;

            _resources = new Label();
            _resources.name = "city-resources";
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

            _presenter = new BuildingSelectionPresenter(
                _city,
                root,
                _dragons,
                () =>
                {
                    _city.Persist();
                    if (GameBootstrap.Game == null)
                    {
                        return;
                    }

                    StartCoroutine(GameBootstrap.Game.Navigator.GoToWorldMap());
                });

            RefreshResources();
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
                        StartCoroutine(GameBootstrap.Game.Navigator.GoToHeroes());
                        break;
                    case LocalPlayerProfile.TutorialSteps.OpenDragons:
                        BetaFocusHints.RequestDragonTower();
                        _city.TrySelectByDefinitionId(BetaFocusHints.DragonTowerBuildingId);
                        BetaJourneyGuide.AttachOrRefresh(root, null);
                        break;
                    case LocalPlayerProfile.TutorialSteps.OpenMap:
                        _city.Persist();
                        StartCoroutine(GameBootstrap.Game.Navigator.GoToWorldMap());
                        break;
                }
            });
        }

        private void OnResourceChanged(object? sender, ResourceChangedEvent args) => RefreshResources();

        public void ForceRefreshResources() => RefreshResources();

        private void RefreshResources()
        {
            if (_resources == null || _city == null)
            {
                return;
            }

            var w = _city.Economy.Wallet;
            var name = LocalPlayerProfile.HasProfile
                ? LocalPlayerProfile.DisplayName
                : "Visitante";
            var level = _city.GetCastleLevel();
            var energy = ReadEnergyDisplay();
            _resources.text =
                $"{name}  ·  Nv.{level}  ·  " +
                $"Ouro {FormatAmount(w.Get(ResourceType.Gold))}  ·  " +
                $"Comida {FormatAmount(w.Get(ResourceType.Food))}  ·  " +
                $"Madeira {FormatAmount(w.Get(ResourceType.Wood))}  ·  " +
                $"Pedra {FormatAmount(w.Get(ResourceType.Stone))}  ·  " +
                $"Ferro {FormatAmount(w.Get(ResourceType.Iron))}  ·  " +
                $"Essência {FormatAmount(w.Get(ResourceType.DragonEssence))}  ·  " +
                $"Diamantes {FormatAmount(w.Get(ResourceType.Diamonds))}  ·  " +
                $"Energia {energy}  ·  " +
                _city.DescribeConstructionQueue();
        }

        private static string FormatAmount(long amount) =>
            amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));

        private static string ReadEnergyDisplay()
        {
            if (!PlayerPrefs.HasKey(EnergyPrefsPrefix + ".current"))
            {
                return "100/100";
            }

            var current = PlayerPrefs.GetInt(EnergyPrefsPrefix + ".current", 100);
            var max = Mathf.Max(1, PlayerPrefs.GetInt(EnergyPrefsPrefix + ".max", 100));
            var culture = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
            return $"{current.ToString("N0", culture)}/{max.ToString("N0", culture)}";
        }
    }
}
