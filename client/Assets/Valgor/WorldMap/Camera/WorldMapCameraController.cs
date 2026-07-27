using UnityEngine;
using UnityEngine.InputSystem;

namespace Valgor.WorldMap.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class WorldMapCameraController : MonoBehaviour
    {
        [SerializeField] private float minZoom = 12f;
        [SerializeField] private float maxZoom = 28f;
        [SerializeField] private float zoomSpeed = 0.02f;
        [SerializeField] private float panSpeed = 0.028f;
        [SerializeField] private float initialZoom = 18f;

        private readonly WorldMapBounds _bounds = new(-20f, 20f, -18f, 20f);
        private UnityEngine.Camera _camera = null!;
        private Vector2 _lastPointer;
        private bool _panning;
        private WorldCameraPersistenceService? _persistence;
        private bool _poseApplied;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.orthographic = true;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.2f, 0.3f, 0.36f);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            if (!_poseApplied)
            {
                transform.position = new Vector3(0f, 28f, 0f);
                _camera.orthographicSize = initialZoom;
                Clamp();
            }
        }

        private void OnDisable() => PersistCurrentPose();

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            var pos = mouse.position.ReadValue();
            var pan = mouse.rightButton.isPressed || mouse.middleButton.isPressed;
            if (mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
            {
                _lastPointer = pos;
                _panning = true;
            }

            if (pan && _panning)
            {
                var delta = pos - _lastPointer;
                transform.position += new Vector3(-delta.x, 0f, -delta.y) * panSpeed;
                _lastPointer = pos;
                Clamp();
            }

            if (mouse.rightButton.wasReleasedThisFrame || mouse.middleButton.wasReleasedThisFrame)
            {
                _panning = false;
                PersistCurrentPose();
            }

            var scroll = mouse.scroll.ReadValue().y * zoomSpeed;
            if (!Mathf.Approximately(scroll, 0f))
            {
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - scroll, minZoom, maxZoom);
                PersistCurrentPose();
            }
        }

        public void BindPersistence(WorldCameraPersistenceService persistence)
        {
            _persistence = persistence ?? throw new System.ArgumentNullException(nameof(persistence));
            ApplyRestoredPose();
        }

        public void ApplyRestoredPose()
        {
            if (_persistence == null)
            {
                return;
            }

            if (_camera == null)
            {
                _camera = GetComponent<UnityEngine.Camera>();
            }

            var state = _persistence.ResolveForRestore();
            transform.position = new Vector3(state.X, state.Y, state.Z);
            _camera.orthographicSize = Mathf.Clamp(state.OrthographicSize, minZoom, maxZoom);
            Clamp();
            _poseApplied = true;
        }

        private void Clamp()
        {
            var clamped = _bounds.ClampPosition(new MapPosition(transform.position.x, transform.position.y, transform.position.z));
            transform.position = new Vector3(clamped.X, Mathf.Max(20f, clamped.Y), clamped.Z);
        }

        public void FocusOn(float x, float z, float? orthographicSize = null)
        {
            transform.position = new Vector3(x, transform.position.y, z);
            Clamp();
            if (orthographicSize.HasValue)
            {
                _camera.orthographicSize = Mathf.Clamp(orthographicSize.Value, minZoom, maxZoom);
            }

            PersistCurrentPose();
        }

        public void PersistCurrentPose()
        {
            if (_persistence == null || _camera == null)
            {
                return;
            }

            _persistence.SavePose(
                transform.position.x,
                transform.position.y,
                transform.position.z,
                _camera.orthographicSize);
        }

        public WorldMapBounds Bounds => _bounds;
    }
}
