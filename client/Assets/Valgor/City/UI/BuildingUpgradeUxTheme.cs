using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>Identidade visual Valgor para modais de evolução (pergaminho/pedra/ouro).</summary>
    public static class BuildingUpgradeUxTheme
    {
        public static readonly Color ScrollBg = new(0.08f, 0.09f, 0.11f, 0.98f);
        public static readonly Color ScrollInner = new(0.11f, 0.12f, 0.15f, 0.96f);
        public static readonly Color DeepBlue = new(0.08f, 0.18f, 0.34f, 1f);
        public static readonly Color DeepBlueButton = new(0.12f, 0.28f, 0.52f, 1f);
        public static readonly Color InstantGold = new(0.78f, 0.62f, 0.22f, 1f);
        public static readonly Color Backdrop = new(0.02f, 0.03f, 0.05f, 0.62f);
        public static readonly Color PreviewStone = new(0.16f, 0.15f, 0.14f, 1f);
        public static readonly Color RowBg = new(0.12f, 0.13f, 0.16f, 0.9f);
        public static readonly Color RowBlocked = new(0.22f, 0.1f, 0.1f, 0.85f);
        public static readonly Color RowOk = new(0.1f, 0.18f, 0.12f, 0.85f);

        public static void ApplyModalShell(VisualElement panel, float maxWidth = 560f)
        {
            panel.style.position = Position.Absolute;
            panel.style.left = new Length(50, LengthUnit.Percent);
            panel.style.top = new Length(50, LengthUnit.Percent);
            panel.style.translate = new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent));
            panel.style.width = new Length(86, LengthUnit.Percent);
            panel.style.maxWidth = maxWidth;
            panel.style.maxHeight = new Length(88, LengthUnit.Percent);
            panel.style.minWidth = 320;
            panel.style.paddingLeft = 16;
            panel.style.paddingRight = 16;
            panel.style.paddingTop = 14;
            panel.style.paddingBottom = 14;
            panel.style.backgroundColor = ScrollBg;
            panel.style.borderTopWidth = 2;
            panel.style.borderBottomWidth = 2;
            panel.style.borderLeftWidth = 2;
            panel.style.borderRightWidth = 2;
            panel.style.borderTopColor = BetaVisualTheme.AgedGold;
            panel.style.borderBottomColor = BetaVisualTheme.AgedGold;
            panel.style.borderLeftColor = BetaVisualTheme.AgedGold;
            panel.style.borderRightColor = BetaVisualTheme.AgedGold;
            panel.style.flexDirection = FlexDirection.Column;
            panel.style.display = DisplayStyle.None;
            panel.pickingMode = PickingMode.Position;
        }

        public static VisualElement CreateBackdrop(string name, Action? onClick = null)
        {
            var backdrop = new VisualElement { name = name };
            backdrop.style.position = Position.Absolute;
            backdrop.style.left = 0;
            backdrop.style.top = 0;
            backdrop.style.right = 0;
            backdrop.style.bottom = 0;
            backdrop.style.backgroundColor = Backdrop;
            backdrop.style.display = DisplayStyle.None;
            backdrop.pickingMode = PickingMode.Position;
            if (onClick != null)
            {
                backdrop.RegisterCallback<ClickEvent>(_ => onClick());
            }

            return backdrop;
        }

        public static Button CreateButton(string text, Action action, ButtonKind kind = ButtonKind.Primary, bool enabled = true)
        {
            var button = new Button(action) { text = text };
            button.SetEnabled(enabled);
            button.style.marginTop = 6;
            button.style.marginLeft = 4;
            button.style.marginRight = 4;
            button.style.paddingLeft = 14;
            button.style.paddingRight = 14;
            button.style.paddingTop = 9;
            button.style.paddingBottom = 9;
            button.style.fontSize = 13;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;

            Color face;
            Color border;
            Color textColor = BetaVisualTheme.TextPrimary;
            switch (kind)
            {
                case ButtonKind.Go:
                    face = DeepBlueButton;
                    border = new Color(0.35f, 0.55f, 0.85f);
                    break;
                case ButtonKind.Instant:
                    face = InstantGold;
                    border = BetaVisualTheme.AgedGoldBright;
                    textColor = new Color(0.12f, 0.1f, 0.05f);
                    break;
                case ButtonKind.Danger:
                    face = new Color(0.35f, 0.14f, 0.12f);
                    border = BetaVisualTheme.Danger;
                    break;
                case ButtonKind.Ghost:
                    face = new Color(0.14f, 0.14f, 0.16f);
                    border = BetaVisualTheme.ButtonBorder;
                    break;
                default:
                    face = BetaVisualTheme.ButtonFace;
                    border = BetaVisualTheme.ButtonBorder;
                    break;
            }

            if (!enabled)
            {
                face = new Color(0.18f, 0.18f, 0.2f, 0.9f);
                textColor = new Color(0.55f, 0.55f, 0.58f);
                button.style.opacity = 0.55f;
            }

            button.style.backgroundColor = face;
            button.style.color = textColor;
            button.style.borderTopColor = border;
            button.style.borderBottomColor = border;
            button.style.borderLeftColor = border;
            button.style.borderRightColor = border;
            return button;
        }

        public static VisualElement CreatePreviewBlock(string labelText)
        {
            var box = new VisualElement();
            box.style.width = 88;
            box.style.height = 88;
            box.style.flexShrink = 0;
            box.style.backgroundColor = PreviewStone;
            box.style.justifyContent = Justify.Center;
            box.style.alignItems = Align.Center;
            box.style.borderTopWidth = 1;
            box.style.borderBottomWidth = 1;
            box.style.borderLeftWidth = 1;
            box.style.borderRightWidth = 1;
            box.style.borderTopColor = BetaVisualTheme.AgedGold;
            box.style.borderBottomColor = BetaVisualTheme.AgedGold;
            box.style.borderLeftColor = BetaVisualTheme.AgedGold;
            box.style.borderRightColor = BetaVisualTheme.AgedGold;
            box.style.marginRight = 12;

            var label = new Label(labelText);
            label.style.color = BetaVisualTheme.AgedGoldBright;
            label.style.fontSize = 11;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.paddingLeft = 4;
            label.style.paddingRight = 4;
            box.Add(label);
            return box;
        }

        public enum ButtonKind
        {
            Primary,
            Go,
            Instant,
            Danger,
            Ghost
        }
    }
}
