using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>Painel Detalhes ancorado à direita — sempre acima do menu contextual.</summary>
    public sealed class BuildingDetailsPanel
    {
        public const string RootName = "building-details-panel";

        private readonly VisualElement _root;
        private readonly Label _title;
        private readonly Label _body;
        private readonly VisualElement _buttons;
        private readonly Label _feedback;

        public BuildingDetailsPanel(VisualElement parent)
        {
            _root = new VisualElement { name = RootName };
            _root.style.position = Position.Absolute;
            _root.style.right = 16;
            _root.style.top = 56;
            _root.style.bottom = 80;
            _root.style.width = 360;
            _root.style.maxWidth = 380;
            _root.style.maxHeight = 760;
            _root.style.paddingLeft = 14;
            _root.style.paddingRight = 14;
            _root.style.paddingTop = 12;
            _root.style.paddingBottom = 12;
            _root.style.backgroundColor = new Color(0.1f, 0.11f, 0.12f, 0.98f);
            _root.style.borderTopWidth = 2;
            _root.style.borderBottomWidth = 2;
            _root.style.borderLeftWidth = 2;
            _root.style.borderRightWidth = 2;
            _root.style.borderTopColor = BetaVisualTheme.AgedGold;
            _root.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _root.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _root.style.borderRightColor = BetaVisualTheme.AgedGold;
            _root.style.flexDirection = FlexDirection.Column;
            _root.style.display = DisplayStyle.None;
            _root.style.opacity = 1f;
            _root.pickingMode = PickingMode.Position;

            _title = new Label();
            _title.style.color = BetaVisualTheme.AgedGoldBright;
            _title.style.fontSize = 16;
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title.style.marginBottom = 8;
            _title.style.flexShrink = 0;
            _title.style.whiteSpace = WhiteSpace.Normal;
            _root.Add(_title);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 120;
            _body = new Label();
            _body.style.color = BetaVisualTheme.TextPrimary;
            _body.style.fontSize = 13;
            _body.style.whiteSpace = WhiteSpace.Normal;
            scroll.Add(_body);
            _root.Add(scroll);

            _feedback = new Label();
            _feedback.style.color = BetaVisualTheme.AgedGoldBright;
            _feedback.style.marginTop = 8;
            _feedback.style.whiteSpace = WhiteSpace.Normal;
            _feedback.style.flexShrink = 0;
            _root.Add(_feedback);

            _buttons = new VisualElement { name = "details-panel-buttons" };
            _buttons.style.flexShrink = 0;
            _buttons.style.marginTop = 4;
            _root.Add(_buttons);

            parent.Add(_root);
        }

        public VisualElement Root => _root;
        public bool IsVisible => _root.resolvedStyle.display == DisplayStyle.Flex ||
                                 _root.style.display == DisplayStyle.Flex;

        public void Show(BuildingDetailsViewModel model, Action onClose)
        {
            _title.text = model.Title;
            _body.text = model.Body;
            _feedback.text = string.Empty;
            _buttons.Clear();

            var close = new Button(onClose) { text = "Fechar" };
            StyleButton(close);
            _buttons.Add(close);

            _root.style.display = DisplayStyle.Flex;
            _root.style.visibility = Visibility.Visible;
            _root.style.opacity = 1f;
            _root.BringToFront();
        }

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
            _buttons.Clear();
        }

        private static void StyleButton(Button button)
        {
            button.style.marginTop = 8;
            button.style.paddingLeft = 12;
            button.style.paddingRight = 12;
            button.style.paddingTop = 8;
            button.style.paddingBottom = 8;
            button.style.backgroundColor = BetaVisualTheme.ButtonFace;
            button.style.color = BetaVisualTheme.TextPrimary;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = BetaVisualTheme.ButtonBorder;
            button.style.borderBottomColor = BetaVisualTheme.ButtonBorder;
            button.style.borderLeftColor = BetaVisualTheme.ButtonBorder;
            button.style.borderRightColor = BetaVisualTheme.ButtonBorder;
            button.style.fontSize = 13;
        }
    }
}
