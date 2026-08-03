using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Valgor.Core;

namespace Valgor.Editor
{
    /// <summary>
    /// Build isolada de homologação da progressão da cidade.
    /// Compila com VALGOR_CITY_PROGRESSION_QA — QA ativo no duplo clique, sem CLI.
    /// Não altera ValgorVersion / pasta das betas normais.
    /// </summary>
    public static class QaCityProgressionWindowsBuild
    {
        public const string BuildFolderName = "Valgor-QA-City-Progression";

        private static readonly string[] Scenes =
        {
            "Assets/_Valgor/Scenes/Bootstrap.unity",
            "Assets/_Valgor/Scenes/Loading.unity",
            "Assets/_Valgor/Scenes/MainMenu.unity",
            "Assets/Valgor/City/Scenes/City.unity",
            "Assets/Valgor/Heroes/Scenes/HeroesDemo.unity",
            "Assets/_Valgor/Scenes/WorldMap.unity"
        };

        [MenuItem("Valgor/Build/Windows QA City Progression")]
        public static void BuildFromMenu()
        {
            var report = Build();
            EditorUtility.DisplayDialog(
                "Valgor QA City Progression",
                report.summary.result == BuildResult.Succeeded
                    ? $"Build OK:\n{GetOutputExe()}"
                    : $"Build falhou: {report.summary.result}",
                "OK");
        }

        /// <summary>CLI: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCli</summary>
        public static void BuildCli()
        {
            var report = Build();
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA City Progression Build OK: {GetOutputExe()}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA City Progression Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        public static BuildReport Build(string? folderNameOverride = null)
        {
            var previousBundle = PlayerSettings.bundleVersion;
            PlayerSettings.companyName = "Valgor Studios";
            PlayerSettings.productName = "Valgor";
            PlayerSettings.bundleVersion = "0.2.4-qa-city";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;

            try
            {
                if (CastleTiersPrefabBuilder.BuildAll(out var castleMsg))
                {
                    Debug.Log($"[Valgor] Castle tiers: {castleMsg}");
                }
                else
                {
                    Debug.LogWarning($"[Valgor] Castle tiers: {castleMsg}");
                }

                if (ConstructionScaffoldPrefabBuilder.BuildAll(out var scaffoldMsg))
                {
                    Debug.Log($"[Valgor] Construction scaffolds: {scaffoldMsg}");
                }
                else
                {
                    Debug.LogWarning($"[Valgor] Construction scaffolds: {scaffoldMsg}");
                }

                var outputDir = GetOutputDir(folderNameOverride);
                Directory.CreateDirectory(outputDir);
                var exe = Path.Combine(outputDir, "Valgor.exe");

                var options = new BuildPlayerOptions
                {
                    scenes = Scenes,
                    locationPathName = exe,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.CompressWithLz4HC,
                    extraScriptingDefines = new[] { CityProgressionQa.ScriptingDefine }
                };

                Debug.Log(
                    $"[Valgor] Building QA City Progression → {exe} " +
                    $"(define {CityProgressionQa.ScriptingDefine})");
                return BuildPipeline.BuildPlayer(options);
            }
            finally
            {
                PlayerSettings.bundleVersion = previousBundle;
            }
        }

        /// <summary>CLI pasta polished: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildPolishedCli</summary>
        public static void BuildPolishedCli()
        {
            var report = Build("Valgor-QA-City-Progression-Polished");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA City Progression Polished Build OK: {GetOutputExe("Valgor-QA-City-Progression-Polished")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Polished Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI pasta smooth: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildSmoothCli</summary>
        public static void BuildSmoothCli()
        {
            var report = Build("Valgor-QA-City-Progression-Smooth");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA City Progression Smooth Build OK: {GetOutputExe("Valgor-QA-City-Progression-Smooth")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Smooth Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI pasta upgrade UX: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildUpgradeUxCli</summary>
        public static void BuildUpgradeUxCli()
        {
            var report = Build("Valgor-QA-Building-Upgrade-UX");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Building Upgrade UX Build OK: {GetOutputExe("Valgor-QA-Building-Upgrade-UX")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Upgrade UX Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI pasta visual polish: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildUpgradeVisualCli</summary>
        public static void BuildUpgradeVisualCli()
        {
            var report = Build("Valgor-QA-Building-Upgrade-Visual");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Building Upgrade Visual Build OK: {GetOutputExe("Valgor-QA-Building-Upgrade-Visual")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Upgrade Visual Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI pasta construction visual: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildConstructionVisualCli</summary>
        public static void BuildConstructionVisualCli()
        {
            var report = Build("Valgor-QA-Building-Construction-Visual");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Building Construction Visual Build OK: {GetOutputExe("Valgor-QA-Building-Construction-Visual")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Construction Visual Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI pasta responsive P1: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildResponsiveP1Cli</summary>
        public static void BuildResponsiveP1Cli()
        {
            var report = Build("Valgor-QA-Responsive-P1-Fix");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Responsive P1 Build OK: {GetOutputExe("Valgor-QA-Responsive-P1-Fix")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Responsive P1 Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Dragão Fase 2: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildDragonPhase2Cli</summary>
        public static void BuildDragonPhase2Cli()
        {
            var report = Build("Valgor-QA-Dragon-Phase2");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Dragon Phase2 Build OK: {GetOutputExe("Valgor-QA-Dragon-Phase2")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Dragon Phase2 Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Dragão Fase 3: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildDragonPhase3Cli</summary>
        public static void BuildDragonPhase3Cli()
        {
            var report = Build("Valgor-QA-Dragon-Phase3");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Dragon Phase3 Build OK: {GetOutputExe("Valgor-QA-Dragon-Phase3")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Dragon Phase3 Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Dragão Fase 4: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildDragonPhase4Cli</summary>
        public static void BuildDragonPhase4Cli()
        {
            var report = Build("Valgor-QA-Dragon-Phase4");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Dragon Phase4 Build OK: {GetOutputExe("Valgor-QA-Dragon-Phase4")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Dragon Phase4 Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Homolog F1–4: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildDragonPhase4HomologCli</summary>
        public static void BuildDragonPhase4HomologCli()
        {
            var report = Build("Valgor-QA-Dragon-Phase4-Homolog");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Dragon Phase4 Homolog Build OK: {GetOutputExe("Valgor-QA-Dragon-Phase4-Homolog")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Dragon Phase4 Homolog Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Dragão Fase 5: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildDragonPhase5Cli</summary>
        public static void BuildDragonPhase5Cli()
        {
            var report = Build("Valgor-QA-Dragon-Phase5");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Dragon Phase5 Build OK: {GetOutputExe("Valgor-QA-Dragon-Phase5")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Dragon Phase5 Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Capital Blueprint: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalBlueprintCli</summary>
        public static void BuildCapitalBlueprintCli()
        {
            var report = Build("Valgor-QA-Capital-Blueprint");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Capital Blueprint Build OK: {GetOutputExe("Valgor-QA-Capital-Blueprint")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Capital Blueprint Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Capital Visual: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalVisualCli</summary>
        public static void BuildCapitalVisualCli()
        {
            var report = Build("Valgor-QA-Capital-Visual");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Capital Visual Build OK: {GetOutputExe("Valgor-QA-Capital-Visual")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Capital Visual Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Capital Art Pass 1: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalArtPass1Cli</summary>
        public static void BuildCapitalArtPass1Cli()
        {
            var report = Build("Valgor-QA-Capital-ArtPass1");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Capital ArtPass1 Build OK: {GetOutputExe("Valgor-QA-Capital-ArtPass1")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Capital ArtPass1 Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Capital Art Pass 2: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalArtPass2Cli</summary>
        public static void BuildCapitalArtPass2Cli()
        {
            var report = Build("Valgor-QA-Capital-ArtPass2");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Capital ArtPass2 Build OK: {GetOutputExe("Valgor-QA-Capital-ArtPass2")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Capital ArtPass2 Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Capital Final UI: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalFinalCli</summary>
        public static void BuildCapitalFinalCli()
        {
            var report = Build("Valgor-QA-Capital-Final");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Capital Final Build OK: {GetOutputExe("Valgor-QA-Capital-Final")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Capital Final Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Capital Art Pipeline: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalArtPipelineCli</summary>
        public static void BuildCapitalArtPipelineCli()
        {
            var report = Build("Valgor-QA-Capital-ArtPipeline");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Capital ArtPipeline Build OK: {GetOutputExe("Valgor-QA-Capital-ArtPipeline")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Capital ArtPipeline Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Capital Art Pass 3: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalArtPass3Cli</summary>
        public static void BuildCapitalArtPass3Cli()
        {
            var report = Build("Valgor-QA-Capital-ArtPass3");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Capital ArtPass3 Build OK: {GetOutputExe("Valgor-QA-Capital-ArtPass3")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Capital ArtPass3 Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Capital Sprint 1 Fortress: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalSprint1Cli</summary>
        public static void BuildCapitalSprint1Cli()
        {
            var report = Build("Valgor-QA-Capital-Sprint1");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Capital Sprint1 Build OK: {GetOutputExe("Valgor-QA-Capital-Sprint1")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Capital Sprint1 Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        /// <summary>CLI Capital Wall HF: -executeMethod Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalWallHfCli</summary>
        public static void BuildCapitalWallHfCli()
        {
            var report = Build("Valgor-QA-Capital-WallHF");
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] QA Capital WallHF Build OK: {GetOutputExe("Valgor-QA-Capital-WallHF")}");
            }
            else
            {
                Debug.LogError($"[Valgor] QA Capital WallHF Build FAIL: {report.summary.result}");
            }

            EditorApplication.Exit(code);
        }

        public static string GetOutputDir(string? folderNameOverride = null)
        {
            var clientRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var repoRoot = Path.GetFullPath(Path.Combine(clientRoot, ".."));
            var folder = string.IsNullOrEmpty(folderNameOverride) ? BuildFolderName : folderNameOverride;
            return Path.Combine(repoRoot, "builds", "windows", folder);
        }

        public static string GetOutputExe(string? folderNameOverride = null) =>
            Path.Combine(GetOutputDir(folderNameOverride), "Valgor.exe");
    }
}
