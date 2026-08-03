using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.UI;
using Valgor.UI.Chat;

namespace Valgor.City.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class CityHudController : MonoBehaviour
    {
        private const string EnergyPrefsPrefix = "valgor.worldmap.energy.v1";

        private UIDocument _document = null!;
        private CityController _city = null!;
        private BuildingSelectionPresenter? _presenter;
        private IDragonGateway? _dragons;
        private PlayerHudPresenter? _playerPresenter;
        private PlayerHudView? _playerView;
        private VisualElement _resourceRow = null!;
        private readonly Dictionary<ResourceType, Label> _resourceLabels = new();
        private Label _queueBuild = null!;
        private Label _queueResearch = null!;
        private Label _queueTrain = null!;
        private Label _queueHeal = null!;
        private Label _queueMarch = null!;
        private Label _chatBody = null!;
        private Label _compatResources = null!;
        private string _chatChannel = "Sistema";
        private IChatProvider _chat = null!;
        private float _nextHudRefreshUnscaled;

        public BuildingSelectionPresenter? Presenter => _presenter;
        public IChatProvider Chat => _chat;

        public void Initialize(CityController city, IDragonGateway? dragons = null)
        {
            _city = city;
            _dragons = dragons ?? city.Dragons;
            _document = GetComponent<UIDocument>();
            _chat = new LocalSystemChatProvider();
            BetaUiPanels.ApplyTo(_document);
            Build();
            _city.Economy.Wallet.Changed += OnResourceChanged;
            _city.BuildingChanged += RefreshAll;
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

        private void Update()
        {
            _presenter?.Tick();
            if (_city != null &&
                (_city.GetActiveConstructionCount() > 0 || Time.unscaledTime >= _nextHudRefreshUnscaled))
            {
                _nextHudRefreshUnscaled = Time.unscaledTime + 0.5f;
                RefreshAll();
            }
        }

        private void Build()
        {
            var root = _document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;
            root.pickingMode = PickingMode.Ignore;

            _playerView = new PlayerHudView(root);
            _playerPresenter = new PlayerHudPresenter(_city, _playerView);

            // Compat QA / scrapers: label oculto com o resumo monolítico antigo.
            _compatResources = new Label { name = "city-resources" };
            _compatResources.style.display = DisplayStyle.None;
            _compatResources.pickingMode = PickingMode.Ignore;
            root.Add(_compatResources);

            BuildResourceBar(root);
            BuildTopMenus(root);
            BuildQueuePanel(root);
            BuildSideShortcuts(root);
            BuildChat(root);

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

            RefreshAll();
            AttachJourneyGuide(root);
        }

        private void BuildResourceBar(VisualElement root)
        {
            _resourceRow = new VisualElement { name = "resource-bar" };
            _resourceRow.style.position = Position.Absolute;
            _resourceRow.style.left = ValgorResponsiveUi.Compact(230, 200);
            _resourceRow.style.right = ValgorResponsiveUi.Compact(220, 160);
            _resourceRow.style.top = ValgorHudLayoutConfig.Margin;
            _resourceRow.style.height = ValgorResponsiveUi.Compact(
                ValgorHudLayoutConfig.TopBarHeight,
                ValgorHudLayoutConfig.TopBarHeight * 0.9f);
            _resourceRow.style.flexDirection = FlexDirection.Row;
            _resourceRow.style.alignItems = Align.Center;
            _resourceRow.style.justifyContent = Justify.Center;
            _resourceRow.style.paddingLeft = 8;
            _resourceRow.style.paddingRight = 8;
            _resourceRow.pickingMode = PickingMode.Ignore;
            ValgorUiTheme.ApplyPanelFrame(_resourceRow);
            root.Add(_resourceRow);

            AddResourceChip(ResourceType.Food, "Alimento", ValgorIconCatalog.Food);
            AddResourceChip(ResourceType.Wood, "Madeira", ValgorIconCatalog.Wood);
            AddResourceChip(ResourceType.Stone, "Pedra", ValgorIconCatalog.Stone);
            AddResourceChip(ResourceType.Iron, "Minério", ValgorIconCatalog.Ore);
            AddResourceChip(ResourceType.Gold, "Ouro", ValgorIconCatalog.Gold);
            AddResourceChip(ResourceType.Diamonds, "Diamantes", ValgorIconCatalog.Diamonds);
        }

        private void AddResourceChip(ResourceType type, string title, string iconKey)
        {
            var chip = new VisualElement { name = "chip-" + type };
            ValgorUiTheme.ApplyChip(chip);
            chip.pickingMode = PickingMode.Ignore;
            chip.tooltip = title;

            var icon = new Label(IconGlyph(iconKey));
            icon.style.color = ValgorUiTheme.AgedGold;
            icon.style.fontSize = 13;
            icon.style.marginRight = 4;
            icon.pickingMode = PickingMode.Ignore;
            chip.Add(icon);

            var value = new Label("0") { name = "val-" + type };
            value.style.color = ValgorUiTheme.TextPrimary;
            value.style.fontSize = ValgorTypographyConfig.ChipSize;
            value.pickingMode = PickingMode.Ignore;
            chip.Add(value);

            _resourceLabels[type] = value;
            _resourceRow.Add(chip);
        }

        private static string IconGlyph(string key) => key switch
        {
            ValgorIconCatalog.Food => "◆",
            ValgorIconCatalog.Wood => "▲",
            ValgorIconCatalog.Stone => "■",
            ValgorIconCatalog.Ore => "●",
            ValgorIconCatalog.Gold => "★",
            ValgorIconCatalog.Diamonds => "◇",
            _ => "•"
        };

        private void BuildTopMenus(VisualElement root)
        {
            var menus = new VisualElement { name = "top-menus" };
            menus.style.position = Position.Absolute;
            menus.style.right = ValgorResponsiveUi.Compact(130, 100);
            menus.style.top = ValgorHudLayoutConfig.Margin;
            menus.style.flexDirection = FlexDirection.Row;
            menus.style.flexWrap = Wrap.Wrap;
            menus.style.maxWidth = ValgorResponsiveUi.Compact(420, 280);
            menus.pickingMode = PickingMode.Ignore;
            root.Add(menus);

            foreach (var title in CapitalFinalHudConfig.DisabledTopMenus)
            {
                var btn = new Button(() => { }) { text = title };
                btn.tooltip = CapitalFinalHudConfig.ComingSoon;
                ValgorUiTheme.ApplyButton(btn, enabled: false);
                btn.style.marginLeft = 4;
                btn.style.fontSize = ValgorTypographyConfig.SmallSize;
                btn.style.color = ValgorUiTheme.TextMuted;
                menus.Add(btn);
            }
        }

        private void BuildQueuePanel(VisualElement root)
        {
            var panel = new VisualElement { name = "queue-panel" };
            panel.style.position = Position.Absolute;
            panel.style.left = ValgorHudLayoutConfig.Margin;
            panel.style.top = ValgorResponsiveUi.Compact(110, 100);
            panel.style.width = ValgorResponsiveUi.Compact(
                ValgorHudLayoutConfig.QueuePanelWidth,
                ValgorHudLayoutConfig.QueuePanelWidth * 0.92f);
            panel.style.paddingLeft = 8;
            panel.style.paddingRight = 8;
            panel.style.paddingTop = 8;
            panel.style.paddingBottom = 8;
            panel.pickingMode = PickingMode.Ignore;
            ValgorUiTheme.ApplyPanelFrame(panel);
            root.Add(panel);

            var title = new Label("Filas");
            title.style.color = ValgorUiTheme.AgedGoldBright;
            title.style.fontSize = ValgorTypographyConfig.BodySize;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 4;
            title.pickingMode = PickingMode.Ignore;
            panel.Add(title);

            _queueBuild = QueueRow(panel, "Construindo");
            _queueResearch = QueueRow(panel, "Pesquisando");
            _queueTrain = QueueRow(panel, "Treinando", disabled: !CapitalFinalHudConfig.TroopTrainingQueueAvailable);
            _queueHeal = QueueRow(panel, "Cura", disabled: !CapitalFinalHudConfig.HealQueueAvailable);
            _queueMarch = QueueRow(panel, "Marchas");
        }

        private static Label QueueRow(VisualElement parent, string title, bool disabled = false)
        {
            var row = new VisualElement();
            row.style.marginBottom = 4;
            row.pickingMode = PickingMode.Ignore;
            parent.Add(row);

            var head = new Label(title + (disabled ? " · Em breve" : string.Empty));
            head.style.color = disabled ? ValgorUiTheme.TextMuted : ValgorUiTheme.TextPrimary;
            head.style.fontSize = ValgorTypographyConfig.SmallSize;
            head.pickingMode = PickingMode.Ignore;
            row.Add(head);

            var body = new Label("—") { name = "queue-" + title };
            body.style.color = ValgorUiTheme.TextMuted;
            body.style.fontSize = ValgorTypographyConfig.SmallSize;
            body.pickingMode = PickingMode.Ignore;
            row.Add(body);
            return body;
        }

        private void BuildSideShortcuts(VisualElement root)
        {
            var col = new VisualElement { name = "side-shortcuts" };
            col.style.position = Position.Absolute;
            col.style.left = ValgorHudLayoutConfig.Margin;
            col.style.bottom = ValgorResponsiveUi.Compact(200, 170);
            col.style.width = ValgorHudLayoutConfig.SideShortcutWidth + 24;
            col.pickingMode = PickingMode.Ignore;
            root.Add(col);

            col.Add(Shortcut("Construção", () =>
            {
                _city.TrySelectByDefinitionId("castle");
                _presenter?.RefreshCurrent();
            }, enabled: true));
            col.Add(Shortcut("Pesquisa", () =>
            {
                _city.TrySelectByDefinitionId("academy");
                _presenter?.RefreshCurrent();
            }, enabled: true));
            col.Add(Shortcut("Missões", () => { }, enabled: true, note: "Use Missões na barra inferior"));
            col.Add(Shortcut("Explorar", () =>
            {
                _city.Persist();
                if (GameBootstrap.Game != null)
                {
                    StartCoroutine(GameBootstrap.Game.Navigator.GoToWorldMap());
                }
            }, enabled: true));
        }

        private static Button Shortcut(string label, System.Action action, bool enabled, string? note = null)
        {
            var btn = new Button(action) { text = label };
            btn.tooltip = note ?? label;
            ValgorUiTheme.ApplyButton(btn, enabled);
            btn.style.marginBottom = 4;
            btn.style.fontSize = 10;
            btn.style.color = enabled ? ValgorUiTheme.TextPrimary : ValgorUiTheme.TextMuted;
            btn.style.whiteSpace = WhiteSpace.Normal;
            return btn;
        }

        private void BuildChat(VisualElement root)
        {
            var chat = new VisualElement { name = "chat-panel" };
            chat.style.position = Position.Absolute;
            chat.style.left = ValgorHudLayoutConfig.Margin;
            chat.style.bottom = ValgorResponsiveUi.Compact(72, 64);
            chat.style.width = ValgorResponsiveUi.Compact(
                ValgorHudLayoutConfig.ChatWidth,
                ValgorHudLayoutConfig.ChatWidth * 0.85f);
            chat.style.height = ValgorResponsiveUi.Compact(
                ValgorHudLayoutConfig.ChatHeight,
                ValgorHudLayoutConfig.ChatHeight * 0.9f);
            chat.style.paddingLeft = 8;
            chat.style.paddingRight = 8;
            chat.style.paddingTop = 6;
            chat.style.paddingBottom = 6;
            chat.pickingMode = PickingMode.Position;
            ValgorUiTheme.ApplyPanelFrame(chat);
            root.Add(chat);

            var tabs = new VisualElement();
            tabs.style.flexDirection = FlexDirection.Row;
            tabs.style.marginBottom = 4;
            chat.Add(tabs);

            foreach (var ch in _chat.Channels)
            {
                var channel = ch;
                var tab = new Button(() =>
                {
                    _chatChannel = channel;
                    RefreshChat();
                })
                {
                    text = channel
                };
                ValgorUiTheme.ApplyButton(tab, enabled: true);
                tab.style.marginRight = 4;
                tab.style.fontSize = 10;
                tab.style.color = ValgorUiTheme.TextPrimary;
                tabs.Add(tab);
            }

            _chatBody = new Label { name = "chat-body" };
            _chatBody.style.color = ValgorUiTheme.TextPrimary;
            _chatBody.style.fontSize = ValgorTypographyConfig.SmallSize;
            _chatBody.style.whiteSpace = WhiteSpace.Normal;
            _chatBody.style.flexGrow = 1;
            _chatBody.pickingMode = PickingMode.Ignore;
            chat.Add(_chatBody);
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

        private void OnResourceChanged(object? sender, ResourceChangedEvent args) => RefreshAll();

        public void ForceRefreshResources() => RefreshAll();

        private void RefreshAll()
        {
            if (_city == null)
            {
                return;
            }

            _playerPresenter?.Refresh();
            RefreshResources();
            RefreshQueues();
            RefreshChat();
        }

        private void RefreshResources()
        {
            if (_resourceLabels.Count == 0 || _city == null)
            {
                return;
            }

            var w = _city.Economy.Wallet;
            SetChip(ResourceType.Food, w.Get(ResourceType.Food), "Alimento");
            SetChip(ResourceType.Wood, w.Get(ResourceType.Wood), "Madeira");
            SetChip(ResourceType.Stone, w.Get(ResourceType.Stone), "Pedra");
            SetChip(ResourceType.Iron, w.Get(ResourceType.Iron), "Minério");
            SetChip(ResourceType.Gold, w.Get(ResourceType.Gold), "Ouro");
            SetChip(ResourceType.Diamonds, w.Get(ResourceType.Diamonds), "Diamantes");

            if (_compatResources != null)
            {
                var name = LocalPlayerProfile.HasProfile
                    ? LocalPlayerProfile.DisplayName
                    : "Visitante";
                _compatResources.text =
                    $"{name}  ·  Nv.{_city.GetCastleLevel()}  ·  " +
                    $"Ouro {ValgorHudFormat.Full(w.Get(ResourceType.Gold))}  ·  " +
                    $"Comida {ValgorHudFormat.Full(w.Get(ResourceType.Food))}  ·  " +
                    $"Madeira {ValgorHudFormat.Full(w.Get(ResourceType.Wood))}  ·  " +
                    $"Pedra {ValgorHudFormat.Full(w.Get(ResourceType.Stone))}  ·  " +
                    $"Ferro {ValgorHudFormat.Full(w.Get(ResourceType.Iron))}  ·  " +
                    $"Diamantes {ValgorHudFormat.Full(w.Get(ResourceType.Diamonds))}  ·  " +
                    _city.DescribeConstructionQueue();
            }
        }

        private void SetChip(ResourceType type, long amount, string title)
        {
            if (!_resourceLabels.TryGetValue(type, out var label))
            {
                return;
            }

            label.text = ValgorHudFormat.Abbreviate(amount);
            var tip = $"{title}: {ValgorHudFormat.Full(amount)}";
            label.tooltip = tip;
            if (label.parent != null)
            {
                label.parent.tooltip = tip;
            }
        }

        private void RefreshQueues()
        {
            _queueBuild.text = _city.DescribeConstructionQueue();
            _queueResearch.text = _city.DescribeResearchQueue();
            _queueTrain.text = CapitalFinalHudConfig.TroopTrainingQueueAvailable
                ? "Ativo"
                : CapitalFinalHudConfig.ComingSoon;
            _queueHeal.text = CapitalFinalHudConfig.HealQueueAvailable
                ? "Ativo"
                : CapitalFinalHudConfig.ComingSoon;

            _queueMarch.text = "0 · ocioso";
        }

        private void RefreshChat()
        {
            if (_chatBody == null || _chat == null)
            {
                return;
            }

            var msgs = _chat.GetMessages(_chatChannel, 6);
            if (msgs.Count == 0)
            {
                _chatBody.text = _chatChannel == "Sistema"
                    ? "Sem mensagens."
                    : $"{_chatChannel}: {CapitalFinalHudConfig.ComingSoon} (somente sistema local).";
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var m in msgs)
            {
                sb.Append('[').Append(m.Author).Append("] ").AppendLine(m.Body);
            }

            _chatBody.text = sb.ToString().TrimEnd();
        }
    }
}
