using UnityEngine;
using UnityEngine.InputSystem;

namespace Valgor.City.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    [DefaultExecutionOrder(-40)]
    public sealed class CityCameraController : MonoBehaviour
    {
        [SerializeField] private float minZoom = 9f;
        [SerializeField] private float maxZoom = 16f;
        [SerializeField] private float zoomSpeed = 0.015f;
        [SerializeField] private float panSpeed = 0.025f;
        [SerializeField] private float initialZoom = 14.5f;
        [SerializeField] private float dragThresholdPixels = 10f;

        private readonly CityBounds _bounds = new(-18f, 18f, -18f, 18f);
        private UnityEngine.Camera _camera = null!;
        private Vector2 _lastPointer;
        private Vector2 _pointerDown;
        private float _lastPinchDistance;
        private bool _isPanning;
        private bool _dragExceededThreshold;
        private static float _suppressClickUntilUnscaled;

        /// <summary>True se o último gesto foi arrasto de câmera — não selecionar prédio.</summary>
        public static bool ShouldSuppressBuildingClick =>
            Time.unscaledTime < _suppressClickUntilUnscaled;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = initialZoom;
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

            var position = mouse.position.ReadValue();
            var leftPressed = mouse.leftButton.isPressed;
            var leftBegan = mouse.leftButton.wasPressedThisFrame;
            var leftEnded = mouse.leftButton.wasReleasedThisFrame;
            var altPanPressed = mouse.rightButton.isPressed || mouse.middleButton.isPressed;
            var altPanBegan = mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame;
            var altPanEnded = mouse.rightButton.wasReleasedThisFrame || mouse.middleButton.wasReleasedThisFrame;

            if (leftBegan || altPanBegan)
            {
                _lastPointer = position;
                _pointerDown = position;
                _isPanning = true;
                _dragExceededThreshold = false;
            }

            if ((leftPressed || altPanPressed) && _isPanning)
            {
                var delta = position - _lastPointer;
                var fromDown = position - _pointerDown;
                if (!_dragExceededThreshold && fromDown.sqrMagnitude >= dragThresholdPixels * dragThresholdPixels)
                {
                    _dragExceededThreshold = true;
                }

                if (_dragExceededThreshold || altPanPressed)
                {
                    Pan(delta);
                    _lastPointer = position;
                }
            }

            if (leftEnded || altPanEnded)
            {
                if (_dragExceededThreshold)
                {
                    _suppressClickUntilUnscaled = Time.unscaledTime + 0.2f;
                }

                _isPanning = false;
                _dragExceededThreshold = false;
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
                _suppressClickUntilUnscaled = Time.unscaledTime + 0.2f;
                return;
            }

            _lastPinchDistance = 0f;

            var first = touchscreen.primaryTouch;
            if (first.press.wasPressedThisFrame)
            {
                _lastPointer = first.position.ReadValue();
                _pointerDown = _lastPointer;
                _isPanning = true;
                _dragExceededThreshold = false;
            }
            else if (first.press.isPressed && _isPanning)
            {
                var position = first.position.ReadValue();
                var delta = position - _lastPointer;
                var fromDown = position - _pointerDown;
                if (!_dragExceededThreshold && fromDown.sqrMagnitude >= dragThresholdPixels * dragThresholdPixels)
                {
                    _dragExceededThreshold = true;
                }

                if (_dragExceededThreshold && delta.sqrMagnitude > 1f)
                {
                    Pan(delta);
                    _lastPointer = position;
                }
            }
            else if (first.press.wasReleasedThisFrame)
            {
                if (_dragExceededThreshold)
                {
                    _suppressClickUntilUnscaled = Time.unscaledTime + 0.2f;
                }

                _isPanning = false;
                _dragExceededThreshold = false;
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
