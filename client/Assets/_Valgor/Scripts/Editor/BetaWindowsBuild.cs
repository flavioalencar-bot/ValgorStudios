using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Valgor.Core;

namespace Valgor.Editor
{
    public static class BetaWindowsBuild
    {
        private static readonly string[] Scenes =
        {
            "Assets/_Valgor/Scenes/Bootstrap.unity",
            "Assets/_Valgor/Scenes/Loading.unity",
            "Assets/_Valgor/Scenes/MainMenu.unity",
            "Assets/Valgor/City/Scenes/City.unity",
            "Assets/_Valgor/Scenes/WorldMap.unity",
            "Assets/Valgor/Heroes/Scenes/HeroesDemo.unity"
        };

        [MenuItem("Valgor/Build/Windows Beta Técnica 0.1")]
        public static void BuildFromMenu()
        {
            var report = Build();
            EditorUtility.DisplayDialog(
                "Valgor Beta Build",
                report.summary.result == BuildResult.Succeeded
                    ? $"Build OK:\n{GetOutputExe()}"
                    : $"Build falhou: {report.summary.result}",
                "OK");
        }

        /// <summary>Entrada para Unity -batchmode -executeMethod Valgor.Editor.BetaWindowsBuild.BuildCli</summary>
        public static void BuildCli()
        {
            var report = Build();
            var code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
            if (code != 0)
            {
                Debug.LogError($"[Valgor] Build falhou: {report.summary.result}");
            }
            else
            {
                Debug.Log($"[Valgor] Build OK: {GetOutputExe()}");
            }

            EditorApplication.Exit(code);
        }

        public static BuildReport Build()
        {
            var outputDir = GetOutputDir();
            Directory.CreateDirectory(outputDir);
            var exe = Path.Combine(outputDir, "Valgor.exe");

            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log($"[Valgor] Building Windows Beta → {exe}");
            return BuildPipeline.BuildPlayer(options);
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
