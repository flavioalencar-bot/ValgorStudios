using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.City.Data;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>Identidade visual Valgor para modais de evolução (pedra/pergaminho/ouro).</summary>
    public static class BuildingUpgradeUxTheme
    {
        public static readonly Color ScrollBg = new(0.07f, 0.08f, 0.1f, 0.985f);
        public static readonly Color ScrollInner = new(0.1f, 0.11f, 0.14f, 0.97f);
        public static readonly Color ParchmentInset = new(0.14f, 0.12f, 0.1f, 0.55f);
        public static readonly Color DeepBlue = new(0.07f, 0.16f, 0.32f, 1f);
        public static readonly Color DeepBlueButton = new(0.11f, 0.26f, 0.5f, 1f);
        public static readonly Color InstantGold = new(0.8f, 0.64f, 0.22f, 1f);
        public static readonly Color Backdrop = new(0.02f, 0.03f, 0.05f, 0.72f);
        public static readonly Color PreviewStone = new(0.11f, 0.11f, 0.13f, 1f);
        public static readonly Color RowBg = new(0.11f, 0.12f, 0.15f, 0.92f);
        public static readonly Color RowBlocked = new(0.24f, 0.1f, 0.1f, 0.9f);
        public static readonly Color RowOk = new(0.09f, 0.18f, 0.12f, 0.9f);
        public static readonly Color FrameOuter = new(0.55f, 0.42f, 0.2f, 1f);
        public static readonly Color FrameInner = new(0.82f, 0.68f, 0.35f, 0.85f);

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
            panel.style.paddingLeft = 18;
            panel.style.paddingRight = 18;
            panel.style.paddingTop = 16;
            panel.style.paddingBottom = 16;
            panel.style.backgroundColor = ScrollBg;
            panel.style.borderTopWidth = 3;
            panel.style.borderBottomWidth = 3;
            panel.style.borderLeftWidth = 3;
            panel.style.borderRightWidth = 3;
            panel.style.borderTopColor = FrameOuter;
            panel.style.borderBottomColor = FrameOuter;
            panel.style.borderLeftColor = FrameOuter;
            panel.style.borderRightColor = FrameOuter;
            panel.style.borderTopLeftRadius = 4;
            panel.style.borderTopRightRadius = 4;
            panel.style.borderBottomLeftRadius = 4;
            panel.style.borderBottomRightRadius = 4;
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
            button.style.paddingLeft = 16;
            button.style.paddingRight = 16;
            button.style.paddingTop = 10;
            button.style.paddingBottom = 10;
            button.style.minHeight = 40;
            button.style.fontSize = 13;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.borderTopWidth = 2;
            button.style.borderBottomWidth = 2;
            button.style.borderLeftWidth = 2;
            button.style.borderRightWidth = 2;
            button.style.borderTopLeftRadius = 3;
            button.style.borderTopRightRadius = 3;
            button.style.borderBottomLeftRadius = 3;
            button.style.borderBottomRightRadius = 3;

            Color face;
            Color border;
            Color textColor = BetaVisualTheme.TextPrimary;
            switch (kind)
            {
                case ButtonKind.Go:
                    face = DeepBlueButton;
                    border = new Color(0.4f, 0.62f, 0.92f);
                    break;
                case ButtonKind.Instant:
                    face = InstantGold;
                    border = BetaVisualTheme.AgedGoldBright;
                    textColor = new Color(0.12f, 0.1f, 0.05f);
                    break;
                case ButtonKind.Danger:
                    face = new Color(0.38f, 0.14f, 0.12f);
                    border = BetaVisualTheme.Danger;
                    break;
                case ButtonKind.Ghost:
                    face = new Color(0.13f, 0.13f, 0.15f);
                    border = FrameOuter;
                    break;
                default:
                    face = DeepBlue;
                    border = FrameInner;
                    break;
            }

            if (!enabled)
            {
                face = new Color(0.16f, 0.16f, 0.18f, 0.92f);
                textColor = new Color(0.5f, 0.5f, 0.52f);
                button.style.opacity = 0.55f;
            }

            button.style.backgroundColor = face;
            button.style.color = textColor;
            button.style.borderTopColor = border;
            button.style.borderBottomColor = Color.Lerp(border, Color.black, 0.35f);
            button.style.borderLeftColor = border;
            button.style.borderRightColor = border;
            return button;
        }

        /// <summary>Host do preview 3D (RenderTexture) com moldura de pedra/ouro.</summary>
        public static VisualElement CreatePreviewHost(string fallbackLabel, string buildingId, int level)
        {
            var frame = new VisualElement { name = "building-preview-frame" };
            frame.style.width = 112;
            frame.style.height = 112;
            frame.style.flexShrink = 0;
            frame.style.marginRight = 14;
            frame.style.backgroundColor = PreviewStone;
            frame.style.borderTopWidth = 2;
            frame.style.borderBottomWidth = 2;
            frame.style.borderLeftWidth = 2;
            frame.style.borderRightWidth = 2;
            frame.style.borderTopColor = FrameInner;
            frame.style.borderBottomColor = FrameOuter;
            frame.style.borderLeftColor = FrameInner;
            frame.style.borderRightColor = FrameOuter;
            frame.style.justifyContent = Justify.Center;
            frame.style.alignItems = Align.Center;
            frame.style.overflow = Overflow.Hidden;

            var host = new VisualElement { name = "building-preview-host" };
            host.style.position = Position.Absolute;
            host.style.left = 4;
            host.style.top = 4;
            host.style.right = 4;
            host.style.bottom = 4;
            host.style.backgroundColor = new Color(0.08f, 0.09f, 0.1f, 1f);
            frame.Add(host);

            var label = new Label(fallbackLabel);
            label.name = "building-preview-fallback";
            label.style.color = BetaVisualTheme.AgedGoldBright;
            label.style.fontSize = 10;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.paddingLeft = 4;
            label.style.paddingRight = 4;
            label.pickingMode = PickingMode.Ignore;
            frame.Add(label);

            try
            {
                BuildingPreviewRenderer.Shared.Show(buildingId, level, host);
                label.style.display = DisplayStyle.None;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Valgor.UI] Preview fallback: {ex.Message}");
                label.style.display = DisplayStyle.Flex;
            }

            return frame;
        }

        public static VisualElement CreateBenefitBox()
        {
            var box = new VisualElement();
            box.style.backgroundColor = ScrollInner;
            box.style.paddingLeft = 12;
            box.style.paddingRight = 12;
            box.style.paddingTop = 10;
            box.style.paddingBottom = 10;
            box.style.marginBottom = 12;
            box.style.borderTopWidth = 1;
            box.style.borderBottomWidth = 1;
            box.style.borderLeftWidth = 1;
            box.style.borderRightWidth = 1;
            box.style.borderTopColor = DeepBlue;
            box.style.borderBottomColor = DeepBlue;
            box.style.borderLeftColor = DeepBlue;
            box.style.borderRightColor = DeepBlue;
            box.style.borderLeftWidth = 3;
            box.style.borderLeftColor = FrameInner;
            return box;
        }

        public static VisualElement CreateStatusRow(bool ok)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = 5;
            row.style.paddingLeft = 10;
            row.style.paddingRight = 10;
            row.style.paddingTop = 7;
            row.style.paddingBottom = 7;
            row.style.backgroundColor = ok ? RowOk : RowBlocked;
            row.style.borderTopWidth = 1;
            row.style.borderBottomWidth = 1;
            row.style.borderLeftWidth = 1;
            row.style.borderRightWidth = 1;
            row.style.borderTopColor = ok
                ? new Color(0.25f, 0.45f, 0.28f)
                : new Color(0.5f, 0.22f, 0.2f);
            row.style.borderBottomColor = row.style.borderTopColor.value;
            row.style.borderLeftColor = row.style.borderTopColor.value;
            row.style.borderRightColor = row.style.borderTopColor.value;
            return row;
        }

        public static Label SectionTitle(string text)
        {
            var label = new Label(text);
            label.style.color = BetaVisualTheme.AgedGold;
            label.style.fontSize = 13;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginTop = 10;
            label.style.marginBottom = 4;
            label.style.letterSpacing = 0.5f;
            return label;
        }

        public static void StopPreview() => BuildingPreviewRenderer.Shared.ClearHost();

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
