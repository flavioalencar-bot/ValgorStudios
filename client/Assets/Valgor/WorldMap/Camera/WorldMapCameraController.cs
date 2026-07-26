using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Valgor.WorldMap.Camera
{
    public readonly struct MapPosition
    {
        public MapPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
    }

    public sealed class WorldMapBounds
    {
        public WorldMapBounds(float minX = -22f, float maxX = 22f, float minZ = -18f, float maxZ = 22f)
        {
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }

        public float MinX { get; }
        public float MaxX { get; }
        public float MinZ { get; }
        public float MaxZ { get; }

        public MapPosition ClampPosition(MapPosition position) =>
            new(
                Math.Clamp(position.X, MinX, MaxX),
                position.Y,
                Math.Clamp(position.Z, MinZ, MaxZ));
    }

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
    }
}
