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

                var sourceAnimator = instance.GetComponent<Animator>();
                if (sourceAnimator != null && sourceAnimator.avatar != null)
                    animator.avatar = sourceAnimator.avatar;
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
                dummy.transform.localScale = Vector3.one * 0.88f;
                usingFallback = true;
            }

            materials.CaptureFromHierarchy();

            var weaponRoot = new GameObject("WeaponRoot");
            weaponRoot.transform.SetParent(root.transform, false);
            var sword = new GameObject("Vortex_DragonSword");
            sword.transform.SetParent(weaponRoot.transform, false);
            // Placeholder mesh marker only — not a final art sword.
            var swordMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            swordMarker.name = "SwordPlaceholder_NotFinalArt";
            swordMarker.transform.SetParent(sword.transform, false);
            swordMarker.transform.localScale = new Vector3(0.05f, 0.05f, 0.7f);
            swordMarker.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            Object.DestroyImmediate(swordMarker.GetComponent<Collider>());
            var swordMat = AssetDatabase.LoadAssetAtPath<Material>(VortexAssetPaths.Materials + "/MAT_Vortex_Sword.mat");
            if (swordMat != null)
                swordMarker.GetComponent<MeshRenderer>().sharedMaterial = swordMat;

            var previewAnchor = new GameObject("PreviewAnchor");
            previewAnchor.transform.SetParent(root.transform, false);

            CreateSockets(root.transform, sockets);
            sockets.Bind(HeroSocketIds.RightHand, EnsureChild(root.transform, HeroSocketIds.RightHand));
            // Attach sword to right hand by default.
            var rightHand = sockets.Get(HeroSocketIds.RightHand);
            if (rightHand != null)
            {
                weaponRoot.transform.SetParent(rightHand, false);
                weaponRoot.transform.localPosition = Vector3.zero;
                weaponRoot.transform.localRotation = Quaternion.identity;
            }

            vfx.Bind(sockets);
            visual.Configure(
                VortexAssetPaths.HeroId,
                animator,
                sockets,
                vfx,
                audio,
                weaponRoot.transform,
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
                EditorUtility.SetDirty(status);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(usingFallback
                ? "Vortex_Hero construído em modo FALLBACK técnico (aguardo FBX real)."
                : "Vortex_Hero construído com modelo fonte importado.");
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

        private static void CreateSockets(Transform root, HeroSocketRegistry registry)
        {
            foreach (var id in HeroSocketIds.Required)
            {
                var t = EnsureChild(root, id);
                // Approximate humanoid socket offsets for fallback preview.
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
                registry.Bind(id, t);
            }
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
