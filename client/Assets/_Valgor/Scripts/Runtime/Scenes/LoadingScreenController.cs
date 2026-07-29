using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Core;
using Valgor.UI;

namespace Valgor.Scenes
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        private ProgressBar _progressBar;
        private Label _stage;
        private Label _brandMessage;

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
            EnsurePanelSettings();
            EnsureUi();
        }

        public void SetProgress(float value) => SetProgress(value, null);

        public void SetProgress(float value, string stageHint)
        {
            if (document == null || document.rootVisualElement == null) return;
            EnsureUi();
            if (_progressBar != null)
            {
                _progressBar.value = Mathf.Clamp01(value) * 100f;
            }

            if (_stage != null)
            {
                if (!string.IsNullOrEmpty(stageHint))
                {
                    _stage.text = stageHint;
                }
                else
                {
                    _stage.text = value >= 0.99f
                        ? "Pronto"
                        : $"Carregando… {(value * 100f):0}%";
                }
            }
        }

        public void SetBrandMessage(string message)
        {
            if (document == null || document.rootVisualElement == null) return;
            EnsureUi();
            if (_brandMessage != null)
            {
                _brandMessage.text = message ?? string.Empty;
            }

            if (_stage != null && !string.IsNullOrEmpty(message))
            {
                _stage.text = message;
            }
        }

        private void EnsurePanelSettings()
        {
            BetaUiPanels.ApplyTo(document);
        }

        private void EnsureUi()
        {
            var root = document.rootVisualElement;
            if (root.Q("loading-root") != null)
            {
                _progressBar ??= root.Q<ProgressBar>("loading-progress");
                _stage ??= root.Q<Label>("loading-stage");
                _brandMessage ??= root.Q<Label>("loading-brand-message");
                return;
            }

            root.Clear();
            root.style.flexGrow = 1;
            // Fundo medieval escuro (gradiente aproximado por camadas).
            root.style.backgroundColor = new Color(0.03f, 0.035f, 0.05f, 1f);
            root.style.justifyContent = Justify.Center;
            root.style.alignItems = Align.Center;

            var atmosphere = new VisualElement { name = "loading-atmosphere", pickingMode = PickingMode.Ignore };
            atmosphere.style.position = Position.Absolute;
            atmosphere.style.left = 0;
            atmosphere.style.right = 0;
            atmosphere.style.top = 0;
            atmosphere.style.bottom = 0;
            atmosphere.style.backgroundColor = new Color(0.08f, 0.06f, 0.03f, 0.35f);
            root.Add(atmosphere);

            var panel = new VisualElement { name = "loading-root" };
            panel.style.width = ValgorResponsiveUi.Compact(540, 420);
            panel.style.maxWidth = Length.Percent(92);
            panel.style.paddingLeft = 32;
            panel.style.paddingRight = 32;
            panel.style.paddingTop = 36;
            panel.style.paddingBottom = 28;
            panel.style.backgroundColor = new Color(0.05f, 0.055f, 0.09f, 0.96f);
            panel.style.borderTopWidth = 2;
            panel.style.borderBottomWidth = 2;
            panel.style.borderLeftWidth = 2;
            panel.style.borderRightWidth = 2;
            panel.style.borderTopColor = BetaVisualTheme.AgedGold;
            panel.style.borderBottomColor = BetaVisualTheme.AgedGold;
            panel.style.borderLeftColor = BetaVisualTheme.AgedGold;
            panel.style.borderRightColor = BetaVisualTheme.AgedGold;
            panel.style.alignItems = Align.Center;
            root.Add(panel);

            // Brasão dourado provisório (losango + cruz).
            var crest = new VisualElement { name = "loading-crest", pickingMode = PickingMode.Ignore };
            crest.style.width = 72;
            crest.style.height = 72;
            crest.style.marginBottom = 14;
            crest.style.backgroundColor = new Color(0.12f, 0.1f, 0.06f, 1f);
            crest.style.borderTopWidth = 3;
            crest.style.borderBottomWidth = 3;
            crest.style.borderLeftWidth = 3;
            crest.style.borderRightWidth = 3;
            crest.style.borderTopColor = BetaVisualTheme.AgedGoldBright;
            crest.style.borderBottomColor = BetaVisualTheme.AgedGoldBright;
            crest.style.borderLeftColor = BetaVisualTheme.AgedGoldBright;
            crest.style.borderRightColor = BetaVisualTheme.AgedGoldBright;
            crest.style.justifyContent = Justify.Center;
            crest.style.alignItems = Align.Center;
            var crestMark = new Label("V");
            crestMark.style.fontSize = 36;
            crestMark.style.color = BetaVisualTheme.AgedGoldBright;
            crestMark.style.unityFontStyleAndWeight = FontStyle.Bold;
            crestMark.style.unityTextAlign = TextAnchor.MiddleCenter;
            crest.Add(crestMark);
            panel.Add(crest);

            var brand = new Label("VALGOR");
            brand.style.fontSize = 46;
            brand.style.color = BetaVisualTheme.AgedGoldBright;
            brand.style.unityFontStyleAndWeight = FontStyle.Bold;
            brand.style.unityTextAlign = TextAnchor.MiddleCenter;
            brand.style.letterSpacing = 10;
            brand.style.marginBottom = 4;
            panel.Add(brand);

            var version = new Label(ValgorVersion.Display);
            version.name = "loading-version";
            version.style.fontSize = 16;
            version.style.color = BetaVisualTheme.AgedGold;
            version.style.unityTextAlign = TextAnchor.MiddleCenter;
            version.style.marginBottom = 18;
            panel.Add(version);

            _brandMessage = new Label("Preparando o reino...") { name = "loading-brand-message" };
            _brandMessage.style.fontSize = 15;
            _brandMessage.style.color = BetaVisualTheme.TextMuted;
            _brandMessage.style.unityTextAlign = TextAnchor.MiddleCenter;
            _brandMessage.style.marginBottom = 18;
            panel.Add(_brandMessage);

            _progressBar = new ProgressBar { name = "loading-progress", title = string.Empty, value = 0 };
            _progressBar.style.height = 26;
            _progressBar.style.width = Length.Percent(100);
            panel.Add(_progressBar);

            _stage = new Label("Preparando o reino...") { name = "loading-stage" };
            _stage.style.marginTop = 12;
            _stage.style.color = BetaVisualTheme.TextPrimary;
            _stage.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(_stage);
        }
    }
}
