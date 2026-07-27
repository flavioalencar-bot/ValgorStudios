using System.Collections;
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
        [SerializeField] private GameObject vortexHeroPrefab;
        [SerializeField] private float autoRotateSpeed = 28f;
        [SerializeField] private bool autoRotate = true;
        [SerializeField] private float minDistance = 4.8f;
        [SerializeField] private float maxDistance = 8.5f;
        [SerializeField] private float defaultDistance = 6.4f;
        [SerializeField] private float dragSensitivity = 0.35f;
        [SerializeField] private float zoomSensitivity = 0.35f;
        [SerializeField] private float focusHeight = 1.02f;
        [SerializeField] private float cameraFov = 38f;
        [SerializeField] private float lookAtBiasY = 0.05f;
        [SerializeField] private float dummyScale = 1f;
        [SerializeField] private float dummyAnchorY = 0f;

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
        private Texture2D _uiPreviewTex;
        private Texture2D _vortexSafeTex;
        private bool _useVortexSafeUi;

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

            if (_uiPreviewTex != null)
            {
                Destroy(_uiPreviewTex);
            }

            if (_vortexSafeTex != null)
            {
                Destroy(_vortexSafeTex);
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
            if (_useVortexSafeUi)
            {
                ApplyTextureToUi();
            }
            else if (previewCamera != null && previewTexture != null)
            {
                BlitPreviewToUi();
            }
        }

        private void ReplaceVisual(string heroId, HeroFaction faction)
        {
            if (_currentDummy != null)
            {
                Destroy(_currentDummy);
                _currentDummy = null;
                _currentVisual = null;
            }

            var isVortex = string.Equals(heroId, "HERO_VORTEX_000", System.StringComparison.Ordinal);
            var resolved = HeroVisualResolver.Resolve(heroId, dummyPrefab, vortexHeroPrefab);
            _usingFallback = resolved.IsTechnicalFallback;
            if (!string.IsNullOrEmpty(resolved.Message))
                Debug.Log($"[HeroPreview] {resolved.Message}");

            var prefab = resolved.Prefab;
            // Player: FBX+URP Lit no preview RT → magenta via UI Toolkit.
            // Aceite: preto/dourado sem magenta — placeholder 2D estável + dummy sanitizado.
#if !UNITY_EDITOR
            if (isVortex)
            {
                prefab = null;
                _usingFallback = true;
                _useVortexSafeUi = true;
                Debug.Log("[HeroPreview] Vortex player: preview seguro preto/dourado.");
            }
            else
            {
                _useVortexSafeUi = false;
            }
#else
            _useVortexSafeUi = false;
#endif
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

            // Sempre sanitiza — preto-dourado no Vortex. Nunca magenta.
            HeroPreviewMaterialSanitizer.Sanitize(_currentDummy, preferBlackGold: isVortex);
            if (isVortex)
            {
                _usingFallback = false;
                StartCoroutine(ResanitizeVortexNextFrames());
            }

            HumanoidDummyFactory.SetLayerRecursive(_currentDummy, HumanoidDummyFactory.ResolveLayer());

            if (_usingFallback && !isVortex)
                ApplyFallbackTint(faction);

            if (_currentDummy.transform.localScale.sqrMagnitude < 0.0001f)
                _currentDummy.transform.localScale = Vector3.one * dummyScale;

            ApplyTextureToUi();
            if (previewCamera != null && !_useVortexSafeUi)
            {
                previewCamera.Render();
                BlitPreviewToUi();
            }
        }

        private IEnumerator ResanitizeVortexNextFrames()
        {
            for (var i = 0; i < 5; i++)
            {
                yield return null;
                if (_currentDummy == null)
                {
                    yield break;
                }

                HeroPreviewMaterialSanitizer.Sanitize(_currentDummy, preferBlackGold: true);
                if (previewCamera != null && !_useVortexSafeUi)
                {
                    previewCamera.Render();
                    BlitPreviewToUi();
                }
            }
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

            // Garante dados URP na câmera de preview (evita Lit/Unlit magenta).
            if (previewCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() == null)
            {
                previewCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            }

            previewCamera.fieldOfView = cameraFov;
            previewCamera.cullingMask = ~0;
            HumanoidDummyFactory.SetLayerRecursive(previewCamera.gameObject, layer);

            if (previewLight == null)
            {
                var lightGo = new GameObject("PreviewLight");
                lightGo.transform.SetParent(transform, false);
                lightGo.transform.localPosition = new Vector3(-1.2f, 2.8f, -2.2f);
                lightGo.transform.localRotation = Quaternion.Euler(35f, 40f, 0f);
                previewLight = lightGo.AddComponent<Light>();
                previewLight.type = LightType.Directional;
                previewLight.intensity = 1.55f;
                previewLight.color = new Color(1f, 0.94f, 0.82f);
                HumanoidDummyFactory.SetLayerRecursive(lightGo, layer);
            }

            previewLight.cullingMask = ~0;

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
                previewCamera.cullingMask = ~0;
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
            if (_previewHost == null)
            {
                return;
            }

            if (_useVortexSafeUi)
            {
                EnsureVortexSafeTexture();
                _previewHost.style.backgroundImage = new StyleBackground(Background.FromTexture2D(_vortexSafeTex));
            }
            else if (previewTexture != null)
            {
                // Prefer Texture2D blit — FromRenderTexture pode pintar magenta no player.
                BlitPreviewToUi();
            }

            _previewHost.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            _previewHost.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            _previewHost.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            _previewHost.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
        }

        private void BlitPreviewToUi()
        {
            if (_previewHost == null || previewTexture == null || !previewTexture.IsCreated())
            {
                return;
            }

            var w = previewTexture.width;
            var h = previewTexture.height;
            if (_uiPreviewTex == null || _uiPreviewTex.width != w || _uiPreviewTex.height != h)
            {
                if (_uiPreviewTex != null)
                {
                    Destroy(_uiPreviewTex);
                }

                _uiPreviewTex = new Texture2D(w, h, TextureFormat.RGBA32, false)
                {
                    name = "HeroPreviewUI",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
            }

            var prev = RenderTexture.active;
            RenderTexture.active = previewTexture;
            _uiPreviewTex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            _uiPreviewTex.Apply(false, false);
            RenderTexture.active = prev;
            _previewHost.style.backgroundImage = new StyleBackground(Background.FromTexture2D(_uiPreviewTex));
        }

        private void EnsureVortexSafeTexture()
        {
            if (_vortexSafeTex != null)
            {
                return;
            }

            const int w = 240;
            const int h = 360;
            _vortexSafeTex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                name = "VortexSafePreview",
                filterMode = FilterMode.Point
            };

            var bg = new Color(0.05f, 0.07f, 0.1f, 1f);
            var body = new Color(0.08f, 0.09f, 0.11f, 1f);
            var gold = new Color(0.82f, 0.64f, 0.22f, 1f);
            var pixels = new Color[w * h];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = bg;
            }

            void FillRect(int x0, int y0, int rw, int rh, Color c)
            {
                for (var y = y0; y < y0 + rh && y < h; y++)
                for (var x = x0; x < x0 + rw && x < w; x++)
                {
                    if (x >= 0 && y >= 0)
                    {
                        pixels[y * w + x] = c;
                    }
                }
            }

            // Silhueta humana provisória preto + acentos dourados (sem magenta).
            FillRect(w / 2 - 28, 70, 56, 90, body);   // torso
            FillRect(w / 2 - 16, 40, 32, 32, body);    // head
            FillRect(w / 2 - 50, 85, 20, 70, body);    // arm L
            FillRect(w / 2 + 30, 85, 20, 70, body);    // arm R
            FillRect(w / 2 - 24, 155, 20, 90, body);   // leg L
            FillRect(w / 2 + 4, 155, 20, 90, body);    // leg R
            FillRect(w / 2 + 42, 70, 10, 110, gold);   // sword
            FillRect(w / 2 + 36, 70, 22, 10, gold);    // guard
            FillRect(w / 2 - 20, 120, 40, 8, gold);    // belt

            _vortexSafeTex.SetPixels(pixels);
            _vortexSafeTex.Apply(false, false);
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
