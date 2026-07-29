using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valgor.City.Camera;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;
using Valgor.Dragons.Visual;
using Valgor.Core.Modules;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Ninho na Torre: visual data-driven por estágio, sem antecipar ritual.
    /// </summary>
    public sealed class DragonNestView : MonoBehaviour
    {
        private Transform _dragonRoot = null!;
        private Transform _occupantsRoot = null!;
        private DragonService? _dragons;
        private readonly List<GameObject> _spawned = new();
        private DragonVisualStage? _displayedStage;
        private int _displayedLevel = int.MinValue;
        private bool _displayedLeveling;
        private TextMesh? _timerMesh;
        private GameObject? _ritualVfx;
        private Coroutine? _swapRoutine;
        private Vector3 _rootWorldPos;
        private Quaternion _rootWorldRot;
        private Vector3 _rootLocalScale;

        public void Bind(DragonService dragons)
        {
            if (_dragons != null)
            {
                _dragons.Changed -= OnDragonsChanged;
            }

            _dragons = dragons;
            EnsureRoots();
            CaptureRootPose();
            _dragons.Changed += OnDragonsChanged;
            Refresh(force: true);
        }

        private void OnDestroy()
        {
            if (_dragons != null)
            {
                _dragons.Changed -= OnDragonsChanged;
            }
        }

        private void Update()
        {
            if (_dragons == null)
            {
                return;
            }

            UpdateRitualOverlay();
        }

        private void OnDragonsChanged(object? sender, DragonChangedEvent e) => Refresh(force: false);

        public void Refresh() => Refresh(force: false);

        private void Refresh(bool force)
        {
            EnsureRoots();
            RestoreRootPose();
            if (_dragons == null || _occupantsRoot == null)
            {
                return;
            }

            var statuses = _dragons.GetDragonStatuses();
            DragonStatusInfo? primary = null;
            foreach (var status in statuses)
            {
                primary = status;
                break;
            }

            if (primary == null)
            {
                ClearSpawned();
                _displayedStage = null;
                HideRitualOverlay();
                return;
            }

            var statusValue = primary.Value;
            var isEgg = statusValue.DragonLevel < 1 ||
                        statusValue.StateLabel.IndexOf("EGG", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        statusValue.StateLabel.IndexOf("HATCH", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        statusValue.GrowthStageLabel.IndexOf("Ovo", System.StringComparison.OrdinalIgnoreCase) >= 0;

            var targetStage = DragonStageVisualCatalog.ResolveDisplayedStage(
                statusValue.DragonLevel,
                statusValue.PendingLevel,
                statusValue.IsLevelingUp,
                isEgg);

            var stageChanged = !_displayedStage.HasValue || _displayedStage.Value != targetStage;
            var levelChanged = _displayedLevel != statusValue.DragonLevel;
            var levelingChanged = _displayedLeveling != statusValue.IsLevelingUp;

            if (!force && !stageChanged && !levelChanged && !levelingChanged && _spawned.Count > 0)
            {
                UpdateRitualOverlay();
                return;
            }

            // Durante ritual: não troca mesh antecipadamente.
            if (statusValue.IsLevelingUp &&
                DragonProgressionRules.IsRitualTarget(statusValue.PendingLevel) &&
                _displayedStage.HasValue &&
                !force)
            {
                _displayedLeveling = true;
                EnsureRitualOverlay(statusValue);
                return;
            }

            if (stageChanged && _displayedStage.HasValue && !force)
            {
                if (_swapRoutine != null)
                {
                    StopCoroutine(_swapRoutine);
                }

                _swapRoutine = StartCoroutine(SwapVisualStable(targetStage, statusValue));
                return;
            }

            ApplyVisualImmediate(targetStage, statusValue);
        }

        private IEnumerator SwapVisualStable(DragonVisualStage targetStage, DragonStatusInfo status)
        {
            var cam = FindFirstObjectByType<CityCameraController>();
            cam?.LockPose();
            cam?.SuppressFocus(0.85f);

            var rootPos = _dragonRoot.position;
            var rootRot = _dragonRoot.rotation;
            var rootScale = _dragonRoot.localScale;

            var config = DragonStageVisualCatalog.Get(targetStage);
            DragonStageVisualCatalog.TrySpawnTransitionVfx(_dragonRoot, config);

            var old = _spawned.Count > 0 ? _spawned[0] : null;
            if (old != null)
            {
                var t0 = Time.unscaledTime;
                while (Time.unscaledTime - t0 < 0.12f)
                {
                    var u = (Time.unscaledTime - t0) / 0.12f;
                    if (old != null)
                    {
                        SetFade(old, 1f - u);
                    }

                    _dragonRoot.position = rootPos;
                    _dragonRoot.rotation = rootRot;
                    _dragonRoot.localScale = rootScale;
                    yield return null;
                }
            }

            ClearSpawned();
            var spawned = DragonStageVisualCatalog.Spawn(_occupantsRoot, config);
            spawned.transform.localPosition = config.LocalPosition;
            _spawned.Add(spawned);
            SetFade(spawned, 0f);

            var t1 = Time.unscaledTime;
            while (Time.unscaledTime - t1 < 0.16f)
            {
                var u = (Time.unscaledTime - t1) / 0.16f;
                SetFade(spawned, u);
                _dragonRoot.position = rootPos;
                _dragonRoot.rotation = rootRot;
                _dragonRoot.localScale = rootScale;
                yield return null;
            }

            SetFade(spawned, 1f);
            _displayedStage = targetStage;
            _displayedLevel = status.DragonLevel;
            _displayedLeveling = status.IsLevelingUp;
            UpdateRitualOverlay();
            RestoreRootPose();
            cam?.UnlockPose();
            _swapRoutine = null;
        }

        private void ApplyVisualImmediate(DragonVisualStage targetStage, DragonStatusInfo status)
        {
            ClearSpawned();
            var config = DragonStageVisualCatalog.Get(targetStage);
            var spawned = DragonStageVisualCatalog.Spawn(_occupantsRoot, config);
            _spawned.Add(spawned);
            _displayedStage = targetStage;
            _displayedLevel = status.DragonLevel;
            _displayedLeveling = status.IsLevelingUp;
            if (status.IsLevelingUp)
            {
                EnsureRitualOverlay(status);
            }
            else
            {
                HideRitualOverlay();
            }

            RestoreRootPose();
        }

        private void EnsureRitualOverlay(DragonStatusInfo status)
        {
            if (_timerMesh == null)
            {
                var timerGo = new GameObject("RitualTimer");
                timerGo.transform.SetParent(_dragonRoot, false);
                timerGo.transform.localPosition = new Vector3(0f, 1.55f, 0f);
                _timerMesh = timerGo.AddComponent<TextMesh>();
                _timerMesh.anchor = TextAnchor.MiddleCenter;
                _timerMesh.alignment = TextAlignment.Center;
                _timerMesh.characterSize = 0.08f;
                _timerMesh.fontSize = 48;
                _timerMesh.color = new Color(1f, 0.85f, 0.45f);
            }

            if (_ritualVfx == null)
            {
                _ritualVfx = new GameObject("RitualSoftVfx");
                _ritualVfx.transform.SetParent(_dragonRoot, false);
                _ritualVfx.transform.localPosition = Vector3.up * 0.5f;
                var ps = _ritualVfx.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 1.2f;
                main.startSize = 0.08f;
                main.startColor = new Color(1f, 0.55f, 0.2f, 0.7f);
                main.loop = true;
                main.maxParticles = 20;
                var emission = ps.emission;
                emission.rateOverTime = 8f;
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.45f;
            }

            _timerMesh.gameObject.SetActive(true);
            _ritualVfx.SetActive(true);
            UpdateRitualOverlay();
        }

        private void UpdateRitualOverlay()
        {
            if (_dragons == null || _timerMesh == null || !_timerMesh.gameObject.activeSelf)
            {
                return;
            }

            foreach (var status in _dragons.GetDragonStatuses())
            {
                if (!status.IsLevelingUp)
                {
                    HideRitualOverlay();
                    return;
                }

                var rem = "?";
                if (_dragons.TryGet(status.DragonId, out var dragon) && dragon.LevelUpEndsAtUtc.HasValue)
                {
                    var secs = System.Math.Max(0, (dragon.LevelUpEndsAtUtc.Value - System.DateTime.UtcNow).TotalSeconds);
                    rem = secs >= 60 ? $"~{secs / 60:0}m" : $"{secs:0}s";
                }

                var ritual = DragonProgressionRules.IsRitualTarget(status.PendingLevel);
                var label = ritual
                    ? DragonProgressionRules.RitualName(status.PendingLevel)
                    : "Evolução";
                _timerMesh.text = $"{label}\n→ Nv.{status.PendingLevel} · {rem}";
                // Mantém visual anterior durante o timer.
                return;
            }

            HideRitualOverlay();
        }

        private void HideRitualOverlay()
        {
            if (_timerMesh != null)
            {
                _timerMesh.gameObject.SetActive(false);
            }

            if (_ritualVfx != null)
            {
                _ritualVfx.SetActive(false);
            }

            _displayedLeveling = false;
        }

        private void EnsureRoots()
        {
            if (_occupantsRoot != null && _dragonRoot != null)
            {
                return;
            }

            var visual = transform.Find("Visual");
            if (visual == null)
            {
                var visualGo = new GameObject("Visual");
                visualGo.transform.SetParent(transform, false);
                visual = visualGo.transform;
            }

            _dragonRoot = visual.Find("DragonRoot");
            if (_dragonRoot == null)
            {
                var rootGo = new GameObject("DragonRoot");
                rootGo.transform.SetParent(visual, false);
                _dragonRoot = rootGo.transform;
            }

            _occupantsRoot = _dragonRoot.Find("NestOccupants");
            if (_occupantsRoot == null)
            {
                var nest = transform.Find("Visual/NestOccupants");
                if (nest != null)
                {
                    nest.SetParent(_dragonRoot, true);
                    _occupantsRoot = nest;
                }
                else
                {
                    var go = new GameObject("NestOccupants");
                    go.transform.SetParent(_dragonRoot, false);
                    _occupantsRoot = go.transform;
                }
            }
        }

        private void CaptureRootPose()
        {
            if (_dragonRoot == null)
            {
                return;
            }

            _rootWorldPos = _dragonRoot.position;
            _rootWorldRot = _dragonRoot.rotation;
            _rootLocalScale = _dragonRoot.localScale;
        }

        private void RestoreRootPose()
        {
            if (_dragonRoot == null)
            {
                return;
            }

            if (_rootLocalScale == Vector3.zero)
            {
                CaptureRootPose();
                return;
            }

            _dragonRoot.position = _rootWorldPos;
            _dragonRoot.rotation = _rootWorldRot;
            _dragonRoot.localScale = _rootLocalScale;
        }

        private void ClearSpawned()
        {
            foreach (var go in _spawned)
            {
                if (go != null)
                {
                    Destroy(go);
                }
            }

            _spawned.Clear();
        }

        private static void SetFade(GameObject root, float alpha)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var mat = renderers[i].material;
                if (mat == null)
                {
                    continue;
                }

                var c = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
                c.a = Mathf.Clamp01(alpha);
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", c);
                }
                else
                {
                    mat.color = c;
                }
            }
        }
    }
}
