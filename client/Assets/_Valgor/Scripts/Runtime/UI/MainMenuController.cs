using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.Core;

namespace Valgor.UI
{
    /// <summary>
    /// Menu principal da Beta Técnica 0.1.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        private Button _playButton = null!;
        private Button _continueButton = null!;
        private Button _settingsButton = null!;
        private VisualElement _settingsPanel = null!;
        private Label _feedback = null!;

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
            EnsurePanelSettings();
            EnsureUi();
        }

        private void OnEnable()
        {
            EnsureUi();
            RefreshContinue();
        }

        private void EnsurePanelSettings()
        {
            if (document.panelSettings != null)
            {
                return;
            }

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            document.panelSettings = settings;
        }

        private void EnsureUi()
        {
            var root = document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;
            root.style.backgroundColor = BetaVisualTheme.Background;
            root.style.justifyContent = Justify.Center;
            root.style.alignItems = Align.Center;
            root.style.paddingLeft = 48;
            root.style.paddingRight = 48;

            var card = new VisualElement();
            card.style.width = 520;
            card.style.paddingLeft = 36;
            card.style.paddingRight = 36;
            card.style.paddingTop = 32;
            card.style.paddingBottom = 28;
            card.style.backgroundColor = BetaVisualTheme.BackgroundPanel;
            card.style.borderTopWidth = 2;
            card.style.borderBottomWidth = 2;
            card.style.borderLeftWidth = 2;
            card.style.borderRightWidth = 2;
            card.style.borderTopColor = BetaVisualTheme.AgedGold;
            card.style.borderBottomColor = BetaVisualTheme.AgedGold;
            card.style.borderLeftColor = BetaVisualTheme.AgedGold;
            card.style.borderRightColor = BetaVisualTheme.AgedGold;
            card.style.alignItems = Align.Center;
            root.Add(card);

            var logo = CreateLabel("VALGOR", 48, BetaVisualTheme.AgedGoldBright);
            logo.style.unityFontStyleAndWeight = FontStyle.Bold;
            logo.style.letterSpacing = 10;
            logo.style.marginBottom = 4;
            card.Add(logo);

            card.Add(CreateLabel("PLACEHOLDER · logotipo provisório", 12, BetaVisualTheme.Placeholder));
            card.Add(CreateLabel("Reinos · Dragões · Heróis", 16, BetaVisualTheme.TextMuted));
            card.Add(CreateLabel(ValgorVersion.Display, 14, BetaVisualTheme.AgedGold));

            _playButton = CreateMenuButton("Jogar", OnPlay);
            card.Add(_playButton);
            _continueButton = CreateMenuButton("Continuar", OnContinue);
            card.Add(_continueButton);
            _settingsButton = CreateMenuButton("Configurações", OnToggleSettings);
            card.Add(_settingsButton);

            _settingsPanel = new VisualElement();
            _settingsPanel.style.display = DisplayStyle.None;
            _settingsPanel.style.marginTop = 16;
            _settingsPanel.style.paddingTop = 12;
            _settingsPanel.style.paddingBottom = 8;
            _settingsPanel.style.width = Length.Percent(100);
            _settingsPanel.Add(CreateLabel("Configurações provisórias", 14, BetaVisualTheme.AgedGoldBright));
            _settingsPanel.Add(CreateLabel("Áudio e gráficos finais virão em sprint futura.", 13, BetaVisualTheme.TextMuted));
            _settingsPanel.Add(CreateLabel($"Versão {ValgorVersion.Bundle} · {ValgorVersion.Display}", 12, BetaVisualTheme.TextMuted));
            card.Add(_settingsPanel);

            _feedback = CreateLabel(string.Empty, 13, BetaVisualTheme.AgedGold);
            _feedback.style.marginTop = 12;
            card.Add(_feedback);
        }

        private void RefreshContinue()
        {
            var hasSave = PlayerPrefs.HasKey("valgor.dragons.v3.meta") ||
                          PlayerPrefs.HasKey("valgor.city.production.v1.meta") ||
                          PlayerPrefs.HasKey("valgor.worldmap.v1.meta");
            _continueButton.SetEnabled(hasSave);
            _continueButton.style.opacity = hasSave ? 1f : 0.45f;
        }

        private void OnPlay()
        {
            if (GameBootstrap.Game == null)
            {
                _feedback.text = "Bootstrap indisponível.";
                return;
            }

            StartCoroutine(GameBootstrap.Game.Navigator.GoToCity());
        }

        private void OnContinue()
        {
            if (GameBootstrap.Game == null)
            {
                _feedback.text = "Bootstrap indisponível.";
                return;
            }

            if (!_continueButton.enabledSelf)
            {
                _feedback.text = "Nenhuma jornada salva encontrada.";
                return;
            }

            StartCoroutine(GameBootstrap.Game.Navigator.GoToCity());
        }

        private void OnToggleSettings()
        {
            var show = _settingsPanel.style.display == DisplayStyle.None;
            _settingsPanel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static Button CreateMenuButton(string text, System.Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginTop = 12;
            button.style.width = Length.Percent(100);
            button.style.height = 52;
            button.style.fontSize = 18;
            button.style.backgroundColor = BetaVisualTheme.DeepBlue;
            button.style.color = BetaVisualTheme.TextPrimary;
            button.style.borderTopWidth = 2;
            button.style.borderBottomWidth = 2;
            button.style.borderLeftWidth = 2;
            button.style.borderRightWidth = 2;
            button.style.borderTopColor = BetaVisualTheme.AgedGold;
            button.style.borderBottomColor = BetaVisualTheme.AgedGold;
            button.style.borderLeftColor = BetaVisualTheme.AgedGold;
            button.style.borderRightColor = BetaVisualTheme.AgedGold;
            return button;
        }

        private static Label CreateLabel(string text, int size, Color color)
        {
            var label = new Label(text);
            label.style.fontSize = size;
            label.style.color = color;
            label.style.marginBottom = 8;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            return label;
        }
    }
}
