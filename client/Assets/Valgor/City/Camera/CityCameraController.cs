using UnityEngine;
using UnityEngine.InputSystem;

namespace Valgor.City.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class CityCameraController : MonoBehaviour
    {
        [SerializeField] private float minZoom = 7f;
        [SerializeField] private float maxZoom = 20f;
        [SerializeField] private float zoomSpeed = 0.015f;
        [SerializeField] private float panSpeed = 0.025f;

        private readonly CityBounds _bounds = new();
        private UnityEngine.Camera _camera = null!;
        private Vector2 _lastPointer;
        private float _lastPinchDistance;
        private bool _isPanning;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.orthographic = true;
            transform.rotation = Quaternion.Euler(30f, 45f, 0f);
            ClampPosition();
        }

        private void Update()
        {
            HandleMouse();
            HandleTouch();
        }

        private void HandleMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            // Botão direito/meio: pan. Esquerdo fica livre para seleção de edifícios.
            var panPressed = mouse.rightButton.isPressed || mouse.middleButton.isPressed;
            var panBegan = mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame;
            var panEnded = mouse.rightButton.wasReleasedThisFrame || mouse.middleButton.wasReleasedThisFrame;
            var position = mouse.position.ReadValue();

            if (panBegan)
            {
                _lastPointer = position;
                _isPanning = true;
            }

            if (panPressed && _isPanning)
            {
                Pan(position - _lastPointer);
                _lastPointer = position;
            }

            if (panEnded)
            {
                _isPanning = false;
            }

            Zoom(mouse.scroll.ReadValue().y * zoomSpeed);
        }

        private void HandleTouch()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return;
            }

            var touches = touchscreen.touches;
            var activeCount = 0;
            Vector2 firstPos = default;
            Vector2 secondPos = default;

            for (var i = 0; i < touches.Count; i++)
            {
                if (!touches[i].press.isPressed)
                {
                    continue;
                }

                if (activeCount == 0)
                {
                    firstPos = touches[i].position.ReadValue();
                }
                else if (activeCount == 1)
                {
                    secondPos = touches[i].position.ReadValue();
                }

                activeCount++;
            }

            if (activeCount >= 2)
            {
                var distance = Vector2.Distance(firstPos, secondPos);
                if (_lastPinchDistance > 0f)
                {
                    Zoom((distance - _lastPinchDistance) * zoomSpeed);
                }

                _lastPinchDistance = distance;
                _isPanning = false;
                return;
            }

            _lastPinchDistance = 0f;

            var first = touchscreen.primaryTouch;
            if (first.press.wasPressedThisFrame)
            {
                _lastPointer = first.position.ReadValue();
                _isPanning = true;
            }
            else if (first.press.isPressed && _isPanning)
            {
                var position = first.position.ReadValue();
                var delta = position - _lastPointer;
                if (delta.sqrMagnitude > 16f)
                {
                    Pan(delta);
                    _lastPointer = position;
                }
            }
            else if (first.press.wasReleasedThisFrame)
            {
                _isPanning = false;
            }
        }

        private void Pan(Vector2 delta)
        {
            transform.position += (-transform.right * delta.x + -transform.forward * delta.y) * panSpeed;
            ClampPosition();
        }

        private void Zoom(float amount)
        {
            if (Mathf.Approximately(amount, 0f))
            {
                return;
            }

            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - amount, minZoom, maxZoom);
        }

        private void ClampPosition()
        {
            var clamped = _bounds.ClampPosition(new CityPosition(transform.position.x, transform.position.y, transform.position.z));
            transform.position = new Vector3(clamped.X, clamped.Y, clamped.Z);
        }
    }
}
