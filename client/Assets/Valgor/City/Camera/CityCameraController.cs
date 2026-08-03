using UnityEngine;
using UnityEngine.InputSystem;

namespace Valgor.City.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    [DefaultExecutionOrder(-40)]
    public sealed class CityCameraController : MonoBehaviour
    {
        // Pose base homologada (castle centrado); zoom aberto p/ Torre (N) + Portão (S).
        private const float OverviewZoom = 17.6f;
        private const float OverviewPosX = -12.8f;
        private const float OverviewPosY = 19.2f;
        private const float OverviewPosZ = -13.6f;

        [SerializeField] private float minZoom = 10f;
        [SerializeField] private float maxZoom = 24f;
        [SerializeField] private float zoomSpeed = 0.015f;
        [SerializeField] private float panSpeed = 0.025f;
        [SerializeField] private float initialZoom = OverviewZoom;
        [SerializeField] private float dragThresholdPixels = 10f;

        private readonly CityBounds _bounds = new(-22f, 22f, -22f, 22f);
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

        /// <summary>True se o último gesto foi arrasto de câmera — não selecionar prédio.</summary>
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
            // Garante overview após bootstrap (evita herdar pose da cena).
            if (!_poseLocked)
            {
                ResetToOverview();
            }
        }

        /// <summary>Enquadramento inicial da Capital (pull-back SW).</summary>
        public void ResetToOverview()
        {
            if (_poseLocked)
            {
                return;
            }

            CancelFocus();
            ApplyOverviewPoseSilent();
            // Evita FocusOn / pan fantasma (ativação de janela) logo após o boot.
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
            transform.rotation = Quaternion.Euler(46f, 40f, 0f);
            // Pull-back SW: Portão (S) + Castelo + Torre (N) + distritos na mesma vista.
            transform.position = new Vector3(OverviewPosX, OverviewPosY, OverviewPosZ);
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

        public PoseSnapshot CapturePose()
        {
            EnsureCamera();
            return new PoseSnapshot(transform.position, transform.rotation, _camera.orthographicSize);
        }

        public void RestorePose(PoseSnapshot pose)
        {
            EnsureCamera();
            CancelFocus();
            transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            _camera.orthographicSize = Mathf.Clamp(pose.OrthographicSize, minZoom, maxZoom);
        }

        /// <summary>Congela pose (posição/rotação/zoom) até <see cref="UnlockPose"/>.</summary>
        public void LockPose()
        {
            _lockedPose = CapturePose();
            _poseLocked = true;
            CancelFocus();
        }

        public void UnlockPose()
        {
            if (_poseLocked)
            {
                RestorePose(_lockedPose);
            }

            _poseLocked = false;
        }

        public void SuppressFocus(float seconds = 0.6f)
        {
            _suppressFocusUntilUnscaled = Mathf.Max(
                _suppressFocusUntilUnscaled,
                Time.unscaledTime + Mathf.Max(0.05f, seconds));
            CancelFocus();
        }

        public void CancelFocus()
        {
            _focusing = false;
            _focusElapsed = 0f;
        }

        public void FocusOn(Vector3 worldTarget, float duration = 0.35f, float? orthographicSize = null)
        {
            if (_poseLocked || Time.unscaledTime < _suppressFocusUntilUnscaled)
            {
                return;
            }

            EnsureCamera();

            // Zoom só muda se o caller pedir explicitamente — troca de tier nunca deve pedir.
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
                RestorePose(_lockedPose);
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
            // Altura acompanha pose inicial / zoom aberto (ex.: Y=22.5 no overview da Capital).
            transform.position = new Vector3(clamped.X, Mathf.Clamp(clamped.Y, 10f, 28f), clamped.Z);
        }

        private Vector3 _focusFrom;
        private Vector3 _focusTo;
        private float _focusDuration = 0.35f;
        private float _focusElapsed;
        private bool _focusing;
    }
}
