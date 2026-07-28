using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Valgor.Editor
{
    /// <summary>
    /// Prefabs a partir do GLB ORIGINAL Tripo (source/), sem reexport Blender.
    /// Escala/pivô só no Transform — preserva UV/malha/BaseColor 1:1.
    /// </summary>
    public static class CastleTiersPrefabBuilder
    {
        public const string ModelsFolder = "Assets/Valgor/City/Art/Castle/Models";
        public const string MaterialsFolder = "Assets/Valgor/City/Art/Castle/Materials";
        public const string PrefabsFolder = "Assets/Valgor/City/Art/Castle/Prefabs";
        public const string ResourcesFolder = "Assets/Valgor/City/Art/Castle/Resources/Valgor";

        public const string SourceGlb =
            @"C:\Valgor_Studio\production\City\Castle\source";

        /// <summary>Footprints alvo (m) aplicados no Transform do prefab.</summary>
        public static readonly float[] TargetFootprint =
        {
            0f, 7.5f, 8.1f, 8.7f, 9.4f, 10.1f, 10.8f
        };

        public const float SourceRoughness = 0.5f;

        [MenuItem("Valgor/City/Castle/Build All Tier Prefabs (GLB/glTFast)")]
        public static void BuildFromMenu()
        {
            var ok = BuildAll(out var msg);
            EditorUtility.DisplayDialog("Castle Tiers Prefabs (GLB)", msg, "OK");
            if (!ok)
            {
                Debug.LogError(msg);
            }
        }

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
            if (!CopyOriginalGlbs(out var copyMsg))
            {
                message = copyMsg;
                return false;
            }

            AssetDatabase.Refresh();
            ConfigureGlbImporters();

            var okCount = 0;
            var parts = new StringBuilder();
            parts.AppendLine(copyMsg);
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

            message = $"{okCount}/6 tiers | importer=glTFast | source=original Tripo GLB (no Blender re-export)\n{parts}";
            WriteDiagnosisSidecar(okCount, message);
            return okCount == 6;
        }

        public static bool BuildTier(int tier, out string message)
        {
            tier = Mathf.Clamp(tier, 1, 6);
            EnsureFolders();

            var modelPath = $"{ModelsFolder}/Castle_Tier{tier}.glb";
            var prefabPath = $"{PrefabsFolder}/Castle_Tier{tier}_Visual.prefab";
            var resourcesPrefabPath = $"{ResourcesFolder}/Castle_Tier{tier}.prefab";
            var resourcesKey = $"Valgor/Castle_Tier{tier}";

            // Remove FBX legado que corrompe UV (não pode coexistir no builder).
            var legacyFbx = $"{ModelsFolder}/Castle_Tier{tier}.fbx";
            if (AssetDatabase.LoadAssetAtPath<Object>(legacyFbx) != null)
            {
                AssetDatabase.DeleteAsset(legacyFbx);
            }

            // Prefabs antigos com PrefabInstance→FBX devem ser apagados (SaveAsPrefabAsset pode preservar o link).
            if (AssetDatabase.LoadAssetAtPath<Object>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            if (AssetDatabase.LoadAssetAtPath<Object>(resourcesPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(resourcesPrefabPath);
            }

            AssetDatabase.Refresh();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                message = $"GLB missing/not imported: {modelPath}";
                return false;
            }

            var modelAssetPath = AssetDatabase.GetAssetPath(model);
            if (!modelAssetPath.EndsWith(".glb", System.StringComparison.OrdinalIgnoreCase))
            {
                message = $"Expected GLB but loaded: {modelAssetPath}";
                return false;
            }

            var root = new GameObject($"Castle_Tier{tier}_Visual");
            // Instantiate (não PrefabUtility) — evita nested PrefabInstance preso no FBX antigo.
            var instance = Object.Instantiate(model);
            instance.name = "Model";
            instance.transform.SetParent(root.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(col);
            }

            // Escala progressiva + base no chão SEM alterar vértices/UV.
            NormalizeTransformOnly(root.transform, TargetFootprint[tier], out var appliedScale, out var footprint);

            // Preferir materiais glTFast intactos; só converte se magenta/erro.
            var matReport = EnsureMaterialsValidPreserve(root, tier, out var slotCount, out var submeshCount);
            var rendererCount = root.GetComponentsInChildren<Renderer>(true).Length;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, resourcesPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var loaded = Resources.Load<GameObject>(resourcesKey);
            message = loaded != null
                ? $"{modelPath} → Resources OK | scale={appliedScale:0.###} fp≈{footprint:0.##}m | slots={slotCount} submeshes={submeshCount} renderers={rendererCount} | {matReport}"
                : $"{prefabPath} saved; Resources pending | {matReport}";
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
        }

        private static bool CopyOriginalGlbs(out string message)
        {
            EnsureFolders();
            var copied = 0;
            for (var tier = 1; tier <= 6; tier++)
            {
                var src = Path.Combine(SourceGlb, $"Castle_Tier{tier}.glb");
                if (!File.Exists(src))
                {
                    message = $"Missing source GLB: {src}";
                    return false;
                }

                var destAbs = Path.GetFullPath(Path.Combine(Application.dataPath, "Valgor/City/Art/Castle/Models", $"Castle_Tier{tier}.glb"));
                Directory.CreateDirectory(Path.GetDirectoryName(destAbs)!);
                File.Copy(src, destAbs, overwrite: true);
                copied++;
            }

            message = $"Copied {copied}/6 ORIGINAL Tripo GLBs (no Blender bake) → {ModelsFolder}";
            return true;
        }

        private static void ConfigureGlbImporters()
        {
            for (var tier = 1; tier <= 6; tier++)
            {
                var path = $"{ModelsFolder}/Castle_Tier{tier}.glb";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(path);
                if (importer == null)
                {
                    continue;
                }

                var so = new SerializedObject(importer);
                TrySetBool(so, "m_Readable", false);
                TrySetInt(so, "m_MeshCompression", 0);
                TrySetBool(so, "m_GenerateSecondaryUV", false);
                TrySetBool(so, "m_SwapUVChannels", false);
                so.ApplyModifiedPropertiesWithoutUndo();
                importer.SaveAndReimport();
            }
        }

        private static void TrySetBool(SerializedObject so, string prop, bool value)
        {
            var p = so.FindProperty(prop);
            if (p != null && p.propertyType == SerializedPropertyType.Boolean)
            {
                p.boolValue = value;
            }
        }

        private static void TrySetInt(SerializedObject so, string prop, int value)
        {
            var p = so.FindProperty(prop);
            if (p != null && p.propertyType == SerializedPropertyType.Integer)
            {
                p.intValue = value;
            }
        }

        /// <summary>Uniform scale + Y snap. Não toca na malha.</summary>
        private static void NormalizeTransformOnly(
            Transform root,
            float targetFootprint,
            out float appliedScale,
            out float footprint)
        {
            appliedScale = 1f;
            footprint = 0f;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var rawFp = Mathf.Max(bounds.size.x, bounds.size.z);
            if (rawFp < 1e-4f)
            {
                return;
            }

            appliedScale = targetFootprint / rawFp;
            root.localScale = Vector3.one * appliedScale;

            // Recompute after scale
            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            footprint = Mathf.Max(bounds.size.x, bounds.size.z);
            var deltaY = -bounds.min.y;
            if (Mathf.Abs(deltaY) > 0.0001f)
            {
                foreach (Transform child in root)
                {
                    child.localPosition += new Vector3(0f, deltaY / Mathf.Max(appliedScale, 1e-6f), 0f);
                }
            }
        }

        /// <summary>
        /// Mantém materiais do glTFast. Só substitui por URP/Lit se shader inválido/magenta.
        /// Copia BaseMap + scale/offset do slot original.
        /// </summary>
        private static string EnsureMaterialsValidPreserve(
            GameObject root,
            int tier,
            out int slotCount,
            out int submeshCount)
        {
            slotCount = 0;
            submeshCount = 0;
            var urp = Shader.Find("Universal Render Pipeline/Lit");
            var converted = 0;
            var kept = 0;

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                {
                    continue;
                }

                var mf = renderer.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    submeshCount += mf.sharedMesh.subMeshCount;
                }

                var shared = renderer.sharedMaterials;
                if (shared == null || shared.Length == 0)
                {
                    continue;
                }

                var next = new Material[shared.Length];
                var changed = false;
                for (var i = 0; i < shared.Length; i++)
                {
                    slotCount++;
                    var src = shared[i];
                    if (src != null && src.shader != null &&
                        !src.shader.name.Contains("InternalError") &&
                        !src.shader.name.Contains("Hidden/InternalError"))
                    {
                        next[i] = src;
                        kept++;
                        continue;
                    }

                    if (urp == null)
                    {
                        next[i] = src;
                        continue;
                    }

                    var matPath = $"{MaterialsFolder}/M_Castle_Tier{tier}_Slot{i}_URP.mat";
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (mat == null)
                    {
                        mat = new Material(urp) { name = $"M_Castle_Tier{tier}_Slot{i}_URP" };
                        AssetDatabase.CreateAsset(mat, matPath);
                    }
                    else
                    {
                        mat.shader = urp;
                    }

                    var baseMap = ExtractBaseMap(src);
                    mat.SetColor("_BaseColor", Color.white);
                    if (baseMap != null)
                    {
                        mat.SetTexture("_BaseMap", baseMap);
                        if (src != null)
                        {
                            // Copia ST se existir no material de origem
                            foreach (var n in new[] { "_BaseMap", "_MainTex" })
                            {
                                if (src.HasProperty(n))
                                {
                                    mat.SetTextureScale("_BaseMap", src.GetTextureScale(n));
                                    mat.SetTextureOffset("_BaseMap", src.GetTextureOffset(n));
                                    break;
                                }
                            }
                        }
                    }

                    if (mat.HasProperty("_Metallic"))
                    {
                        mat.SetFloat("_Metallic", 0f);
                    }

                    if (mat.HasProperty("_Smoothness"))
                    {
                        mat.SetFloat("_Smoothness", 1f - SourceRoughness);
                    }

                    EditorUtility.SetDirty(mat);
                    next[i] = mat;
                    converted++;
                    changed = true;
                }

                if (changed)
                {
                    renderer.sharedMaterials = next;
                }
            }

            AssetDatabase.SaveAssets();
            return $"kept={kept} convertedFallback={converted} (glTFast materials preferred)";
        }

        private static Texture ExtractBaseMap(Material src)
        {
            if (src == null)
            {
                return null;
            }

            foreach (var name in new[] { "_BaseMap", "_MainTex", "baseColorTexture", "_BaseColorMap", "_albedo", "_Albedo" })
            {
                if (src.HasProperty(name) && src.GetTexture(name) != null)
                {
                    return src.GetTexture(name);
                }
            }

            var ids = src.GetTexturePropertyNameIDs();
            foreach (var id in ids)
            {
                var tex = src.GetTexture(id);
                if (tex != null)
                {
                    return tex;
                }
            }

            return null;
        }

        private static void WriteDiagnosisSidecar(int okCount, string message)
        {
            var abs = Path.GetFullPath(Path.Combine(Application.dataPath, "../..", "docs/releases/beta-0.2.4-evidence"));
            Directory.CreateDirectory(abs);
            File.WriteAllText(Path.Combine(abs, "prefab-build-log.txt"), $"ok={okCount}\n{message}\n", Encoding.UTF8);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Valgor/City/Art");
            EnsureFolder("Assets/Valgor/City/Art/Castle");
            EnsureFolder(ModelsFolder);
            EnsureFolder(PrefabsFolder);
            EnsureFolder("Assets/Valgor/City/Art/Castle/Textures");
            EnsureFolder(MaterialsFolder);
            EnsureFolder("Assets/Valgor/City/Art/Castle/Resources");
            EnsureFolder(ResourcesFolder);
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
