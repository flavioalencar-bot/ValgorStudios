using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Valgor.Core;

namespace Valgor.Editor
{
    /// <summary>
    /// Build Windows da beta executÃ¡vel. Projeto fonte: client/ apenas.
    /// NÃ£o usa builds/_unity-beta-project (obsoleto).
    /// </summary>
    public static class BetaWindowsBuild
    {
        private static readonly string[] Scenes =
        {
            "Assets/_Valgor/Scenes/Bootstrap.unity",
            "Assets/_Valgor/Scenes/Loading.unity",
            "Assets/_Valgor/Scenes/MainMenu.unity",
            "Assets/Valgor/City/Scenes/City.unity",
            "Assets/Valgor/Heroes/Scenes/HeroesDemo.unity",
            "Assets/_Valgor/Scenes/WorldMap.unity"
        };

        [MenuItem("Valgor/Build/Windows Beta 0.2.4-Tier1")]
        public static void BuildFromMenu()
        {
            var report = Build();
            EditorUtility.DisplayDialog(
                "Valgor Beta 0.2.4-Tier1",
                report.summary.result == BuildResult.Succeeded
                    ? $"Build OK:\n{GetOutputExe()}"
                    : $"Build falhou: {report.summary.result}",
                "OK");
        }

        /// <summary>Entrada CLI: -executeMethod Valgor.Editor.BetaWindowsBuild.BuildCli</summary>
        public static void BuildCli()
        {
            var report = Build();
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code != 0)
            {
                Debug.LogError($"[Valgor] Build falhou: {report.summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type is LogType.Error or LogType.Exception)
                        {
                            Debug.LogError($"[Valgor] {msg.content}");
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"[Valgor] Build OK: {GetOutputExe()}");
            }

            EditorApplication.Exit(code);
        }

        public static BuildReport Build()
        {
            ApplyBetaPlayerSettings();
            if (!CastleTier1PrefabBuilder.Build(out var castleMsg))
            {
                Debug.LogWarning($"[Valgor] Castle Tier1 prefab: {castleMsg}");
            }
            else
            {
                Debug.Log($"[Valgor] Castle Tier1 prefab: {castleMsg}");
            }

            var outputDir = GetOutputDir();
            Directory.CreateDirectory(outputDir);
            var exe = Path.Combine(outputDir, "Valgor.exe");

            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,
                // Sem BuildOptions.Development — remove watermark "Development Build" da validação.
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log($"[Valgor] Building Windows {ValgorVersion.Display} → {exe}");
            Debug.Log($"[Valgor] Scenes: {string.Join(" | ", Scenes)}");
            return BuildPipeline.BuildPlayer(options);
        }

        private static void ApplyBetaPlayerSettings()
        {
            PlayerSettings.companyName = "Valgor Studios";
            PlayerSettings.productName = "Valgor";
            PlayerSettings.bundleVersion = ValgorVersion.Bundle;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.usePlayerLog = true;
            // Input System only: ProjectSettings.asset already has activeInputHandler: 1 (PlayerSettings API unavailable here).
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        }

        public static string GetOutputDir()
        {
            var clientRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var repoRoot = Path.GetFullPath(Path.Combine(clientRoot, ".."));
            return Path.Combine(repoRoot, "builds", "windows", ValgorVersion.BuildFolderName);
        }

        public static string GetOutputExe() => Path.Combine(GetOutputDir(), "Valgor.exe");
    }
}
