using System.Collections.Generic;
using UnityEngine;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Visual
{
    /// <summary>
    /// Catálogo runtime dos visuais por estágio. Prefabs em Resources sobrescrevem placeholders.
    /// </summary>
    public static class DragonStageVisualCatalog
    {
        private static readonly Dictionary<DragonVisualStage, DragonStageVisualConfig> Defaults = BuildDefaults();
        private static Dictionary<DragonVisualStage, DragonStageVisualConfig>? _resolved;

        public static IReadOnlyDictionary<DragonVisualStage, DragonStageVisualConfig> All
        {
            get
            {
                EnsureResolved();
                return _resolved!;
            }
        }

        public static DragonStageVisualConfig Get(DragonVisualStage stage)
        {
            EnsureResolved();
            return _resolved!.TryGetValue(stage, out var cfg) ? cfg : Defaults[DragonVisualStage.Hatchling];
        }

        public static DragonVisualStage ResolveVisualStage(int dragonLevel, bool isEggOrHatching)
        {
            if (isEggOrHatching || dragonLevel <= 0)
            {
                return DragonVisualStage.Egg;
            }

            return DragonProgressionRules.VisualStageForLevel(dragonLevel);
        }

        /// <summary>
        /// Durante ritual/level-up: mantém visual do nível atual (não antecipa o PendingLevel).
        /// </summary>
        public static DragonVisualStage ResolveDisplayedStage(
            int dragonLevel,
            int pendingLevel,
            bool isLevelingUp,
            bool isEggOrHatching)
        {
            if (isEggOrHatching || dragonLevel <= 0)
            {
                return DragonVisualStage.Egg;
            }

            // Nunca usar pendingLevel para o mesh — só após conclusão.
            _ = pendingLevel;
            _ = isLevelingUp;
            return DragonProgressionRules.VisualStageForLevel(dragonLevel);
        }

        public static GameObject Spawn(Transform parent, DragonStageVisualConfig config)
        {
            GameObject? go = null;
            if (!config.PlaceholderFlag && !string.IsNullOrEmpty(config.PrefabResourcePath))
            {
                var prefab = Resources.Load<GameObject>(config.PrefabResourcePath);
                if (prefab != null)
                {
                    go = Object.Instantiate(prefab, parent, false);
                    go.name = $"DragonVisual_{config.Stage}";
                }
            }

            if (go == null)
            {
                go = DragonStagePlaceholderFactory.Create(parent, config);
            }

            go.transform.localPosition = config.LocalPosition;
            go.transform.localRotation = config.LocalRotation;
            go.transform.localScale = config.LocalScale;
            ApplyAnimator(go, config);
            ApplyLightPreset(go, config);
            return go;
        }

        public static void TrySpawnTransitionVfx(Transform parent, DragonStageVisualConfig config)
        {
            if (string.IsNullOrEmpty(config.TransitionVfxResourcePath))
            {
                DragonStagePlaceholderFactory.SpawnSoftBurst(parent, config.PlaceholderTint);
                return;
            }

            var prefab = Resources.Load<GameObject>(config.TransitionVfxResourcePath);
            if (prefab == null)
            {
                DragonStagePlaceholderFactory.SpawnSoftBurst(parent, config.PlaceholderTint);
                return;
            }

            var vfx = Object.Instantiate(prefab, parent, false);
            vfx.name = "DragonTransitionVfx";
            Object.Destroy(vfx, 2.5f);
        }

        private static void EnsureResolved()
        {
            if (_resolved != null)
            {
                return;
            }

            _resolved = new Dictionary<DragonVisualStage, DragonStageVisualConfig>(Defaults);
            var asset = Resources.Load<DragonStageVisualCatalogAsset>("Valgor/Dragons/DragonStageVisualCatalog");
            if (asset == null || asset.Stages == null)
            {
                return;
            }

            foreach (var entry in asset.Stages)
            {
                if (entry == null)
                {
                    continue;
                }

                _resolved[entry.Stage] = entry;
            }
        }

        private static void ApplyAnimator(GameObject go, DragonStageVisualConfig config)
        {
            if (string.IsNullOrEmpty(config.AnimatorControllerResourcePath))
            {
                return;
            }

            var controller = Resources.Load<RuntimeAnimatorController>(config.AnimatorControllerResourcePath);
            if (controller == null)
            {
                return;
            }

            var animator = go.GetComponentInChildren<Animator>() ?? go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
        }

        private static void ApplyLightPreset(GameObject go, DragonStageVisualConfig config)
        {
            var lightGo = new GameObject("StageLight");
            lightGo.transform.SetParent(go.transform, false);
            lightGo.transform.localPosition = new Vector3(0.35f, 1.1f, -0.4f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 3.2f;
            switch (config.LightPreset)
            {
                case "ember-soft":
                    light.color = new Color(1f, 0.55f, 0.25f);
                    light.intensity = 0.55f;
                    break;
                case "ash-warm":
                    light.color = new Color(0.95f, 0.7f, 0.4f);
                    light.intensity = 0.7f;
                    break;
                case "copper":
                    light.color = new Color(0.9f, 0.5f, 0.28f);
                    light.intensity = 0.85f;
                    break;
                case "forge":
                    light.color = new Color(1f, 0.4f, 0.15f);
                    light.intensity = 1.0f;
                    break;
                case "ancestral-gold":
                    light.color = new Color(1f, 0.82f, 0.35f);
                    light.intensity = 1.15f;
                    break;
                default:
                    light.color = new Color(1f, 0.6f, 0.3f);
                    light.intensity = 0.6f;
                    break;
            }
        }

        private static Dictionary<DragonVisualStage, DragonStageVisualConfig> BuildDefaults()
        {
            return new Dictionary<DragonVisualStage, DragonStageVisualConfig>
            {
                [DragonVisualStage.Egg] = new()
                {
                    Stage = DragonVisualStage.Egg,
                    DisplayNamePt = "Ovo",
                    PrefabResourcePath = "Valgor/Dragons/Visuals/Egg",
                    LocalPosition = new Vector3(0f, 0.28f, 0f),
                    LocalScale = new Vector3(0.55f, 0.72f, 0.55f),
                    PreviewCameraOffset = new Vector3(0f, 1.0f, -2.0f),
                    LightPreset = "ember-soft",
                    TransitionVfxResourcePath = "Valgor/Dragons/Vfx/StageSoft",
                    PlaceholderFlag = true,
                    PlaceholderTint = new Color(0.88f, 0.74f, 0.48f)
                },
                [DragonVisualStage.Hatchling] = new()
                {
                    Stage = DragonVisualStage.Hatchling,
                    DisplayNamePt = "Filhote",
                    PrefabResourcePath = "Valgor/Dragons/Visuals/Hatchling",
                    LocalPosition = new Vector3(0f, 0.45f, 0f),
                    LocalScale = new Vector3(0.42f, 0.48f, 0.42f),
                    PreviewCameraOffset = new Vector3(0f, 1.1f, -2.2f),
                    LightPreset = "ember-soft",
                    TransitionVfxResourcePath = "Valgor/Dragons/Vfx/StageSoft",
                    PlaceholderFlag = true,
                    PlaceholderTint = new Color(0.72f, 0.32f, 0.16f)
                },
                [DragonVisualStage.Young] = new()
                {
                    Stage = DragonVisualStage.Young,
                    DisplayNamePt = "Jovem",
                    PrefabResourcePath = "Valgor/Dragons/Visuals/Young",
                    LocalPosition = new Vector3(0f, 0.52f, 0f),
                    LocalScale = new Vector3(0.52f, 0.58f, 0.52f),
                    PreviewCameraOffset = new Vector3(0f, 1.25f, -2.4f),
                    LightPreset = "ash-warm",
                    TransitionVfxResourcePath = "Valgor/Dragons/Vfx/StageSoft",
                    PlaceholderFlag = true,
                    PlaceholderTint = new Color(0.78f, 0.38f, 0.14f)
                },
                [DragonVisualStage.Adolescent] = new()
                {
                    Stage = DragonVisualStage.Adolescent,
                    DisplayNamePt = "Adolescente",
                    PrefabResourcePath = "Valgor/Dragons/Visuals/Adolescent",
                    LocalPosition = new Vector3(0f, 0.58f, 0f),
                    LocalScale = new Vector3(0.62f, 0.7f, 0.62f),
                    PreviewCameraOffset = new Vector3(0f, 1.35f, -2.6f),
                    LightPreset = "copper",
                    TransitionVfxResourcePath = "Valgor/Dragons/Vfx/StageSoft",
                    PlaceholderFlag = true,
                    PlaceholderTint = new Color(0.62f, 0.34f, 0.18f)
                },
                [DragonVisualStage.YoungAdult] = new()
                {
                    Stage = DragonVisualStage.YoungAdult,
                    DisplayNamePt = "Adulto jovem",
                    PrefabResourcePath = "Valgor/Dragons/Visuals/YoungAdult",
                    LocalPosition = new Vector3(0f, 0.64f, 0f),
                    LocalScale = new Vector3(0.72f, 0.82f, 0.72f),
                    PreviewCameraOffset = new Vector3(0f, 1.45f, -2.8f),
                    LightPreset = "forge",
                    TransitionVfxResourcePath = "Valgor/Dragons/Vfx/StageSoft",
                    PlaceholderFlag = true,
                    PlaceholderTint = new Color(0.55f, 0.26f, 0.12f)
                },
                [DragonVisualStage.Adult] = new()
                {
                    Stage = DragonVisualStage.Adult,
                    DisplayNamePt = "Adulto",
                    PrefabResourcePath = "Valgor/Dragons/Visuals/Adult",
                    LocalPosition = new Vector3(0f, 0.7f, 0f),
                    LocalScale = new Vector3(0.82f, 0.92f, 0.82f),
                    PreviewCameraOffset = new Vector3(0f, 1.55f, -3.0f),
                    LightPreset = "forge",
                    TransitionVfxResourcePath = "Valgor/Dragons/Vfx/StageSoft",
                    PlaceholderFlag = true,
                    PlaceholderTint = new Color(0.42f, 0.22f, 0.14f)
                },
                [DragonVisualStage.Ancestral] = new()
                {
                    Stage = DragonVisualStage.Ancestral,
                    DisplayNamePt = "Ancestral",
                    PrefabResourcePath = "Valgor/Dragons/Visuals/Ancestral",
                    LocalPosition = new Vector3(0f, 0.78f, 0f),
                    LocalScale = new Vector3(0.95f, 1.05f, 0.95f),
                    PreviewCameraOffset = new Vector3(0f, 1.7f, -3.3f),
                    LightPreset = "ancestral-gold",
                    TransitionVfxResourcePath = "Valgor/Dragons/Vfx/StageSoft",
                    PlaceholderFlag = true,
                    PlaceholderTint = new Color(0.72f, 0.52f, 0.22f)
                }
            };
        }
    }
}
