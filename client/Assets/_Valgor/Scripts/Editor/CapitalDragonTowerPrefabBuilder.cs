using System.IO;
using UnityEditor;
using UnityEngine;
using Valgor.City.Visual;

namespace Valgor.Editor
{
    /// <summary>
    /// Prefab + catálogo para VAL_Capital_Architecture_DragonTower.
    /// Corrige mapeamento: mesh NÃO vai para castle; castle fica no CastleRealVisualLoader.
    /// </summary>
    public static class CapitalDragonTowerPrefabBuilder
    {
        public const string FbxPath =
            "Assets/Art/Capital/Architecture/DragonTower/FBX/VAL_Capital_Architecture_DragonTower.fbx";
        public const string TexturePath =
            "Assets/Art/Capital/Architecture/DragonTower/Textures/VAL_Capital_Architecture_DragonTower_BaseColor.jpg";
        public const string MaterialPath =
            "Assets/Art/Capital/Architecture/DragonTower/Materials/M_VAL_Capital_Architecture_DragonTower.mat";
        public const string PrefabPath =
            "Assets/Art/Capital/Architecture/DragonTower/Prefabs/VAL_Capital_Architecture_DragonTower.prefab";
        public const string CatalogPath =
            "Assets/Art/Capital/Resources/Valgor/CityArtAssetCatalog.asset";

        public const string BuildingId = "dragon-tower";
        /// <summary>~65% da altura anterior (9.5→6.15) — Castelo dominante; torre = 2º.</summary>
        public const float TargetHeight = 6.15f;
        public const float SourceRoughness = 0.5f;

        [MenuItem("Valgor/Art/Capital/Build DragonTower Prefab + Catalog (fix mapping)")]
        public static void BuildFromMenu()
        {
            var ok = BuildAll(out var msg);
            EditorUtility.DisplayDialog("DragonTower Art", msg, "OK");
            if (!ok)
            {
                Debug.LogError(msg);
            }
        }

        /// <summary>CLI: -executeMethod Valgor.Editor.CapitalDragonTowerPrefabBuilder.BuildCli</summary>
        public static void BuildCli()
        {
            var code = BuildAll(out var msg) ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] DragonTower Art OK: {msg}");
            }
            else
            {
                Debug.LogError($"[Valgor] DragonTower Art FAIL: {msg}");
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

            var root = new GameObject("VAL_Capital_Architecture_DragonTower");
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

        /// <summary>
        /// dragon-tower → DragonTower prefab; castle → sem HF (só CastleRealVisualLoader).
        /// </summary>
        private static bool EnsureCatalog(GameObject dragonTowerPrefab, out string message)
        {
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

            var castle = catalog.FindByBuildingId("castle");
            if (castle == null)
            {
                message = "Slot building.castle ausente no catálogo.";
                return false;
            }

            // CRÍTICO: nunca Castle → DragonTower.
            castle.HighFidelityPrefab = null;
            castle.MediumPrefab = null;
            castle.LowPrefab = null;
            castle.UsesExistingCastleLoader = true;
            castle.SlotReady = true;

            var tower = catalog.FindByBuildingId(BuildingId);
            if (tower == null)
            {
                message = "Slot building.dragon-tower ausente no catálogo.";
                return false;
            }

            tower.HighFidelityPrefab = dragonTowerPrefab;
            tower.MediumPrefab = dragonTowerPrefab;
            tower.LowPrefab = null;
            tower.UsesExistingCastleLoader = false;
            tower.SlotReady = true;

            // Remove prefab castle Import_v1 do catálogo se ainda existir como asset órfão no slot.
            EditorUtility.SetDirty(catalog);
            message =
                $"Catalog OK: castle→CastleLoader only; {BuildingId}→{dragonTowerPrefab.name}";
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
                mat = new Material(shader) { name = "M_VAL_Capital_Architecture_DragonTower" };
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
            EnsureFolder("Assets/Art/Capital/Architecture/DragonTower");
            EnsureFolder("Assets/Art/Capital/Architecture/DragonTower/FBX");
            EnsureFolder("Assets/Art/Capital/Architecture/DragonTower/Textures");
            EnsureFolder("Assets/Art/Capital/Architecture/DragonTower/Materials");
            EnsureFolder("Assets/Art/Capital/Architecture/DragonTower/Prefabs");
            EnsureFolder("Assets/Art/Capital/Architecture/DragonTower/Source");
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
