#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Valgor.Heroes.Characters;
using Valgor.Heroes.Characters.Vortex;
using Valgor.Heroes.Preview360;

namespace Valgor.Heroes.EditorTools
{
    public static class VortexPrefabBuilder
    {
        public static GameObject BuildOrUpdate()
        {
            VortexPipelineMenus.EnsureFolders();
            EnsureMaterials();
            var controller = EnsureAnimatorController();
            BindAnimationClips(controller);
            var hasSource = HeroVisualResolver.HasVortexSourceModel();
            GameObject modelSource = null;
            if (hasSource)
            {
                foreach (var path in VortexAssetPaths.RequiredModelCandidates)
                {
                    modelSource = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (modelSource != null) break;
                }
            }

            var root = new GameObject("Vortex_Hero");
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            var sockets = root.AddComponent<HeroSocketRegistry>();
            var materials = root.AddComponent<HeroMaterialController>();
            var lod = root.AddComponent<HeroLODController>();
            lod.EnsureGroup();
            var vfx = root.AddComponent<HeroVfxController>();
            var audio = root.AddComponent<HeroAudioController>();
            var visual = root.AddComponent<HeroVisualController>();

            var modelRoot = new GameObject("Model");
            modelRoot.transform.SetParent(root.transform, false);

            var usingFallback = true;
            Transform modelInstance = null;
            if (modelSource != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(modelSource);
                if (instance == null)
                    instance = Object.Instantiate(modelSource);
                instance.name = "Vortex_Model";
                instance.transform.SetParent(modelRoot.transform, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                usingFallback = false;
                modelInstance = instance.transform;

                var sourceAnimator = instance.GetComponentInChildren<Animator>();
                if (sourceAnimator != null)
                {
                    if (sourceAnimator.avatar != null)
                        animator.avatar = sourceAnimator.avatar;
                    // Drive from root animator only.
                    sourceAnimator.applyRootMotion = false;
                    sourceAnimator.enabled = false;
                }

                NormalizeModelTransform(instance);
            }
            else
            {
                var dummyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HumanoidDummyFactory.PrefabPath);
                if (dummyPrefab == null)
                    dummyPrefab = HumanoidDummyPrefabBuilder.CreateOrUpdatePrefab();

                var dummy = (GameObject)PrefabUtility.InstantiatePrefab(dummyPrefab);
                dummy.name = "TechnicalFallback_HumanoidDummy";
                dummy.transform.SetParent(modelRoot.transform, false);
                dummy.transform.localPosition = Vector3.zero;
                dummy.transform.localRotation = Quaternion.identity;
                dummy.transform.localScale = Vector3.one;
                usingFallback = true;
            }

            materials.CaptureFromHierarchy();

            CreateSockets(root.transform, sockets, modelInstance);
            var weaponRoot = AttachWeapon(root.transform, sockets, usingFallback);

            var previewAnchor = new GameObject("PreviewAnchor");
            previewAnchor.transform.SetParent(root.transform, false);

            BuildSpecialVfx(root.transform, sockets, vfx);
            vfx.Bind(sockets);
            visual.Configure(
                VortexAssetPaths.HeroId,
                animator,
                sockets,
                vfx,
                audio,
                weaponRoot,
                previewAnchor.transform,
                usingFallback);

            HumanoidDummyFactory.SetLayerRecursive(root, HumanoidDummyFactory.ResolveLayer());

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, VortexAssetPaths.HeroPrefab);
            Object.DestroyImmediate(root);

            TryMarkAddressable(VortexAssetPaths.HeroPrefab, VortexAssetPaths.AddressablePrefabKey);
            BindCatalogVisual(usingFallback);

            var status = AssetDatabase.LoadAssetAtPath<VortexPipelineStatusSO>(VortexAssetPaths.PipelineStatus);
            if (status != null)
            {
                status.UsingTechnicalFallback = usingFallback;
                status.Phase = usingFallback
                    ? VortexPipelinePhase.WaitingForSourceModel
                    : VortexPipelinePhase.PrefabBuilt;
                status.PrefabPath = VortexAssetPaths.HeroPrefab;
                status.SourceModelPath = hasSource ? VortexAssetPaths.Lod0 : string.Empty;
                EditorUtility.SetDirty(status);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(usingFallback
                ? "Vortex_Hero construído em modo FALLBACK técnico (aguardo FBX real)."
                : "Vortex_Hero construído com modelo fonte importado (Humanoid + animações + espada).");
            return prefab;
        }

        public static void EnsureMaterials()
        {
            CreateUrpMat("MAT_Vortex_Skin", new Color(0.72f, 0.55f, 0.45f));
            CreateUrpMat("MAT_Vortex_Hair", new Color(0.08f, 0.07f, 0.07f));
            CreateUrpMat("MAT_Vortex_ArmorBlack", new Color(0.08f, 0.09f, 0.1f), metallic: 0.75f, smooth: 0.45f);
            CreateUrpMat("MAT_Vortex_ArmorGold", new Color(0.78f, 0.62f, 0.22f), metallic: 0.85f, smooth: 0.55f);
            CreateUrpMat("MAT_Vortex_Cloth", new Color(0.12f, 0.12f, 0.16f));
            CreateUrpMat("MAT_Vortex_Eyes", new Color(0.15f, 0.55f, 0.85f), emission: new Color(0.05f, 0.2f, 0.35f));
            CreateUrpMat("MAT_Vortex_Sword", new Color(0.55f, 0.55f, 0.6f), metallic: 0.9f, smooth: 0.65f);
        }

        public static AnimatorController EnsureAnimatorController()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(VortexAssetPaths.AnimatorController);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(VortexAssetPaths.AnimatorController);

            var root = controller.layers[0].stateMachine;
            foreach (var required in HeroAnimationIds.Required)
            {
                if (root.states.Any(s => s.state.name == required)) continue;
                root.AddState(required);
            }

            var idle = root.states.FirstOrDefault(s => s.state.name == HeroAnimationIds.Idle).state;
            if (idle != null) root.defaultState = idle;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        public static void BindAnimationClips(AnimatorController controller)
        {
            if (controller == null) return;
            if (!File.Exists(VortexAssetPaths.Lod0)) return;

            var clips = AssetDatabase.LoadAllAssetsAtPath(VortexAssetPaths.Lod0)
                .OfType<AnimationClip>()
                .Where(c => c != null && !c.name.StartsWith("__preview"))
                .ToArray();
            if (clips.Length == 0)
            {
                Debug.LogWarning("Vortex: nenhum AnimationClip encontrado em Vortex_LOD0.fbx.");
                return;
            }

            var root = controller.layers[0].stateMachine;
            var bound = 0;
            foreach (var child in root.states)
            {
                var state = child.state;
                var clip = FindClip(clips, state.name);
                if (clip == null) continue;
                state.motion = clip;
                bound++;
            }

            EditorUtility.SetDirty(controller);
            Debug.Log($"Vortex: {bound}/{HeroAnimationIds.Required.Length} clips ligados ao Animator.");
        }

        private static AnimationClip FindClip(AnimationClip[] clips, string stateName)
        {
            foreach (var clip in clips)
            {
                if (clip.name == stateName) return clip;
            }

            foreach (var clip in clips)
            {
                if (clip.name.EndsWith("|" + stateName) || clip.name.EndsWith("_" + stateName))
                    return clip;
            }

            foreach (var clip in clips)
            {
                if (clip.name.Contains(stateName))
                    return clip;
            }

            return null;
        }

        private static Transform AttachWeapon(Transform root, HeroSocketRegistry sockets, bool usingFallback)
        {
            var weaponRoot = new GameObject("WeaponRoot").transform;
            weaponRoot.SetParent(root, false);

            if (usingFallback)
            {
                var sword = new GameObject("Vortex_DragonSword");
                sword.transform.SetParent(weaponRoot, false);
                var swordMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                swordMarker.name = "SwordPlaceholder_NotFinalArt";
                swordMarker.transform.SetParent(sword.transform, false);
                swordMarker.transform.localScale = new Vector3(0.05f, 0.05f, 0.7f);
                swordMarker.transform.localPosition = new Vector3(0f, 0f, 0.35f);
                Object.DestroyImmediate(swordMarker.GetComponent<Collider>());
                var swordMat = AssetDatabase.LoadAssetAtPath<Material>(VortexAssetPaths.Materials + "/MAT_Vortex_Sword.mat");
                if (swordMat != null)
                    swordMarker.GetComponent<MeshRenderer>().sharedMaterial = swordMat;
            }
            else
            {
                var swordSrc = AssetDatabase.LoadAssetAtPath<GameObject>(VortexAssetPaths.DragonSword);
                if (swordSrc != null)
                {
                    var sword = (GameObject)PrefabUtility.InstantiatePrefab(swordSrc);
                    if (sword == null) sword = Object.Instantiate(swordSrc);
                    sword.name = "Vortex_DragonSword";
                    sword.transform.SetParent(weaponRoot, false);
                    sword.transform.localPosition = Vector3.zero;
                    sword.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                    sword.transform.localScale = Vector3.one;
                }
                else
                {
                    Debug.LogWarning($"Espada ausente: {VortexAssetPaths.DragonSword}");
                }
            }

            ParentWeaponToSocket(weaponRoot, sockets, HeroSocketIds.RightHand);
            return weaponRoot;
        }

        private static void ParentWeaponToSocket(Transform weaponRoot, HeroSocketRegistry sockets, string socketId)
        {
            var socket = sockets.Get(socketId);
            if (socket == null) return;
            weaponRoot.SetParent(socket, false);
            weaponRoot.localPosition = Vector3.zero;
            weaponRoot.localRotation = Quaternion.identity;
        }

        private static void BuildSpecialVfx(Transform root, HeroSocketRegistry sockets, HeroVfxController vfx)
        {
            var chest = sockets.Get(HeroSocketIds.ChestVfx) ?? root;
            var dragon = sockets.Get(HeroSocketIds.DragonLink) ?? chest;

            var aura = CreateLoopParticles(
                dragon,
                "VFX_DragonAura",
                new Color(1f, 0.55f, 0.12f, 0.85f),
                startSize: 0.35f,
                rate: 40f,
                radius: 0.55f);
            var runes = CreateLoopParticles(
                chest,
                "VFX_GoldenRunes",
                new Color(1f, 0.84f, 0.25f, 0.95f),
                startSize: 0.12f,
                rate: 24f,
                radius: 0.4f);

            vfx.Configure(aura, runes, HeroVfxController.DefaultSpecialAuraSeconds);
        }

        private static ParticleSystem CreateLoopParticles(
            Transform parent,
            string name,
            Color color,
            float startSize,
            float rate,
            float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = 1.2f;
            main.startSize = startSize;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 128;

            var emission = ps.emission;
            emission.rateOverTime = rate;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(new Color(1f, 0.9f, 0.5f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLife.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                             ?? Shader.Find("Particles/Standard Unlit")
                             ?? Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    renderer.sharedMaterial = new Material(shader) { color = color };
                }
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        private static void NormalizeModelTransform(GameObject instance)
        {
            if (instance == null) return;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            // Feet to origin, centered on XZ.
            var offset = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
            instance.transform.localPosition += offset;

            // Keep ~2.05 m standing height if importer scaled oddly.
            var height = bounds.size.y;
            if (height > 0.01f)
            {
                var target = VortexAssetPaths.TargetHeightMeters;
                var factor = target / height;
                if (factor > 0.01f && Mathf.Abs(factor - 1f) > 0.03f && Mathf.Abs(factor - 1f) < 0.5f)
                {
                    instance.transform.localScale *= factor;
                    // Re-center feet after scale
                    renderers = instance.GetComponentsInChildren<Renderer>(true);
                    bounds = renderers[0].bounds;
                    for (var i = 1; i < renderers.Length; i++)
                        bounds.Encapsulate(renderers[i].bounds);
                    offset = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
                    instance.transform.localPosition += offset;
                }
            }
        }

        private static void CreateSockets(Transform root, HeroSocketRegistry registry, Transform modelRoot)
        {
            foreach (var id in HeroSocketIds.Required)
            {
                var t = EnsureChild(root, id);
                var bone = FindBoneTransform(modelRoot, SocketBoneCandidates(id));
                if (bone != null)
                {
                    t.SetParent(bone, false);
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                }
                else
                {
                    t.localPosition = id switch
                    {
                        HeroSocketIds.RightHand => new Vector3(0.45f, 1.25f, 0.1f),
                        HeroSocketIds.LeftHand => new Vector3(-0.45f, 1.25f, 0.1f),
                        HeroSocketIds.BackWeapon => new Vector3(0f, 1.35f, -0.2f),
                        HeroSocketIds.HipWeapon => new Vector3(0.2f, 0.95f, 0.05f),
                        HeroSocketIds.HeadVfx => new Vector3(0f, 1.95f, 0f),
                        HeroSocketIds.ChestVfx => new Vector3(0f, 1.4f, 0.15f),
                        HeroSocketIds.FootLeftVfx => new Vector3(-0.12f, 0.05f, 0f),
                        HeroSocketIds.FootRightVfx => new Vector3(0.12f, 0.05f, 0f),
                        HeroSocketIds.DragonLink => new Vector3(0f, 1.5f, -0.35f),
                        _ => Vector3.zero
                    };
                }

                registry.Bind(id, t);
            }
        }

        private static string[] SocketBoneCandidates(string socketId) => socketId switch
        {
            HeroSocketIds.RightHand => new[] { "RightHand", "hand_r", "mixamorig:RightHand" },
            HeroSocketIds.LeftHand => new[] { "LeftHand", "hand_l", "mixamorig:LeftHand" },
            HeroSocketIds.BackWeapon => new[] { "UpperChest", "Chest", "Spine2" },
            HeroSocketIds.HipWeapon => new[] { "Hips", "hip" },
            HeroSocketIds.HeadVfx => new[] { "Head", "head" },
            HeroSocketIds.ChestVfx => new[] { "Chest", "Spine1", "UpperChest" },
            HeroSocketIds.FootLeftVfx => new[] { "LeftFoot", "foot_l" },
            HeroSocketIds.FootRightVfx => new[] { "RightFoot", "foot_r" },
            HeroSocketIds.DragonLink => new[] { "UpperChest", "Chest", "Spine" },
            _ => System.Array.Empty<string>()
        };

        private static Transform FindBoneTransform(Transform modelRoot, string[] candidates)
        {
            if (modelRoot == null || candidates == null || candidates.Length == 0) return null;
            var all = modelRoot.GetComponentsInChildren<Transform>(true);
            foreach (var candidate in candidates)
            {
                foreach (var t in all)
                {
                    if (t.name == candidate || t.name.EndsWith(candidate))
                        return t;
                }
            }

            return null;
        }

        private static Transform EnsureChild(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void CreateUrpMat(string name, Color color, float metallic = 0f, float smooth = 0.4f, Color? emission = null)
        {
            var path = VortexAssetPaths.Materials + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Valgor/Heroes/DummyUnlit")
                             ?? Shader.Find("Standard");
                mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smooth);
            if (emission.HasValue)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emission.Value);
                }
            }

            EditorUtility.SetDirty(mat);
        }

        private static void TryMarkAddressable(string assetPath, string address)
        {
            // Soft dependency: if Addressables settings are not initialized, skip with warning.
            var settingsType = System.Type.GetType(
                "UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");
            if (settingsType == null)
            {
                Debug.LogWarning(
                    $"Addressables Editor API indisponível. Marque manualmente '{assetPath}' com address '{address}'.");
                return;
            }

            var settingsProp = settingsType.GetProperty("Settings",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var settings = settingsProp?.GetValue(null);
            if (settings == null)
            {
                Debug.LogWarning(
                    $"Addressables Settings ausente. Crie via Window → Asset Management → Addressables. Address alvo: {address}");
                return;
            }

            Debug.Log($"Addressable key planejada para Vortex: {address} → {assetPath}");
        }

        private static void BindCatalogVisual(bool usingFallback)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Data.HeroCatalogSO>(
                "Assets/Valgor/Heroes/Data/Generated/HeroCatalog.asset");
            if (catalog == null) return;
            foreach (var hero in catalog.Heroes)
            {
                if (hero == null || hero.Id != VortexAssetPaths.HeroId) continue;
                // Keep gameplay fields untouched; only ensure addressable key is correct.
                if (hero.PrefabAddress != VortexAssetPaths.AddressablePrefabKey)
                {
                    hero.PrefabAddress = VortexAssetPaths.AddressablePrefabKey;
                    EditorUtility.SetDirty(hero);
                }

                EditorUtility.SetDirty(catalog);
                break;
            }
        }
    }
}
#endif
