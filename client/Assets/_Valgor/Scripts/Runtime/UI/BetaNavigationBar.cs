using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.Core;
using Valgor.Navigation;

namespace Valgor.UI
{
    /// <summary>
    /// Barra de navegação inferior — apenas cenas de gameplay (não Splash/Loading/Menu).
    /// </summary>
    public sealed class BetaNavigationBar : MonoBehaviour
    {
        private UIDocument _document = null!;
        private VisualElement _missionsPanel = null!;
        private Label _missionsBody = null!;

        public static void Ensure()
        {
            if (FindFirstObjectByType<BetaNavigationBar>() != null)
            {
                return;
            }

            var host = new GameObject("BetaNavigationBar");
            DontDestroyOnLoad(host);
            host.AddComponent<BetaNavigationBar>();
        }

        private void Awake()
        {
            _document = gameObject.GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            BetaUiPanels.ApplyTo(_document, sortingOrder: 100);
            Build();
        }

        private void Update()
        {
            if (_document == null || GameBootstrap.Game == null)
            {
                return;
            }

            var state = GameBootstrap.Game.StateMachine.Current;
            var show = state is GameState.PlayerCity or GameState.Heroes or GameState.WorldMap;
            _document.rootVisualElement.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Build()
        {
            var root = _document.rootVisualElement;
            root.Clear();
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.bottom = 0;
            root.style.top = StyleKeyword.Auto;
            root.style.height = ValgorResponsiveUi.Compact(64, 56);
            root.pickingMode = PickingMode.Position;

            var bar = new VisualElement();
            bar.style.flexDirection = FlexDirection.Row;
            bar.style.alignItems = Align.Center;
            bar.style.justifyContent = Justify.Center;
            bar.style.flexGrow = 1;
            bar.style.backgroundColor = new Color(0.08f, 0.09f, 0.1f, 0.92f);
            bar.style.borderTopWidth = 2;
            bar.style.borderTopColor = BetaVisualTheme.AgedGold;
            bar.style.paddingLeft = ValgorResponsiveUi.Compact(12, 6);
            bar.style.paddingRight = ValgorResponsiveUi.Compact(12, 6);
            root.Add(bar);

            bar.Add(NavButton("Cidade", () =>
            {
                PlayerPrefs.Save();
                HideMissions();
                Run(n => n.GoToCity());
            }));
            bar.Add(NavButton("Heróis", () =>
            {
                PlayerPrefs.Save();
                HideMissions();
                Run(n => n.GoToHeroes());
            }));
            bar.Add(NavButton("Dragões", () =>
            {
                PlayerPrefs.Save();
                HideMissions();
                Run(n => n.GoToDragonTower());
            }));
            bar.Add(NavButton("Mapa", () =>
            {
                PlayerPrefs.Save();
                HideMissions();
                BetaMissions.Notify(MissionEvent.OpenWorldMap);
                Run(n => n.GoToWorldMap());
            }));
            bar.Add(NavButton("Missões", ToggleMissions));

            BuildMissionsPanel(root);
        }

        private void BuildMissionsPanel(VisualElement root)
        {
            _missionsPanel = new VisualElement { name = "missions-panel" };
            _missionsPanel.style.display = DisplayStyle.None;
            _missionsPanel.style.position = Position.Absolute;
            _missionsPanel.style.left = Length.Percent(50);
            _missionsPanel.style.bottom = ValgorResponsiveUi.Compact(76, 62);
            _missionsPanel.style.translate = new Translate(Length.Percent(-50), 0);
            _missionsPanel.style.width = Length.Percent(ValgorResponsiveUi.IsNarrowScreen ? 92 : 70);
            _missionsPanel.style.maxWidth = 420;
            _missionsPanel.style.maxHeight = ValgorResponsiveUi.Compact(420, 320);
            _missionsPanel.style.backgroundColor = BetaVisualTheme.BackgroundPanel;
            _missionsPanel.style.borderTopWidth = 2;
            _missionsPanel.style.borderBottomWidth = 2;
            _missionsPanel.style.borderLeftWidth = 2;
            _missionsPanel.style.borderRightWidth = 2;
            _missionsPanel.style.borderTopColor = BetaVisualTheme.AgedGold;
            _missionsPanel.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _missionsPanel.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _missionsPanel.style.borderRightColor = BetaVisualTheme.AgedGold;
            _missionsPanel.style.paddingLeft = ValgorResponsiveUi.Compact(16, 10);
            _missionsPanel.style.paddingRight = ValgorResponsiveUi.Compact(16, 10);
            _missionsPanel.style.paddingTop = ValgorResponsiveUi.Compact(14, 8);
            _missionsPanel.style.paddingBottom = ValgorResponsiveUi.Compact(12, 8);
            _missionsPanel.style.overflow = Overflow.Hidden;

            var title = new Label("Missões — Capítulo do Comandante");
            title.style.color = BetaVisualTheme.AgedGoldBright;
            title.style.fontSize = ValgorResponsiveUi.Compact(16, 14);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            title.style.flexShrink = 0;
            _missionsPanel.Add(title);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 80;
            scroll.style.maxHeight = ValgorResponsiveUi.Compact(300, 180);
            _missionsBody = new Label { name = "missions-body" };
            _missionsBody.style.whiteSpace = WhiteSpace.Normal;
            _missionsBody.style.color = BetaVisualTheme.TextPrimary;
            _missionsBody.style.fontSize = 13;
            scroll.Add(_missionsBody);
            _missionsPanel.Add(scroll);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.marginTop = 10;
            row.Add(NavButton("Recolher", ClaimCurrentReward));
            row.Add(NavButton("Fechar", HideMissions));
            _missionsPanel.Add(row);

            root.Add(_missionsPanel);
        }

        public void DebugToggleMissions() => ToggleMissions();

        private void ToggleMissions()
        {
            if (_missionsPanel.style.display == DisplayStyle.Flex)
            {
                HideMissions();
                return;
            }

            RefreshMissionsBody();
            _missionsPanel.style.display = DisplayStyle.Flex;
        }

        private void HideMissions()
        {
            if (_missionsPanel != null)
            {
                _missionsPanel.style.display = DisplayStyle.None;
            }
        }

        private void RefreshMissionsBody()
        {
            var lines = new System.Text.StringBuilder();
            for (var i = 0; i < BetaMissions.MissionCount; i++)
            {
                var state = BetaMissions.IsClaimed(i)
                    ? "Recolhida"
                    : BetaMissions.IsComplete(i)
                        ? "Concluída — pronta"
                        : i == BetaMissions.ActiveChapter
                            ? "Em andamento"
                            : "Bloqueada";
                lines.AppendLine($"{i + 1}. {BetaMissions.Titles[i]} [{state}]");
                lines.AppendLine($"   {BetaMissions.Objectives[i]}");
                lines.AppendLine($"   Recompensa: {BetaMissions.DiamondRewards[i]} diamantes");
                lines.AppendLine();
            }

            _missionsBody.text = lines.ToString().TrimEnd();
        }

        private void ClaimCurrentReward()
        {
            for (var i = 0; i < BetaMissions.MissionCount; i++)
            {
                if (!BetaMissions.CanClaim(i))
                {
                    continue;
                }

                if (!BetaMissions.TryClaim(i, out var diamonds, out var error))
                {
                    Debug.LogWarning("[Valgor] Missão: " + error);
                    RefreshMissionsBody();
                    return;
                }

                GrantDiamonds(diamonds);
                Debug.Log($"[Valgor] Missão recompensa: +{diamonds} diamantes ({BetaMissions.Titles[i]})");
                RefreshMissionsBody();
                return;
            }

            RefreshMissionsBody();
        }

        private static void GrantDiamonds(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            // Runtime não referencia Valgor.City — fila diamantes para a City consumir.
            var pending = PlayerPrefs.GetInt("valgor.missions.v1.pendingDiamonds", 0) + amount;
            PlayerPrefs.SetInt("valgor.missions.v1.pendingDiamonds", pending);
            PlayerPrefs.Save();
            Debug.Log($"[Valgor] Missão: +{amount} diamantes enfileirados (pendente={pending}).");
        }

        private Button NavButton(string text, Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginLeft = 3;
            button.style.marginRight = 3;
            button.style.paddingLeft = ValgorResponsiveUi.Compact(10, 4);
            button.style.paddingRight = ValgorResponsiveUi.Compact(10, 4);
            button.style.paddingTop = 8;
            button.style.paddingBottom = 8;
            button.style.minWidth = ValgorResponsiveUi.Compact(72, 56);
            button.style.flexGrow = 1;
            button.style.flexShrink = 1;
            button.style.fontSize = ValgorResponsiveUi.Compact(14, 12);
            button.style.backgroundColor = BetaVisualTheme.ButtonFace;
            button.style.color = BetaVisualTheme.TextPrimary;
            button.style.borderTopColor = BetaVisualTheme.ButtonBorder;
            button.style.borderBottomColor = BetaVisualTheme.ButtonBorder;
            button.style.borderLeftColor = BetaVisualTheme.ButtonBorder;
            button.style.borderRightColor = BetaVisualTheme.ButtonBorder;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            return button;
        }

        private void Run(Func<GameNavigator, System.Collections.IEnumerator> route)
        {
            if (GameBootstrap.Game == null)
            {
                Debug.LogWarning("[Valgor] Navigator indisponível.");
                return;
            }

            StartCoroutine(route(GameBootstrap.Game.Navigator));
        }
    }
}
