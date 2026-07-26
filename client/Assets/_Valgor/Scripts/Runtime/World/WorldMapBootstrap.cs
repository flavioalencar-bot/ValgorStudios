using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.Core.Modules;

namespace Valgor.World
{
    /// <summary>
    /// Mapa mundial provisório: retorno à cidade e registro do módulo.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class WorldMapBootstrap : MonoBehaviour, IWorldMapModule
    {
        [SerializeField] private UIDocument document;
        private Button _returnButton;

        public bool IsLoaded { get; private set; }

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
            EnsurePanelSettings();
            GameBootstrap.Services?.Register<IWorldMapModule>(this);
            EnsureUi();
        }

        private void OnEnable()
        {
            Enter();
            EnsureUi();
            _returnButton?.RegisterCallback<ClickEvent>(OnReturnClicked);
        }

        private void OnDisable()
        {
            _returnButton?.UnregisterCallback<ClickEvent>(OnReturnClicked);
            if (IsLoaded)
            {
                Exit();
            }
        }

        public void Enter() => IsLoaded = true;

        public void Exit() => IsLoaded = false;

        private void OnReturnClicked(ClickEvent _)
        {
            StartCoroutine(GameBootstrap.Game.Navigator.GoToCity());
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
            root.style.backgroundColor = new Color(0.07f, 0.1f, 0.09f);
            root.style.paddingLeft = 48;
            root.style.paddingTop = 48;
            root.style.justifyContent = Justify.Center;

            root.Add(CreateLabel("VALGOR", 14, new Color(0.35f, 0.7f, 0.47f)));
            root.Add(CreateLabel("World Map", 42, new Color(0.91f, 0.93f, 0.96f)));
            root.Add(CreateLabel("Mapa mundial provisório", 14, new Color(0.6f, 0.67f, 0.74f)));

            _returnButton = new Button { name = "btn-return-city", text = "Voltar para a Cidade" };
            _returnButton.style.backgroundColor = new Color(0.35f, 0.7f, 0.47f);
            _returnButton.style.color = new Color(0.05f, 0.07f, 0.05f);
            _returnButton.style.fontSize = 16;
            _returnButton.style.paddingLeft = 24;
            _returnButton.style.paddingRight = 24;
            _returnButton.style.paddingTop = 12;
            _returnButton.style.paddingBottom = 12;
            _returnButton.style.maxWidth = 280;
            _returnButton.style.borderTopWidth = 0;
            _returnButton.style.borderBottomWidth = 0;
            _returnButton.style.borderLeftWidth = 0;
            _returnButton.style.borderRightWidth = 0;
            root.Add(_returnButton);
        }

        private static Label CreateLabel(string text, int size, Color color)
        {
            var label = new Label(text);
            label.style.fontSize = size;
            label.style.color = color;
            label.style.marginBottom = 12;
            return label;
        }
    }
}
