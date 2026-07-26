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
        private void OnPreprocessModel()
        {
            if (!IsVortexModelPath(assetPath)) return;
            var importer = assetImporter as ModelImporter;
            if (importer == null) return;

            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.isReadable = true;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation = ModelImporterMaterialLocation.External;

            var fileName = System.IO.Path.GetFileName(assetPath);
            if (fileName.StartsWith("Vortex_DragonSword"))
            {
                importer.animationType = ModelImporterAnimationType.None;
                importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
                return;
            }

            // Character: Humanoid avatar from this model.
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.animationCompression = ModelImporterAnimationCompression.Optimal;
            importer.importAnimation = true;
        }

        private static void OnPostprocessAllAssets(
            string[] imported,
            string[] deleted,
            string[] movedTo,
            string[] movedFrom)
        {
            var touch = false;
            foreach (var path in imported)
            {
                if (IsVortexCharacterModelPath(path)) touch = true;
            }

            foreach (var path in movedTo)
            {
                if (IsVortexCharacterModelPath(path)) touch = true;
            }

            if (!touch) return;

            EditorApplication.delayCall += () =>
            {
                VortexPipelineMenus.EnsureFolders();
                VortexPrefabBuilder.BuildOrUpdate();
                var report = VortexAssetImportValidator.ValidateAll();
                VortexPipelineMenus.RefreshStatusFromReport(report);
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

        private static bool IsVortexCharacterModelPath(string path)
        {
            if (!IsVortexModelPath(path)) return false;
            var name = System.IO.Path.GetFileName(path);
            return !name.StartsWith("Vortex_DragonSword");
        }
    }
}
#endif
