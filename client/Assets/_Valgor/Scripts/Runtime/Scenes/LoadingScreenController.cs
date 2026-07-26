using UnityEngine;
using UnityEngine.UIElements;
using Valgor.UI;

namespace Valgor.Scenes
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        private ProgressBar? _progressBar;
        private Label? _stage;

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
            EnsurePanelSettings();
            EnsureUi();
        }

        public void SetProgress(float value)
        {
            EnsureUi();
            if (_progressBar != null)
            {
                _progressBar.value = Mathf.Clamp01(value) * 100f;
            }

            if (_stage != null)
            {
                _stage.text = value >= 0.99f ? "Pronto" : $"Carregando… {(value * 100f):0}%";
            }
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
            if (root.Q("loading-root") != null)
            {
                _progressBar ??= root.Q<ProgressBar>("loading-progress");
                _stage ??= root.Q<Label>("loading-stage");
                return;
            }

            root.Clear();
            root.style.flexGrow = 1;
            root.style.backgroundColor = BetaVisualTheme.Background;
            root.style.justifyContent = Justify.Center;
            root.style.alignItems = Align.Center;

            var panel = new VisualElement { name = "loading-root" };
            panel.style.width = 480;
            panel.style.paddingLeft = 28;
            panel.style.paddingRight = 28;
            panel.style.paddingTop = 24;
            panel.style.paddingBottom = 24;
            panel.style.backgroundColor = BetaVisualTheme.BackgroundPanel;
            panel.style.borderTopWidth = 2;
            panel.style.borderBottomWidth = 2;
            panel.style.borderLeftWidth = 2;
            panel.style.borderRightWidth = 2;
            panel.style.borderTopColor = BetaVisualTheme.AgedGold;
            panel.style.borderBottomColor = BetaVisualTheme.AgedGold;
            panel.style.borderLeftColor = BetaVisualTheme.AgedGold;
            panel.style.borderRightColor = BetaVisualTheme.AgedGold;
            root.Add(panel);

            var brand = new Label("VALGOR");
            brand.style.fontSize = 36;
            brand.style.color = BetaVisualTheme.AgedGoldBright;
            brand.style.unityFontStyleAndWeight = FontStyle.Bold;
            brand.style.unityTextAlign = TextAnchor.MiddleCenter;
            brand.style.marginBottom = 8;
            panel.Add(brand);

            var version = new Label(Valgor.Core.ValgorVersion.Display);
            version.style.fontSize = 13;
            version.style.color = BetaVisualTheme.TextMuted;
            version.style.unityTextAlign = TextAnchor.MiddleCenter;
            version.style.marginBottom = 18;
            panel.Add(version);

            _progressBar = new ProgressBar { name = "loading-progress", title = "Carregando", value = 0 };
            _progressBar.style.height = 28;
            panel.Add(_progressBar);

            _stage = new Label("Carregando…") { name = "loading-stage" };
            _stage.style.marginTop = 10;
            _stage.style.color = BetaVisualTheme.TextPrimary;
            _stage.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(_stage);
        }
    }
}
