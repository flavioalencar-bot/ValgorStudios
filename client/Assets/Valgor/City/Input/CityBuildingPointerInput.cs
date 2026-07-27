using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Valgor.City.Buildings;
using Valgor.City.Camera;
using Valgor.City.UI;

namespace Valgor.City.Input
{
    /// <summary>
    /// Seleção de edifícios via Input System + Physics raycast.
    /// Necessário com activeInputHandler = Input System only (OnMouse* não dispara).
    /// </summary>
    [DefaultExecutionOrder(50)]
    public sealed class CityBuildingPointerInput : MonoBehaviour
    {
        private static float _suppressUntilUnscaled;

        private UnityEngine.Camera _camera = null!;
        private int _buildingLayer = -1;
        private LayerMask _mask;

        /// <summary>Bloqueia raycast 3D após clique em menu/UI (evita fechar o painel no mesmo frame).</summary>
        public static void SuppressWorldClicks(float seconds = 0.3f) =>
            _suppressUntilUnscaled = Mathf.Max(_suppressUntilUnscaled, Time.unscaledTime + seconds);

        public static bool IsWorldClickSuppressed => Time.unscaledTime < _suppressUntilUnscaled;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>() ?? UnityEngine.Camera.main;
            _buildingLayer = LayerMask.NameToLayer("Building");
            _mask = _buildingLayer >= 0 ? (1 << _buildingLayer) : Physics.DefaultRaycastLayers;
        }

        private void LateUpdate()
        {
            if (_camera == null)
            {
                _camera = UnityEngine.Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            if (!TryGetPointerRelease(out var screenPos))
            {
                return;
            }

            if (IsWorldClickSuppressed ||
                CityCameraController.ShouldSuppressBuildingClick ||
                IsOverBlockingUi(screenPos))
            {
                return;
            }

            TrySelectAtScreen(screenPos);
        }

        private static bool TryGetPointerRelease(out Vector2 screenPos)
        {
            screenPos = default;
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
            {
                screenPos = mouse.position.ReadValue();
                return true;
            }

            var touch = Touchscreen.current?.primaryTouch;
            if (touch != null && touch.press.wasReleasedThisFrame)
            {
                screenPos = touch.position.ReadValue();
                return true;
            }

            return false;
        }

        private void TrySelectAtScreen(Vector2 screenPos)
        {
            var ray = _camera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, 500f, _mask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            var collectProxy = hit.collider.GetComponentInParent<BuildingCollectableClickProxy>();
            if (collectProxy != null)
            {
                collectProxy.NotifyClicked();
                return;
            }

            var selectProxy = hit.collider.GetComponentInParent<BuildingSelectionClickProxy>();
            if (selectProxy != null)
            {
                selectProxy.NotifyClicked();
                return;
            }

            var view = hit.collider.GetComponentInParent<BuildingView>();
            view?.NotifyClicked();
        }

        private static bool IsOverBlockingUi(Vector2 screenPos)
        {
            var documents = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            for (var i = 0; i < documents.Length; i++)
            {
                var doc = documents[i];
                if (doc == null)
                {
                    continue;
                }

                var root = doc.rootVisualElement;
                if (root?.panel == null || root.style.display == DisplayStyle.None)
                {
                    continue;
                }

                var panelPos = RuntimePanelUtils.ScreenToPanel(root.panel, screenPos);

                // Menu contextual / painéis modais: bloquear mesmo se Pick falhar no frame do clique.
                if (HitsNamedPanel(root, panelPos, global::Valgor.City.UI.BuildingContextMenu.RootName) ||
                    HitsNamedPanel(root, panelPos, global::Valgor.City.UI.BuildingDetailsPanel.RootName) ||
                    HitsNamedPanel(root, panelPos, "building-action-panel"))
                {
                    return true;
                }

                var picked = root.panel.Pick(panelPos);
                if (picked == null)
                {
                    continue;
                }

                for (var el = picked; el != null; el = el.parent)
                {
                    if (el is Button || el.pickingMode == PickingMode.Position)
                    {
                        if (el.pickingMode == PickingMode.Ignore && el is not Button)
                        {
                            continue;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HitsNamedPanel(VisualElement root, Vector2 panelPos, string name)
        {
            var el = root.Q(name);
            if (el == null || el.style.display == DisplayStyle.None)
            {
                return false;
            }

            return el.worldBound.Contains(panelPos);
        }
    }
}
