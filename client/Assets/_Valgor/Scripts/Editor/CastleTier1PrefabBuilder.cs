using UnityEditor;
using UnityEngine;

namespace Valgor.Editor
{
    /// <summary>
    /// Gera o prefab Resources/Valgor/Castle_Tier1 a partir do FBX staged.
    /// </summary>
    public static class CastleTier1PrefabBuilder
    {
        public const string ModelPath = "Assets/Valgor/City/Art/Castle/Models/Castle_Tier1.fbx";
        public const string PrefabPath = "Assets/Valgor/City/Art/Castle/Prefabs/Castle_Tier1_Visual.prefab";
        public const string ResourcesPrefabPath = "Assets/Valgor/City/Art/Castle/Resources/Valgor/Castle_Tier1.prefab";
        public const string ResourcesKey = "Valgor/Castle_Tier1";

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
            AssetDatabase.Refresh();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                message = $"Model missing: {ModelPath}";
                return false;
            }

            EnsureFolders();

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

            // URP-safe materials when importer left Legacy/broken.
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                {
                    continue;
                }

                var mats = renderer.sharedMaterials;
                var changed = false;
                for (var i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null || mats[i].shader == null ||
                        mats[i].shader.name.Contains("Hidden/InternalError", System.StringComparison.Ordinal))
                    {
                        var color = new Color(0.56f, 0.53f, 0.48f);
                        var tex = mats[i] != null && mats[i].HasProperty("_MainTex")
                            ? mats[i].mainTexture as Texture2D
                            : null;
                        if (tex == null && mats[i] != null && mats[i].HasProperty("_BaseMap"))
                        {
                            tex = mats[i].GetTexture("_BaseMap") as Texture2D;
                        }

                        var safe = new Material(Shader.Find("Universal Render Pipeline/Lit")
                                                ?? Shader.Find("Sprites/Default")
                                                ?? Shader.Find("Standard"));
                        safe.name = "M_Castle_Atlas_Runtime";
                        if (safe.HasProperty("_BaseColor"))
                        {
                            safe.SetColor("_BaseColor", color);
                        }

                        if (safe.HasProperty("_Color"))
                        {
                            safe.SetColor("_Color", color);
                        }

                        if (tex != null)
                        {
                            if (safe.HasProperty("_BaseMap"))
                            {
                                safe.SetTexture("_BaseMap", tex);
                            }

                            if (safe.HasProperty("_MainTex"))
                            {
                                safe.mainTexture = tex;
                            }
                        }

                        mats[i] = safe;
                        changed = true;
                    }
                }

                if (changed)
                {
                    renderer.sharedMaterials = mats;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, ResourcesPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var loaded = Resources.Load<GameObject>(ResourcesKey);
            message = loaded != null
                ? $"{PrefabPath} + Resources '{ResourcesKey}' OK"
                : $"{PrefabPath} saved but Resources.Load failed (may need domain reload)";
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Valgor/City/Art");
            EnsureFolder("Assets/Valgor/City/Art/Castle");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Models");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Prefabs");
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
