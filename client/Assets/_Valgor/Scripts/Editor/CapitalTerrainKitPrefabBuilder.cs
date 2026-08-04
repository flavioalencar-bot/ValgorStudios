using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Valgor.Editor
{
    /// <summary>
    /// Importa GLBs do CapitalTerrainKit_01 → Models + Prefabs Resources.
    /// </summary>
    public static class CapitalTerrainKitPrefabBuilder
    {
        public const string ProductionSource =
            @"C:\Valgor_Studio\production\City\Capital\Terrain\source";
        public const string ModelsFolder =
            "Assets/Valgor/City/Art/Capital/Environment/Models";
        public const string PrefabFolder =
            "Assets/Valgor/City/Art/Capital/Environment/Resources/Valgor/Capital/Environment";

        [MenuItem("Valgor/City/Environment/Import CapitalTerrainKit_01 GLBs")]
        public static void ImportFromMenu()
        {
            var msg = ImportAll();
            EditorUtility.DisplayDialog("CapitalTerrainKit_01", msg, "OK");
        }

        /// <summary>CLI: -executeMethod Valgor.Editor.CapitalTerrainKitPrefabBuilder.ImportCli</summary>
        public static void ImportCli()
        {
            // Bloqueio absoluto de API Tripo — este método só copia/importa arquivos locais.
            if (HasNoTripoFlag() || File.Exists(@"C:\Valgor_Studio\production\City\Capital\Terrain\TRIPOS_PAUSED.flag"))
            {
                Debug.Log("[Valgor] CapitalTerrainKit ImportCli: NoTripo — sem harvest de lotes; só Models já locais.");
                var msg = ImportExistingModelsOnly();
                Debug.Log($"[Valgor] CapitalTerrainKit import (local-only): {msg}");
                EditorApplication.Exit(0);
                return;
            }

            var full = ImportAll();
            Debug.Log($"[Valgor] CapitalTerrainKit import: {full}");
            EditorApplication.Exit(0);
        }

        private static bool HasNoTripoFlag()
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (string.Equals(arg, "-capitalWorldNoTripoCorrection", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ImportExistingModelsOnly()
        {
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Valgor/City/Art/Capital/Environment/Models")));
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Valgor/City/Art/Capital/Environment/Resources/Valgor/Capital/Environment")));
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var imported = 0;
            var prefabs = 0;
            var files = Directory.GetFiles(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Valgor/City/Art/Capital/Environment/Models")), "*.glb");
            foreach (var abs in files)
            {
                var file = Path.GetFileName(abs);
                var assetPath = $"{ModelsFolder}/{file}";
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                imported++;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (model == null)
                {
                    continue;
                }

                var key = Path.GetFileNameWithoutExtension(file);
                var prefabPath = $"{PrefabFolder}/{key}.prefab";
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                if (instance == null)
                {
                    instance = UnityEngine.Object.Instantiate(model);
                }

                StripColliders(instance);
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                UnityEngine.Object.DestroyImmediate(instance);
                prefabs++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return $"local-only GLBs={imported} prefabs={prefabs} (Tripo API=0)";
        }

        public static string ImportAll()
        {
            if (HasNoTripoFlag() || File.Exists(@"C:\Valgor_Studio\production\City\Capital\Terrain\TRIPOS_PAUSED.flag"))
            {
                return ImportExistingModelsOnly();
            }

            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Valgor/City/Art/Capital/Environment/Models")));
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Valgor/City/Art/Capital/Environment/Resources/Valgor/Capital/Environment")));

            if (!Directory.Exists(ProductionSource))
            {
                return $"Source ausente: {ProductionSource}";
            }

            HarvestProductionGlbs();

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var imported = 0;
            var prefabs = 0;
            var files = Directory.GetFiles(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Valgor/City/Art/Capital/Environment/Models")), "*.glb");
            foreach (var abs in files)
            {
                var file = Path.GetFileName(abs);
                var assetPath = $"{ModelsFolder}/{file}";
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                imported++;

                var model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (model == null)
                {
                    continue;
                }

                var key = Path.GetFileNameWithoutExtension(file);
                var prefabPath = $"{PrefabFolder}/{key}.prefab";
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                if (instance == null)
                {
                    instance = UnityEngine.Object.Instantiate(model);
                }

                StripColliders(instance);
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                UnityEngine.Object.DestroyImmediate(instance);
                prefabs++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return $"GLBs={imported} prefabs={prefabs} → {PrefabFolder}";
        }

        private static void HarvestProductionGlbs()
        {
            var destAbs = Path.GetFullPath(Path.Combine(Application.dataPath,
                "Valgor/City/Art/Capital/Environment/Models"));
            Directory.CreateDirectory(destAbs);

            var roots = new[]
            {
                ProductionSource,
                @"C:\Valgor_Studio\production\City\Capital\Terrain\lote1-mountains",
                @"C:\Valgor_Studio\production\City\Capital\Terrain\lote2-cliffs-plateaus",
                @"C:\Valgor_Studio\production\City\Capital\Terrain\lote3-rivers",
                @"C:\Valgor_Studio\production\City\Capital\Terrain\lote4-waterfalls-bridges",
                @"C:\Valgor_Studio\production\City\Capital\Terrain\lote5-rocks"
            };

            foreach (var root in roots)
            {
                if (!Directory.Exists(root))
                {
                    continue;
                }

                foreach (var glb in Directory.GetFiles(root, "model.glb", SearchOption.AllDirectories))
                {
                    var key = InferAssetKey(glb);
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    var destName = $"VAL_Capital_Environment_{key}.glb";
                    var dest = Path.Combine(destAbs, destName);
                    File.Copy(glb, dest, true);
                }

                foreach (var glb in Directory.GetFiles(root, "VAL_Capital_Environment_*.glb", SearchOption.AllDirectories))
                {
                    var dest = Path.Combine(destAbs, Path.GetFileName(glb));
                    File.Copy(glb, dest, true);
                }
            }
        }

        private static string InferAssetKey(string modelGlbPath)
        {
            // .../lote1-mountains/Mountain_A/tripo-out/.../model.glb
            var dir = new DirectoryInfo(Path.GetDirectoryName(modelGlbPath)!);
            while (dir != null)
            {
                var name = dir.Name;
                if (name.StartsWith("Mountain_", StringComparison.Ordinal) ||
                    name.StartsWith("DistantMountain_", StringComparison.Ordinal) ||
                    name.StartsWith("Cliff_", StringComparison.Ordinal) ||
                    name.StartsWith("Plateau_", StringComparison.Ordinal) ||
                    name.StartsWith("Valley_", StringComparison.Ordinal) ||
                    name.StartsWith("River_", StringComparison.Ordinal) ||
                    name.StartsWith("RiverBank_", StringComparison.Ordinal) ||
                    name.StartsWith("Waterfall_", StringComparison.Ordinal) ||
                    name.StartsWith("Rock_", StringComparison.Ordinal) ||
                    name.StartsWith("Slope_", StringComparison.Ordinal) ||
                    name.StartsWith("Embankment_", StringComparison.Ordinal) ||
                    name is "Stream" or "Lake_A" or "StoneBridge_A" or "WoodBridge_A")
                {
                    return name;
                }

                dir = dir.Parent;
            }

            return string.Empty;
        }

        private static void StripColliders(GameObject root)
        {
            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(col);
            }
        }
    }
}
