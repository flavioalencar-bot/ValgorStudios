using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Core;
using Valgor.UI;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Banner discreto + painel de progressão (somente -cityProgressionQA).
    /// </summary>
    public sealed class CityProgressionQaHud : MonoBehaviour
    {
        private CityProgressionQaController _qa = null!;
        private UIDocument _document = null!;
        private Label _banner = null!;
        private Label _status = null!;
        private Label _castleInfo = null!;
        private VisualElement _panel = null!;
        private bool _panelOpen;

        public void Initialize(CityProgressionQaController qa)
        {
            _qa = qa;
            _document = GetComponent<UIDocument>();
            if (_document == null)
            {
                _document = gameObject.AddComponent<UIDocument>();
            }

            BetaUiPanels.ApplyTo(_document);
            Build();
        }

        private void Update()
        {
            if (_castleInfo == null || _qa == null)
            {
                return;
            }

            _castleInfo.text =
                $"Castelo Nv.{_qa.GetCastleLevel()}  ·  Tier visual {_qa.GetCastleVisualTier()}";
            if (_status != null)
            {
                _status.text = _qa.Status;
            }
        }

        private void Build()
        {
            var root = _document.rootVisualElement;
            // Não limpa o HUD da City — adiciona overlay.
            _banner = new Label(CityProgressionQa.BannerText);
            _banner.name = "qa-homolog-banner";
            _banner.style.position = Position.Absolute;
            _banner.style.top = 52;
            _banner.style.left = 12;
            _banner.style.paddingLeft = 10;
            _banner.style.paddingRight = 10;
            _banner.style.paddingTop = 4;
            _banner.style.paddingBottom = 4;
            _banner.style.fontSize = 11;
            _banner.style.color = new Color(0.92f, 0.85f, 0.55f, 0.95f);
            _banner.style.backgroundColor = new Color(0.05f, 0.05f, 0.06f, 0.55f);
            _banner.pickingMode = PickingMode.Ignore;
            root.Add(_banner);

            var toggle = new Button(TogglePanel) { text = "QA Progressão" };
            toggle.name = "qa-progress-toggle";
            toggle.style.position = Position.Absolute;
            toggle.style.top = 52;
            toggle.style.right = 12;
            toggle.style.fontSize = 11;
            toggle.style.paddingLeft = 10;
            toggle.style.paddingRight = 10;
            toggle.style.paddingTop = 4;
            toggle.style.paddingBottom = 4;
            toggle.style.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 0.92f);
            toggle.style.color = BetaVisualTheme.TextPrimary;
            root.Add(toggle);

            _panel = new VisualElement();
            _panel.name = "qa-progress-panel";
            _panel.style.position = Position.Absolute;
            _panel.style.top = 84;
            _panel.style.right = 12;
            _panel.style.width = 280;
            _panel.style.paddingLeft = 12;
            _panel.style.paddingRight = 12;
            _panel.style.paddingTop = 10;
            _panel.style.paddingBottom = 10;
            _panel.style.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 0.94f);
            _panel.style.display = DisplayStyle.None;
            _panel.style.borderTopWidth = 1;
            _panel.style.borderBottomWidth = 1;
            _panel.style.borderLeftWidth = 1;
            _panel.style.borderRightWidth = 1;
            _panel.style.borderTopColor = BetaVisualTheme.AgedGold;
            _panel.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _panel.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _panel.style.borderRightColor = BetaVisualTheme.AgedGold;

            var title = new Label("Homologação — Castelo");
            title.style.color = BetaVisualTheme.AgedGold;
            title.style.fontSize = 13;
            title.style.marginBottom = 6;
            _panel.Add(title);

            _castleInfo = new Label();
            _castleInfo.style.color = BetaVisualTheme.TextPrimary;
            _castleInfo.style.fontSize = 12;
            _castleInfo.style.marginBottom = 8;
            _panel.Add(_castleInfo);

            _panel.Add(MakeButton("Evoluir +1", () => _qa.RequestEvolvePlusOne()));
            _panel.Add(MakeButton("Evoluir até próximo Tier", () => _qa.RequestEvolveToNextTier()));
            _panel.Add(MakeButton("Evoluir até Nv.30", () => _qa.RequestEvolveTo30()));
            _panel.Add(MakeButton("Voltar para Nv.1", () => _qa.RequestResetTo1()));
            _panel.Add(MakeButton("Salvar", () => _qa.RequestSave()));
            _panel.Add(MakeButton("Recarregar save", () => _qa.RequestReload()));

            _status = new Label();
            _status.style.color = new Color(0.7f, 0.75f, 0.8f);
            _status.style.fontSize = 11;
            _status.style.marginTop = 8;
            _status.style.whiteSpace = WhiteSpace.Normal;
            _panel.Add(_status);

            root.Add(_panel);
        }

        public void TogglePanel()
        {
            _panelOpen = !_panelOpen;
            _panel.style.display = _panelOpen ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void OpenPanel()
        {
            _panelOpen = true;
            _panel.style.display = DisplayStyle.Flex;
        }

        private static Button MakeButton(string text, System.Action action)
        {
            var btn = new Button(action) { text = text };
            btn.style.marginTop = 4;
            btn.style.height = 28;
            btn.style.fontSize = 12;
            btn.style.backgroundColor = BetaVisualTheme.ButtonFace;
            btn.style.color = BetaVisualTheme.TextPrimary;
            return btn;
        }
    }
}
