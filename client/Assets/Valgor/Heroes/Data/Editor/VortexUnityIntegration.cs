#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Valgor.Heroes.Characters.Vortex;

namespace Valgor.Heroes.EditorTools
{
    /// <summary>
    /// Batch entry after Blender export copied FBX into Models/.
    /// </summary>
    public static class VortexUnityIntegration
    {
        public static void IntegrateFromCommandLine()
        {
            VortexPipelineMenus.EnsureFolders();

            AssetDatabase.Refresh();

            // Force Humanoid reimport after FBX overwrite.
            foreach (var path in VortexAssetPaths.RequiredModelCandidates)
            {
                if (!File.Exists(path)) continue;
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.importAnimation = true;
                    importer.SaveAndReimport();
                }
                else
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }

            if (File.Exists(VortexAssetPaths.DragonSword))
            {
                AssetDatabase.ImportAsset(VortexAssetPaths.DragonSword, ImportAssetOptions.ForceUpdate);
            }

            var prefab = VortexPrefabBuilder.BuildOrUpdate();
            HeroesDemoSceneBuilder.BuildDemoScene();
            var report = VortexAssetImportValidator.ValidateAll();
            VortexPipelineMenus.RefreshStatusFromReport(report);
            Debug.Log("[Valgor] Vortex Unity integration complete.\n" + report);
            if (prefab == null)
                throw new System.InvalidOperationException("Vortex_Hero.prefab failed to build.");
            if (!report.HasSourceModel || report.UsingFallbackPrefab)
                throw new System.InvalidOperationException(
                    "Expected real Vortex source model integrated (not technical fallback).");
            if (!report.AvatarOk)
                throw new System.InvalidOperationException("Expected Humanoid Avatar on Vortex_LOD0.fbx.");
        }
    }
}
#endif
