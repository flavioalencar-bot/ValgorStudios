using System.IO;
using UnityEditor;
using UnityEngine;
using Valgor.City.Visual;

namespace Valgor.Editor
{
    /// <summary>
    /// Gera prefabs próprios de andaime em Resources/Valgor/Construction.
    /// </summary>
    public static class ConstructionScaffoldPrefabBuilder
    {
        private const string ResourcesDir =
            "Assets/Valgor/City/Art/Construction/Resources/Valgor/Construction";

        [MenuItem("Valgor/Art/Bake Construction Scaffold Prefabs")]
        public static void BuildFromMenu()
        {
            var ok = BuildAll(out var msg);
            EditorUtility.DisplayDialog(
                "Construction Scaffolds",
                ok ? $"OK\n{msg}" : $"Falhou\n{msg}",
                "OK");
        }

        public static bool BuildAll(out string message)
        {
            Directory.CreateDirectory(
                Path.GetFullPath(Path.Combine(Application.dataPath, "..", ResourcesDir)));

            var count = 0;
            foreach (ConstructionScaffoldSize size in System.Enum.GetValues(typeof(ConstructionScaffoldSize)))
            {
                var go = ConstructionScaffoldBuilder.Build(size);
                var path = $"{ResourcesDir}/{ConstructionScaffoldCatalog.PrefabAssetName(size)}.prefab";
                PrefabUtility.SaveAsPrefabAsset(go, path);
                Object.DestroyImmediate(go);
                count++;
                AssetDatabase.ImportAsset(path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            message = $"baked {count} scaffolds → {ResourcesDir}";
            Debug.Log($"[Valgor] {message}");
            return count > 0;
        }
    }
}
