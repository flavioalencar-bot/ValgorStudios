using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.Core;
using Valgor.Navigation;

namespace Valgor.UI
{
    /// <summary>
    /// Barra de navegação inferior única da beta (sem duplicar botões laterais da City).
    /// </summary>
    public sealed class BetaNavigationBar : MonoBehaviour
    {
        private UIDocument _document = null!;
        private Label _hint = null!;

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
            if (_hint == null || GameBootstrap.Game == null)
            {
                return;
            }

            var state = GameBootstrap.Game.StateMachine.Current;
            _hint.text = state switch
            {
                GameState.PlayerCity => "Cidade",
                GameState.MainMenu => "Menu",
                GameState.Heroes => "Heróis",
                GameState.WorldMap => "Mapa",
                GameState.Loading or GameState.Bootstrapping => string.Empty,
                _ => string.Empty
            };

            var hideOnBoot = state is GameState.Bootstrapping or GameState.Loading;
            _document.rootVisualElement.style.display = hideOnBoot ? DisplayStyle.None : DisplayStyle.Flex;
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
            root.style.height = 64;
            root.pickingMode = PickingMode.Position;

            var bar = new VisualElement();
            bar.style.flexDirection = FlexDirection.Row;
            bar.style.alignItems = Align.Center;
            bar.style.justifyContent = Justify.Center;
            bar.style.flexGrow = 1;
            bar.style.backgroundColor = new Color(0.08f, 0.09f, 0.1f, 0.92f);
            bar.style.borderTopWidth = 2;
            bar.style.borderTopColor = BetaVisualTheme.AgedGold;
            bar.style.paddingLeft = 12;
            bar.style.paddingRight = 12;
            root.Add(bar);

            bar.Add(NavButton("Cidade", () =>
            {
                PlayerPrefs.Save();
                Run(n => n.GoToCity());
            }));
            bar.Add(NavButton("Heróis", () =>
            {
                PlayerPrefs.Save();
                Run(n => n.GoToHeroes());
            }));
            bar.Add(NavButton("Dragões", () =>
            {
                PlayerPrefs.Save();
                Run(n => n.GoToDragonTower());
            }));
            bar.Add(NavButton("Mapa", () =>
            {
                PlayerPrefs.Save();
                Run(n => n.GoToWorldMap());
            }));
            bar.Add(NavButton("Missões", () =>
            {
                // Feedback jogável — sem log técnico na experiência do jogador.
                var root = _document.rootVisualElement;
                var toast = root.Q("missions-toast") ?? new Label("Missões em breve.") { name = "missions-toast" };
                toast.style.position = Position.Absolute;
                toast.style.left = Length.Percent(50);
                toast.style.bottom = 80;
                toast.style.translate = new Translate(Length.Percent(-50), 0);
                toast.style.color = BetaVisualTheme.AgedGoldBright;
                toast.style.fontSize = 14;
                toast.style.backgroundColor = new Color(0.08f, 0.09f, 0.1f, 0.92f);
                toast.style.paddingLeft = 12;
                toast.style.paddingRight = 12;
                toast.style.paddingTop = 8;
                toast.style.paddingBottom = 8;
                if (toast.parent == null)
                {
                    root.Add(toast);
                }
            }));

            _hint = new Label();
            _hint.style.position = Position.Absolute;
            _hint.style.right = 16;
            _hint.style.color = BetaVisualTheme.AgedGoldBright;
            _hint.style.fontSize = 12;
            _hint.pickingMode = PickingMode.Ignore;
            _hint.style.display = DisplayStyle.None; // evita rótulos técnicos no rodapé
            bar.Add(_hint);
        }

        private Button NavButton(string text, Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginRight = 10;
            button.style.paddingLeft = 18;
            button.style.paddingRight = 18;
            button.style.paddingTop = 10;
            button.style.paddingBottom = 10;
            button.style.minWidth = 96;
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
            button.style.fontSize = 14;
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
