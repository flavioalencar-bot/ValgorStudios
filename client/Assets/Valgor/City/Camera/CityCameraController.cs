using UnityEngine;
using UnityEngine.InputSystem;

namespace Valgor.City.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class CityCameraController : MonoBehaviour
    {
        [SerializeField] private float minZoom = 9f;
        [SerializeField] private float maxZoom = 16f;
        [SerializeField] private float zoomSpeed = 0.015f;
        [SerializeField] private float panSpeed = 0.025f;
        [SerializeField] private float initialZoom = 14.5f;

        private readonly CityBounds _bounds = new(-18f, 18f, -18f, 18f);
        private UnityEngine.Camera _camera = null!;
        private Vector2 _lastPointer;
        private float _lastPinchDistance;
        private bool _isPanning;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = initialZoom;
            // Enquadra castelo + Torre dos Dragões (NE) com margem para HUD.
            transform.rotation = Quaternion.Euler(42f, 45f, 0f);
            transform.position = new Vector3(-15.5f, 19.5f, -15.5f);
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

        /// <summary>
        /// Centraliza suavemente o ponto de olhar da câmera isométrica sobre o alvo no chão.
        /// </summary>
        public void FocusOn(Vector3 worldTarget, float duration = 0.35f)
        {
            if (_camera == null)
            {
                _camera = GetComponent<UnityEngine.Camera>();
            }

            var lookPoint = ProjectLookPointOnGround();
            var delta = worldTarget - lookPoint;
            delta.y = 0f;
            _focusFrom = transform.position;
            _focusTo = transform.position + delta;
            _focusDuration = Mathf.Max(0.05f, duration);
            _focusElapsed = 0f;
            _focusing = true;
        }

        private Vector3 ProjectLookPointOnGround()
        {
            var plane = new Plane(Vector3.up, Vector3.zero);
            var ray = new Ray(transform.position, transform.forward);
            if (plane.Raycast(ray, out var enter))
            {
                return ray.GetPoint(enter);
            }

            return new Vector3(transform.position.x, 0f, transform.position.z);
        }

        private void LateUpdate()
        {
            if (!_focusing)
            {
                return;
            }

            _focusElapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_focusElapsed / _focusDuration);
            // Ease-out suave.
            t = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(_focusFrom, _focusTo, t);
            ClampPosition();
            if (_focusElapsed >= _focusDuration)
            {
                _focusing = false;
            }
        }

        private void ClampPosition()
        {
            var clamped = _bounds.ClampPosition(new CityPosition(transform.position.x, transform.position.y, transform.position.z));
            transform.position = new Vector3(clamped.X, Mathf.Clamp(clamped.Y, 10f, 20f), clamped.Z);
        }

        private Vector3 _focusFrom;
        private Vector3 _focusTo;
        private float _focusDuration = 0.35f;
        private float _focusElapsed;
        private bool _focusing;
    }
}
