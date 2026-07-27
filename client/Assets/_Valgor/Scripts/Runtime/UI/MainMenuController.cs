using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.Core;

namespace Valgor.UI
{
    /// <summary>
    /// Menu principal da Beta 0.1 — Novo Jogo, Continuar, Configurações, Créditos, Sair.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        private VisualElement _homePanel;
        private VisualElement _firstAccessPanel;
        private VisualElement _introPanel;
        private VisualElement _settingsPanel;
        private VisualElement _creditsPanel;
        private VisualElement _confirmNewPanel;
        private TextField _nameField;
        private Label _feedback;
        private Button _continueButton;
        private Slider _volumeSlider;
        private int _introCardIndex;
        private Label _introBody;

        private static readonly string[] IntroTitles =
        {
            "Bem-vindo a Valgor",
            "Heróis e Dragões",
            "Sua cidade aguarda"
        };

        private static readonly string[] IntroBodies =
        {
            "Você é o senhor de um reino nascente. Reúna recursos, proteja sua cidade e conquiste o mapa.",
            "Vortex, o Rei dos Dragões, lidera seus heróis. Na Torre dos Dragões, Ember e Ash aguardam seu comando.",
            "Colete na cidade, marche no mapa mundial e volte para fortalecer o que é seu. A jornada começa agora."
        };

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
            EnsurePanelSettings();
            EnsureUi();
        }

        private void OnEnable()
        {
            EnsureUi();
            BetaPlayerSettings.ApplyRuntime();
            LocalPlayerProfile.ApplyToSession(GameBootstrap.Game?.Session);
            RefreshHomeMode();
        }

        private void EnsurePanelSettings() => BetaUiPanels.ApplyTo(document);

        private void EnsureUi()
        {
            var root = document.rootVisualElement;
            if (root.Q("main-menu-root") != null)
            {
                CacheRefs(root);
                return;
            }

            root.Clear();
            root.style.flexGrow = 1;
            root.style.backgroundColor = new Color(0.03f, 0.035f, 0.05f, 1f);
            root.style.justifyContent = Justify.Center;
            root.style.alignItems = Align.Center;
            root.style.paddingLeft = 48;
            root.style.paddingRight = 48;

            var veil = new VisualElement { pickingMode = PickingMode.Ignore };
            veil.style.position = Position.Absolute;
            veil.style.left = 0;
            veil.style.right = 0;
            veil.style.top = 0;
            veil.style.bottom = 0;
            veil.style.backgroundColor = new Color(0.1f, 0.07f, 0.03f, 0.28f);
            root.Add(veil);

            var card = new VisualElement { name = "main-menu-root" };
            card.style.width = 560;
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

            var crest = new VisualElement { pickingMode = PickingMode.Ignore };
            crest.style.width = 56;
            crest.style.height = 56;
            crest.style.marginBottom = 10;
            crest.style.backgroundColor = new Color(0.12f, 0.1f, 0.06f, 1f);
            crest.style.borderTopWidth = 2;
            crest.style.borderBottomWidth = 2;
            crest.style.borderLeftWidth = 2;
            crest.style.borderRightWidth = 2;
            crest.style.borderTopColor = BetaVisualTheme.AgedGoldBright;
            crest.style.borderBottomColor = BetaVisualTheme.AgedGoldBright;
            crest.style.borderLeftColor = BetaVisualTheme.AgedGoldBright;
            crest.style.borderRightColor = BetaVisualTheme.AgedGoldBright;
            crest.style.justifyContent = Justify.Center;
            crest.style.alignItems = Align.Center;
            var crestMark = CreateLabel("V", 28, BetaVisualTheme.AgedGoldBright);
            crestMark.style.unityFontStyleAndWeight = FontStyle.Bold;
            crestMark.style.marginBottom = 0;
            crest.Add(crestMark);
            card.Add(crest);

            var logo = CreateLabel("VALGOR", 48, BetaVisualTheme.AgedGoldBright);
            logo.style.unityFontStyleAndWeight = FontStyle.Bold;
            logo.style.letterSpacing = 10;
            logo.style.marginBottom = 2;
            card.Add(logo);
            card.Add(CreateLabel(ValgorVersion.ProductLine, 15, BetaVisualTheme.AgedGold));
            card.Add(CreateLabel("Reinos · Dragões · Heróis", 14, BetaVisualTheme.TextMuted));

            _homePanel = new VisualElement { name = "home-panel" };
            _homePanel.style.width = Length.Percent(100);
            _homePanel.style.alignItems = Align.Center;
            _homePanel.style.marginTop = 12;
            card.Add(_homePanel);

            _homePanel.Add(CreateMenuButton("Novo Jogo", OnNewGame, "btn-new-game"));
            _homePanel.Add(CreateMenuButton("Continuar", OnContinue, "btn-continue"));
            _homePanel.Add(CreateMenuButton("Configurações", OnToggleSettings, "btn-settings"));
            _homePanel.Add(CreateMenuButton("Créditos", OnToggleCredits, "btn-credits"));
            _homePanel.Add(CreateMenuButton("Sair", OnQuit, "btn-quit"));

            _continueButton = _homePanel.Q<Button>("btn-continue");

            _firstAccessPanel = BuildFirstAccessPanel();
            card.Add(_firstAccessPanel);

            _confirmNewPanel = BuildConfirmNewPanel();
            card.Add(_confirmNewPanel);

            _introPanel = BuildIntroPanel();
            card.Add(_introPanel);

            _settingsPanel = BuildSettingsPanel();
            card.Add(_settingsPanel);

            _creditsPanel = BuildCreditsPanel();
            card.Add(_creditsPanel);

            _feedback = CreateLabel(string.Empty, 13, BetaVisualTheme.AgedGold);
            _feedback.name = "menu-feedback";
            _feedback.style.marginTop = 12;
            _feedback.style.whiteSpace = WhiteSpace.Normal;
            _feedback.style.unityTextAlign = TextAnchor.MiddleCenter;
            card.Add(_feedback);
        }

        private void CacheRefs(VisualElement root)
        {
            _homePanel ??= root.Q("home-panel");
            _firstAccessPanel ??= root.Q("first-access-panel");
            _confirmNewPanel ??= root.Q("confirm-new-panel");
            _introPanel ??= root.Q("intro-panel");
            _settingsPanel ??= root.Q("settings-panel");
            _creditsPanel ??= root.Q("credits-panel");
            _nameField ??= root.Q<TextField>("player-name");
            _feedback ??= root.Q<Label>("menu-feedback");
            _continueButton ??= root.Q<Button>("btn-continue");
            _introBody ??= root.Q<Label>("intro-body");
            _volumeSlider ??= root.Q<Slider>("settings-volume");
        }

        private VisualElement BuildFirstAccessPanel()
        {
            var panel = new VisualElement { name = "first-access-panel" };
            panel.style.display = DisplayStyle.None;
            panel.style.width = Length.Percent(100);
            panel.style.marginTop = 16;
            panel.Add(CreateLabel("Como devemos chamá-lo, senhor?", 16, BetaVisualTheme.AgedGoldBright));
            panel.Add(CreateLabel("Nome entre 3 e 20 caracteres.", 12, BetaVisualTheme.TextMuted));
            _nameField = new TextField { name = "player-name", value = string.Empty, label = string.Empty };
            _nameField.style.marginTop = 12;
            _nameField.style.marginBottom = 8;
            panel.Add(_nameField);
            panel.Add(CreateMenuButton("Confirmar", OnConfirmFirstAccess, "btn-confirm-start"));
            panel.Add(CreateMenuButton("Voltar", () =>
            {
                HideOverlays();
                RefreshHomeMode();
            }, "btn-back-first"));
            return panel;
        }

        private VisualElement BuildConfirmNewPanel()
        {
            var panel = new VisualElement { name = "confirm-new-panel" };
            panel.style.display = DisplayStyle.None;
            panel.style.width = Length.Percent(100);
            panel.style.marginTop = 16;
            panel.Add(CreateLabel("Apagar progresso atual?", 16, BetaVisualTheme.AgedGoldBright));
            var warn = CreateLabel(
                "Um Novo Jogo apaga o save local e inicia outra jornada.",
                13,
                BetaVisualTheme.TextMuted);
            warn.style.whiteSpace = WhiteSpace.Normal;
            warn.style.marginBottom = 8;
            panel.Add(warn);
            panel.Add(CreateMenuButton("Apagar e começar", OnConfirmWipeAndStart, "btn-confirm-wipe"));
            panel.Add(CreateMenuButton("Cancelar", () =>
            {
                HideOverlays();
                RefreshHomeMode();
            }, "btn-cancel-wipe"));
            return panel;
        }

        private VisualElement BuildIntroPanel()
        {
            var panel = new VisualElement { name = "intro-panel" };
            panel.style.display = DisplayStyle.None;
            panel.style.width = Length.Percent(100);
            panel.style.marginTop = 12;
            var title = CreateLabel(IntroTitles[0], 18, BetaVisualTheme.AgedGoldBright);
            title.name = "intro-title";
            panel.Add(title);
            _introBody = CreateLabel(IntroBodies[0], 14, BetaVisualTheme.TextPrimary);
            _introBody.name = "intro-body";
            _introBody.style.whiteSpace = WhiteSpace.Normal;
            _introBody.style.marginTop = 10;
            _introBody.style.marginBottom = 12;
            _introBody.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(_introBody);
            panel.Add(CreateMenuButton("Avançar", OnIntroAdvance, "btn-intro-next"));
            return panel;
        }

        private VisualElement BuildSettingsPanel()
        {
            var panel = new VisualElement { name = "settings-panel" };
            panel.style.display = DisplayStyle.None;
            panel.style.marginTop = 16;
            panel.style.width = Length.Percent(100);
            panel.Add(CreateLabel("Configurações", 16, BetaVisualTheme.AgedGoldBright));

            _volumeSlider = new Slider("Volume", 0f, 1f)
            {
                name = "settings-volume",
                value = BetaPlayerSettings.MasterVolume,
                showInputField = false
            };
            _volumeSlider.style.marginTop = 12;
            _volumeSlider.style.marginBottom = 8;
            _volumeSlider.RegisterValueChangedCallback(evt => BetaPlayerSettings.MasterVolume = evt.newValue);
            panel.Add(_volumeSlider);

            var music = new Toggle("Música")
            {
                name = "settings-music",
                value = BetaPlayerSettings.MusicEnabled
            };
            music.RegisterValueChangedCallback(evt => BetaPlayerSettings.MusicEnabled = evt.newValue);
            panel.Add(music);

            var sfx = new Toggle("Efeitos sonoros")
            {
                name = "settings-sfx",
                value = BetaPlayerSettings.SfxEnabled
            };
            sfx.style.marginBottom = 8;
            sfx.RegisterValueChangedCallback(evt => BetaPlayerSettings.SfxEnabled = evt.newValue);
            panel.Add(sfx);

            panel.Add(CreateLabel(ValgorVersion.ProductLine, 12, BetaVisualTheme.TextMuted));
            panel.Add(CreateMenuButton("Voltar", () =>
            {
                panel.style.display = DisplayStyle.None;
                RefreshHomeMode();
            }, "btn-settings-back"));
            return panel;
        }

        private VisualElement BuildCreditsPanel()
        {
            var panel = new VisualElement { name = "credits-panel" };
            panel.style.display = DisplayStyle.None;
            panel.style.marginTop = 16;
            panel.style.width = Length.Percent(100);
            panel.Add(CreateLabel("Créditos", 16, BetaVisualTheme.AgedGoldBright));
            var body = CreateLabel(
                "Valgor Studios\nBeta 0.1 — jornada inicial do jogador\nReinos · Dragões · Heróis",
                13,
                BetaVisualTheme.TextPrimary);
            body.style.whiteSpace = WhiteSpace.Normal;
            body.style.marginTop = 10;
            body.style.marginBottom = 12;
            panel.Add(body);
            panel.Add(CreateMenuButton("Voltar", () =>
            {
                panel.style.display = DisplayStyle.None;
                RefreshHomeMode();
            }, "btn-credits-back"));
            return panel;
        }

        private void HideOverlays()
        {
            if (_firstAccessPanel != null) _firstAccessPanel.style.display = DisplayStyle.None;
            if (_confirmNewPanel != null) _confirmNewPanel.style.display = DisplayStyle.None;
            if (_introPanel != null) _introPanel.style.display = DisplayStyle.None;
            if (_settingsPanel != null) _settingsPanel.style.display = DisplayStyle.None;
            if (_creditsPanel != null) _creditsPanel.style.display = DisplayStyle.None;
        }

        private void RefreshHomeMode()
        {
            var canContinue = LocalPlayerProfile.CanContinue();

            HideOverlays();
            if (_homePanel != null)
            {
                _homePanel.style.display = DisplayStyle.Flex;
            }

            if (_continueButton != null)
            {
                _continueButton.style.display = canContinue ? DisplayStyle.Flex : DisplayStyle.None;
                _continueButton.SetEnabled(canContinue);
            }

            if (_feedback != null)
            {
                _feedback.text = canContinue
                    ? $"Comandante: {LocalPlayerProfile.DisplayName}"
                    : "Inicie um Novo Jogo para começar sua jornada.";
            }
        }

        private void OnNewGame()
        {
            _homePanel.style.display = DisplayStyle.None;
            HideOverlays();
            if (LocalPlayerProfile.CanContinue() || LocalPlayerProfile.HasProfile)
            {
                _confirmNewPanel.style.display = DisplayStyle.Flex;
                _feedback.text = string.Empty;
                return;
            }

            OpenNameEntry();
        }

        private void OnConfirmWipeAndStart()
        {
            LocalPlayerProfile.WipeAllForNewJourney();
            GameBootstrap.Game?.Session?.ClearAuthentication();
            OpenNameEntry();
        }

        private void OpenNameEntry()
        {
            HideOverlays();
            _homePanel.style.display = DisplayStyle.None;
            _firstAccessPanel.style.display = DisplayStyle.Flex;
            _feedback.text = string.Empty;
            if (_nameField != null) _nameField.value = string.Empty;
        }

        private void OnConfirmFirstAccess()
        {
            var raw = _nameField != null ? _nameField.value : string.Empty;
            if (!LocalPlayerProfile.Create(raw, out var error))
            {
                _feedback.text = error;
                return;
            }

            LocalPlayerProfile.ApplyToSession(GameBootstrap.Game?.Session);
            PlayerPrefs.Save();
            _feedback.text = $"Bem-vindo, {LocalPlayerProfile.DisplayName}.";
            _firstAccessPanel.style.display = DisplayStyle.None;
            BeginIntro();
        }

        private void BeginIntro()
        {
            _introCardIndex = 0;
            ShowIntroCard();
            _homePanel.style.display = DisplayStyle.None;
            HideOverlays();
            _introPanel.style.display = DisplayStyle.Flex;
        }

        private void ShowIntroCard()
        {
            var title = _introPanel.Q<Label>("intro-title");
            if (title != null) title.text = IntroTitles[_introCardIndex];
            if (_introBody != null) _introBody.text = IntroBodies[_introCardIndex];
            var next = _introPanel.Q<Button>("btn-intro-next");
            if (next != null)
            {
                next.text = _introCardIndex >= IntroTitles.Length - 1 ? "Entrar na cidade" : "Avançar";
            }
        }

        private void OnIntroAdvance()
        {
            if (_introCardIndex < IntroTitles.Length - 1)
            {
                _introCardIndex++;
                ShowIntroCard();
                return;
            }

            LocalPlayerProfile.MarkIntroDone();
            EnterCity();
        }

        private void OnContinue()
        {
            if (GameBootstrap.Game == null)
            {
                _feedback.text = "Não foi possível continuar.";
                return;
            }

            if (!LocalPlayerProfile.CanContinue())
            {
                _feedback.text = "Nenhuma jornada salva encontrada.";
                return;
            }

            LocalPlayerProfile.ApplyToSession(GameBootstrap.Game.Session);
            if (!LocalPlayerProfile.IntroDone)
            {
                BeginIntro();
                return;
            }

            EnterCity();
        }

        private void EnterCity()
        {
            if (GameBootstrap.Game == null)
            {
                _feedback.text = "Não foi possível entrar na cidade.";
                return;
            }

            _feedback.text = "Entrando na cidade…";
            StartCoroutine(EnterCityRoutine());
        }

        private IEnumerator EnterCityRoutine()
        {
            yield return GameBootstrap.Game.Navigator.GoToCity();
            PlayerPrefs.Save();
        }

        private void OnToggleSettings()
        {
            var show = _settingsPanel.style.display == DisplayStyle.None;
            HideOverlays();
            _homePanel.style.display = show ? DisplayStyle.None : DisplayStyle.Flex;
            _settingsPanel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (show && _volumeSlider != null)
            {
                _volumeSlider.SetValueWithoutNotify(BetaPlayerSettings.MasterVolume);
            }
        }

        private void OnToggleCredits()
        {
            var show = _creditsPanel.style.display == DisplayStyle.None;
            HideOverlays();
            _homePanel.style.display = show ? DisplayStyle.None : DisplayStyle.Flex;
            _creditsPanel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void OnQuit()
        {
            PlayerPrefs.Save();
#if UNITY_EDITOR
            var editorApp = System.Type.GetType("UnityEditor.EditorApplication,UnityEditor");
            editorApp?.GetProperty("isPlaying")?.SetValue(null, false);
#else
            Application.Quit();
#endif
        }

        private static Button CreateMenuButton(string text, System.Action action, string name = null)
        {
            var button = new Button(action) { text = text, name = name };
            button.style.marginTop = 12;
            button.style.width = Length.Percent(100);
            button.style.height = 52;
            button.style.fontSize = 18;
            button.style.backgroundColor = BetaVisualTheme.DeepBlue;
            button.style.color = BetaVisualTheme.TextPrimary;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
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
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.marginBottom = 4;
            return label;
        }
    }
}
