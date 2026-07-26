using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Heroes.Data;

namespace Valgor.Heroes.Preview360
{
    /// <summary>
    /// Isolated 360° preview rig: dedicated camera, light, RenderTexture and humanoid dummy.
    /// </summary>
    public sealed class HeroPreviewController : MonoBehaviour
    {
        public const int RenderWidth = 512;
        public const int RenderHeight = 640;

        [SerializeField] private Camera previewCamera;
        [SerializeField] private Light previewLight;
        [SerializeField] private Transform focusPoint;
        [SerializeField] private Transform dummyAnchor;
        [SerializeField] private RenderTexture previewTexture;
        [SerializeField] private GameObject dummyPrefab;
        [SerializeField] private float autoRotateSpeed = 28f;
        [SerializeField] private bool autoRotate = true;
        [SerializeField] private float minDistance = 2.2f;
        [SerializeField] private float maxDistance = 5.5f;
        [SerializeField] private float defaultDistance = 3.4f;
        [SerializeField] private float dragSensitivity = 0.35f;
        [SerializeField] private float zoomSensitivity = 0.35f;

        private GameObject _currentDummy;
        private Material _dummyMaterial;
        private VisualElement _previewHost;
        private float _yaw;
        private float _pitch = 12f;
        private float _distance;
        private bool _dragging;
        private Vector2 _lastPointer;
        private string _currentHeroId;
        private bool _rigReady;

        public Camera PreviewCamera => previewCamera;
        public RenderTexture PreviewTexture => previewTexture;
        public Transform DummyAnchor => dummyAnchor;
        public GameObject CurrentDummy => _currentDummy;

        public void BindUi(VisualElement previewHost)
        {
            if (_previewHost != null)
            {
                UnregisterUiCallbacks(_previewHost);
            }

            _previewHost = previewHost;
            EnsureRig();
            ApplyTextureToUi();
            RegisterUiCallbacks(_previewHost);
        }

        public void SetDummyPrefab(GameObject prefab) => dummyPrefab = prefab;

        public void ShowHero(string heroId, HeroFaction faction)
        {
            EnsureRig();
            EnsureMaterial();
            HumanoidDummyFactory.ApplyColor(_dummyMaterial, HeroPreviewFactionColors.ForFaction(faction));

            if (_currentDummy == null || _currentHeroId != heroId)
            {
                ReplaceDummy(heroId);
            }
            else
            {
                HumanoidDummyFactory.ApplyMaterial(_currentDummy, _dummyMaterial);
            }

            _currentHeroId = heroId;
            FrameCamera();
            if (previewCamera != null)
            {
                previewCamera.Render();
            }
        }

        private void Awake()
        {
            _distance = defaultDistance;
            EnsureRig();
        }

        private void OnDestroy()
        {
            if (_previewHost != null)
            {
                UnregisterUiCallbacks(_previewHost);
            }

            if (_dummyMaterial != null)
            {
                Destroy(_dummyMaterial);
            }

            if (previewTexture != null)
            {
                previewTexture.Release();
            }
        }

        private void LateUpdate()
        {
            if (!_rigReady || dummyAnchor == null) return;

            if (autoRotate && !_dragging)
            {
                _yaw += autoRotateSpeed * Time.deltaTime;
            }

            dummyAnchor.localRotation = Quaternion.Euler(0f, _yaw, 0f);
            FrameCamera();
        }

        private void EnsureRig()
        {
            if (_rigReady && previewCamera != null && focusPoint != null && dummyAnchor != null && previewTexture != null)
            {
                return;
            }

            var layer = HumanoidDummyFactory.ResolveLayer();
            transform.position = new Vector3(200f, 0f, 0f);

            if (focusPoint == null)
            {
                var focusGo = new GameObject("FocusPoint");
                focusGo.transform.SetParent(transform, false);
                focusGo.transform.localPosition = new Vector3(0f, 1.15f, 0f);
                focusPoint = focusGo.transform;
                HumanoidDummyFactory.SetLayerRecursive(focusGo, layer);
            }

            if (dummyAnchor == null)
            {
                var anchorGo = new GameObject("DummyAnchor");
                anchorGo.transform.SetParent(transform, false);
                anchorGo.transform.localPosition = Vector3.zero;
                dummyAnchor = anchorGo.transform;
                HumanoidDummyFactory.SetLayerRecursive(anchorGo, layer);
            }

            if (previewCamera == null)
            {
                var camGo = new GameObject("PreviewCamera");
                camGo.transform.SetParent(transform, false);
                previewCamera = camGo.AddComponent<Camera>();
                previewCamera.clearFlags = CameraClearFlags.SolidColor;
                previewCamera.backgroundColor = new Color(0.05f, 0.07f, 0.1f, 1f);
                previewCamera.fieldOfView = 35f;
                previewCamera.nearClipPlane = 0.1f;
                previewCamera.farClipPlane = 50f;
                previewCamera.depth = 10;
                previewCamera.enabled = true;
            }

            previewCamera.cullingMask = 1 << layer;
            HumanoidDummyFactory.SetLayerRecursive(previewCamera.gameObject, layer);

            if (previewLight == null)
            {
                var lightGo = new GameObject("PreviewLight");
                lightGo.transform.SetParent(transform, false);
                lightGo.transform.localPosition = new Vector3(-1.2f, 2.8f, -2.2f);
                lightGo.transform.localRotation = Quaternion.Euler(35f, 40f, 0f);
                previewLight = lightGo.AddComponent<Light>();
                previewLight.type = LightType.Directional;
                previewLight.intensity = 1.35f;
                previewLight.color = Color.white;
                previewLight.cullingMask = 1 << layer;
                HumanoidDummyFactory.SetLayerRecursive(lightGo, layer);
            }

            if (previewTexture == null)
            {
                previewTexture = new RenderTexture(RenderWidth, RenderHeight, 24, RenderTextureFormat.ARGB32)
                {
                    name = "HeroPreviewRT",
                    antiAliasing = 2
                };
                previewTexture.Create();
            }

            previewCamera.targetTexture = previewTexture;
            _distance = Mathf.Clamp(_distance <= 0f ? defaultDistance : _distance, minDistance, maxDistance);
            FrameCamera();
            _rigReady = true;
        }

        private void EnsureMaterial()
        {
            if (_dummyMaterial != null) return;
            _dummyMaterial = HumanoidDummyFactory.CreateUrpCompatibleMaterial(HeroPreviewFactionColors.GuardaDaOrdem);
        }

        private void ReplaceDummy(string heroId)
        {
            if (_currentDummy != null)
            {
                Destroy(_currentDummy);
                _currentDummy = null;
            }

            EnsureMaterial();

            if (dummyPrefab != null)
            {
                _currentDummy = Instantiate(dummyPrefab, dummyAnchor, false);
                _currentDummy.name = $"Dummy_{heroId}";
                _currentDummy.transform.localPosition = Vector3.zero;
                _currentDummy.transform.localRotation = Quaternion.identity;
                _currentDummy.transform.localScale = Vector3.one;
                HumanoidDummyFactory.SetLayerRecursive(_currentDummy, HumanoidDummyFactory.ResolveLayer());
                HumanoidDummyFactory.ApplyMaterial(_currentDummy, _dummyMaterial);
            }
            else
            {
                _currentDummy = HumanoidDummyFactory.Create(dummyAnchor, _dummyMaterial);
                _currentDummy.name = $"Dummy_{heroId}";
            }

            if (_currentDummy.transform.localScale.sqrMagnitude < 0.0001f)
            {
                _currentDummy.transform.localScale = Vector3.one;
            }
        }

        private void FrameCamera()
        {
            if (previewCamera == null || focusPoint == null) return;
            var focus = focusPoint.position;
            var rotation = Quaternion.Euler(_pitch, 25f, 0f);
            var offset = rotation * new Vector3(0f, 0f, -_distance);
            previewCamera.transform.position = focus + offset;
            previewCamera.transform.LookAt(focus);
        }

        private void ApplyTextureToUi()
        {
            if (_previewHost == null || previewTexture == null) return;
            _previewHost.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(previewTexture));
            _previewHost.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
        }

        private void RegisterUiCallbacks(VisualElement host)
        {
            if (host == null) return;
            host.RegisterCallback<PointerDownEvent>(OnPointerDown);
            host.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            host.RegisterCallback<PointerUpEvent>(OnPointerUp);
            host.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            host.RegisterCallback<WheelEvent>(OnWheel);
        }

        private void UnregisterUiCallbacks(VisualElement host)
        {
            if (host == null) return;
            host.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            host.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            host.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            host.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            host.UnregisterCallback<WheelEvent>(OnWheel);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            _dragging = true;
            _lastPointer = evt.position;
            _previewHost?.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging) return;
            var delta = (Vector2)evt.position - _lastPointer;
            _yaw += delta.x * dragSensitivity;
            _pitch = Mathf.Clamp(_pitch - delta.y * dragSensitivity * 0.35f, 0f, 40f);
            _lastPointer = evt.position;
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_dragging) return;
            _dragging = false;
            if (_previewHost != null && _previewHost.HasPointerCapture(evt.pointerId))
            {
                _previewHost.ReleasePointer(evt.pointerId);
            }

            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt) => _dragging = false;

        private void OnWheel(WheelEvent evt)
        {
            _distance = Mathf.Clamp(_distance + evt.delta.y * zoomSensitivity * 0.02f, minDistance, maxDistance);
            evt.StopPropagation();
        }
    }
}
