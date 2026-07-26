using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Heroes.Characters;
using Valgor.Heroes.Data;

namespace Valgor.Heroes.Preview360
{
    /// <summary>
    /// Isolated 360° preview rig: dedicated camera, light, RenderTexture and hero visual / fallback.
    /// </summary>
    public sealed class HeroPreviewController : MonoBehaviour
    {
        public const int RenderWidth = 480;
        public const int RenderHeight = 720;

        [SerializeField] private Camera previewCamera;
        [SerializeField] private Light previewLight;
        [SerializeField] private Transform focusPoint;
        [SerializeField] private Transform dummyAnchor;
        [SerializeField] private RenderTexture previewTexture;
        [SerializeField] private GameObject dummyPrefab;
        [SerializeField] private float autoRotateSpeed = 28f;
        [SerializeField] private bool autoRotate = true;
        [SerializeField] private float minDistance = 4.5f;
        [SerializeField] private float maxDistance = 8f;
        [SerializeField] private float defaultDistance = 5.8f;
        [SerializeField] private float dragSensitivity = 0.35f;
        [SerializeField] private float zoomSensitivity = 0.35f;
        [SerializeField] private float focusHeight = 1.05f;
        [SerializeField] private float cameraFov = 46f;
        [SerializeField] private float lookAtBiasY = 0.2f;
        [SerializeField] private float dummyScale = 0.88f;
        [SerializeField] private float dummyAnchorY = 0.12f;

        private GameObject _currentDummy;
        private HeroVisualController _currentVisual;
        private Material _dummyMaterial;
        private VisualElement _previewHost;
        private float _yaw;
        private float _pitch;
        private float _distance;
        private bool _dragging;
        private Vector2 _lastPointer;
        private string _currentHeroId;
        private bool _rigReady;
        private bool _usingFallback;

        public Camera PreviewCamera => previewCamera;
        public RenderTexture PreviewTexture => previewTexture;
        public Transform DummyAnchor => dummyAnchor;
        public GameObject CurrentDummy => _currentDummy;
        public HeroVisualController CurrentVisual => _currentVisual;
        public bool UsingTechnicalFallback => _usingFallback;

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

            if (_currentDummy == null || _currentHeroId != heroId)
            {
                ReplaceVisual(heroId, faction);
            }
            else if (_usingFallback)
            {
                ApplyFallbackTint(faction);
            }

            _currentHeroId = heroId;
            _currentVisual?.PlayIdle();
            FrameCamera();
            if (previewCamera != null)
            {
                previewCamera.Render();
            }
        }

        public void PlaySpecialPower()
        {
            _currentVisual?.PlaySpecialPower();
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

        private void ReplaceVisual(string heroId, HeroFaction faction)
        {
            if (_currentDummy != null)
            {
                Destroy(_currentDummy);
                _currentDummy = null;
                _currentVisual = null;
            }

            var resolved = HeroVisualResolver.Resolve(heroId, dummyPrefab);
            _usingFallback = resolved.IsTechnicalFallback;
            if (!string.IsNullOrEmpty(resolved.Message))
                Debug.Log($"[HeroPreview] {resolved.Message}");

            var prefab = resolved.Prefab;
            if (prefab == null)
            {
                EnsureMaterial();
                _currentDummy = HumanoidDummyFactory.Create(dummyAnchor, _dummyMaterial);
                _currentDummy.name = $"Fallback_{heroId}";
                _currentDummy.transform.localScale = Vector3.one * dummyScale;
                _usingFallback = true;
            }
            else
            {
                _currentDummy = Instantiate(prefab, dummyAnchor, false);
                _currentDummy.name = $"Visual_{heroId}";
                _currentDummy.transform.localPosition = Vector3.zero;
                _currentDummy.transform.localRotation = Quaternion.identity;
                _currentDummy.transform.localScale = Vector3.one * dummyScale;
                _currentVisual = _currentDummy.GetComponent<HeroVisualController>();
            }

            HumanoidDummyFactory.SetLayerRecursive(_currentDummy, HumanoidDummyFactory.ResolveLayer());

            if (_usingFallback)
                ApplyFallbackTint(faction);

            if (_currentDummy.transform.localScale.sqrMagnitude < 0.0001f)
                _currentDummy.transform.localScale = Vector3.one * dummyScale;
        }

        private void ApplyFallbackTint(HeroFaction faction)
        {
            EnsureMaterial();
            HumanoidDummyFactory.ApplyColor(_dummyMaterial, HeroPreviewFactionColors.ForFaction(faction));
            if (_currentDummy == null) return;
            foreach (var renderer in _currentDummy.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer == null) continue;
                // Don't retint named final materials if somehow present.
                if (renderer.sharedMaterial != null &&
                    renderer.sharedMaterial.name.StartsWith("MAT_Vortex_"))
                    continue;
                renderer.sharedMaterial = _dummyMaterial;
            }
        }

        private void EnsureMaterial()
        {
            if (_dummyMaterial != null) return;
            _dummyMaterial = HumanoidDummyFactory.CreateUrpCompatibleMaterial(HeroPreviewFactionColors.GuardaDaOrdem);
        }

        private void EnsureRig()
        {
            // Always refresh framing parameters even if the rig already exists (layout retunes).
            if (_rigReady && previewCamera != null && focusPoint != null && dummyAnchor != null && previewTexture != null)
            {
                ApplyFramingDefaults();
                FrameCamera();
                return;
            }

            var layer = HumanoidDummyFactory.ResolveLayer();
            transform.position = new Vector3(200f, 0f, 0f);

            if (focusPoint == null)
            {
                var focusGo = new GameObject("FocusPoint");
                focusGo.transform.SetParent(transform, false);
                focusGo.transform.localPosition = new Vector3(0f, focusHeight, 0f);
                focusPoint = focusGo.transform;
                HumanoidDummyFactory.SetLayerRecursive(focusGo, layer);
            }
            else
            {
                focusPoint.localPosition = new Vector3(0f, focusHeight, 0f);
            }

            if (dummyAnchor == null)
            {
                var anchorGo = new GameObject("DummyAnchor");
                anchorGo.transform.SetParent(transform, false);
                dummyAnchor = anchorGo.transform;
                HumanoidDummyFactory.SetLayerRecursive(anchorGo, layer);
            }

            dummyAnchor.localPosition = new Vector3(0f, dummyAnchorY, 0f);

            if (previewCamera == null)
            {
                var camGo = new GameObject("PreviewCamera");
                camGo.transform.SetParent(transform, false);
                previewCamera = camGo.AddComponent<Camera>();
                previewCamera.clearFlags = CameraClearFlags.SolidColor;
                previewCamera.backgroundColor = new Color(0.05f, 0.07f, 0.1f, 1f);
                previewCamera.nearClipPlane = 0.1f;
                previewCamera.farClipPlane = 50f;
                previewCamera.depth = 10;
                previewCamera.enabled = true;
            }

            previewCamera.fieldOfView = cameraFov;
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
            ApplyFramingDefaults();
            FrameCamera();
            _rigReady = true;
        }

        private void ApplyFramingDefaults()
        {
            if (focusPoint != null)
            {
                focusPoint.localPosition = new Vector3(0f, focusHeight, 0f);
            }

            if (dummyAnchor != null)
            {
                dummyAnchor.localPosition = new Vector3(0f, dummyAnchorY, 0f);
            }

            if (previewCamera != null)
            {
                previewCamera.fieldOfView = cameraFov;
                previewCamera.cullingMask = 1 << HumanoidDummyFactory.ResolveLayer();
            }

            if (_distance < minDistance || _distance > maxDistance || _distance < defaultDistance * 0.95f)
            {
                _distance = defaultDistance;
            }
        }

        private void FrameCamera()
        {
            if (previewCamera == null || focusPoint == null) return;

            ApplyFramingDefaults();

            var focus = focusPoint.position;
            var lookTarget = focus + Vector3.up * lookAtBiasY;
            // Keep camera nearly level so head and feet share the vertical frustum.
            var yawRad = 14f * Mathf.Deg2Rad;
            var x = Mathf.Sin(yawRad) * _distance;
            var z = -Mathf.Cos(yawRad) * _distance;
            var height = focusHeight + 0.15f + _pitch * 0.02f;
            previewCamera.transform.position = transform.TransformPoint(new Vector3(x, height, z));
            previewCamera.transform.LookAt(lookTarget);
            previewCamera.fieldOfView = cameraFov;
        }

        private void ApplyTextureToUi()
        {
            if (_previewHost == null || previewTexture == null) return;
            // Contain keeps the full body visible; Cover was cropping feet in the UI panel.
            _previewHost.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(previewTexture));
            _previewHost.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            _previewHost.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            _previewHost.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            _previewHost.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
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
            _pitch = Mathf.Clamp(_pitch - delta.y * dragSensitivity * 0.35f, -8f, 18f);
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
