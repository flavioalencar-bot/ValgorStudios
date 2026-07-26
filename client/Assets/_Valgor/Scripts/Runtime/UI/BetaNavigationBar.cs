using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.Core;
using Valgor.Navigation;

namespace Valgor.UI
{
    /// <summary>
    /// Barra de navegação provisória DDOL da Beta Técnica 0.1.
    /// </summary>
    public sealed class BetaNavigationBar : MonoBehaviour
    {
        private UIDocument _document = null!;
        private Label _route = null!;

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
            EnsurePanelSettings();
            Build();
        }

        private void Update()
        {
            if (_route == null || GameBootstrap.Game == null)
            {
                return;
            }

            _route.text = $"{ValgorVersion.Display} · {GameBootstrap.Game.StateMachine.Current}";
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
            settings.sortingOrder = 100;
            _document.panelSettings = settings;
        }

        private void Build()
        {
            var root = _document.rootVisualElement;
            root.Clear();
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.height = 52;
            root.pickingMode = PickingMode.Position;

            var bar = new VisualElement();
            bar.style.flexDirection = FlexDirection.Row;
            bar.style.alignItems = Align.Center;
            bar.style.flexGrow = 1;
            bar.style.backgroundColor = BetaVisualTheme.BackgroundPanel;
            bar.style.borderBottomWidth = 2;
            bar.style.borderBottomColor = BetaVisualTheme.AgedGold;
            bar.style.paddingLeft = 12;
            bar.style.paddingRight = 12;
            root.Add(bar);

            bar.Add(NavButton("Menu", () => Run(n => n.GoToMainMenu())));
            bar.Add(NavButton("Cidade", () => Run(n => n.GoToCity())));
            bar.Add(NavButton("Heróis", () => Run(n => n.GoToHeroes())));
            bar.Add(NavButton("Dragões", () => Run(n => n.GoToDragonTower())));
            bar.Add(NavButton("Mapa", () => Run(n => n.GoToWorldMap())));

            _route = new Label();
            _route.style.marginLeft = 16;
            _route.style.color = BetaVisualTheme.AgedGoldBright;
            _route.style.fontSize = 12;
            _route.style.unityFontStyleAndWeight = FontStyle.Bold;
            bar.Add(_route);
        }

        private Button NavButton(string text, Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginRight = 8;
            button.style.paddingLeft = 14;
            button.style.paddingRight = 14;
            button.style.paddingTop = 8;
            button.style.paddingBottom = 8;
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
            button.style.fontSize = 13;
            return button;
        }

        private void Run(Func<Navigation.GameNavigator, System.Collections.IEnumerator> route)
        {
            if (GameBootstrap.Game == null)
            {
                Debug.LogError("[Valgor] Navigator indisponível.");
                return;
            }

            StartCoroutine(route(GameBootstrap.Game.Navigator));
        }
    }
}
