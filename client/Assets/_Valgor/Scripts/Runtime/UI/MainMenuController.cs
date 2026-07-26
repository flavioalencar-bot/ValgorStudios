using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;

namespace Valgor.UI
{
    /// <summary>
    /// Menu principal: entra na cidade do jogador.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        private Button _enterCityButton;

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
            EnsurePanelSettings();
            EnsureUi();
        }

        private void OnEnable()
        {
            EnsureUi();
            RefreshSessionLabel();
            _enterCityButton?.RegisterCallback<ClickEvent>(OnEnterCityClicked);
        }

        private void OnDisable()
        {
            _enterCityButton?.UnregisterCallback<ClickEvent>(OnEnterCityClicked);
        }

        private void OnEnterCityClicked(ClickEvent _)
        {
            if (GameBootstrap.Game == null)
            {
                Debug.LogError("[Valgor] GameBootstrap.Game is null.");
                return;
            }

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
            root.style.backgroundColor = new Color(0.06f, 0.08f, 0.11f);
            root.style.paddingLeft = 48;
            root.style.paddingRight = 48;
            root.style.paddingTop = 48;
            root.style.paddingBottom = 48;
            root.style.justifyContent = Justify.Center;

            root.Add(CreateLabel("VALGOR", 14, new Color(0.24f, 0.55f, 0.99f), "brand"));
            root.Add(CreateLabel("Main Menu", 42, new Color(0.91f, 0.93f, 0.96f), "title"));
            root.Add(CreateLabel(string.Empty, 14, new Color(0.6f, 0.67f, 0.74f), "session-label"));

            _enterCityButton = new Button { name = "btn-enter-city", text = "Entrar na Cidade" };
            _enterCityButton.style.backgroundColor = new Color(0.24f, 0.55f, 0.99f);
            _enterCityButton.style.color = Color.white;
            _enterCityButton.style.fontSize = 16;
            _enterCityButton.style.paddingLeft = 24;
            _enterCityButton.style.paddingRight = 24;
            _enterCityButton.style.paddingTop = 12;
            _enterCityButton.style.paddingBottom = 12;
            _enterCityButton.style.borderTopWidth = 0;
            _enterCityButton.style.borderBottomWidth = 0;
            _enterCityButton.style.borderLeftWidth = 0;
            _enterCityButton.style.borderRightWidth = 0;
            _enterCityButton.style.maxWidth = 280;
            root.Add(_enterCityButton);
        }

        private void RefreshSessionLabel()
        {
            var label = document.rootVisualElement.Q<Label>("session-label");
            if (label == null)
            {
                return;
            }

            if (GameBootstrap.Game == null)
            {
                label.text = "Sessão indisponível";
                return;
            }

            var session = GameBootstrap.Game.Session;
            label.text = session.IsActive
                ? $"Sessão {session.SessionId.ToString()[..8]} · {GameBootstrap.Game.StateMachine.Current}"
                : "Sessão inativa";
        }

        private static Label CreateLabel(string text, int size, Color color, string name)
        {
            var label = new Label(text) { name = name };
            label.style.fontSize = size;
            label.style.color = color;
            label.style.marginBottom = 12;
            return label;
        }
    }
}
