#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Valgor.Heroes.Characters;
using Valgor.Heroes.Characters.Vortex;
using Valgor.Heroes.Data;
using Valgor.Heroes.Preview360;

namespace Valgor.Heroes.EditorTools
{
    public static class VortexPipelineMenus
    {
        [MenuItem("Valgor/Heroes/Vortex/Validate Source Assets")]
        public static void ValidateSourceAssets()
        {
            EnsureFolders();
            var report = VortexAssetImportValidator.ValidateAll();
            UpdateStatusFromReport(report);
            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog(
                "Vortex — Validate Source Assets",
                report.Summary,
                "OK");
        }

        [MenuItem("Valgor/Heroes/Vortex/Build Vortex Prefab")]
        public static void BuildVortexPrefab()
        {
            EnsureFolders();
            var prefab = VortexPrefabBuilder.BuildOrUpdate();
            var report = VortexAssetImportValidator.ValidateAll();
            UpdateStatusFromReport(report);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Vortex prefab: {VortexAssetPaths.HeroPrefab}\n{report.Summary}");
        }

        [MenuItem("Valgor/Heroes/Vortex/Open Vortex Preview")]
        public static void OpenVortexPreview()
        {
            EnsureFolders();
            VortexPrefabBuilder.BuildOrUpdate();
            HeroesDemoSceneBuilder.BuildDemoScene();
            EditorSceneManager.OpenScene(HeroesDemoSceneBuilder.ScenePath, OpenSceneMode.Single);
            Debug.Log("HeroesDemo aberta para preview de Vortex.");
        }

        [MenuItem("Valgor/Heroes/Vortex/Create Folder Scaffold")]
        public static void CreateFolderScaffold() => EnsureFolders();

        /// <summary>Batch/CI entry: scaffold + build shell prefab + validate.</summary>
        public static void BuildFromCommandLine()
        {
            EnsureFolders();
            VortexPrefabBuilder.BuildOrUpdate();
            var report = VortexAssetImportValidator.ValidateAll();
            UpdateStatusFromReport(report);
            Debug.Log("[Valgor] Vortex pipeline:\n" + report);
            if (!report.PrefabReady)
                throw new System.InvalidOperationException("Vortex_Hero.prefab não foi gerado.");
        }

        public static void EnsureFolders()
        {
            CreateFolderRecursive(VortexAssetPaths.Models);
            CreateFolderRecursive(VortexAssetPaths.Textures);
            CreateFolderRecursive(VortexAssetPaths.Materials);
            CreateFolderRecursive(VortexAssetPaths.Animations);
            CreateFolderRecursive(VortexAssetPaths.Prefabs);
            CreateFolderRecursive(VortexAssetPaths.Portraits);
            CreateFolderRecursive(VortexAssetPaths.Vfx);
            CreateFolderRecursive(VortexAssetPaths.Audio);
            CreateFolderRecursive(VortexAssetPaths.Data);

            WriteTextIfMissing(
                VortexAssetPaths.Models + "/PLACE_FBX_HERE.md",
                VortexDropInstructions.ModelsReadme);
            WriteTextIfMissing(
                VortexAssetPaths.Textures + "/PLACE_TEXTURES_HERE.md",
                VortexDropInstructions.TexturesReadme);
            WriteTextIfMissing(
                VortexAssetPaths.Animations + "/PLACE_ANIMATIONS_HERE.md",
                VortexDropInstructions.AnimationsReadme);
            WriteTextIfMissing(
                VortexAssetPaths.Root + "/README.md",
                VortexDropInstructions.RootReadme);

            EnsureImportProfile();
            EnsurePipelineStatus();
            VortexPrefabBuilder.EnsureMaterials();
            VortexPrefabBuilder.EnsureAnimatorController();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureImportProfile()
        {
            if (AssetDatabase.LoadAssetAtPath<HeroModelImportProfile>(VortexAssetPaths.ImportProfile) != null)
                return;

            var profile = ScriptableObject.CreateInstance<HeroModelImportProfile>();
            profile.HeroId = VortexAssetPaths.HeroId;
            profile.ExpectedHeightMeters = VortexAssetPaths.TargetHeightMeters;
            profile.PrefabOutputPath = VortexAssetPaths.HeroPrefab;
            profile.AddressableKey = VortexAssetPaths.AddressablePrefabKey;
            AssetDatabase.CreateAsset(profile, VortexAssetPaths.ImportProfile);
        }

        private static void EnsurePipelineStatus()
        {
            if (AssetDatabase.LoadAssetAtPath<VortexPipelineStatusSO>(VortexAssetPaths.PipelineStatus) != null)
                return;

            var status = ScriptableObject.CreateInstance<VortexPipelineStatusSO>();
            AssetDatabase.CreateAsset(status, VortexAssetPaths.PipelineStatus);
        }

        private static void UpdateStatusFromReport(VortexValidationReport report)
        {
            var status = AssetDatabase.LoadAssetAtPath<VortexPipelineStatusSO>(VortexAssetPaths.PipelineStatus)
                         ?? ScriptableObject.CreateInstance<VortexPipelineStatusSO>();
            if (AssetDatabase.LoadAssetAtPath<VortexPipelineStatusSO>(VortexAssetPaths.PipelineStatus) == null)
                AssetDatabase.CreateAsset(status, VortexAssetPaths.PipelineStatus);

            status.LastValidationReport = report.ToString();
            status.UsingTechnicalFallback = !report.HasSourceModel || report.UsingFallbackPrefab;
            status.SourceModelPath = report.DetectedSourceModelPath;
            status.Phase = report.HasSourceModel
                ? (report.PrefabReady && report.AllCriticalPassed
                    ? VortexPipelinePhase.Validated
                    : VortexPipelinePhase.SourcePresent)
                : VortexPipelinePhase.WaitingForSourceModel;
            if (report.PrefabReady && status.Phase == VortexPipelinePhase.SourcePresent)
                status.Phase = VortexPipelinePhase.PrefabBuilt;

            EditorUtility.SetDirty(status);
            AssetDatabase.SaveAssets();
        }

        private static void CreateFolderRecursive(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;
            var parts = assetFolder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void WriteTextIfMissing(string assetPath, string contents)
        {
            if (File.Exists(assetPath)) return;
            File.WriteAllText(assetPath, contents);
            AssetDatabase.ImportAsset(assetPath);
        }
    }

    public static class VortexDropInstructions
    {
        public const string RootReadme =
@"# Vortex — pipeline do herói real

Fonte de verdade: `docs/game-design/heroes/VALGOR_SPRINT_HERO_REAL_VORTEX.md`

## Status

Enquanto o FBX/GLB final não estiver em `Models/`, o jogo usa fallback técnico (`HumanoidDummy`)
dentro do shell `Prefabs/Vortex_Hero.prefab`.

## Menus

- Valgor → Heroes → Vortex → Validate Source Assets
- Valgor → Heroes → Vortex → Build Vortex Prefab
- Valgor → Heroes → Vortex → Open Vortex Preview

## Não alterar

Dados de gameplay de `HERO_VORTEX_000` (seed / catálogo).
";

        public const string ModelsReadme =
@"# Coloque aqui o modelo 3D de Vortex

Arquivos aceitos (prioridade):

1. `Vortex_LOD0.fbx` (obrigatório para LOD0)
2. `Vortex_LOD1.fbx`
3. `Vortex_LOD2.fbx`
4. Alternativa única: `Vortex.fbx` ou `Vortex.glb`

Requisitos: Humanoid, ~2.05 m, pivô nos pés, olhando +Z, T-pose/A-pose.
Produzir fora do Unity (Blender / Maya / Character Creator / etc.).
";

        public const string TexturesReadme =
@"# Texturas obrigatórias

- Vortex_Body_BaseColor.png / Normal.png / Mask.png (até 2048)
- Vortex_Armor_BaseColor.png / Normal.png / Mask.png (até 2048)
- Vortex_Weapon_BaseColor.png / Normal.png (até 2048)

Mask: Metallic + Roughness/Smoothness + AO empacotados.
Sem 4K no MVP.
";

        public const string AnimationsReadme =
@"# Clips de animação (nomes exatos)

Idle, Idle_Combat, Walk, Run, Turn_Left, Turn_Right,
Attack_01, Attack_02, Heavy_Attack, Special_Power,
Hit_Front, Hit_Back, Stun, Victory, Defeat, Death

Importe FBX com animações ou clips .anim e associe ao `Vortex_Animator.controller`.
";
    }

    public sealed class VortexValidationReport
    {
        public bool HasSourceModel;
        public string DetectedSourceModelPath;
        public bool PrefabReady;
        public bool UsingFallbackPrefab = true;
        public bool AvatarOk;
        public bool SocketsOk;
        public bool MaterialsOk;
        public bool AnimationsOk;
        public bool TexturesOk;
        public bool LodOk;
        public bool CatalogOk;
        public readonly List<string> Errors = new();
        public readonly List<string> Warnings = new();

        public bool AllCriticalPassed =>
            PrefabReady && SocketsOk && MaterialsOk && CatalogOk &&
            (!HasSourceModel || (AvatarOk && TexturesOk));

        public string Summary =>
            HasSourceModel
                ? $"Fonte: {DetectedSourceModelPath}. Prefab={PrefabReady}. Erros={Errors.Count}. Avisos={Warnings.Count}."
                : $"AGUARDANDO FBX em {VortexAssetPaths.Models}/. Pipeline/shell prontos. Erros={Errors.Count}. Avisos={Warnings.Count}.";

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Vortex Asset Validation ===");
            sb.AppendLine(Summary);
            sb.AppendLine($"HasSourceModel={HasSourceModel}");
            sb.AppendLine($"PrefabReady={PrefabReady} UsingFallback={UsingFallbackPrefab}");
            sb.AppendLine($"AvatarOk={AvatarOk} SocketsOk={SocketsOk} MaterialsOk={MaterialsOk}");
            sb.AppendLine($"AnimationsOk={AnimationsOk} TexturesOk={TexturesOk} LodOk={LodOk} CatalogOk={CatalogOk}");
            foreach (var e in Errors) sb.AppendLine("ERROR: " + e);
            foreach (var w in Warnings) sb.AppendLine("WARN: " + w);
            return sb.ToString();
        }
    }

    public static class VortexAssetImportValidator
    {
        public static VortexValidationReport ValidateAll()
        {
            var report = new VortexValidationReport();
            DetectSource(report);
            ValidateTextures(report);
            ValidateMaterials(report);
            ValidatePrefab(report);
            ValidateCatalog(report);
            ValidateAnimations(report);
            return report;
        }

        private static void DetectSource(VortexValidationReport report)
        {
            foreach (var path in VortexAssetPaths.RequiredModelCandidates)
            {
                if (!File.Exists(path)) continue;
                report.HasSourceModel = true;
                report.DetectedSourceModelPath = path;
                break;
            }

            if (!report.HasSourceModel)
            {
                report.Warnings.Add(
                    $"Modelo 3D ausente. Esperado: {VortexAssetPaths.Lod0} (ou Vortex.fbx / Vortex.glb).");
                return;
            }

            var importer = AssetImporter.GetAtPath(report.DetectedSourceModelPath) as ModelImporter;
            if (importer == null)
            {
                report.Errors.Add("Importer do modelo não é ModelImporter.");
                return;
            }

            report.AvatarOk = importer.animationType == ModelImporterAnimationType.Human;
            if (!report.AvatarOk)
                report.Errors.Add("Animation Type deve ser Humanoid (ModelImporterAnimationType.Human).");

            if (!Mathf.Approximately(importer.globalScale, 1f))
                report.Warnings.Add($"Scale Factor={importer.globalScale}, esperado 1.");
        }

        private static void ValidateTextures(VortexValidationReport report)
        {
            var missing = VortexAssetPaths.RequiredTextures.Where(p => !File.Exists(p)).ToList();
            report.TexturesOk = missing.Count == 0;
            if (!report.HasSourceModel)
            {
                report.Warnings.Add($"Texturas ainda não fornecidas ({missing.Count} faltando).");
                return;
            }

            foreach (var path in missing)
                report.Errors.Add("Textura ausente: " + path);

            foreach (var path in VortexAssetPaths.RequiredTextures.Where(File.Exists))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                var max = path.Contains("Weapon")
                    ? VortexAssetPaths.MaxWeaponTextureSize
                    : VortexAssetPaths.MaxBodyTextureSize;
                if (importer.maxTextureSize > max)
                    report.Warnings.Add($"{path} maxTextureSize={importer.maxTextureSize} > {max}");
                if (path.Contains("Normal") && importer.textureType != TextureImporterType.NormalMap)
                    report.Errors.Add($"{path} deve ser Normal Map.");
            }
        }

        private static void ValidateMaterials(VortexValidationReport report)
        {
            var missing = VortexAssetPaths.RequiredMaterials
                .Where(p => AssetDatabase.LoadAssetAtPath<Material>(p) == null).ToList();
            report.MaterialsOk = missing.Count == 0;
            foreach (var path in missing)
                report.Warnings.Add("Material placeholder ausente (será criado no Build): " + path);
        }

        private static void ValidatePrefab(VortexValidationReport report)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VortexAssetPaths.HeroPrefab);
            report.PrefabReady = prefab != null;
            if (prefab == null)
            {
                report.Warnings.Add("Vortex_Hero.prefab ainda não construído. Rode Build Vortex Prefab.");
                return;
            }

            var visual = prefab.GetComponent<HeroVisualController>();
            var sockets = prefab.GetComponent<HeroSocketRegistry>();
            report.UsingFallbackPrefab = visual == null || visual.UsingTechnicalFallback || !report.HasSourceModel;
            List<string> missingSockets = null;
            report.SocketsOk = sockets != null && sockets.HasAllRequired(out missingSockets);
            if (!report.SocketsOk)
                report.Errors.Add("Sockets faltando: " + (missingSockets == null ? "(registry ausente)" : string.Join(", ", missingSockets)));

            report.LodOk = prefab.GetComponent<HeroLODController>() != null
                           || prefab.GetComponentInChildren<LODGroup>() != null;
            if (!report.LodOk)
                report.Warnings.Add("LOD Group / HeroLODController ausente.");

            if (report.HasSourceModel && report.UsingFallbackPrefab)
                report.Warnings.Add("Fonte FBX presente, mas prefab ainda em fallback. Rode Build Vortex Prefab.");
        }

        private static void ValidateCatalog(VortexValidationReport report)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<HeroCatalogSO>(
                "Assets/Valgor/Heroes/Data/Generated/HeroCatalog.asset");
            var vortex = catalog?.Heroes?.FirstOrDefault(h => h != null && h.Id == VortexAssetPaths.HeroId);
            report.CatalogOk = vortex != null
                               && vortex.PrefabAddress == VortexAssetPaths.AddressablePrefabKey;
            if (vortex == null)
                report.Errors.Add("HERO_VORTEX_000 não encontrado no HeroCatalog.");
            else if (vortex.PrefabAddress != VortexAssetPaths.AddressablePrefabKey)
                report.Errors.Add($"PrefabAddress esperado '{VortexAssetPaths.AddressablePrefabKey}', atual '{vortex.PrefabAddress}'.");
        }

        private static void ValidateAnimations(VortexValidationReport report)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(VortexAssetPaths.AnimatorController);
            report.AnimationsOk = controller != null;
            if (controller == null)
            {
                report.Warnings.Add("Vortex_Animator.controller ausente (criado no Build).");
                return;
            }

            var names = new HashSet<string>();
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                    names.Add(state.state.name);
            }

            foreach (var required in HeroAnimationIds.Required)
            {
                if (!names.Contains(required))
                    report.Warnings.Add($"Estado de animação ausente no controller: {required}");
            }
        }
    }
}
#endif
