using System.IO;
using UnityEditor;
using UnityEngine;
using Valgor.City.Visual;

namespace Valgor.Editor
{
    /// <summary>
    /// Prefab + material + catálogo para VAL_Capital_Architecture_Castle_Import_v1.
    /// Fallback: se falhar, loader Resources / proxy permanecem.
    /// </summary>
    public static class CapitalCastleImportV1PrefabBuilder
    {
        public const string FbxPath =
            "Assets/Art/Capital/Architecture/Castle/FBX/VAL_Capital_Architecture_Castle_Import_v1.fbx";
        public const string TexturePath =
            "Assets/Art/Capital/Architecture/Castle/Textures/VAL_Capital_Architecture_Castle_Import_v1_BaseColor.jpg";
        public const string MaterialPath =
            "Assets/Art/Capital/Architecture/Castle/Materials/M_VAL_Capital_Architecture_Castle_Import_v1.mat";
        public const string PrefabPath =
            "Assets/Art/Capital/Architecture/Castle/Prefabs/VAL_Capital_Architecture_Castle_Import_v1.prefab";
        public const string CatalogPath =
            "Assets/Art/Capital/Resources/Valgor/CityArtAssetCatalog.asset";

        public const float TargetHeight = 9.5f;
        public const float SourceRoughness = 0.5f;

        [MenuItem("Valgor/Art/Capital/Build Castle Import v1 Prefab + Catalog")]
        public static void BuildFromMenu()
        {
            var ok = BuildAll(out var msg);
            EditorUtility.DisplayDialog("Castle Import v1", msg, "OK");
            if (!ok)
            {
                Debug.LogError(msg);
            }
        }

        /// <summary>
        /// CLI legado: NÃO associa mesh Import_v1 ao castelo.
        /// Redireciona para o builder da Torre dos Dragões + limpa slot castle.
        /// </summary>
        public static void BuildCli()
        {
            var code = CapitalDragonTowerPrefabBuilder.BuildAll(out var msg) ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] Castle Import v1 OK (remapped→dragon-tower): {msg}");
            }
            else
            {
                Debug.LogError($"[Valgor] Castle Import v1 FAIL: {msg}");
            }

            EditorApplication.Exit(code);
        }

        public static bool BuildAll(out string message)
        {
            EnsureFolders();
            if (!File.Exists(ToAbs(FbxPath)))
            {
                message = $"FBX ausente: {FbxPath}";
                return false;
            }

            if (!File.Exists(ToAbs(TexturePath)))
            {
                message = $"Textura ausente: {TexturePath}";
                return false;
            }

            AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);
            ConfigureTextureImporter(TexturePath);
            ConfigureModelImporter(FbxPath);

            var mat = EnsureMaterial(out var matMsg);
            if (mat == null)
            {
                message = matMsg;
                return false;
            }

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (model == null)
            {
                message = $"Falha ao carregar FBX: {FbxPath}";
                return false;
            }

            var root = new GameObject("VAL_Capital_Architecture_Castle_Import_v1");
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
                    mats[i] = mat;
                }

                renderer.sharedMaterials = mats;
                rendererCount++;
            }

            FitToGroundCentered(root, TargetHeight);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(PrefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                message = "Prefab não foi salvo.";
                return false;
            }

            if (!EnsureCatalog(prefab, out var catalogMsg))
            {
                message = catalogMsg;
                return false;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            message =
                $"{PrefabPath} | renderers={rendererCount} | height≈{TargetHeight} | {matMsg} | {catalogMsg}";
            return true;
        }

        private static bool EnsureCatalog(GameObject castlePrefab, out string message)
        {
            // CRÍTICO: Import_v1 / mesh fantasy NÃO deve ir para castle.
            // Este builder legado não escreve HF no castelo.
            _ = castlePrefab;
            EnsureFolder("Assets/Art/Capital/Resources");
            EnsureFolder("Assets/Art/Capital/Resources/Valgor");

            var catalog = AssetDatabase.LoadAssetAtPath<CityArtAssetCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<CityArtAssetCatalog>();
                catalog.PopulateDefaultSlots();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            if (catalog.Entries.Count == 0)
            {
                catalog.PopulateDefaultSlots();
            }

            var entry = catalog.FindByBuildingId("castle");
            if (entry == null)
            {
                message = "Slot building.castle ausente no catálogo.";
                return false;
            }

            entry.HighFidelityPrefab = null;
            entry.MediumPrefab = null;
            entry.LowPrefab = null;
            entry.UsesExistingCastleLoader = true;
            entry.SlotReady = true;
            EditorUtility.SetDirty(catalog);
            message =
                $"Catalog OK {CatalogPath} castle→CastleRealVisualLoader only (HF limpo; use DragonTower builder)";
            return true;
        }

        private static Material EnsureMaterial(out string message)
        {
            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (baseMap == null)
            {
                message = "BaseColor não carregou.";
                return null!;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                message = "Shader URP/Lit não encontrado.";
                return null!;
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                mat = new Material(shader) { name = "M_VAL_Capital_Architecture_Castle_Import_v1" };
                AssetDatabase.CreateAsset(mat, MaterialPath);
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

            EditorUtility.SetDirty(mat);
            message = $"URP Lit BaseMap smoothness={smoothness:0.##}";
            return mat;
        }

        private static void FitToGroundCentered(GameObject root, float targetHeight)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds ComputeBounds()
            {
                var b = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; i++)
                {
                    b.Encapsulate(renderers[i].bounds);
                }

                return b;
            }

            var model = root.transform.Find("Model");
            if (model == null && root.transform.childCount > 0)
            {
                model = root.transform.GetChild(0);
            }

            // Root fica em escala 1; tamanho vive no Model (compatível com Instantiate scale=1).
            root.transform.localScale = Vector3.one;
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            if (model != null)
            {
                model.localPosition = Vector3.zero;
                model.localRotation = Quaternion.identity;
                model.localScale = Vector3.one;
            }

            var bounds = ComputeBounds();
            var height = Mathf.Max(0.01f, bounds.size.y);
            var scale = targetHeight / height;

            if (model != null)
            {
                model.localScale = Vector3.one * scale;
            }
            else
            {
                root.transform.localScale = Vector3.one * scale;
            }

            bounds = ComputeBounds();
            // Correção em world-space (InverseTransformVector respeita escala do parent).
            var shiftWorld = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
            if (model != null)
            {
                model.localPosition += root.transform.InverseTransformVector(shiftWorld);
            }
            else
            {
                root.transform.position += shiftWorld;
            }
        }

        private static void ConfigureTextureImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.sRGBTexture = true;
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
        }

        private static void ConfigureModelImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.animationType = ModelImporterAnimationType.None;
            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        private static string ToAbs(string assetPath) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/Capital");
            EnsureFolder("Assets/Art/Capital/Architecture");
            EnsureFolder("Assets/Art/Capital/Architecture/Castle");
            EnsureFolder("Assets/Art/Capital/Architecture/Castle/FBX");
            EnsureFolder("Assets/Art/Capital/Architecture/Castle/Textures");
            EnsureFolder("Assets/Art/Capital/Architecture/Castle/Materials");
            EnsureFolder("Assets/Art/Capital/Architecture/Castle/Prefabs");
            EnsureFolder("Assets/Art/Capital/Architecture/Castle/Source");
            EnsureFolder("Assets/Art/Capital/Resources");
            EnsureFolder("Assets/Art/Capital/Resources/Valgor");
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
