using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.City.Visual;

namespace Valgor.City.UI
{
    /// <summary>
    /// Preview 3D isolado para modais (RenderTexture + câmera/luz próprias).
    /// Não altera a câmera da City nem a lógica de gameplay.
    /// </summary>
    public sealed class BuildingPreviewRenderer : MonoBehaviour
    {
        public const int Size = 256;
        public const int PreviewLayer = 30;

        private static BuildingPreviewRenderer? _shared;

        private Camera _camera = null!;
        private Light _light = null!;
        private Transform _anchor = null!;
        private RenderTexture _rt = null!;
        private Texture2D _uiTex = null!;
        private GameObject? _visualRoot;
        private VisualElement? _host;
        private string _buildingId = string.Empty;
        private int _level = -1;
        private float _yaw;
        private bool _active;

        public static BuildingPreviewRenderer Shared
        {
            get
            {
                if (_shared != null)
                {
                    return _shared;
                }

                var go = new GameObject("Valgor_BuildingPreviewRig");
                DontDestroyOnLoad(go);
                _shared = go.AddComponent<BuildingPreviewRenderer>();
                return _shared;
            }
        }

        public void Show(string buildingId, int level, VisualElement host)
        {
            EnsureRig();
            _host = host;
            _active = true;
            var id = buildingId ?? string.Empty;
            var lv = Math.Max(0, level);
            if (!string.Equals(id, _buildingId, StringComparison.Ordinal) || lv != _level)
            {
                RebuildVisual(id, lv);
                _buildingId = id;
                _level = lv;
                _yaw = 25f;
            }

            FrameCamera();
            RenderAndBlit();
        }

        public void ClearHost()
        {
            _host = null;
            _active = false;
        }

        private void LateUpdate()
        {
            if (!_active || _anchor == null || _visualRoot == null)
            {
                return;
            }

            _yaw += 18f * Time.unscaledDeltaTime;
            _anchor.localRotation = Quaternion.Euler(0f, _yaw, 0f);
            FrameCamera();
            RenderAndBlit();
        }

        private void OnDestroy()
        {
            if (_shared == this)
            {
                _shared = null;
            }

            if (_rt != null)
            {
                _rt.Release();
                Destroy(_rt);
            }

            if (_uiTex != null)
            {
                Destroy(_uiTex);
            }
        }

        private void EnsureRig()
        {
            if (_camera != null)
            {
                return;
            }

            transform.position = new Vector3(4800f, 0f, 4800f);

            if (!IsLayerDefined(PreviewLayer))
            {
                // Layer 30 pode não ter nome — cullingMask usa bit mesmo assim.
            }

            var anchorGo = new GameObject("PreviewAnchor");
            anchorGo.transform.SetParent(transform, false);
            _anchor = anchorGo.transform;

            var camGo = new GameObject("PreviewCamera");
            camGo.transform.SetParent(transform, false);
            _camera = camGo.AddComponent<Camera>();
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.09f, 0.1f, 0.12f, 0f);
            _camera.orthographic = false;
            _camera.fieldOfView = 32f;
            _camera.nearClipPlane = 0.05f;
            _camera.farClipPlane = 80f;
            _camera.cullingMask = 1 << PreviewLayer;
            _camera.enabled = false;
            _camera.allowHDR = false;
            _camera.allowMSAA = true;

            var lightGo = new GameObject("PreviewLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localRotation = Quaternion.Euler(42f, -35f, 0f);
            _light = lightGo.AddComponent<Light>();
            _light.type = LightType.Directional;
            _light.color = new Color(1f, 0.94f, 0.82f);
            _light.intensity = 1.15f;
            _light.cullingMask = 1 << PreviewLayer;
            _light.shadows = LightShadows.None;

            var fillGo = new GameObject("PreviewFill");
            fillGo.transform.SetParent(transform, false);
            fillGo.transform.localRotation = Quaternion.Euler(15f, 140f, 0f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.45f, 0.55f, 0.75f);
            fill.intensity = 0.35f;
            fill.cullingMask = 1 << PreviewLayer;
            fill.shadows = LightShadows.None;

            _rt = new RenderTexture(Size, Size, 16, RenderTextureFormat.ARGB32)
            {
                name = "BuildingPreviewRT",
                antiAliasing = 2,
                filterMode = FilterMode.Bilinear
            };
            _rt.Create();
            _camera.targetTexture = _rt;

            _uiTex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = "BuildingPreviewUI",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        private void RebuildVisual(string buildingId, int level)
        {
            if (_visualRoot != null)
            {
                Destroy(_visualRoot);
                _visualRoot = null;
            }

            _visualRoot = new GameObject($"Preview_{buildingId}");
            _visualRoot.transform.SetParent(_anchor, false);
            _visualRoot.transform.localPosition = Vector3.zero;
            _visualRoot.transform.localRotation = Quaternion.identity;
            _visualRoot.transform.localScale = Vector3.one;

            var tint = ResolveTint(buildingId);
            CityBuildingMeshFactory.Build(buildingId, _visualRoot.transform, tint, Math.Max(1, level));
            SanitizePreviewHierarchy(_visualRoot);
            SetLayerRecursive(_visualRoot, PreviewLayer);
        }

        private void FrameCamera()
        {
            if (_camera == null || _visualRoot == null)
            {
                return;
            }

            var bounds = EncapsulateRenderers(_visualRoot.transform);
            if (bounds.size.sqrMagnitude < 0.01f)
            {
                bounds = new Bounds(Vector3.up * 1.5f, Vector3.one * 3f);
            }

            var center = bounds.center;
            var radius = bounds.extents.magnitude;
            var framing = FramingFor(_buildingId);
            var distance = Mathf.Clamp(radius * framing.DistanceMul, framing.MinDistance, framing.MaxDistance);
            var height = center.y + framing.HeightBias;
            var offset = Quaternion.Euler(framing.Pitch, 0f, 0f) * new Vector3(0f, 0f, -distance);
            _camera.transform.position = new Vector3(center.x, height, center.z) + offset;
            _camera.transform.LookAt(center + Vector3.up * framing.LookAtBiasY);
        }

        private void RenderAndBlit()
        {
            if (_camera == null || _rt == null || _uiTex == null)
            {
                return;
            }

            _camera.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = _rt;
            _uiTex.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
            _uiTex.Apply(false);
            RenderTexture.active = prev;

            if (_host != null)
            {
                _host.style.backgroundImage = new StyleBackground(Background.FromTexture2D(_uiTex));
                _host.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                _host.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                _host.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                _host.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            }
        }

        private static void SanitizePreviewHierarchy(GameObject root)
        {
            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                Destroy(col);
            }

            foreach (var body in root.GetComponentsInChildren<Rigidbody>(true))
            {
                body.isKinematic = true;
                body.detectCollisions = false;
            }

            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                {
                    continue;
                }

                // Remove scripts de gameplay; mantém apenas o que for necessário ao render.
                var typeName = behaviour.GetType().Name;
                if (typeName.Contains("Click", StringComparison.Ordinal) ||
                    typeName.Contains("View", StringComparison.Ordinal) ||
                    typeName.Contains("Collect", StringComparison.Ordinal) ||
                    typeName.Contains("Input", StringComparison.Ordinal))
                {
                    Destroy(behaviour);
                }
            }

            // Remove badges/UI 3D se clonados.
            var kill = new List<Transform>();
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                var n = t.name;
                if (n is "ResourceIcon" or "LockedBadge" or "CollectableMarker" or "Name" or "LabelPlate" or "Rim")
                {
                    kill.Add(t);
                }
            }

            foreach (var t in kill)
            {
                if (t != null)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        private static Bounds EncapsulateRenderers(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.position, Vector3.one);
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static Color ResolveTint(string buildingId) => buildingId switch
        {
            "castle" => new Color(0.55f, 0.5f, 0.45f),
            "farm" => new Color(0.45f, 0.62f, 0.32f),
            "warehouse" => new Color(0.55f, 0.48f, 0.35f),
            "hospital" => new Color(0.75f, 0.78f, 0.82f),
            "wall" => new Color(0.5f, 0.5f, 0.52f),
            _ => new Color(0.55f, 0.52f, 0.48f)
        };

        private readonly struct Framing
        {
            public float DistanceMul;
            public float MinDistance;
            public float MaxDistance;
            public float Pitch;
            public float HeightBias;
            public float LookAtBiasY;
        }

        private static Framing FramingFor(string buildingId) => buildingId switch
        {
            "castle" => new Framing
            {
                DistanceMul = 2.05f, MinDistance = 8f, MaxDistance = 22f, Pitch = 22f, HeightBias = 0.4f, LookAtBiasY = 0.2f
            },
            "farm" => new Framing
            {
                DistanceMul = 2.2f, MinDistance = 5f, MaxDistance = 12f, Pitch = 28f, HeightBias = 0.2f, LookAtBiasY = 0.1f
            },
            "warehouse" => new Framing
            {
                DistanceMul = 2.1f, MinDistance = 5.5f, MaxDistance = 14f, Pitch = 24f, HeightBias = 0.25f, LookAtBiasY = 0.15f
            },
            "hospital" => new Framing
            {
                DistanceMul = 2.15f, MinDistance = 5.5f, MaxDistance = 14f, Pitch = 26f, HeightBias = 0.3f, LookAtBiasY = 0.15f
            },
            "wall" => new Framing
            {
                DistanceMul = 2.3f, MinDistance = 6f, MaxDistance = 16f, Pitch = 20f, HeightBias = 0.15f, LookAtBiasY = 0.05f
            },
            _ => new Framing
            {
                DistanceMul = 2.15f, MinDistance = 5f, MaxDistance = 16f, Pitch = 25f, HeightBias = 0.25f, LookAtBiasY = 0.1f
            }
        };

        private static bool IsLayerDefined(int layer) =>
            !string.IsNullOrEmpty(LayerMask.LayerToName(layer));
    }
}
