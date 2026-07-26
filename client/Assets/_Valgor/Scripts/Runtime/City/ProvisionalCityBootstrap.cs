using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.Core.Modules;

namespace Valgor.City
{
    /// <summary>
    /// Cidade provisória para validar o fluxo de navegação do Game Core.
    /// Substituída pela Player City Foundation.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ProvisionalCityBootstrap : MonoBehaviour, IPlayerCityModule
    {
        [SerializeField] private UIDocument document;
        private Button _worldMapButton;
        private Button _debugMenuButton;

        public bool IsLoaded { get; private set; }

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
            EnsurePanelSettings();
            GameBootstrap.Services?.Register<IPlayerCityModule>(this);
            EnsureUi();
        }

        private void OnEnable()
        {
            Enter();
            EnsureUi();
            _worldMapButton?.RegisterCallback<ClickEvent>(OnWorldMap);
            _debugMenuButton?.RegisterCallback<ClickEvent>(OnDebugMenu);
        }

        private void OnDisable()
        {
            _worldMapButton?.UnregisterCallback<ClickEvent>(OnWorldMap);
            _debugMenuButton?.UnregisterCallback<ClickEvent>(OnDebugMenu);
            if (IsLoaded)
            {
                Exit();
            }
        }

        public void Enter() => IsLoaded = true;

        public void Exit() => IsLoaded = false;

        private void OnWorldMap(ClickEvent _)
        {
            StartCoroutine(GameBootstrap.Game.Navigator.GoToWorldMap());
        }

        private void OnDebugMenu(ClickEvent _)
        {
            StartCoroutine(GameBootstrap.Game.Navigator.GoToMainMenu());
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
            root.style.backgroundColor = new Color(0.12f, 0.14f, 0.16f);
            root.style.paddingLeft = 40;
            root.style.paddingTop = 40;

            var title = new Label("City (provisional)");
            title.style.fontSize = 36;
            title.style.color = Color.white;
            title.style.marginBottom = 8;
            root.Add(title);

            var session = new Label(GameBootstrap.Game != null
                ? $"Sessão ativa: {GameBootstrap.Game.Session.IsActive} · Estado: {GameBootstrap.Game.StateMachine.Current}"
                : "Sem GameBootstrap");
            session.style.color = new Color(0.7f, 0.75f, 0.8f);
            session.style.marginBottom = 24;
            root.Add(session);

            _worldMapButton = CreateButton("btn-world-map", "Abrir Mapa Mundial");
            _debugMenuButton = CreateButton("btn-debug-menu", "Debug: Main Menu");
            root.Add(_worldMapButton);
            root.Add(_debugMenuButton);
        }

        private static Button CreateButton(string name, string text)
        {
            var button = new Button { name = name, text = text };
            button.style.marginBottom = 12;
            button.style.paddingLeft = 16;
            button.style.paddingRight = 16;
            button.style.paddingTop = 10;
            button.style.paddingBottom = 10;
            button.style.maxWidth = 280;
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.backgroundColor = new Color(0.24f, 0.55f, 0.99f);
            button.style.color = Color.white;
            return button;
        }
    }
}
