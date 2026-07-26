using UnityEngine;
using UnityEngine.InputSystem;

namespace Valgor.WorldMap.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class WorldMapCameraController : MonoBehaviour
    {
        [SerializeField] private float minZoom = 8f;
        [SerializeField] private float maxZoom = 28f;
        [SerializeField] private float zoomSpeed = 0.02f;
        [SerializeField] private float panSpeed = 0.03f;

        private readonly WorldMapBounds _bounds = new();
        private UnityEngine.Camera _camera = null!;
        private Vector2 _lastPointer;
        private bool _panning;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.orthographic = true;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            transform.position = new Vector3(0f, 30f, 0f);
            Clamp();
        }

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
            }

            var scroll = mouse.scroll.ReadValue().y * zoomSpeed;
            if (!Mathf.Approximately(scroll, 0f))
            {
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - scroll, minZoom, maxZoom);
            }
        }

        private void Clamp()
        {
            var clamped = _bounds.ClampPosition(new MapPosition(transform.position.x, transform.position.y, transform.position.z));
            transform.position = new Vector3(clamped.X, clamped.Y, clamped.Z);
        }

        /// <summary>
        /// Centraliza a câmera no ponto e aplica zoom dentro dos limites configurados.
        /// </summary>
        public void FocusOn(float x, float z, float? orthographicSize = null)
        {
            transform.position = new Vector3(x, transform.position.y, z);
            Clamp();
            if (orthographicSize.HasValue)
            {
                _camera.orthographicSize = Mathf.Clamp(orthographicSize.Value, minZoom, maxZoom);
            }
        }

        public WorldMapBounds Bounds => _bounds;
    }
}
