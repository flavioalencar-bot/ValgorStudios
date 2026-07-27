using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>
    /// Menu contextual compacto (botões circulares) ancorado ao edifício.
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
            _root.style.minWidth = 150;
            _root.style.maxWidth = 210;
            _root.style.paddingLeft = 10;
            _root.style.paddingRight = 10;
            _root.style.paddingTop = 8;
            _root.style.paddingBottom = 10;
            _root.style.backgroundColor = new Color(0.08f, 0.09f, 0.1f, 0.96f);
            _root.style.borderTopWidth = 2;
            _root.style.borderBottomWidth = 2;
            _root.style.borderLeftWidth = 2;
            _root.style.borderRightWidth = 2;
            _root.style.borderTopColor = BetaVisualTheme.AgedGold;
            _root.style.borderBottomColor = BetaVisualTheme.AgedGold;
            _root.style.borderLeftColor = BetaVisualTheme.AgedGold;
            _root.style.borderRightColor = BetaVisualTheme.AgedGold;
            _root.style.borderTopLeftRadius = 10;
            _root.style.borderTopRightRadius = 10;
            _root.style.borderBottomLeftRadius = 10;
            _root.style.borderBottomRightRadius = 10;
            _root.pickingMode = PickingMode.Position;
            _root.style.display = DisplayStyle.None;

            _title = new Label();
            _title.style.color = BetaVisualTheme.AgedGoldBright;
            _title.style.fontSize = 13;
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title.style.whiteSpace = WhiteSpace.Normal;
            _title.style.marginBottom = 8;
            _title.style.unityTextAlign = TextAnchor.MiddleCenter;
            _title.pickingMode = PickingMode.Ignore;
            _root.Add(_title);

            _actionsHost = new VisualElement { name = "context-actions" };
            _actionsHost.style.flexDirection = FlexDirection.Row;
            _actionsHost.style.flexWrap = Wrap.Wrap;
            _actionsHost.style.justifyContent = Justify.Center;
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
                    text = CompactLabel(captured)
                };
                StyleCircularButton(button, captured);
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

        public void Reposition(VisualElement panelRoot, UnityEngine.Camera camera, Vector3 worldAnchor)
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

        private static string CompactLabel(BuildingContextActionInfo info)
        {
            if (!info.Enabled)
            {
                return info.Label.Length <= 8 ? info.Label : info.Label[..7] + "…";
            }

            return info.Label;
        }

        private static void StyleCircularButton(Button button, BuildingContextActionInfo info)
        {
            button.style.marginLeft = 4;
            button.style.marginRight = 4;
            button.style.marginTop = 4;
            button.style.marginBottom = 4;
            button.style.width = 64;
            button.style.height = 64;
            button.style.borderTopLeftRadius = 32;
            button.style.borderTopRightRadius = 32;
            button.style.borderBottomLeftRadius = 32;
            button.style.borderBottomRightRadius = 32;
            button.style.paddingLeft = 4;
            button.style.paddingRight = 4;
            button.style.paddingTop = 4;
            button.style.paddingBottom = 4;
            button.style.fontSize = 11;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.whiteSpace = WhiteSpace.Normal;
            button.style.borderTopWidth = 2;
            button.style.borderBottomWidth = 2;
            button.style.borderLeftWidth = 2;
            button.style.borderRightWidth = 2;

            if (!info.Enabled)
            {
                button.style.backgroundColor = new Color(0.18f, 0.12f, 0.12f, 0.95f);
                button.style.borderTopColor = new Color(0.55f, 0.22f, 0.2f);
                button.style.borderBottomColor = new Color(0.55f, 0.22f, 0.2f);
                button.style.borderLeftColor = new Color(0.55f, 0.22f, 0.2f);
                button.style.borderRightColor = new Color(0.55f, 0.22f, 0.2f);
                button.style.color = new Color(0.85f, 0.55f, 0.5f);
            }
            else if (info.Action == BuildingContextAction.Collect)
            {
                button.style.backgroundColor = new Color(0.14f, 0.28f, 0.18f, 0.96f);
                button.style.borderTopColor = new Color(0.35f, 0.75f, 0.42f);
                button.style.borderBottomColor = new Color(0.35f, 0.75f, 0.42f);
                button.style.borderLeftColor = new Color(0.35f, 0.75f, 0.42f);
                button.style.borderRightColor = new Color(0.35f, 0.75f, 0.42f);
                button.style.color = BetaVisualTheme.TextPrimary;
            }
            else
            {
                button.style.backgroundColor = new Color(0.14f, 0.15f, 0.18f, 0.96f);
                button.style.borderTopColor = BetaVisualTheme.AgedGold;
                button.style.borderBottomColor = BetaVisualTheme.AgedGold;
                button.style.borderLeftColor = BetaVisualTheme.AgedGold;
                button.style.borderRightColor = BetaVisualTheme.AgedGold;
                button.style.color = BetaVisualTheme.TextPrimary;
            }

            button.SetEnabled(info.Enabled);
        }
    }
}
