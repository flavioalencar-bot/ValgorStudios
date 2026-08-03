using System.IO;
using UnityEditor;
using UnityEngine;
using Valgor.City.Visual;

namespace Valgor.Editor
{
    /// <summary>
    /// Sprint 1 — gera kit modular da muralha com material do WallGate.
    /// Prefabs em Prefabs/Modules + cópia Material em Resources.
    /// </summary>
    public static class CapitalFortressWallKitBuilder
    {
        public const string MaterialPath =
            "Assets/Art/Capital/Architecture/Walls/Materials/M_VAL_Capital_Architecture_WallGate.mat";
        public const string ResourcesMaterialPath =
            "Assets/Art/Capital/Resources/Valgor/M_VAL_Capital_Architecture_WallGate.mat";
        public const string ModulesPrefabFolder =
            "Assets/Art/Capital/Architecture/Walls/Prefabs/Modules";
        public const string LodFolder =
            "Assets/Art/Capital/Architecture/Walls/LOD";
        public const string ResourcesModulesFolder =
            "Assets/Art/Capital/Resources/Valgor/WallModules";

        [MenuItem("Valgor/Art/Capital/Build Fortress Wall Kit (Sprint 1)")]
        public static void BuildFromMenu()
        {
            var ok = BuildAll(out var msg);
            EditorUtility.DisplayDialog("Fortress Wall Kit", msg, "OK");
            if (!ok)
            {
                Debug.LogError(msg);
            }
        }

        /// <summary>CLI: -executeMethod Valgor.Editor.CapitalFortressWallKitBuilder.BuildCli</summary>
        public static void BuildCli()
        {
            var code = BuildAll(out var msg) ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] Fortress Wall Kit OK: {msg}");
            }
            else
            {
                Debug.LogError($"[Valgor] Fortress Wall Kit FAIL: {msg}");
            }

            EditorApplication.Exit(code);
        }

        public static bool BuildAll(out string message)
        {
            EnsureFolders();
            if (!File.Exists(Abs(MaterialPath)))
            {
                message = $"Material WallGate ausente: {MaterialPath}. Rode o builder do Portão antes.";
                return false;
            }

            if (!EnsureResourcesMaterial(out var matMsg))
            {
                message = matMsg;
                return false;
            }

            var built = 0;
            foreach (var id in CityFortressWallConfig.ModuleIds)
            {
                if (id == "WallGate")
                {
                    // Prefab do portão já existe — só registra LOD0 pointer.
                    EnsureLodPlaceholder(id);
                    built++;
                    continue;
                }

                var tempRoot = new GameObject("_KitBakeRoot");
                var module = CityFortressWallModuleFactory.Create(id, tempRoot.transform);
                module.transform.SetParent(null, false);
                Object.DestroyImmediate(tempRoot);

                // Aplicar material real do WallGate em pedras.
                var gateMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                foreach (var renderer in module.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null)
                    {
                        continue;
                    }

                    var n = renderer.gameObject.name;
                    if (n.IndexOf("Walk", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("Deck", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }

                    if (n.IndexOf("Flag", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("Flame", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("Pole", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }

                    renderer.sharedMaterial = gateMat;
                }

                var prefabPath = $"{ModulesPrefabFolder}/VAL_Capital_{id}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                {
                    AssetDatabase.DeleteAsset(prefabPath);
                }

                PrefabUtility.SaveAsPrefabAsset(module, prefabPath);
                Object.DestroyImmediate(module);

                // Resources (runtime load opcional).
                var resPath = $"{ResourcesModulesFolder}/{id}.prefab";
                var saved = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (saved != null)
                {
                    if (AssetDatabase.LoadAssetAtPath<GameObject>(resPath) != null)
                    {
                        AssetDatabase.DeleteAsset(resPath);
                    }

                    PrefabUtility.SaveAsPrefabAsset(saved, resPath);
                }

                EnsureLodPlaceholder(id);
                built++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            message =
                $"Kit {CityFortressWallConfig.KitId}: {built}/{CityFortressWallConfig.ModuleCount} módulos | {matMsg} | LOD0–3 pastas";
            return built > 0;
        }

        private static bool EnsureResourcesMaterial(out string message)
        {
            var source = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (source == null)
            {
                message = "Não carregou material fonte.";
                return false;
            }

            // Copia asset para Resources (mesmo conteúdo).
            if (AssetDatabase.LoadAssetAtPath<Material>(ResourcesMaterialPath) != null)
            {
                AssetDatabase.DeleteAsset(ResourcesMaterialPath);
            }

            if (!AssetDatabase.CopyAsset(MaterialPath, ResourcesMaterialPath))
            {
                // Fallback: cria material novo apontando mesma textura.
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/Art/Capital/Architecture/Walls/Textures/VAL_Capital_Architecture_WallGate_BaseColor.jpg");
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var mat = new Material(shader) { name = "M_VAL_Capital_Architecture_WallGate" };
                if (tex != null && mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                    mat.SetColor("_BaseColor", Color.white);
                }

                AssetDatabase.CreateAsset(mat, ResourcesMaterialPath);
            }

            message = $"Resources material OK: {ResourcesMaterialPath}";
            return AssetDatabase.LoadAssetAtPath<Material>(ResourcesMaterialPath) != null;
        }

        private static void EnsureLodPlaceholder(string moduleId)
        {
            for (var lod = 0; lod < CityFortressWallConfig.LodLevels; lod++)
            {
                var folder = $"{LodFolder}/LOD{lod}";
                EnsureFolder(folder);
                var marker = $"{folder}/{moduleId}.lod.txt";
                var abs = Abs(marker);
                Directory.CreateDirectory(Path.GetDirectoryName(abs)!);
                if (!File.Exists(abs))
                {
                    File.WriteAllText(abs, $"LOD{lod} placeholder for {moduleId} — use LOD0 mesh until simplified.");
                    AssetDatabase.ImportAsset(marker);
                }
            }
        }

        private static string Abs(string assetPath) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Art/Capital/Architecture/Walls/Prefabs");
            EnsureFolder(ModulesPrefabFolder);
            EnsureFolder(LodFolder);
            EnsureFolder("Assets/Art/Capital/Resources/Valgor");
            EnsureFolder(ResourcesModulesFolder);
            for (var lod = 0; lod < CityFortressWallConfig.LodLevels; lod++)
            {
                EnsureFolder($"{LodFolder}/LOD{lod}");
            }
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
