using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Heroes.Data;
using Valgor.Heroes.Factions;
using Valgor.Heroes.Preview360;
using Valgor.Heroes.SpecialPowers;

namespace Valgor.Heroes.UI
{
    /// <summary>
    /// Demo screen: roster, faction filters, detail panel, special-power simulation and 360 preview.
    /// Combat results remain client prediction only.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HeroesDemoController : MonoBehaviour
    {
        [SerializeField] private HeroCatalogSO catalog;
        [SerializeField] private FactionConfigSO factionConfig;
        [SerializeField] private HeroPreviewController previewController;

        private UIDocument _document;
        private VisualElement _cardsRoot;
        private VisualElement _previewImage;
        private Label _factionHint;
        private Label _detailName;
        private Label _detailTitle;
        private Label _detailMeta;
        private Label _detailPower;
        private Label _powerState;
        private Label _powerTimer;
        private Button _activateButton;
        private VisualElement _powerFill;
        private HeroFaction? _filter;
        private HeroDefinitionSO _selected;
        private readonly Dictionary<string, HeroRuntimeState> _runtimes = new(StringComparer.Ordinal);
        private readonly SpecialPowerStateMachine _machine = new();
        private double _demoTime;

        public HeroCatalogSO Catalog
        {
            get => catalog;
            set => catalog = value;
        }

        public FactionConfigSO FactionConfig
        {
            get => factionConfig;
            set => factionConfig = value;
        }

        public void BindPreview(HeroPreviewController preview)
        {
            previewController = preview;
        }

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _demoTime = 0d;
            EnsureCatalogBindings();
        }

        private void EnsureCatalogBindings()
        {
            if (catalog != null && factionConfig != null) return;

#if UNITY_EDITOR
            if (catalog == null)
            {
                catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroCatalogSO>(
                    "Assets/Valgor/Heroes/Data/Generated/HeroCatalog.asset");
            }

            if (factionConfig == null)
            {
                factionConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<FactionConfigSO>(
                    "Assets/Valgor/Heroes/Data/Generated/FactionConfig.asset");
            }
#endif
        }

        private void OnEnable()
        {
            var root = _document.rootVisualElement;
            _cardsRoot = root.Q<VisualElement>("hero-cards");
            _previewImage = root.Q<VisualElement>("hero-preview-image");
            _factionHint = root.Q<Label>("faction-hint");
            _detailName = root.Q<Label>("detail-name");
            _detailTitle = root.Q<Label>("detail-title");
            _detailMeta = root.Q<Label>("detail-meta");
            _detailPower = root.Q<Label>("detail-power");
            _powerState = root.Q<Label>("power-state");
            _powerTimer = root.Q<Label>("power-timer");
            _activateButton = root.Q<Button>("activate-special");
            _powerFill = root.Q<VisualElement>("power-fill");

            BindFilter(root, "filter-all", null);
            BindFilter(root, "filter-rosa", HeroFaction.RosaDeSangue);
            BindFilter(root, "filter-asas", HeroFaction.AsasDoAmanhecer);
            BindFilter(root, "filter-guarda", HeroFaction.GuardaDaOrdem);

            if (_activateButton != null)
            {
                _activateButton.clicked -= OnActivateClicked;
                _activateButton.clicked += OnActivateClicked;
            }

            if (previewController != null && _previewImage != null)
            {
                previewController.BindUi(_previewImage);
            }

            EnsureRuntimes();
            RebuildRoster();
            if (_selected == null && catalog != null && catalog.Heroes.Count > 0)
            {
                SelectHero(catalog.Heroes[0]);
            }
            else if (_selected != null)
            {
                RefreshDetail();
                UpdatePreview(hero: _selected);
            }
        }

        private void Update()
        {
            _demoTime += Time.deltaTime;
            if (_selected == null) return;
            RefreshPowerUi();
        }

        private void BindFilter(VisualElement root, string name, HeroFaction? faction)
        {
            var button = root.Q<Button>(name);
            if (button == null) return;
            button.clicked += () =>
            {
                _filter = faction;
                RebuildRoster();
            };
        }

        private void EnsureRuntimes()
        {
            if (catalog == null) return;
            foreach (var hero in catalog.Heroes)
            {
                if (hero == null || string.IsNullOrWhiteSpace(hero.Id)) continue;
                if (_runtimes.ContainsKey(hero.Id)) continue;
                _runtimes[hero.Id] = new HeroRuntimeState { HeroId = hero.Id };
            }
        }

        private void RebuildRoster()
        {
            if (_cardsRoot == null) return;
            _cardsRoot.Clear();
            if (catalog == null) return;

            foreach (var hero in catalog.Heroes)
            {
                if (hero == null) continue;
                if (_filter.HasValue && hero.Faction != _filter.Value) continue;

                var card = BuildCard(hero);
                _cardsRoot.Add(card);
            }

            if (_factionHint != null)
            {
                _factionHint.text = _filter.HasValue
                    ? HeroFactionResolver.Describe(_filter.Value)
                    : $"Todos · {catalog.Heroes.Count} heróis";
            }
        }

        private VisualElement BuildCard(HeroDefinitionSO hero)
        {
            var card = new Button();
            card.AddToClassList("hero-card");
            if (_selected != null && _selected.Id == hero.Id)
            {
                card.AddToClassList("hero-card--selected");
            }

            var name = new Label(hero.ResolveDisplayName());
            name.AddToClassList("hero-card__name");
            var title = new Label(hero.Title);
            title.AddToClassList("hero-card__title");
            var rarity = new Label(hero.Rarity.ToString());
            rarity.AddToClassList("hero-card__rarity");
            var faction = new Label(HeroFactionIds.ToId(hero.Faction));
            faction.AddToClassList("hero-card__faction");
            var powerName = hero.SpecialPower != null ? hero.SpecialPower.DisplayName : "—";
            var power = new Label(powerName);
            power.AddToClassList("hero-card__power");

            card.Add(name);
            card.Add(title);
            card.Add(rarity);
            card.Add(faction);
            card.Add(power);
            card.clicked += () => SelectHero(hero);
            return card;
        }

        private void SelectHero(HeroDefinitionSO hero)
        {
            _selected = hero;
            RebuildRoster();
            RefreshDetail();
            UpdatePreview(hero);
        }

        private void UpdatePreview(HeroDefinitionSO hero)
        {
            if (previewController == null || hero == null) return;
            previewController.ShowHero(hero.Id, hero.Faction);
        }

        private void RefreshDetail()
        {
            if (_selected == null) return;

            if (_detailName != null) _detailName.text = _selected.ResolveDisplayName();
            if (_detailTitle != null) _detailTitle.text = _selected.Title;
            if (_detailMeta != null)
            {
                _detailMeta.text =
                    $"{_selected.Rarity} · {HeroFactionIds.ToId(_selected.Faction)} · {_selected.ClassName}\n" +
                    $"{_selected.Role} · {_selected.Position}\n" +
                    $"Arma: {_selected.WeaponId} · Elemento: {_selected.ElementId}";
            }

            if (_detailPower != null && _selected.SpecialPower != null)
            {
                var p = _selected.SpecialPower;
                _detailPower.text =
                    $"{p.DisplayName}\nAtivo {p.ActiveDurationSec:0}s · Recarga {p.CooldownSec:0}s";
            }

            RefreshPowerUi();
        }

        private void RefreshPowerUi()
        {
            if (_selected == null || !_runtimes.TryGetValue(_selected.Id, out var runtime))
            {
                return;
            }

            var state = _machine.Evaluate(_demoTime, runtime);
            runtime.SpecialState = state;
            var power = _selected.SpecialPower;

            if (_powerState != null)
            {
                _powerState.text = state.ToString().ToUpperInvariant();
                _powerState.RemoveFromClassList("power-state--ready");
                _powerState.RemoveFromClassList("power-state--active");
                _powerState.RemoveFromClassList("power-state--cooldown");
                _powerState.AddToClassList(state switch
                {
                    SpecialPowerState.Ready => "power-state--ready",
                    SpecialPowerState.Active => "power-state--active",
                    _ => "power-state--cooldown"
                });
            }

            float remaining = 0f;
            float total = 1f;
            if (state == SpecialPowerState.Active && power != null)
            {
                remaining = (float)(runtime.ActiveUntilServerTime - _demoTime);
                total = Mathf.Max(0.01f, power.ActiveDurationSec);
            }
            else if (state == SpecialPowerState.Cooldown && power != null)
            {
                remaining = (float)(runtime.CooldownUntilServerTime - _demoTime);
                total = Mathf.Max(0.01f, power.CooldownSec);
            }

            if (_powerTimer != null)
            {
                _powerTimer.text = state == SpecialPowerState.Ready
                    ? "Disponível"
                    : $"{Mathf.Max(0f, remaining):0.0}s";
            }

            if (_powerFill != null)
            {
                var ratio = state == SpecialPowerState.Ready ? 0f : Mathf.Clamp01(remaining / total);
                _powerFill.style.height = Length.Percent(ratio * 100f);
            }

            if (_activateButton != null)
            {
                _activateButton.SetEnabled(state == SpecialPowerState.Ready);
                _activateButton.text = state == SpecialPowerState.Ready
                    ? "Ativar poder especial"
                    : "Indisponível";
            }
        }

        private void OnActivateClicked()
        {
            if (_selected?.SpecialPower == null) return;
            if (!_runtimes.TryGetValue(_selected.Id, out var runtime)) return;

            var state = _machine.Evaluate(_demoTime, runtime);
            if (!_machine.CanActivate(state)) return;

            _machine.PredictLocalActivation(runtime, _selected.SpecialPower, _demoTime);
            RefreshPowerUi();
        }
    }
}
