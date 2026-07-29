using System;
using UnityEngine;
using Valgor.City.Data;
using Valgor.City.Visual;

namespace Valgor.City.Buildings
{
    [RequireComponent(typeof(Collider))]
    public sealed class BuildingView : MonoBehaviour
    {
        private Renderer[] _renderers = Array.Empty<Renderer>();
        private Color[] _identityColors = Array.Empty<Color>();
        private Color[] _baseColors = Array.Empty<Color>();
        private Transform? _visual;
        private TextMesh _label = null!;
        private TextMesh? _bubbleLabel;
        private GameObject? _collectableMarker;
        private Renderer? _resourceIconRenderer;
        private GameObject? _upgradeArrow;
        private GameObject? _lockedBadge;
        private GameObject? _readyBadge;
        private GameObject? _constructionRoot;
        private Transform? _progressFill;
        private TextMesh? _progressLabel;
        private BuildingConstructionVisual? _constructionVisual;
        private bool _selected;
        private float _labelHeight = 3.2f;
        private long _collectAmount;
        private ResourceType? _collectResource;

        public event Action<BuildingView>? Clicked;
        public event Action? CollectRequested;
        public BuildingInstance Instance { get; private set; } = null!;

        public void NotifyClicked() => Clicked?.Invoke(this);

        public void NotifyCollectRequested() => CollectRequested?.Invoke();

        public void Initialize(BuildingInstance instance, BuildingDefinition definition, float labelHeight = 3.2f)
        {
            Instance = instance;
            name = definition.DisplayName;
            _labelHeight = labelHeight;
            _visual = transform.Find("Visual");
            CacheRenderers();
            ApplyStateTint();
            _label = CreateLabel(FormatLabel(definition));
            _label.gameObject.SetActive(false);
            _collectableMarker = CreateCollectableMarker();
            _upgradeArrow = CreateUpgradeArrow();
            _lockedBadge = CreateStatusBadge("LockedBadge", new Color(0.35f, 0.38f, 0.45f), isLock: true);
            _readyBadge = CreateStatusBadge("ReadyBadge", new Color(0.72f, 0.58f, 0.28f), isLock: false);
            _constructionRoot = CreateBuildingWorldUi();
            _constructionVisual = BuildingConstructionVisual.Ensure(transform, instance.DefinitionId);
            SetCollectable(0, null);
            SetUpgradeAvailable(false);
            SetConstructionProgress(0f, string.Empty, false);
            RefreshStatusBadges();
        }

        /// <summary>
        /// Sincroniza o visual real do Castelo com o nível atual (só definitionId=castle).
        /// </summary>
        public void SyncCastleVisual(bool animate = false)
        {
            if (Instance == null ||
                !string.Equals(Instance.DefinitionId, "castle", StringComparison.Ordinal))
            {
                return;
            }

            _visual ??= transform.Find("Visual");
            if (_visual == null)
            {
                return;
            }

            var cam = UnityEngine.Object.FindFirstObjectByType<Valgor.City.Camera.CityCameraController>();
            var pose = cam?.CapturePose();
            cam?.SuppressFocus(0.85f);
            if (!animate)
            {
                cam?.LockPose();
            }

            var logicalPos = transform.position;
            var logicalRot = transform.rotation;
            var logicalScale = transform.localScale;

            if (!CastleRealVisualLoader.Sync(_visual, Instance.Level, animate, out var detail, out var deferred))
            {
                Debug.LogWarning($"[Valgor.City] SyncCastleVisual failed: {detail}");
                if (CastleRealVisualLoader.FindAttachedTier(_visual) <= 0)
                {
                    CastleTierVisual.Build(_visual, Color.white, visualTier: 1);
                }
            }

            transform.SetPositionAndRotation(logicalPos, logicalRot);
            transform.localScale = logicalScale;

            if (!animate)
            {
                if (pose.HasValue)
                {
                    cam?.RestorePose(pose.Value);
                }

                cam?.UnlockPose();
            }

            if (deferred)
            {
                return;
            }

            RecacheAfterCastleVisualSwap();
            ApplyCastlePresentationLabelHeight();
        }

        private void ApplyCastlePresentationLabelHeight()
        {
            if (Instance == null ||
                !string.Equals(Instance.DefinitionId, "castle", StringComparison.Ordinal))
            {
                return;
            }

            var presentation = CastleTierPresentation.ForBuildingLevel(Instance.Level);
            _labelHeight = presentation.LabelHeight;
            if (_label != null)
            {
                _label.transform.localPosition = Vector3.up * (_labelHeight + 0.35f);
            }
        }

        public void RecacheAfterCastleVisualSwap()
        {
            CacheRenderers();
            ApplyStateTint();
            ApplyCastlePresentationLabelHeight();
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            ApplyColors();
            // Seleção não muda escala — collider e silhueta permanecem estáveis.

            if (_label != null)
            {
                _label.gameObject.SetActive(selected);
            }

            if (Instance != null &&
                string.Equals(Instance.DefinitionId, "wall", StringComparison.Ordinal))
            {
                CityEnvironmentBuilder.SetFortificationsHighlighted(selected);
            }

            RefreshCollectableLabel();
        }

        public void SetCollectable(long amount, ResourceType? resource)
        {
            _collectAmount = amount;
            _collectResource = resource;
            var show = amount > 0;
            if (_collectableMarker != null)
            {
                _collectableMarker.SetActive(show);
                if (show && resource.HasValue && _resourceIconRenderer != null)
                {
                    CityVisualMaterials.Apply(_resourceIconRenderer, ResourceIconColor(resource.Value));
                }
            }

            RefreshCollectableLabel();
        }

        private void RefreshCollectableLabel()
        {
            if (_bubbleLabel == null)
            {
                return;
            }

            // Sem abreviações técnicas no mundo: quantidade só com prédio selecionado.
            var showAmount = _selected && _collectAmount > 0;
            _bubbleLabel.gameObject.SetActive(showAmount);
            if (showAmount)
            {
                _bubbleLabel.text = FormatAmount(_collectAmount);
            }
        }

        public void SetUpgradeAvailable(bool available)
        {
            if (_upgradeArrow != null)
            {
                _upgradeArrow.SetActive(available && Instance.State != BuildingState.Upgrading);
            }
        }

        public void SetConstructionProgress(float progress01, string timeLabel, bool active)
        {
            if (_constructionVisual == null && Instance != null)
            {
                _constructionVisual = BuildingConstructionVisual.Ensure(transform, Instance.DefinitionId);
            }

            _constructionVisual?.SetActive(active);

            if (_constructionRoot == null)
            {
                return;
            }

            _constructionRoot.SetActive(active);
            if (!active)
            {
                return;
            }

            // Mantém o UI acima do visual atual (sem mover o BuildingRoot).
            var uiHeight = _labelHeight + 0.35f;
            if (_visual != null)
            {
                var b = GetWorldBounds();
                uiHeight = Mathf.Max(uiHeight, (b.max.y - transform.position.y) + 0.85f);
            }

            _constructionRoot.transform.localPosition = Vector3.up * uiHeight;

            if (_progressFill != null)
            {
                var p = Mathf.Clamp01(progress01);
                _progressFill.localScale = new Vector3(Mathf.Max(0.05f, p), 1f, 1f);
                _progressFill.localPosition = new Vector3((-0.5f + p * 0.5f) * 1.6f, 0f, -0.02f);
            }

            if (_progressLabel != null)
            {
                _progressLabel.text = string.IsNullOrEmpty(timeLabel) ? "…" : timeLabel;
            }
        }

        public void RefreshLabel(BuildingDefinition definition)
        {
            if (_label != null)
            {
                _label.text = FormatLabel(definition);
            }
        }

        public void RefreshStateColor()
        {
            ApplyStateTint();
            RefreshStatusBadges();
        }

        private void RefreshStatusBadges()
        {
            var locked = Instance.State == BuildingState.Locked;
            var readyNew = Instance.State == BuildingState.Available && Instance.Level <= 0;
            if (_lockedBadge != null)
            {
                _lockedBadge.SetActive(locked);
            }

            if (_readyBadge != null)
            {
                // “Construir disponível” — distintivo dourado pequeno (não confunde com upgrade).
                _readyBadge.SetActive(readyNew && !locked);
            }
        }

        private string FormatLabel(BuildingDefinition definition)
        {
            if (Instance.State == BuildingState.Upgrading)
            {
                return $"{definition.DisplayName} · Nv.{Instance.Level}→{Instance.Level + 1}";
            }

            if (Instance.State == BuildingState.Available && Instance.Level <= 0)
            {
                return $"{definition.DisplayName} · Construir";
            }

            return $"{definition.DisplayName} · Nv.{Math.Max(0, Instance.Level)}";
        }

        /// <summary>Bounds world do visual (ou collider) para posicionar o menu fora da silhueta.</summary>
        public Bounds GetWorldBounds()
        {
            var renderers = _visual != null
                ? _visual.GetComponentsInChildren<Renderer>()
                : GetComponentsInChildren<Renderer>();
            var has = false;
            var bounds = new Bounds(transform.position, Vector3.one * 0.5f);
            for (var i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy)
                {
                    continue;
                }

                // Ignora labels/badges/FX de obra (não entram no framing da câmera/menu).
                var n = r.gameObject.name;
                if (n is "Name" or "LabelPlate" or "CollectableMarker" or "Rim" or "ResourceIcon"
                    or "LockedBadge" or "ReadyBadge" or "ConstructionOverlay" or "BuildingWorldUI"
                    or "ConstructionVisualRoot" or "Scaffold" or "DustVFX" or "DebrisVFX" or "WorkAudio"
                    or "ConstructionTimer" or "ConstructionProgress" or "ConstructionStatusIcon"
                    or "BuildIcon" or "ProgressTrack" or "ProgressFill" or "Amount" or "Beam"
                    or "Glyph" or "Stem" or "Tip")
                {
                    continue;
                }

                if (IsUnderExcludedAncestor(r.transform))
                {
                    continue;
                }

                var parentName = r.transform.parent != null ? r.transform.parent.name : string.Empty;
                if (parentName is "Name" or "CollectableMarker" or "LockedBadge" or "ReadyBadge"
                    or "BuildingWorldUI" or "ConstructionVisualRoot" or "Scaffold")
                {
                    continue;
                }

                if (!has)
                {
                    bounds = r.bounds;
                    has = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            if (!has)
            {
                var col = GetComponent<Collider>();
                if (col != null)
                {
                    return col.bounds;
                }
            }

            return bounds;
        }

        public Rect GetScreenRect(UnityEngine.Camera camera)
        {
            var b = GetWorldBounds();
            var corners = new[]
            {
                new Vector3(b.min.x, b.min.y, b.min.z),
                new Vector3(b.min.x, b.min.y, b.max.z),
                new Vector3(b.min.x, b.max.y, b.min.z),
                new Vector3(b.min.x, b.max.y, b.max.z),
                new Vector3(b.max.x, b.min.y, b.min.z),
                new Vector3(b.max.x, b.min.y, b.max.z),
                new Vector3(b.max.x, b.max.y, b.min.z),
                new Vector3(b.max.x, b.max.y, b.max.z)
            };

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            for (var i = 0; i < corners.Length; i++)
            {
                var sp = camera.WorldToScreenPoint(corners[i]);
                if (sp.z < 0f)
                {
                    continue;
                }

                min = Vector2.Min(min, new Vector2(sp.x, sp.y));
                max = Vector2.Max(max, new Vector2(sp.x, sp.y));
            }

            if (min.x > max.x)
            {
                var c = camera.WorldToScreenPoint(b.center);
                return new Rect(c.x - 40f, c.y - 40f, 80f, 80f);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private bool[] _lockAccent = Array.Empty<bool>();

        private void CacheRenderers()
        {
            _renderers = _visual != null
                ? _visual.GetComponentsInChildren<Renderer>()
                : GetComponentsInChildren<Renderer>();
            _identityColors = new Color[_renderers.Length];
            _baseColors = new Color[_renderers.Length];
            _lockAccent = new bool[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                _identityColors[i] = ReadColor(_renderers[i]);
                _baseColors[i] = _identityColors[i];
                _lockAccent[i] = IsAccentRenderer(_renderers[i]);
            }
        }

        private static bool IsAccentRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }

            for (var t = renderer.transform; t != null; t = t.parent)
            {
                var n = t.name;
                if (n.StartsWith("Accent_", StringComparison.Ordinal)
                    || n.Contains("LionCrest", StringComparison.Ordinal)
                    || n.Contains("CrestBanner", StringComparison.Ordinal)
                    || n.Contains("GateEagle", StringComparison.Ordinal)
                    || n.Contains("BannerCloth", StringComparison.Ordinal)
                    || n.Contains("MainGate", StringComparison.Ordinal)
                    || CastleRealVisualLoader.IsRealCastleRenderer(t))
                {
                    return true;
                }

                if (t.name == "Visual" || t.name.StartsWith("Slot_", StringComparison.Ordinal))
                {
                    break;
                }
            }

            return false;
        }

        private void ApplyStateTint()
        {
            var tint = CityLayout.ToTint(Instance.State);
            for (var i = 0; i < _renderers.Length; i++)
            {
                _baseColors[i] = _lockAccent[i]
                    ? _identityColors[i]
                    : CityVisualMaterials.MixState(_identityColors[i], tint);
            }

            ApplyColors();
        }

        private void ApplyColors()
        {
            // Castelo real: nunca RuntimeSafeMaterials / recolor genérico.
            if (Instance != null &&
                string.Equals(Instance.DefinitionId, "castle", StringComparison.Ordinal))
            {
                return;
            }

            for (var i = 0; i < _renderers.Length; i++)
            {
                // Assets reais texturizados (ex.: Castle_Tier*) — não sobrescrever materiais.
                if (_lockAccent[i])
                {
                    continue;
                }

                var color = _selected
                    ? Color.Lerp(_baseColors[i], new Color(0.86f, 0.72f, 0.38f), 0.42f)
                    : _baseColors[i];
                CityVisualMaterials.Apply(_renderers[i], color);
            }
        }

        private TextMesh CreateLabel(string displayName)
        {
            var labelObject = new GameObject("Name");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.up * (_labelHeight + 0.35f);
            labelObject.transform.rotation = Quaternion.Euler(30f, 45f, 0f);

            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "LabelPlate";
            plate.transform.SetParent(labelObject.transform, false);
            plate.transform.localPosition = Vector3.zero;
            plate.transform.localScale = new Vector3(2.8f, 0.38f, 0.08f);
            Destroy(plate.GetComponent<Collider>());
            CityVisualMaterials.Apply(plate.GetComponent<Renderer>(), new Color(0.12f, 0.11f, 0.1f, 0.92f));

            var label = labelObject.AddComponent<TextMesh>();
            label.text = displayName;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.12f;
            label.fontSize = 42;
            label.color = new Color(0.95f, 0.9f, 0.78f);
            return label;
        }

        private GameObject CreateCollectableMarker()
        {
            // Medalhão plano (não esfera verde genérica) + ícone de recurso.
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "CollectableMarker";
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = Vector3.up * (_labelHeight + 0.85f);
            marker.transform.localScale = new Vector3(0.48f, 0.06f, 0.48f);
            marker.transform.localRotation = Quaternion.Euler(90f, 45f, 0f);
            CityVisualMaterials.Apply(marker.GetComponent<Renderer>(), new Color(0.14f, 0.42f, 0.28f));

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "Rim";
            rim.transform.SetParent(marker.transform, false);
            rim.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            rim.transform.localScale = new Vector3(1.15f, 0.35f, 1.15f);
            Destroy(rim.GetComponent<Collider>());
            CityVisualMaterials.Apply(rim.GetComponent<Renderer>(), new Color(0.72f, 0.58f, 0.28f));

            var icon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            icon.name = "ResourceIcon";
            icon.transform.SetParent(marker.transform, false);
            icon.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            icon.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            icon.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            Destroy(icon.GetComponent<Collider>());
            _resourceIconRenderer = icon.GetComponent<Renderer>();
            CityVisualMaterials.Apply(_resourceIconRenderer, new Color(0.95f, 0.85f, 0.35f));

            var amountObject = new GameObject("Amount");
            amountObject.transform.SetParent(marker.transform, false);
            amountObject.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            amountObject.transform.localRotation = Quaternion.Euler(-90f, 0f, -45f);
            amountObject.transform.localScale = Vector3.one * 0.55f;
            _bubbleLabel = amountObject.AddComponent<TextMesh>();
            _bubbleLabel.anchor = TextAnchor.MiddleCenter;
            _bubbleLabel.alignment = TextAlignment.Center;
            _bubbleLabel.characterSize = 0.18f;
            _bubbleLabel.fontSize = 48;
            _bubbleLabel.color = new Color(0.95f, 0.92f, 0.8f);
            _bubbleLabel.text = string.Empty;
            amountObject.SetActive(false);

            var click = marker.AddComponent<BuildingCollectableClickProxy>();
            click.Bind(() => NotifyCollectRequested());
            return marker;
        }

        private GameObject CreateUpgradeArrow()
        {
            // Chevron dourado (upgrade) — menor e integrado.
            var arrow = new GameObject("UpgradeArrow");
            arrow.transform.SetParent(transform, false);
            arrow.transform.localPosition = Vector3.up * (_labelHeight + 1.35f);

            var stem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stem.name = "Stem";
            stem.transform.SetParent(arrow.transform, false);
            stem.transform.localPosition = Vector3.zero;
            stem.transform.localScale = new Vector3(0.14f, 0.42f, 0.14f);
            Destroy(stem.GetComponent<Collider>());
            CityVisualMaterials.Apply(stem.GetComponent<Renderer>(), new Color(0.78f, 0.62f, 0.28f));

            var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tip.name = "Tip";
            tip.transform.SetParent(arrow.transform, false);
            tip.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            tip.transform.localScale = new Vector3(0.32f, 0.22f, 0.14f);
            tip.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            Destroy(tip.GetComponent<Collider>());
            CityVisualMaterials.Apply(tip.GetComponent<Renderer>(), new Color(0.9f, 0.75f, 0.35f));

            arrow.SetActive(false);
            return arrow;
        }

        private GameObject CreateStatusBadge(string name, Color color, bool isLock)
        {
            var root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.up * (_labelHeight + 0.55f);

            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Plate";
            plate.transform.SetParent(root.transform, false);
            plate.transform.localScale = new Vector3(0.38f, 0.38f, 0.08f);
            plate.transform.localRotation = Quaternion.Euler(25f, 45f, 0f);
            Destroy(plate.GetComponent<Collider>());
            CityVisualMaterials.Apply(plate.GetComponent<Renderer>(), color);

            var glyph = GameObject.CreatePrimitive(isLock ? PrimitiveType.Cylinder : PrimitiveType.Sphere);
            glyph.name = "Glyph";
            glyph.transform.SetParent(root.transform, false);
            glyph.transform.localPosition = new Vector3(0f, 0f, -0.06f);
            glyph.transform.localScale = isLock
                ? new Vector3(0.16f, 0.08f, 0.16f)
                : Vector3.one * 0.16f;
            glyph.transform.localRotation = Quaternion.Euler(25f, 45f, 0f);
            Destroy(glyph.GetComponent<Collider>());
            CityVisualMaterials.Apply(glyph.GetComponent<Renderer>(),
                isLock ? new Color(0.18f, 0.18f, 0.22f) : new Color(0.95f, 0.88f, 0.55f));

            root.SetActive(false);
            return root;
        }

        private static bool IsUnderExcludedAncestor(Transform t)
        {
            var p = t;
            while (p != null)
            {
                var n = p.name;
                if (n is "ConstructionVisualRoot" or "BuildingWorldUI" or "ConstructionOverlay"
                    or "CollectableMarker" or "UpgradeArrow")
                {
                    return true;
                }

                // Prefab instance names.
                if (n.StartsWith("ConstructionScaffold_", StringComparison.Ordinal) ||
                    n.StartsWith("Scaffold_", StringComparison.Ordinal))
                {
                    return true;
                }

                p = p.parent;
            }

            return false;
        }

        private GameObject CreateBuildingWorldUi()
        {
            var root = new GameObject("BuildingWorldUI");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.up * (_labelHeight + 0.15f);
            root.transform.localRotation = Quaternion.Euler(30f, 45f, 0f);

            var icon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            icon.name = "ConstructionStatusIcon";
            icon.transform.SetParent(root.transform, false);
            icon.transform.localPosition = new Vector3(-1.05f, 0.15f, 0f);
            icon.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
            Destroy(icon.GetComponent<Collider>());
            CityVisualMaterials.Apply(icon.GetComponent<Renderer>(), new Color(0.78f, 0.62f, 0.28f));

            // Ícone de martelo/obra (dois volumes).
            var hammer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hammer.name = "BuildIcon";
            hammer.transform.SetParent(icon.transform, false);
            hammer.transform.localPosition = new Vector3(0.15f, 0.25f, -0.4f);
            hammer.transform.localScale = new Vector3(0.35f, 0.9f, 0.35f);
            hammer.transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
            Destroy(hammer.GetComponent<Collider>());
            CityVisualMaterials.Apply(hammer.GetComponent<Renderer>(), new Color(0.9f, 0.78f, 0.4f));

            var track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.name = "ConstructionProgress";
            track.transform.SetParent(root.transform, false);
            track.transform.localPosition = new Vector3(0.15f, 0.15f, 0f);
            track.transform.localScale = new Vector3(1.6f, 0.18f, 0.08f);
            Destroy(track.GetComponent<Collider>());
            CityVisualMaterials.Apply(track.GetComponent<Renderer>(), new Color(0.12f, 0.12f, 0.14f));

            var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.name = "ProgressFill";
            fill.transform.SetParent(track.transform, false);
            fill.transform.localPosition = new Vector3(-0.4f, 0f, -0.02f);
            fill.transform.localScale = new Vector3(0.2f, 1f, 1f);
            Destroy(fill.GetComponent<Collider>());
            CityVisualMaterials.Apply(fill.GetComponent<Renderer>(), new Color(0.28f, 0.55f, 0.85f));
            _progressFill = fill.transform;

            var timeObj = new GameObject("ConstructionTimer");
            timeObj.transform.SetParent(root.transform, false);
            timeObj.transform.localPosition = new Vector3(0.15f, 0.55f, 0f);
            _progressLabel = timeObj.AddComponent<TextMesh>();
            _progressLabel.anchor = TextAnchor.MiddleCenter;
            _progressLabel.alignment = TextAlignment.Center;
            _progressLabel.characterSize = 0.085f;
            _progressLabel.fontSize = 52;
            _progressLabel.color = new Color(0.96f, 0.9f, 0.72f);
            _progressLabel.text = "0s";

            root.SetActive(false);
            return root;
        }

        private GameObject CreateConstructionOverlay() => CreateBuildingWorldUi();

        private static Color ResourceIconColor(ResourceType resource) => resource switch
        {
            ResourceType.Gold => new Color(0.95f, 0.82f, 0.28f),
            ResourceType.Food => new Color(0.45f, 0.85f, 0.35f),
            ResourceType.Wood => new Color(0.55f, 0.38f, 0.22f),
            ResourceType.Stone => new Color(0.62f, 0.62f, 0.66f),
            ResourceType.Iron => new Color(0.55f, 0.58f, 0.7f),
            ResourceType.DragonEssence => new Color(0.45f, 0.35f, 0.85f),
            _ => new Color(0.35f, 0.85f, 0.45f)
        };

        private static string FormatAmount(long amount)
        {
            if (amount >= 1_000_000)
            {
                return (amount / 1_000_000f).ToString("0.#") + "M";
            }

            if (amount >= 10_000)
            {
                return (amount / 1_000f).ToString("0.#") + "K";
            }

            return amount.ToString();
        }

        private static Color ReadColor(Renderer renderer)
        {
            var material = renderer.sharedMaterial;
            if (material == null) return Color.gray;
            if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
            if (material.HasProperty("_Color")) return material.GetColor("_Color");
            return material.color;
        }
    }

    /// <summary>Proxy de clique no indicador de coleta (Input System raycast).</summary>
    public sealed class BuildingCollectableClickProxy : MonoBehaviour
    {
        private Action? _onClick;

        public void Bind(Action onClick) => _onClick = onClick;

        public void NotifyClicked() => _onClick?.Invoke();
    }

    /// <summary>
    /// Encaminha clique de colliders satélite (ex.: segmentos da Muralha) para o BuildingView lógico.
    /// </summary>
    public sealed class BuildingSelectionClickProxy : MonoBehaviour
    {
        private BuildingView? _target;

        public BuildingView? Target => _target;

        public void Bind(BuildingView target) => _target = target;

        public void NotifyClicked() => _target?.NotifyClicked();
    }
}
