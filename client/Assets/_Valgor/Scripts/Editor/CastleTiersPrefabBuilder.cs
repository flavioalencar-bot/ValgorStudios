using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Valgor.Editor
{
    /// <summary>
    /// Gera prefabs Resources/Valgor/Castle_Tier1..6 com URP/Lit + BaseMap.
    /// </summary>
    public static class CastleTiersPrefabBuilder
    {
        public const string StagingTextures =
            @"C:\Valgor_Studio\production\City\Castle\unity_staging\Textures";

        public const float SourceRoughness = 0.5f;

        [MenuItem("Valgor/City/Castle/Build All Tier Prefabs")]
        public static void BuildFromMenu()
        {
            var ok = BuildAll(out var msg);
            EditorUtility.DisplayDialog("Castle Tiers Prefabs", msg, "OK");
            if (!ok)
            {
                Debug.LogError(msg);
            }
        }

        /// <summary>CLI: -executeMethod Valgor.Editor.CastleTiersPrefabBuilder.BuildCli</summary>
        public static void BuildCli()
        {
            var code = BuildAll(out var msg) ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] Castle tiers prefabs OK: {msg}");
            }
            else
            {
                Debug.LogError($"[Valgor] Castle tiers prefabs FAIL: {msg}");
            }

            EditorApplication.Exit(code);
        }

        public static bool BuildAll(out string message)
        {
            EnsureFolders();
            var okCount = 0;
            var parts = new System.Text.StringBuilder();
            for (var tier = 1; tier <= 6; tier++)
            {
                if (BuildTier(tier, out var tierMsg))
                {
                    okCount++;
                    parts.AppendLine($"T{tier}: OK — {tierMsg}");
                }
                else
                {
                    parts.AppendLine($"T{tier}: FAIL — {tierMsg}");
                    Debug.LogError($"[Valgor] Castle Tier{tier} prefab FAIL: {tierMsg}");
                }
            }

            message = $"{okCount}/6 tiers\n{parts}";
            return okCount > 0;
        }

        public static bool BuildTier(int tier, out string message)
        {
            tier = Mathf.Clamp(tier, 1, 6);
            EnsureFolders();

            var modelPath = $"Assets/Valgor/City/Art/Castle/Models/Castle_Tier{tier}.fbx";
            var textureAssetPath = $"Assets/Valgor/City/Art/Castle/Textures/Castle_Tier{tier}_BaseColor.jpg";
            var materialAssetPath = $"Assets/Valgor/City/Art/Castle/Materials/M_Castle_Tier{tier}_URP.mat";
            var prefabPath = $"Assets/Valgor/City/Art/Castle/Prefabs/Castle_Tier{tier}_Visual.prefab";
            var resourcesPrefabPath = $"Assets/Valgor/City/Art/Castle/Resources/Valgor/Castle_Tier{tier}.prefab";
            var resourcesKey = $"Valgor/Castle_Tier{tier}";
            var diskTex = Path.Combine(StagingTextures, $"Castle_Tier{tier}_BaseColor.jpg");

            if (!EnsureBaseColorTexture(diskTex, textureAssetPath, out var texMsg))
            {
                message = texMsg;
                return false;
            }

            AssetDatabase.Refresh();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                message = $"Model missing: {modelPath}";
                return false;
            }

            var urpMat = EnsureUrpLitMaterial(tier, textureAssetPath, materialAssetPath, out var matMsg);
            if (urpMat == null)
            {
                message = matMsg;
                return false;
            }

            var root = new GameObject($"Castle_Tier{tier}_Visual");
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

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, resourcesPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var loaded = Resources.Load<GameObject>(resourcesKey);
            var smoothness = 1f - SourceRoughness;
            message = loaded != null
                ? $"{prefabPath} + Resources OK | URP Lit | Metallic=0 Smoothness={smoothness:0.##} | renderers={rendererCount} | {matMsg}"
                : $"{prefabPath} saved; Resources pending reload | {matMsg}";
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
        }

        private static bool EnsureBaseColorTexture(string diskSource, string textureAssetPath, out string message)
        {
            EnsureFolder("Assets/Valgor/City/Art/Castle/Textures");
            var absDest = Path.GetFullPath(Path.Combine(Application.dataPath, "Valgor/City/Art/Castle/Textures", Path.GetFileName(textureAssetPath)));
            Directory.CreateDirectory(Path.GetDirectoryName(absDest)!);

            if (!File.Exists(diskSource))
            {
                // Fallback: already in Assets
                if (!File.Exists(absDest))
                {
                    message = $"Base color missing: {diskSource}";
                    return false;
                }
            }
            else
            {
                File.Copy(diskSource, absDest, overwrite: true);
            }

            AssetDatabase.ImportAsset(textureAssetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
            if (importer != null)
            {
                importer.sRGBTexture = true;
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 4096;
                importer.SaveAndReimport();
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
            message = tex != null
                ? $"BaseColor OK {textureAssetPath} {tex.width}x{tex.height}"
                : $"BaseColor import failed: {textureAssetPath}";
            return tex != null;
        }

        private static Material EnsureUrpLitMaterial(
            int tier,
            string textureAssetPath,
            string materialAssetPath,
            out string message)
        {
            EnsureFolder("Assets/Valgor/City/Art/Castle/Materials");
            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
            if (baseMap == null)
            {
                message = "BaseMap texture not loaded";
                return null!;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                message = "URP Lit shader not found";
                return null!;
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
            if (mat == null)
            {
                mat = new Material(shader) { name = $"M_Castle_Tier{tier}_URP" };
                AssetDatabase.CreateAsset(mat, materialAssetPath);
            }
            else
            {
                mat.shader = shader;
            }

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
                $"URP/Lit {materialAssetPath} BaseMap={textureAssetPath} " +
                $"Metallic=0 Roughness={SourceRoughness} Smoothness={smoothness}";
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
