using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.UI
{
    /// <summary>Tema visual oficial Valgor (Capital Final) — grafite, ouro envelhecido, bronze.</summary>
    public static class ValgorUiTheme
    {
        public static readonly Color Graphite = new(0.07f, 0.08f, 0.1f, 0.94f);
        public static readonly Color GraphiteDeep = new(0.05f, 0.06f, 0.08f, 0.96f);
        public static readonly Color StoneDark = new(0.16f, 0.15f, 0.14f, 1f);
        public static readonly Color Bronze = new(0.55f, 0.38f, 0.22f, 1f);
        public static readonly Color AgedGold = BetaVisualTheme.AgedGold;
        public static readonly Color AgedGoldBright = BetaVisualTheme.AgedGoldBright;
        public static readonly Color TextPrimary = BetaVisualTheme.TextPrimary;
        public static readonly Color TextMuted = BetaVisualTheme.TextMuted;
        public static readonly Color TextDanger = BetaVisualTheme.Danger;
        public static readonly Color TextSuccess = BetaVisualTheme.Success;
        public static readonly Color ButtonFace = new(0.12f, 0.14f, 0.18f, 0.95f);
        public static readonly Color ButtonDisabled = new(0.12f, 0.12f, 0.13f, 0.7f);
        public static readonly Color ChipFace = new(0.1f, 0.11f, 0.13f, 0.92f);
        public static readonly Color AccentBorder = AgedGold;

        public static void ApplyPanelFrame(VisualElement el)
        {
            el.style.backgroundColor = Graphite;
            el.style.borderTopWidth = 2;
            el.style.borderBottomWidth = 2;
            el.style.borderLeftWidth = 2;
            el.style.borderRightWidth = 2;
            el.style.borderTopColor = AccentBorder;
            el.style.borderBottomColor = AccentBorder;
            el.style.borderLeftColor = AccentBorder;
            el.style.borderRightColor = AccentBorder;
            el.style.borderTopLeftRadius = 4;
            el.style.borderTopRightRadius = 4;
            el.style.borderBottomLeftRadius = 4;
            el.style.borderBottomRightRadius = 4;
        }

        public static void ApplyButton(VisualElement el, bool enabled = true)
        {
            el.style.backgroundColor = enabled ? ButtonFace : ButtonDisabled;
            el.style.borderTopWidth = 1;
            el.style.borderBottomWidth = 1;
            el.style.borderLeftWidth = 1;
            el.style.borderRightWidth = 1;
            var border = enabled ? Bronze : StoneDark;
            el.style.borderTopColor = border;
            el.style.borderBottomColor = border;
            el.style.borderLeftColor = border;
            el.style.borderRightColor = border;
            el.style.paddingLeft = 8;
            el.style.paddingRight = 8;
            el.style.paddingTop = 4;
            el.style.paddingBottom = 4;
            el.SetEnabled(enabled);
        }

        public static void ApplyChip(VisualElement el)
        {
            el.style.backgroundColor = ChipFace;
            el.style.borderTopWidth = 1;
            el.style.borderBottomWidth = 1;
            el.style.borderLeftWidth = 1;
            el.style.borderRightWidth = 1;
            el.style.borderTopColor = Bronze;
            el.style.borderBottomColor = Bronze;
            el.style.borderLeftColor = Bronze;
            el.style.borderRightColor = Bronze;
            el.style.borderTopLeftRadius = 3;
            el.style.borderTopRightRadius = 3;
            el.style.borderBottomLeftRadius = 3;
            el.style.borderBottomRightRadius = 3;
            el.style.paddingLeft = 6;
            el.style.paddingRight = 8;
            el.style.paddingTop = 4;
            el.style.paddingBottom = 4;
            el.style.marginRight = 6;
            el.style.flexDirection = FlexDirection.Row;
            el.style.alignItems = Align.Center;
        }
    }

    /// <summary>Materiais UI compartilhados (procedurais) — evita espalhar cores.</summary>
    public static class ValgorUiMaterialCatalog
    {
        public static Color PanelFill => ValgorUiTheme.Graphite;
        public static Color PanelBorder => ValgorUiTheme.AgedGold;
        public static Color ChipFill => ValgorUiTheme.ChipFace;
        public static Color DisabledFill => ValgorUiTheme.ButtonDisabled;
    }
}
