using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Core;
using Valgor.Heroes.Data;
using Valgor.Heroes.Factions;
using Valgor.Heroes.Preview360;
using Valgor.Heroes.SpecialPowers;
using Valgor.UI;

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

        private Label _previewLabel;

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
            _previewLabel = root.Q<Label>("preview-label");

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
            if (catalog != null && catalog.Heroes.Count > 0)
            {
                HeroDefinitionSO pick = null;
                foreach (var hero in catalog.Heroes)
                {
                    if (hero != null && string.Equals(hero.Id, "HERO_VORTEX_000", StringComparison.Ordinal))
                    {
                        pick = hero;
                        break;
                    }
                }

                SelectHero(pick != null ? pick : catalog.Heroes[0]);
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

            var portrait = new VisualElement();
            portrait.AddToClassList("hero-card__portrait");
            portrait.style.backgroundColor = PortraitColor(hero);
            var portraitMark = new Label(Initials(hero.ResolveDisplayName()));
            portraitMark.AddToClassList("hero-card__portrait-mark");
            portrait.Add(portraitMark);
            card.Add(portrait);

            var name = new Label(hero.ResolveDisplayName());
            name.AddToClassList("hero-card__name");
            var title = new Label(hero.Title);
            title.AddToClassList("hero-card__title");
            var rarity = new Label(RarityLabel(hero.Rarity));
            rarity.AddToClassList("hero-card__rarity");
            var stars = new Label(StarsFor(hero.Rarity));
            stars.AddToClassList("hero-card__stars");
            var faction = new Label(HeroFactionIds.ToDisplayName(hero.Faction));
            faction.AddToClassList("hero-card__faction");
            var level = new Label("Nv.1");
            level.AddToClassList("hero-card__level");

            card.Add(name);
            card.Add(title);
            card.Add(rarity);
            card.Add(stars);
            card.Add(faction);
            card.Add(level);
            card.clicked += () => SelectHero(hero);
            return card;
        }

        private static string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ');
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0][0].ToString() + parts[^1][0]).ToUpperInvariant();
        }

        private static Color PortraitColor(HeroDefinitionSO hero) => hero.Faction switch
        {
            HeroFaction.GuardaDaOrdem => new Color(0.18f, 0.22f, 0.32f),
            HeroFaction.AsasDoAmanhecer => new Color(0.28f, 0.24f, 0.16f),
            HeroFaction.RosaDeSangue => new Color(0.32f, 0.14f, 0.18f),
            _ => new Color(0.22f, 0.24f, 0.28f)
        };

        private static string RarityLabel(HeroRarity rarity) => rarity switch
        {
            HeroRarity.Comum => "Comum",
            HeroRarity.Rara => "Rara",
            HeroRarity.Epica => "Épica",
            HeroRarity.Lendaria => "Lendária",
            HeroRarity.Mitica => "Mítica",
            _ => rarity.ToString()
        };

        private static string StarsFor(HeroRarity rarity) => rarity switch
        {
            HeroRarity.Comum => "★",
            HeroRarity.Rara => "★★",
            HeroRarity.Epica => "★★★",
            HeroRarity.Lendaria => "★★★★",
            HeroRarity.Mitica => "★★★★★",
            _ => "★"
        };

        private void SelectHero(HeroDefinitionSO hero)
        {
            _selected = hero;
            RebuildRoster();
            RefreshDetail();
            UpdatePreview(hero);
            if (hero != null &&
                (string.Equals(hero.Id, "HERO_VORTEX_000", StringComparison.Ordinal) ||
                 string.Equals(hero.ResolveDisplayName(), "Vortex", StringComparison.OrdinalIgnoreCase)))
            {
                BetaMissions.Notify(MissionEvent.ViewVortex);
                BetaJourneyGuide.NotifyVortexViewed();
            }
        }

        private void UpdatePreview(HeroDefinitionSO hero)
        {
            if (previewController == null || hero == null) return;
            previewController.ShowHero(hero.Id, hero.Faction);
            if (_previewLabel != null)
            {
                _previewLabel.text = "Arraste para girar · scroll para zoom";
            }
        }

        private void RefreshDetail()
        {
            if (_selected == null) return;

            if (_detailName != null) _detailName.text = _selected.ResolveDisplayName();
            if (_detailTitle != null) _detailTitle.text = _selected.Title;
            if (_detailMeta != null)
            {
                var marchLine = string.Equals(_selected.Id, "HERO_VORTEX_000", System.StringComparison.Ordinal)
                    ? "\nFormação de marcha: Vortex · Poder 280"
                    : "\nFormação de marcha: Vortex é o líder";
                _detailMeta.text =
                    $"{RarityLabel(_selected.Rarity)} · {HeroFactionIds.ToDisplayName(_selected.Faction)} · {_selected.ClassName}\n" +
                    $"{_selected.Role} · {_selected.Position}\n" +
                    $"Arma: {_selected.WeaponId} · Elemento: {_selected.ElementId}" +
                    marchLine;
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
            previewController?.PlaySpecialPower();
            RefreshPowerUi();
        }
    }
}
