using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.UI
{
    /// <summary>
    /// Ajustes de layout para resoluções baixas (ex.: 1080×640) sem mudar regras de jogo.
    /// </summary>
    public static class ValgorResponsiveUi
    {
        public const int ShortHeightPx = 720;
        public const int NarrowWidthPx = 1366;

        public static bool IsShortScreen => Screen.height > 0 && Screen.height <= ShortHeightPx;
        public static bool IsNarrowScreen => Screen.width > 0 && Screen.width <= NarrowWidthPx;

        /// <summary>Altura máxima do modal em % da tela (mais alto em telas curtas para caber footer).</summary>
        public static float ModalMaxHeightPercent => IsShortScreen ? 94f : 88f;

        public static float ModalWidthPercent => IsShortScreen ? 94f : 86f;

        public static int SafePad => IsShortScreen ? 8 : 14;

        public static void ApplyPanelScaleDefaults(PanelSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            // Equilibra width/height — evita overflow vertical em 1080×640 com ref 1920×1080.
            settings.match = 0.5f;
            if (settings.referenceResolution.x < 100 || settings.referenceResolution.y < 100)
            {
                settings.referenceResolution = new Vector2Int(1920, 1080);
            }
        }

        public static void ApplySafeFullRoot(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            root.style.flexGrow = 1;
            root.style.width = Length.Percent(100);
            root.style.height = Length.Percent(100);
            root.style.minHeight = 0;
            var pad = SafePad;
            root.style.paddingLeft = pad;
            root.style.paddingRight = pad;
            root.style.paddingTop = pad;
            root.style.paddingBottom = pad;
        }

        /// <summary>
        /// Scroll que preenche o espaço disponível (não cresce além da viewport).
        /// </summary>
        public static void ConstrainToViewport(ScrollView scroll)
        {
            if (scroll == null)
            {
                return;
            }

            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 0;
            scroll.style.height = Length.Percent(100);
            scroll.style.maxHeight = Length.Percent(100);
            scroll.style.width = Length.Percent(100);
        }

        public static void ApplyModalShell(VisualElement panel, float preferredMaxWidth)
        {
            if (panel == null)
            {
                return;
            }

            panel.style.position = Position.Absolute;
            panel.style.left = Length.Percent(50);
            panel.style.top = Length.Percent(50);
            panel.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
            panel.style.width = Length.Percent(ModalWidthPercent);
            panel.style.maxWidth = preferredMaxWidth;
            panel.style.maxHeight = Length.Percent(ModalMaxHeightPercent);
            panel.style.minWidth = IsNarrowScreen ? 280 : 320;
            panel.style.minHeight = 0;

            var pad = IsShortScreen ? 12 : 18;
            panel.style.paddingLeft = pad;
            panel.style.paddingRight = pad;
            panel.style.paddingTop = IsShortScreen ? 10 : 16;
            panel.style.paddingBottom = IsShortScreen ? 10 : 16;
            panel.style.flexDirection = FlexDirection.Column;
            panel.style.overflow = Overflow.Hidden;
        }

        public static void TightenScrollBody(ScrollView scroll, float minHeight = 80f)
        {
            if (scroll == null)
            {
                return;
            }

            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = IsShortScreen ? Mathf.Min(minHeight, 72f) : minHeight;
            scroll.style.maxHeight = StyleKeyword.Null;
        }

        public static float Compact(float normal, float shortValue) =>
            IsShortScreen ? shortValue : normal;
    }
}
