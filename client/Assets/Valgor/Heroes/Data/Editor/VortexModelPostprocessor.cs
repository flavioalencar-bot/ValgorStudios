#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Valgor.Heroes.Characters.Vortex;

namespace Valgor.Heroes.EditorTools
{
    /// <summary>
    /// When a Vortex FBX/GLB is imported into the Models folder, rebuild the hero prefab automatically.
    /// </summary>
    public sealed class VortexModelPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] imported,
            string[] deleted,
            string[] movedTo,
            string[] movedFrom)
        {
            var touch = false;
            foreach (var path in imported)
            {
                if (IsVortexModelPath(path)) touch = true;
            }

            foreach (var path in movedTo)
            {
                if (IsVortexModelPath(path)) touch = true;
            }

            if (!touch) return;

            EditorApplication.delayCall += () =>
            {
                VortexPipelineMenus.EnsureFolders();
                VortexPrefabBuilder.BuildOrUpdate();
                var report = VortexAssetImportValidator.ValidateAll();
                Debug.Log("Vortex: source model detectado — prefab reconstruído.\n" + report);
            };
        }

        private static bool IsVortexModelPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (!path.Replace('\\', '/').StartsWith(VortexAssetPaths.Models)) return false;
            var name = System.IO.Path.GetFileName(path);
            return name.StartsWith("Vortex") &&
                   (name.EndsWith(".fbx") || name.EndsWith(".glb") || name.EndsWith(".gltf"));
        }
    }
}
#endif
