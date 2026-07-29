using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>
    /// Menu contextual premium: botões circulares flutuantes (sem painel preto pesado).
    /// </summary>
    public sealed class BuildingContextMenu
    {
        public const string RootName = "building-context-menu";
        private const float ButtonSize = 64f;
        private const float ButtonRadius = 32f;

        private readonly VisualElement _root;
        private readonly VisualElement _actionsHost;
        private readonly BuildingContextMenuPositioner _positioner;
        private Action<BuildingContextAction>? _onAction;
        private BuildingContextAction? _selectedAction;
        private readonly List<ContextButtonView> _buttons = new();

        public BuildingContextMenu(VisualElement parent)
        {
            _positioner = new BuildingContextMenuPositioner();

            _root = new VisualElement { name = RootName };
            _root.style.position = Position.Absolute;
            _root.style.minWidth = 160;
            _root.style.maxWidth = 320;
            _root.style.paddingLeft = 8;
            _root.style.paddingRight = 8;
            _root.style.paddingTop = 8;
            _root.style.paddingBottom = 8;
            // Fundo quase invisível — botões flutuantes.
            _root.style.backgroundColor = new Color(0.04f, 0.05f, 0.07f, 0.22f);
            _root.style.borderTopWidth = 0;
            _root.style.borderBottomWidth = 0;
            _root.style.borderLeftWidth = 0;
            _root.style.borderRightWidth = 0;
            _root.style.borderTopLeftRadius = 16;
            _root.style.borderTopRightRadius = 16;
            _root.style.borderBottomLeftRadius = 16;
            _root.style.borderBottomRightRadius = 16;
            _root.pickingMode = PickingMode.Position;
            _root.style.display = DisplayStyle.None;

            _actionsHost = new VisualElement { name = "context-actions" };
            _actionsHost.style.flexDirection = FlexDirection.Row;
            _actionsHost.style.flexWrap = Wrap.Wrap;
            _actionsHost.style.justifyContent = Justify.Center;
            _actionsHost.style.alignItems = Align.FlexStart;
            _root.Add(_actionsHost);
            parent.Add(_root);
        }

        public VisualElement Root => _root;
        public bool IsVisible => _root.style.display == DisplayStyle.Flex;

        public void Show(
            IReadOnlyList<BuildingContextActionInfo> actions,
            Action<BuildingContextAction> onAction,
            BuildingContextAction? selectedAction = null)
        {
            _onAction = onAction;
            _selectedAction = selectedAction;
            _actionsHost.Clear();
            _buttons.Clear();

            foreach (var info in actions)
            {
                var view = CreateButton(info);
                _buttons.Add(view);
                _actionsHost.Add(view.Column);
            }

            ApplySelectedStyles();
            _root.style.display = DisplayStyle.Flex;
        }

        public void SetSelectedAction(BuildingContextAction? action)
        {
            _selectedAction = action;
            ApplySelectedStyles();
        }

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
            _actionsHost.Clear();
            _buttons.Clear();
            _onAction = null;
            _selectedAction = null;
        }

        public void Reposition(
            VisualElement panelRoot,
            UnityEngine.Camera camera,
            Rect buildingScreenRect,
            bool reserveRightPanel = false)
        {
            if (!IsVisible)
            {
                return;
            }

            _root.schedule.Execute(() =>
            {
                var height = _root.resolvedStyle.height > 1f ? _root.resolvedStyle.height : -1f;
                var width = _root.resolvedStyle.width > 1f ? _root.resolvedStyle.width : -1f;
                _positioner.Apply(
                    _root,
                    panelRoot,
                    camera,
                    buildingScreenRect,
                    height,
                    reserveRightPanel,
                    measuredWidth: width);
            });
        }

        private ContextButtonView CreateButton(BuildingContextActionInfo info)
        {
            var captured = info;
            var column = new VisualElement();
            column.style.alignItems = Align.Center;
            column.style.marginLeft = 8;
            column.style.marginRight = 8;
            column.style.marginTop = 2;
            column.style.marginBottom = 2;
            column.style.width = ButtonSize + 12f;

            var shadow = new VisualElement();
            shadow.pickingMode = PickingMode.Ignore;
            shadow.style.position = Position.Absolute;
            shadow.style.width = ButtonSize;
            shadow.style.height = ButtonSize;
            shadow.style.left = 6;
            shadow.style.top = 5;
            shadow.style.backgroundColor = new Color(0f, 0f, 0f, 0.4f);
            shadow.style.borderTopLeftRadius = ButtonRadius;
            shadow.style.borderTopRightRadius = ButtonRadius;
            shadow.style.borderBottomLeftRadius = ButtonRadius;
            shadow.style.borderBottomRightRadius = ButtonRadius;
            column.Add(shadow);

            var button = new Button(() =>
            {
                if (!captured.Enabled)
                {
                    return;
                }

                _selectedAction = captured.Action;
                ApplySelectedStyles();
                _onAction?.Invoke(captured.Action);
            });
            button.name = $"ctx-btn-{captured.Action}-{captured.Label}";
            button.text = string.Empty;
            button.style.width = ButtonSize;
            button.style.height = ButtonSize;
            button.style.borderTopLeftRadius = ButtonRadius;
            button.style.borderTopRightRadius = ButtonRadius;
            button.style.borderBottomLeftRadius = ButtonRadius;
            button.style.borderBottomRightRadius = ButtonRadius;
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            button.style.paddingTop = 0;
            button.style.paddingBottom = 0;
            button.style.alignItems = Align.Center;
            button.style.justifyContent = Justify.Center;
            button.style.borderTopWidth = 2;
            button.style.borderBottomWidth = 2;
            button.style.borderLeftWidth = 2;
            button.style.borderRightWidth = 2;
            button.focusable = false;

            var icon = new Label(IconGlyph(captured.Icon));
            icon.pickingMode = PickingMode.Ignore;
            icon.style.fontSize = captured.Icon == BuildingContextIcon.Crown ? 24 : 22;
            icon.style.unityTextAlign = TextAnchor.MiddleCenter;
            icon.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.Add(icon);

            var label = new Label(ShortLabel(captured.Label));
            label.pickingMode = PickingMode.Ignore;
            label.style.marginTop = 6;
            label.style.fontSize = 11;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.color = BetaVisualTheme.TextPrimary;

            var tip = !string.IsNullOrEmpty(captured.DisabledReason)
                ? captured.DisabledReason
                : null;
            if (!string.IsNullOrEmpty(tip))
            {
                button.tooltip = tip;
                label.tooltip = tip;
            }

            var view = new ContextButtonView(captured, column, button, icon, label, shadow);
            ApplyButtonVisual(view, hovered: false, pressed: false);

            button.RegisterCallback<MouseEnterEvent>(_ =>
            {
                if (captured.Enabled)
                {
                    ApplyButtonVisual(view, hovered: true, pressed: false);
                }
            });
            button.RegisterCallback<MouseLeaveEvent>(_ =>
                ApplyButtonVisual(view, hovered: false, pressed: false));
            button.RegisterCallback<MouseDownEvent>(_ =>
            {
                if (captured.Enabled)
                {
                    ApplyButtonVisual(view, hovered: true, pressed: true);
                }
            });
            button.RegisterCallback<MouseUpEvent>(_ =>
                ApplyButtonVisual(view, hovered: true, pressed: false));

            button.SetEnabled(captured.Enabled);
            column.Add(button);
            column.Add(label);
            return view;
        }

        private void ApplySelectedStyles()
        {
            for (var i = 0; i < _buttons.Count; i++)
            {
                ApplyButtonVisual(_buttons[i], hovered: false, pressed: false);
            }
        }

        private void ApplyButtonVisual(ContextButtonView view, bool hovered, bool pressed)
        {
            var info = view.Info;
            var selected = _selectedAction.HasValue && _selectedAction.Value == info.Action;
            var maxLevel = string.Equals(info.Label, "Nível Máximo", StringComparison.Ordinal);

            Color face;
            Color border;
            Color iconColor;
            Color labelColor;

            if (!info.Enabled || maxLevel)
            {
                // Dourado/cinza nobre para nível máximo; vermelho suave para demais bloqueios.
                if (maxLevel)
                {
                    face = new Color(0.22f, 0.2f, 0.14f, 0.95f);
                    border = new Color(0.72f, 0.62f, 0.38f, 0.95f);
                    iconColor = new Color(0.88f, 0.78f, 0.48f, 0.95f);
                    labelColor = new Color(0.82f, 0.74f, 0.52f, 0.95f);
                }
                else
                {
                    face = new Color(0.16f, 0.14f, 0.14f, 0.92f);
                    border = new Color(0.55f, 0.28f, 0.26f, 0.95f);
                    iconColor = new Color(0.7f, 0.48f, 0.45f, 0.9f);
                    labelColor = new Color(0.72f, 0.5f, 0.48f, 0.95f);
                }
            }
            else if (pressed)
            {
                face = new Color(0.1f, 0.12f, 0.16f, 0.98f);
                border = BetaVisualTheme.AgedGoldBright;
                iconColor = BetaVisualTheme.AgedGoldBright;
                labelColor = BetaVisualTheme.AgedGoldBright;
            }
            else if (selected)
            {
                face = new Color(0.28f, 0.2f, 0.08f, 0.98f);
                border = BetaVisualTheme.AgedGoldBright;
                iconColor = BetaVisualTheme.AgedGoldBright;
                labelColor = BetaVisualTheme.AgedGoldBright;
            }
            else if (hovered)
            {
                face = new Color(0.18f, 0.22f, 0.3f, 0.98f);
                border = new Color(0.95f, 0.82f, 0.48f, 1f);
                iconColor = BetaVisualTheme.TextPrimary;
                labelColor = BetaVisualTheme.AgedGoldBright;
            }
            else if (info.Action == BuildingContextAction.Collect)
            {
                face = new Color(0.12f, 0.26f, 0.16f, 0.96f);
                border = new Color(0.4f, 0.78f, 0.48f, 1f);
                iconColor = new Color(0.75f, 0.95f, 0.78f, 1f);
                labelColor = BetaVisualTheme.TextPrimary;
            }
            else if (info.Action == BuildingContextAction.Decoration)
            {
                face = new Color(0.18f, 0.14f, 0.24f, 0.96f);
                border = new Color(0.72f, 0.55f, 0.9f, 0.95f);
                iconColor = new Color(0.9f, 0.8f, 1f, 1f);
                labelColor = BetaVisualTheme.TextPrimary;
            }
            else
            {
                face = new Color(0.11f, 0.14f, 0.2f, 0.96f);
                border = BetaVisualTheme.AgedGold;
                iconColor = BetaVisualTheme.AgedGoldBright;
                labelColor = BetaVisualTheme.TextPrimary;
            }

            view.Button.style.backgroundColor = face;
            view.Button.style.borderTopColor = border;
            view.Button.style.borderBottomColor = border;
            view.Button.style.borderLeftColor = border;
            view.Button.style.borderRightColor = border;
            var thick = selected || hovered || pressed ? 3 : 2;
            view.Button.style.borderBottomWidth = thick;
            view.Button.style.borderTopWidth = thick;
            view.Icon.style.color = iconColor;
            view.Label.style.color = labelColor;
            view.Shadow.style.opacity = hovered || selected || pressed ? 0.9f : 0.5f;
            view.Button.style.scale = pressed
                ? new Scale(new Vector2(0.94f, 0.94f))
                : new Scale(Vector2.one);
        }

        private static string ShortLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return string.Empty;
            }

            return label.Length <= 12 ? label : label[..11] + "…";
        }

        private static string IconGlyph(BuildingContextIcon icon) =>
            icon switch
            {
                BuildingContextIcon.Brush => "✎",
                BuildingContextIcon.Info => "i",
                BuildingContextIcon.Upgrade => "▲",
                BuildingContextIcon.Crown => "♛",
                BuildingContextIcon.Collect => "◆",
                BuildingContextIcon.Open => "►",
                BuildingContextIcon.Feed => "+",
                BuildingContextIcon.Send => "»",
                BuildingContextIcon.Train => "+",
                BuildingContextIcon.Research => "*",
                BuildingContextIcon.Produce => "o",
                _ => "•"
            };

        private sealed class ContextButtonView
        {
            public ContextButtonView(
                BuildingContextActionInfo info,
                VisualElement column,
                Button button,
                Label icon,
                Label label,
                VisualElement shadow)
            {
                Info = info;
                Column = column;
                Button = button;
                Icon = icon;
                Label = label;
                Shadow = shadow;
            }

            public BuildingContextActionInfo Info { get; }
            public VisualElement Column { get; }
            public Button Button { get; }
            public Label Icon { get; }
            public Label Label { get; }
            public VisualElement Shadow { get; }
        }
    }
}
