using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Valgor.Editor
{
    /// <summary>
    /// Prefab Resources/Valgor/Castle_Tier1 com material URP/Lit + Base Map do GLB.
    /// </summary>
    public static class CastleTier1PrefabBuilder
    {
        public const string ModelPath = "Assets/Valgor/City/Art/Castle/Models/Castle_Tier1.fbx";
        public const string PrefabPath = "Assets/Valgor/City/Art/Castle/Prefabs/Castle_Tier1_Visual.prefab";
        public const string ResourcesPrefabPath = "Assets/Valgor/City/Art/Castle/Resources/Valgor/Castle_Tier1.prefab";
        public const string ResourcesKey = "Valgor/Castle_Tier1";

        public const string TextureSourceDisk =
            @"C:\Valgor_Studio\production\City\Castle\unity_staging\Textures\fantasy_castle_3d_model_basecolor.jpg";

        public const string TextureAssetPath =
            "Assets/Valgor/City/Art/Castle/Textures/Castle_Tier1_BaseColor.jpg";

        public const string MaterialAssetPath =
            "Assets/Valgor/City/Art/Castle/Materials/M_Castle_Tier1_URP.mat";

        /// <summary>Roughness constante do Tripo (sem mapa) → Smoothness = 1 - Roughness.</summary>
        public const float SourceRoughness = 0.5f;

        [MenuItem("Valgor/City/Castle/Build Tier1 Prefab")]
        public static void BuildFromMenu()
        {
            var ok = Build(out var msg);
            EditorUtility.DisplayDialog("Castle Tier1 Prefab", msg, "OK");
            if (!ok)
            {
                Debug.LogError(msg);
            }
        }

        /// <summary>CLI: -executeMethod Valgor.Editor.CastleTier1PrefabBuilder.BuildCli</summary>
        public static void BuildCli()
        {
            var code = Build(out var msg) ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] Castle Tier1 prefab OK: {msg}");
            }
            else
            {
                Debug.LogError($"[Valgor] Castle Tier1 prefab FAIL: {msg}");
            }

            EditorApplication.Exit(code);
        }

        public static bool Build(out string message)
        {
            EnsureFolders();
            if (!EnsureBaseColorTexture(out var texMsg))
            {
                message = texMsg;
                return false;
            }

            AssetDatabase.Refresh();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                message = $"Model missing: {ModelPath}";
                return false;
            }

            var urpMat = EnsureUrpLitMaterial(out var matMsg);
            if (urpMat == null)
            {
                message = matMsg;
                return false;
            }

            var root = new GameObject("Castle_Tier1_Visual");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            if (instance == null)
            {
                instance = Object.Instantiate(model);
            }

            instance.name = "Model";
            instance.transform.SetParent(root.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(col);
            }

            var rendererCount = 0;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                {
                    continue;
                }

                var slotCount = Mathf.Max(1, renderer.sharedMaterials.Length);
                var mats = new Material[slotCount];
                for (var i = 0; i < slotCount; i++)
                {
                    mats[i] = urpMat;
                }

                renderer.sharedMaterials = mats;
                rendererCount++;
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, ResourcesPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var loaded = Resources.Load<GameObject>(ResourcesKey);
            var smoothness = 1f - SourceRoughness;
            message = loaded != null
                ? $"{PrefabPath} + Resources OK | URP Lit BaseMap | Metallic=0 Smoothness={smoothness:0.##} | renderers={rendererCount}"
                : $"{PrefabPath} saved; Resources.Load pending domain reload | mat={MaterialAssetPath}";
            Debug.Log($"[Valgor] Castle materials: {matMsg}");
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;
        }

        private static bool EnsureBaseColorTexture(out string message)
        {
            EnsureFolder("Assets/Valgor/City/Art/Castle/Textures");
            var absDest = Path.GetFullPath(Path.Combine(Application.dataPath, "Valgor/City/Art/Castle/Textures/Castle_Tier1_BaseColor.jpg"));
            Directory.CreateDirectory(Path.GetDirectoryName(absDest)!);

            if (!File.Exists(TextureSourceDisk))
            {
                message = $"Base color missing on disk: {TextureSourceDisk}";
                return false;
            }

            File.Copy(TextureSourceDisk, absDest, overwrite: true);
            AssetDatabase.ImportAsset(TextureAssetPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(TextureAssetPath) as TextureImporter;
            if (importer != null)
            {
                importer.sRGBTexture = true;
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 4096;
                importer.SaveAndReimport();
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureAssetPath);
            message = tex != null
                ? $"BaseColor OK {TextureAssetPath} {tex.width}x{tex.height}"
                : $"BaseColor import failed: {TextureAssetPath}";
            return tex != null;
        }

        private static Material EnsureUrpLitMaterial(out string message)
        {
            EnsureFolder("Assets/Valgor/City/Art/Castle/Materials");
            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureAssetPath);
            if (baseMap == null)
            {
                message = "BaseMap texture not loaded";
                return null!;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                message = "URP Lit shader not found";
                return null;
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialAssetPath);
            if (mat == null)
            {
                mat = new Material(shader) { name = "M_Castle_Tier1_URP" };
                AssetDatabase.CreateAsset(mat, MaterialAssetPath);
            }
            else
            {
                mat.shader = shader;
            }

            // Tripo: only Base Color map. Metallic=0, Roughness=0.5 → Smoothness=0.5
            var smoothness = 1f - SourceRoughness;
            mat.SetColor("_BaseColor", Color.white);
            mat.SetTexture("_BaseMap", baseMap);
            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0f);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", smoothness);
            }

            // No normal / occlusion / emission maps in source GLB.
            if (mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", null);
            }

            if (mat.HasProperty("_OcclusionMap"))
            {
                mat.SetTexture("_OcclusionMap", null);
            }

            if (mat.HasProperty("_EmissionMap"))
            {
                mat.SetTexture("_EmissionMap", null);
            }

            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", Color.black);
            }

            mat.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            message =
                $"URP/Lit {MaterialAssetPath} BaseMap={TextureAssetPath} " +
                $"Metallic=0 Roughness={SourceRoughness} Smoothness={smoothness} " +
                "Normal=none Occlusion=none Emission=none";
            return mat;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Valgor/City/Art");
            EnsureFolder("Assets/Valgor/City/Art/Castle");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Models");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Prefabs");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Textures");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Materials");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Resources");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Resources/Valgor");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = path.Substring(0, path.LastIndexOf('/'));
            var name = path.Substring(path.LastIndexOf('/') + 1);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
