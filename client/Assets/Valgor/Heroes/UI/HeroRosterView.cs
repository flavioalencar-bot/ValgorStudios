using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Heroes.Data;
using Valgor.Heroes.Factions;

namespace Valgor.Heroes.UI
{
    public sealed class HeroCardView
    {
        public VisualElement Root { get; }
        public string HeroId { get; private set; }

        private readonly Label _name;
        private readonly Label _meta;
        private readonly Label _rarity;

        public HeroCardView()
        {
            Root = new VisualElement();
            Root.AddToClassList("hero-card");
            _name = new Label();
            _name.AddToClassList("hero-card__name");
            _rarity = new Label();
            _rarity.AddToClassList("hero-card__rarity");
            _meta = new Label();
            _meta.AddToClassList("hero-card__meta");
            Root.Add(_name);
            Root.Add(_rarity);
            Root.Add(_meta);
        }

        public void Bind(HeroDefinitionSO hero, int level = 1, int stars = 1, int fragments = 0, string activeSkinId = null)
        {
            HeroId = hero.Id;
            _name.text = hero.ResolveDisplayName();
            _rarity.text = hero.Rarity.ToString();
            _meta.text = $"{HeroFactionIds.ToId(hero.Faction)} · {hero.ClassName} · Nv {level} · ★{stars} · Frag {fragments} · Skin {activeSkinId ?? hero.DefaultSkinId}";
        }
    }

    public sealed class SpecialPowerCooldownView
    {
        private readonly VisualElement _fill;
        private readonly Label _timer;
        private readonly VisualElement _root;

        public VisualElement Root => _root;

        public SpecialPowerCooldownView()
        {
            _root = new VisualElement();
            _root.AddToClassList("special-cooldown");
            _fill = new VisualElement();
            _fill.AddToClassList("special-cooldown__fill");
            _timer = new Label();
            _timer.AddToClassList("special-cooldown__timer");
            _root.Add(_fill);
            _root.Add(_timer);
        }

        public void SetReady()
        {
            _fill.style.height = Length.Percent(0);
            _timer.text = "PRONTO";
            _root.RemoveFromClassList("special-cooldown--blocked");
        }

        public void SetActive(float remainingSec, float totalSec)
        {
            var ratio = totalSec <= 0f ? 0f : Mathf.Clamp01(remainingSec / totalSec);
            _fill.style.height = Length.Percent(ratio * 100f);
            _timer.text = $"ATIVO {remainingSec:0}";
            _root.RemoveFromClassList("special-cooldown--blocked");
        }

        public void SetCooldown(float remainingSec, float totalSec)
        {
            var ratio = totalSec <= 0f ? 0f : Mathf.Clamp01(remainingSec / totalSec);
            _fill.style.height = Length.Percent(ratio * 100f);
            _timer.text = $"{remainingSec:0}";
            _root.AddToClassList("special-cooldown--blocked");
        }
    }

    public sealed class SpecialPowerButtonView
    {
        public Button Button { get; }
        public SpecialPowerCooldownView Cooldown { get; }

        public SpecialPowerButtonView(string powerName)
        {
            Button = new Button { text = powerName };
            Button.AddToClassList("special-power-button");
            Cooldown = new SpecialPowerCooldownView();
            Button.Add(Cooldown.Root);
        }

        public void SetInteractable(bool value) => Button.SetEnabled(value);
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class HeroRosterView : MonoBehaviour
    {
        [SerializeField] private HeroCatalogSO catalog;
        [SerializeField] private FactionConfigSO factionConfig;

        private UIDocument _document;
        private VisualElement _cardsRoot;
        private Label _factionHint;
        private HeroFaction? _filter;
        private readonly List<HeroCardView> _cards = new();

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            var root = _document.rootVisualElement;
            _cardsRoot = root.Q<VisualElement>("hero-cards") ?? root;
            _factionHint = root.Q<Label>("faction-hint");

            BindFilterButton(root, "filter-all", null);
            BindFilterButton(root, "filter-rosa", HeroFaction.RosaDeSangue);
            BindFilterButton(root, "filter-asas", HeroFaction.AsasDoAmanhecer);
            BindFilterButton(root, "filter-guarda", HeroFaction.GuardaDaOrdem);
            Rebuild();
        }

        private void BindFilterButton(VisualElement root, string name, HeroFaction? faction)
        {
            var button = root.Q<Button>(name);
            if (button == null) return;
            button.clicked += () =>
            {
                _filter = faction;
                Rebuild();
            };
        }

        private void Rebuild()
        {
            _cardsRoot.Clear();
            _cards.Clear();
            if (catalog == null) return;

            foreach (var hero in catalog.Heroes)
            {
                if (hero == null) continue;
                if (_filter.HasValue && hero.Faction != _filter.Value) continue;

                var card = new HeroCardView();
                card.Bind(hero);
                _cards.Add(card);
                _cardsRoot.Add(card.Root);
            }

            if (_factionHint != null)
            {
                _factionHint.text = _filter.HasValue
                    ? HeroFactionResolver.Describe(_filter.Value)
                    : "Todas as facções";
            }
        }
    }
}
