using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>
    /// Toast flutuante curto (ex.: placeholder de skins).
    /// </summary>
    public sealed class BuildingContextToast
    {
        private readonly VisualElement _root;
        private readonly Label _label;
        private IVisualElementScheduledItem? _hideJob;

        public BuildingContextToast(VisualElement parent)
        {
            _root = new VisualElement { name = "building-context-toast" };
            _root.style.position = Position.Absolute;
            _root.style.left = Length.Percent(50);
            _root.style.bottom = 96;
            _root.style.translate = new Translate(Length.Percent(-50), 0);
            _root.style.minWidth = 280;
            _root.style.maxWidth = 420;
            _root.style.paddingLeft = 18;
            _root.style.paddingRight = 18;
            _root.style.paddingTop = 12;
            _root.style.paddingBottom = 12;
            _root.style.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 0.94f);
            _root.style.borderTopWidth = 2;
            _root.style.borderBottomWidth = 2;
            _root.style.borderLeftWidth = 2;
            _root.style.borderRightWidth = 2;
            _root.style.borderTopColor = BetaVisualTheme.AgedGold;
            _root.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _root.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _root.style.borderRightColor = BetaVisualTheme.AgedGold;
            _root.style.borderTopLeftRadius = 12;
            _root.style.borderTopRightRadius = 12;
            _root.style.borderBottomLeftRadius = 12;
            _root.style.borderBottomRightRadius = 12;
            _root.style.display = DisplayStyle.None;
            _root.pickingMode = PickingMode.Ignore;

            _label = new Label();
            _label.style.color = BetaVisualTheme.TextPrimary;
            _label.style.fontSize = 14;
            _label.style.unityTextAlign = TextAnchor.MiddleCenter;
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _label.style.whiteSpace = WhiteSpace.Normal;
            _label.pickingMode = PickingMode.Ignore;
            _root.Add(_label);
            parent.Add(_root);
        }

        public void Show(string message, float seconds = 2.4f)
        {
            _hideJob?.Pause();
            _label.text = message ?? string.Empty;
            _root.style.display = DisplayStyle.Flex;
            _root.BringToFront();
            _hideJob = _root.schedule.Execute(Hide).StartingIn((long)(seconds * 1000f));
        }

        public void Hide() => _root.style.display = DisplayStyle.None;
    }
}
