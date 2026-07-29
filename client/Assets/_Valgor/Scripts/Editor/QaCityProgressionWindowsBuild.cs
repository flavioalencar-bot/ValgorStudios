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

        public static BuildReport Build()
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

                var outputDir = GetOutputDir();
                Directory.CreateDirectory(outputDir);
                var exe = Path.Combine(outputDir, "Valgor.exe");

                var options = new BuildPlayerOptions
                {
                    scenes = Scenes,
                    locationPathName = exe,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.CompressWithLz4HC,
                    // Define só nesta build — duplo clique ativa QA sem argumento CLI.
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

        public static string GetOutputDir()
        {
            var clientRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var repoRoot = Path.GetFullPath(Path.Combine(clientRoot, ".."));
            return Path.Combine(repoRoot, "builds", "windows", BuildFolderName);
        }

        public static string GetOutputExe() => Path.Combine(GetOutputDir(), "Valgor.exe");
    }
}
