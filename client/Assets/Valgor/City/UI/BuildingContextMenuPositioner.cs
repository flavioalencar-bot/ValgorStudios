using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.City.UI
{
    /// <summary>
    /// Posiciona o menu fora da silhueta do edifício (preferência: abaixo → lados).
    /// </summary>
    public sealed class BuildingContextMenuPositioner
    {
        private readonly float _menuWidth;
        private readonly float _menuEstimatedHeight;
        private readonly float _margin;
        private readonly float _gap;
        private readonly float _bottomNavReserve;
        private readonly float _topHudReserve;
        private readonly float _sidePanelReserve;

        public BuildingContextMenuPositioner(
            float menuWidth = 260f,
            float menuEstimatedHeight = 110f,
            float margin = 14f,
            float gap = 16f,
            float bottomNavReserve = 78f,
            float topHudReserve = 72f,
            float sidePanelReserve = 360f)
        {
            _menuWidth = menuWidth;
            _menuEstimatedHeight = menuEstimatedHeight;
            _margin = margin;
            _gap = gap;
            _bottomNavReserve = bottomNavReserve;
            _topHudReserve = topHudReserve;
            _sidePanelReserve = sidePanelReserve;
        }

        public void Apply(
            VisualElement menu,
            VisualElement root,
            UnityEngine.Camera camera,
            Rect buildingScreenRect,
            float measuredHeight = -1f,
            bool reserveRightPanel = false,
            float measuredWidth = -1f)
        {
            if (menu == null || root == null || camera == null)
            {
                return;
            }

            var width = measuredWidth > 1f ? measuredWidth : _menuWidth;
            var height = measuredHeight > 1f ? measuredHeight : _menuEstimatedHeight;

            var panelH = root.resolvedStyle.height > 1f ? root.resolvedStyle.height : Screen.height;
            var panelW = root.resolvedStyle.width > 1f ? root.resolvedStyle.width : Screen.width;

            // Converte rect de pixels de tela → coordenadas do painel UI.
            var sx = panelW / Mathf.Max(1f, Screen.width);
            var sy = panelH / Mathf.Max(1f, Screen.height);
            var bLeft = buildingScreenRect.xMin * sx;
            var bRight = buildingScreenRect.xMax * sx;
            var bTop = (Screen.height - buildingScreenRect.yMax) * sy;
            var bBottom = (Screen.height - buildingScreenRect.yMin) * sy;
            var bCenterX = (bLeft + bRight) * 0.5f;
            var bCenterY = (bTop + bBottom) * 0.5f;

            var minLeft = _margin;
            var maxLeft = panelW - width - _margin - (reserveRightPanel ? _sidePanelReserve : 0f);
            var minTop = _topHudReserve + _margin;
            var maxTop = panelH - height - _bottomNavReserve - _margin;

            var gap = _gap;
            // 1) Abaixo do edifício (com folga extra para não tocar a base).
            var belowTop = bBottom + gap + 10f;
            var belowLeft = bCenterX - width * 0.5f;
            if (InSafe(belowLeft, belowTop, minLeft, maxLeft, minTop, maxTop) &&
                !OverlapsCenter(belowLeft, belowTop, width, height, bCenterX, bCenterY, bLeft, bRight, bTop, bBottom))
            {
                Place(menu, Clamp(belowLeft, minLeft, maxLeft), Clamp(belowTop, minTop, maxTop));
                return;
            }

            // 2) Direita.
            var rightLeft = bRight + _gap;
            var rightTop = bCenterY - height * 0.5f;
            if (!reserveRightPanel &&
                InSafe(rightLeft, rightTop, minLeft, maxLeft, minTop, maxTop) &&
                !OverlapsCenter(rightLeft, rightTop, width, height, bCenterX, bCenterY, bLeft, bRight, bTop, bBottom))
            {
                Place(menu, Clamp(rightLeft, minLeft, maxLeft), Clamp(rightTop, minTop, maxTop));
                return;
            }

            // 3) Esquerda.
            var leftLeft = bLeft - width - _gap;
            var leftTop = bCenterY - height * 0.5f;
            if (InSafe(leftLeft, leftTop, minLeft, maxLeft, minTop, maxTop) &&
                !OverlapsCenter(leftLeft, leftTop, width, height, bCenterX, bCenterY, bLeft, bRight, bTop, bBottom))
            {
                Place(menu, Clamp(leftLeft, minLeft, maxLeft), Clamp(leftTop, minTop, maxTop));
                return;
            }

            // 4) Acima (último recurso).
            var aboveTop = bTop - height - _gap;
            var aboveLeft = bCenterX - width * 0.5f;
            Place(
                menu,
                Clamp(aboveLeft, minLeft, maxLeft),
                Clamp(aboveTop, minTop, maxTop));
        }

        private static bool InSafe(
            float left,
            float top,
            float minLeft,
            float maxLeft,
            float minTop,
            float maxTop) =>
            left >= minLeft - 1f && left <= maxLeft + 1f &&
            top >= minTop - 1f && top <= maxTop + 1f;

        private static bool OverlapsCenter(
            float left,
            float top,
            float width,
            float height,
            float cx,
            float cy,
            float bLeft,
            float bRight,
            float bTop,
            float bBottom)
        {
            var menu = new Rect(left, top, width, height);
            // Núcleo visual: 55% central da silhueta.
            var coreW = (bRight - bLeft) * 0.55f;
            var coreH = (bBottom - bTop) * 0.55f;
            var core = new Rect(cx - coreW * 0.5f, cy - coreH * 0.5f, coreW, coreH);
            return menu.Overlaps(core);
        }

        private static float Clamp(float v, float min, float max) =>
            Mathf.Clamp(v, min, Mathf.Max(min, max));

        private static void Place(VisualElement menu, float left, float top)
        {
            menu.style.position = Position.Absolute;
            menu.style.left = left;
            menu.style.top = top;
            menu.style.right = StyleKeyword.Auto;
            menu.style.bottom = StyleKeyword.Auto;
        }
    }
}
