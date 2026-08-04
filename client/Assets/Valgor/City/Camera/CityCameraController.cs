using UnityEngine;
using UnityEngine.InputSystem;
using Valgor.City.Visual;

namespace Valgor.City.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    [DefaultExecutionOrder(-40)]
    public sealed class CityCameraController : MonoBehaviour
    {
        // Referência estável (pré–World Foundation, commit 8cd4d78) com fill ~70%:
        // zoom um pouco mais fechado que 22 para a Capital ocupar a tela útil.
        private static float OverviewZoom =>
            CityPaintedMapConfig.Enabled
                ? CityPaintedMapConfig.OverviewZoom
                : CityCapitalLayoutRuntime.IsPreviewV2 ? 13.5f : 14f;
        private static float OverviewPitch =>
            CityPaintedMapConfig.Enabled
                ? CityPaintedMapConfig.OverviewPitch
                : CityCapitalLayoutRuntime.IsPreviewV2 ? 48f : 46f;
        private static float OverviewYaw =>
            CityPaintedMapConfig.Enabled
                ? CityPaintedMapConfig.OverviewYaw
                : CityCapitalLayoutRuntime.IsPreviewV2 ? 36f : 40f;
        private static float OverviewHeight =>
            CityPaintedMapConfig.Enabled
                ? 26f
                : CityCapitalLayoutRuntime.IsPreviewV2 ? 18f : 18f;

        [SerializeField] private float minZoom = 8f;
        [SerializeField] private float maxZoom = 20f;
        [SerializeField] private float zoomSpeed = 0.015f;
        [SerializeField] private float mouseScrollZoomSpeed = 0.055f;
        [SerializeField] private float panSpeed = 0.025f;
        [SerializeField] private float initialZoom = 14f;
        [SerializeField] private float dragThresholdPixels = 10f;

        private readonly CityBounds _bounds = new(-28f, 28f, -28f, 28f);
        private UnityEngine.Camera _camera = null!;
        private Vector2 _lastPointer;
        private Vector2 _pointerDown;
        private float _lastPinchDistance;
        private bool _isPanning;
        private bool _dragExceededThreshold;
        private static float _suppressClickUntilUnscaled;
        private float _suppressFocusUntilUnscaled;
        private bool _poseLocked;
        private PoseSnapshot _lockedPose;
        private float _holdOverviewUntilUnscaled;

        public static bool ShouldSuppressBuildingClick =>
            Time.unscaledTime < _suppressClickUntilUnscaled;

        public readonly struct PoseSnapshot
        {
            public PoseSnapshot(Vector3 position, Quaternion rotation, float orthographicSize)
            {
                Position = position;
                Rotation = rotation;
                OrthographicSize = orthographicSize;
            }

            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
            public float OrthographicSize { get; }
        }

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.orthographic = true;
            ResetToOverview();
        }

        private void Start()
        {
            if (!_poseLocked)
            {
                ResetToOverview();
            }
        }

        public void ResetToOverview()
        {
            if (_poseLocked)
            {
                return;
            }

            CancelFocus();
            ApplyOverviewPoseSilent();
            _suppressFocusUntilUnscaled = Mathf.Max(
                _suppressFocusUntilUnscaled,
                Time.unscaledTime + 2.5f);
            _holdOverviewUntilUnscaled = Mathf.Max(
                _holdOverviewUntilUnscaled,
                Time.unscaledTime + 2.5f);
        }

        private void Update()
        {
            if (_poseLocked)
            {
                RestorePose(_lockedPose);
                return;
            }

            if (Time.unscaledTime < _holdOverviewUntilUnscaled)
            {
                ApplyOverviewPoseSilent();
                return;
            }

            HandleMouse();
            HandleTouch();
        }

        private void ApplyOverviewPoseSilent()
        {
            EnsureCamera();
            if (maxZoom < OverviewZoom)
            {
                maxZoom = OverviewZoom;
            }

            initialZoom = OverviewZoom;
            _camera.orthographic = true;
            _camera.orthographicSize = OverviewZoom;
            var rot = Quaternion.Euler(OverviewPitch, OverviewYaw, 0f);
            transform.rotation = rot;
            var forward = rot * Vector3.forward;
            var t = OverviewHeight / Mathf.Max(0.05f, -forward.y);

            // Estável: origem. V2: leve offset para Castelo + Torre NE.
            var focus = Vector3.zero;
            if (CityCapitalLayoutRuntime.IsPreviewV2 &&
                CityCapitalLayoutConfig.TryGet("castle", out var castleEntry))
            {
                var p = castleEntry.Position;
                focus = new Vector3(p.X + 1.5f, 0f, p.Z * 0.2f - 0.5f);
            }

            transform.position = focus - forward * t;
            ClampPosition();
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

            Zoom(mouse.scroll.ReadValue().y * mouseScrollZoomSpeed);
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
            if (activeCount == 1)
            {
                var pos = firstPos;
                if (!_isPanning)
                {
                    _lastPointer = pos;
                    _pointerDown = pos;
                    _isPanning = true;
                    _dragExceededThreshold = false;
                }
                else
                {
                    var delta = pos - _lastPointer;
                    if (!_dragExceededThreshold &&
                        (pos - _pointerDown).sqrMagnitude >= dragThresholdPixels * dragThresholdPixels)
                    {
                        _dragExceededThreshold = true;
                    }

                    if (_dragExceededThreshold)
                    {
                        Pan(delta);
                        _lastPointer = pos;
                    }
                }
            }
            else
            {
                if (_isPanning && _dragExceededThreshold)
                {
                    _suppressClickUntilUnscaled = Time.unscaledTime + 0.2f;
                }

                _isPanning = false;
                _dragExceededThreshold = false;
            }
        }

        private void Pan(Vector2 screenDelta)
        {
            EnsureCamera();
            var right = transform.right;
            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            var world = (-right * screenDelta.x - forward * screenDelta.y) * panSpeed *
                        (_camera.orthographicSize / 20f);
            transform.position += world;
            ClampPosition();
        }

        private void Zoom(float amount)
        {
            if (Mathf.Abs(amount) < 0.0001f)
            {
                return;
            }

            EnsureCamera();
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - amount, minZoom, maxZoom);
        }

        public PoseSnapshot CapturePose()
        {
            EnsureCamera();
            return new PoseSnapshot(transform.position, transform.rotation, _camera.orthographicSize);
        }

        public void RestorePose(PoseSnapshot pose)
        {
            CancelFocus();
            transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            EnsureCamera();
            _camera.orthographicSize = Mathf.Clamp(pose.OrthographicSize, minZoom, maxZoom);
        }

        /// <summary>Congela a pose atual até <see cref="UnlockPose"/>.</summary>
        public void LockPose()
        {
            _lockedPose = CapturePose();
            _poseLocked = true;
        }

        public void LockPose(PoseSnapshot pose)
        {
            _poseLocked = true;
            _lockedPose = pose;
            RestorePose(pose);
        }

        public void UnlockPose() => _poseLocked = false;

        public void ReleaseOverviewHold()
        {
            _holdOverviewUntilUnscaled = 0f;
            _suppressFocusUntilUnscaled = 0f;
        }

        public void CancelFocus()
        {
            _focusing = false;
            _focusElapsed = 0f;
        }

        public void SuppressFocus(float seconds = 0.6f)
        {
            _suppressFocusUntilUnscaled = Mathf.Max(
                _suppressFocusUntilUnscaled,
                Time.unscaledTime + Mathf.Max(0.05f, seconds));
            CancelFocus();
        }

        public void FocusOn(Vector3 worldTarget, float duration = 0.35f, float? orthographicSize = null)
        {
            if (_poseLocked || Time.unscaledTime < _suppressFocusUntilUnscaled)
            {
                return;
            }

            EnsureCamera();
            if (orthographicSize.HasValue)
            {
                _camera.orthographicSize = Mathf.Clamp(orthographicSize.Value, minZoom, maxZoom);
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

        private void EnsureCamera()
        {
            if (_camera == null)
            {
                _camera = GetComponent<UnityEngine.Camera>();
            }
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
            if (_poseLocked)
            {
                return;
            }

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
            transform.position = new Vector3(clamped.X, Mathf.Clamp(clamped.Y, 10f, 28f), clamped.Z);
        }

        private Vector3 _focusFrom;
        private Vector3 _focusTo;
        private float _focusDuration = 0.35f;
        private float _focusElapsed;
        private bool _focusing;
    }
}
