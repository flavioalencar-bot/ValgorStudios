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
        private readonly float _sidePanelReserve;

        public BuildingContextMenuPositioner(
            float menuWidth = 200f,
            float menuEstimatedHeight = 200f,
            float margin = 16f,
            float gapFromBuilding = 52f,
            float bottomNavReserve = 72f,
            float topHudReserve = 56f,
            float sidePanelReserve = 360f)
        {
            _menuWidth = menuWidth;
            _menuEstimatedHeight = menuEstimatedHeight;
            _margin = margin;
            _gapFromBuilding = gapFromBuilding;
            _bottomNavReserve = bottomNavReserve;
            _topHudReserve = topHudReserve;
            _sidePanelReserve = sidePanelReserve;
        }

        public void Apply(
            VisualElement menu,
            VisualElement root,
            UnityEngine.Camera camera,
            Vector3 worldAnchor,
            float measuredHeight = -1f,
            bool reserveRightPanel = false)
        {
            if (menu == null || root == null || camera == null)
            {
                return;
            }

            var screen = camera.WorldToScreenPoint(worldAnchor);
            if (screen.z < 0f)
            {
                Place(menu, root.layout.width * 0.5f - _menuWidth * 0.5f, root.layout.height * 0.4f);
                return;
            }

            var panelH = root.resolvedStyle.height > 1f ? root.resolvedStyle.height : Screen.height;
            var panelW = root.resolvedStyle.width > 1f ? root.resolvedStyle.width : Screen.width;
            var uiX = screen.x * (panelW / Mathf.Max(1f, Screen.width));
            var uiY = (Screen.height - screen.y) * (panelH / Mathf.Max(1f, Screen.height));

            var height = measuredHeight > 0f ? measuredHeight : _menuEstimatedHeight;
            // Prefere lado com mais espaço; se painel modal à direita, força menu à esquerda do prédio.
            var spaceRight = panelW - uiX;
            var spaceLeft = uiX;
            var preferRight = spaceRight >= spaceLeft + 24f;
            if (reserveRightPanel)
            {
                preferRight = false;
            }

            var left = preferRight
                ? uiX + _gapFromBuilding
                : uiX - _menuWidth - _gapFromBuilding;

            // Ligeiramente acima do centro visual — não cobre o corpo do prédio.
            var top = uiY - height * 0.75f;

            var minLeft = _margin;
            var maxLeft = panelW - _menuWidth - _margin - (reserveRightPanel ? _sidePanelReserve : 0f);
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
