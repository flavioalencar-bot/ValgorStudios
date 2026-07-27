using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>
    /// Menu contextual compacto ancorado ao edifício selecionado.
    /// </summary>
    public sealed class BuildingContextMenu
    {
        public const string RootName = "building-context-menu";

        private readonly VisualElement _root;
        private readonly Label _title;
        private readonly VisualElement _actionsHost;
        private readonly BuildingContextMenuPositioner _positioner;
        private Action<BuildingContextAction>? _onAction;

        public BuildingContextMenu(VisualElement parent)
        {
            _positioner = new BuildingContextMenuPositioner();
            _root = new VisualElement { name = RootName };
            _root.style.position = Position.Absolute;
            _root.style.width = 168;
            _root.style.paddingLeft = 8;
            _root.style.paddingRight = 8;
            _root.style.paddingTop = 8;
            _root.style.paddingBottom = 8;
            _root.style.backgroundColor = new Color(0.09f, 0.1f, 0.11f, 0.96f);
            _root.style.borderTopWidth = 2;
            _root.style.borderBottomWidth = 2;
            _root.style.borderLeftWidth = 2;
            _root.style.borderRightWidth = 2;
            _root.style.borderTopColor = BetaVisualTheme.AgedGold;
            _root.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _root.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _root.style.borderRightColor = BetaVisualTheme.AgedGold;
            _root.pickingMode = PickingMode.Position;
            _root.style.display = DisplayStyle.None;

            _title = new Label();
            _title.style.color = BetaVisualTheme.AgedGoldBright;
            _title.style.fontSize = 13;
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title.style.whiteSpace = WhiteSpace.Normal;
            _title.style.marginBottom = 6;
            _title.pickingMode = PickingMode.Ignore;
            _root.Add(_title);

            _actionsHost = new VisualElement { name = "context-actions" };
            _root.Add(_actionsHost);
            parent.Add(_root);
        }

        public VisualElement Root => _root;
        public bool IsVisible => _root.style.display == DisplayStyle.Flex;

        public void Show(
            string title,
            IReadOnlyList<BuildingContextActionInfo> actions,
            Action<BuildingContextAction> onAction)
        {
            _onAction = onAction;
            _title.text = title;
            _actionsHost.Clear();
            foreach (var info in actions)
            {
                var captured = info;
                var button = new Button(() =>
                {
                    if (!captured.Enabled)
                    {
                        return;
                    }

                    _onAction?.Invoke(captured.Action);
                })
                {
                    text = captured.Enabled
                        ? captured.Label
                        : $"{captured.Label}…"
                };
                StyleActionButton(button, captured.Enabled);
                if (!captured.Enabled && !string.IsNullOrEmpty(captured.DisabledReason))
                {
                    button.tooltip = captured.DisabledReason;
                }

                _actionsHost.Add(button);
            }

            _root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
            _actionsHost.Clear();
            _onAction = null;
        }

        public void Reposition(VisualElement panelRoot, Camera camera, Vector3 worldAnchor)
        {
            if (!IsVisible)
            {
                return;
            }

            _root.schedule.Execute(() =>
            {
                var height = _root.resolvedStyle.height > 1f
                    ? _root.resolvedStyle.height
                    : -1f;
                _positioner.Apply(_root, panelRoot, camera, worldAnchor, height);
            });
        }

        private static void StyleActionButton(Button button, bool enabled)
        {
            button.style.marginTop = 4;
            button.style.marginBottom = 0;
            button.style.width = Length.Percent(100);
            button.style.paddingLeft = 8;
            button.style.paddingRight = 8;
            button.style.paddingTop = 7;
            button.style.paddingBottom = 7;
            button.style.fontSize = 12;
            button.style.unityTextAlign = TextAnchor.MiddleLeft;
            button.style.backgroundColor = enabled
                ? BetaVisualTheme.ButtonFace
                : new Color(0.12f, 0.12f, 0.13f, 0.85f);
            button.style.color = enabled
                ? BetaVisualTheme.TextPrimary
                : BetaVisualTheme.TextMuted;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = BetaVisualTheme.ButtonBorder;
            button.style.borderBottomColor = BetaVisualTheme.ButtonBorder;
            button.style.borderLeftColor = BetaVisualTheme.ButtonBorder;
            button.style.borderRightColor = BetaVisualTheme.ButtonBorder;
            button.SetEnabled(enabled);
        }
    }
}
