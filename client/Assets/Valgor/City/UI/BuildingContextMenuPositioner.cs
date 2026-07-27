using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.City.UI
{
    /// <summary>
    /// Posiciona o menu contextual perto do edifício, sem cobri-lo e dentro da área segura.
    /// </summary>
    public sealed class BuildingContextMenuPositioner
    {
        private readonly float _menuWidth;
        private readonly float _menuEstimatedHeight;
        private readonly float _margin;
        private readonly float _gapFromBuilding;
        private readonly float _bottomNavReserve;
        private readonly float _topHudReserve;

        public BuildingContextMenuPositioner(
            float menuWidth = 168f,
            float menuEstimatedHeight = 280f,
            float margin = 16f,
            float gapFromBuilding = 18f,
            float bottomNavReserve = 72f,
            float topHudReserve = 56f)
        {
            _menuWidth = menuWidth;
            _menuEstimatedHeight = menuEstimatedHeight;
            _margin = margin;
            _gapFromBuilding = gapFromBuilding;
            _bottomNavReserve = bottomNavReserve;
            _topHudReserve = topHudReserve;
        }

        public void Apply(
            VisualElement menu,
            VisualElement root,
            UnityEngine.Camera camera,
            Vector3 worldAnchor,
            float measuredHeight = -1f)
        {
            if (menu == null || root == null || camera == null)
            {
                return;
            }

            var screen = camera.WorldToScreenPoint(worldAnchor);
            if (screen.z < 0f)
            {
                // Atrás da câmera — centraliza com segurança.
                Place(menu, root.layout.width * 0.5f - _menuWidth * 0.5f, root.layout.height * 0.4f);
                return;
            }

            // UI Toolkit: Y cresce para baixo; ScreenPoint Y cresce para cima.
            var panelH = root.resolvedStyle.height > 1f ? root.resolvedStyle.height : Screen.height;
            var panelW = root.resolvedStyle.width > 1f ? root.resolvedStyle.width : Screen.width;
            var uiX = screen.x * (panelW / Mathf.Max(1f, Screen.width));
            var uiY = (Screen.height - screen.y) * (panelH / Mathf.Max(1f, Screen.height));

            var height = measuredHeight > 0f ? measuredHeight : _menuEstimatedHeight;
            var preferRight = uiX < panelW * 0.55f;
            var left = preferRight
                ? uiX + _gapFromBuilding
                : uiX - _menuWidth - _gapFromBuilding;

            var top = uiY - height * 0.35f;

            var minLeft = _margin;
            var maxLeft = panelW - _menuWidth - _margin;
            var minTop = _topHudReserve + _margin;
            var maxTop = panelH - height - _bottomNavReserve - _margin;

            left = Mathf.Clamp(left, minLeft, Mathf.Max(minLeft, maxLeft));
            top = Mathf.Clamp(top, minTop, Mathf.Max(minTop, maxTop));

            Place(menu, left, top);
        }

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
