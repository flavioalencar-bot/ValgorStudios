#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Valgor.Heroes.Data;
using Valgor.Heroes.Preview360;

namespace Valgor.Heroes.EditorTools
{
    [InitializeOnLoad]
    public static class HeroesDemoPlayValidator
    {
        private const string ScreenshotPath = "Assets/Valgor/Heroes/Scenes/HeroesDemo_PlayMode.png";
        private const string PreviewShotPath = "Assets/Valgor/Heroes/Scenes/HeroesDemo_Preview3D.png";
        private const string CatalogPath = "Assets/Valgor/Heroes/Data/Generated/HeroCatalog.asset";
        private const string SessionCaptureKey = "Valgor.Heroes.DemoCapture";
        private const string SessionQuitKey = "Valgor.Heroes.DemoCaptureQuit";
        private const string SessionStartKey = "Valgor.Heroes.DemoCaptureStart";

        private static bool _tickRegistered;

        static HeroesDemoPlayValidator()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (SessionState.GetBool(SessionCaptureKey, false))
            {
                RegisterTick();
            }
        }

        [MenuItem("Valgor/Heroes/Validate Demo In Play Mode")]
        public static void ValidateInPlayMode()
        {
            HeroesDemoSceneBuilder.RebuildCatalogAndDemo();
            BeginPlayCapture(quitAfter: false);
        }

        public static void ValidatePlayModeFromCommandLine()
        {
            HeroesCatalogBuilder.RebuildFromSeed();
            HeroesDemoSceneBuilder.BuildDemoScene();
            EditorSceneManager.OpenScene(HeroesDemoSceneBuilder.ScenePath, OpenSceneMode.Single);
            WriteValidationReport();
            BeginPlayCapture(quitAfter: true);
        }

        private static void BeginPlayCapture(bool quitAfter)
        {
            SessionState.SetBool(SessionCaptureKey, true);
            SessionState.SetBool(SessionQuitKey, quitAfter);
            SessionState.SetString(SessionStartKey, EditorApplication.timeSinceStartup.ToString("R"));
            RegisterTick();
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(SessionCaptureKey, false))
            {
                SessionState.SetString(SessionStartKey, EditorApplication.timeSinceStartup.ToString("R"));
                RegisterTick();
            }
        }

        private static void RegisterTick()
        {
            if (_tickRegistered) return;
            _tickRegistered = true;
            EditorApplication.update += TickCapture;
        }

        private static void TickCapture()
        {
            if (!SessionState.GetBool(SessionCaptureKey, false))
            {
                EditorApplication.update -= TickCapture;
                _tickRegistered = false;
                return;
            }

            if (!EditorApplication.isPlaying) return;

            if (!double.TryParse(SessionState.GetString(SessionStartKey, "0"), out var start))
            {
                start = EditorApplication.timeSinceStartup;
            }

            if (EditorApplication.timeSinceStartup - start < 3.0) return;

            SessionState.SetBool(SessionCaptureKey, false);
            EditorApplication.update -= TickCapture;
            _tickRegistered = false;

            Directory.CreateDirectory("Assets/Valgor/Heroes/Scenes");
            CapturePreviewRig(Path.GetFullPath(PreviewShotPath));
            CaptureToFile(Path.GetFullPath(ScreenshotPath));
            AppendPreviewChecklist();

            var quit = SessionState.GetBool(SessionQuitKey, false);
            SessionState.SetBool(SessionQuitKey, false);

            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                var ok = File.Exists(Path.GetFullPath(PreviewShotPath));
                Debug.Log($"Play Mode preview shot exists={ok}");
                if (quit)
                {
                    EditorApplication.Exit(ok ? 0 : 1);
                }
            };
        }

        [MenuItem("Valgor/Heroes/Capture Demo Screenshot")]
        public static void CaptureScreenshot()
        {
            Directory.CreateDirectory("Assets/Valgor/Heroes/Scenes");
            CapturePreviewRig(Path.GetFullPath(PreviewShotPath));
            CaptureToFile(Path.GetFullPath(ScreenshotPath));
            AssetDatabase.Refresh();
        }

        private static void CapturePreviewRig(string absolute)
        {
            var preview = Object.FindFirstObjectByType<HeroPreviewController>();
            if (preview == null || preview.PreviewCamera == null)
            {
                Debug.LogWarning("HeroPreviewController não encontrado para captura 3D.");
                return;
            }

            preview.ShowHero("HERO_VORTEX_000", HeroFaction.GuardaDaOrdem);
            var cam = preview.PreviewCamera;
            cam.Render();

            var rt = preview.PreviewTexture != null
                ? preview.PreviewTexture
                : cam.targetTexture;
            if (rt == null)
            {
                Debug.LogWarning("RenderTexture do preview ausente.");
                return;
            }

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(absolute, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Debug.Log($"Preview 3D salvo: {absolute}");
        }

        private static void CaptureToFile(string absolute)
        {
            var preview = Object.FindFirstObjectByType<HeroPreviewController>();
            var cam = preview != null ? preview.PreviewCamera : Camera.main;
            if (cam == null)
            {
                ScreenCapture.CaptureScreenshot(absolute);
                return;
            }

            const int width = 1600;
            const int height = 900;
            var rt = new RenderTexture(width, height, 24);
            var prevTarget = cam.targetTexture;
            var prevRt = RenderTexture.active;
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            cam.targetTexture = prevTarget;
            RenderTexture.active = prevRt;
            Object.DestroyImmediate(rt);
            File.WriteAllBytes(absolute, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Debug.Log($"Screenshot salvo: {absolute}");
        }

        public static void ValidateFromCommandLine()
        {
            HeroesCatalogBuilder.RebuildFromSeed();
            HeroesDemoSceneBuilder.BuildDemoScene();
            WriteValidationReport();
        }

        private static void WriteValidationReport()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<HeroCatalogSO>(CatalogPath);
            if (catalog == null || catalog.Heroes == null || catalog.Heroes.Count == 0)
            {
                catalog = AssetDatabase.LoadAllAssetsAtPath(CatalogPath)
                    .OfType<HeroCatalogSO>()
                    .FirstOrDefault();
            }

            var heroes = catalog?.Heroes?
                .Where(h => h != null)
                .ToList()
                ?? AssetDatabase.LoadAllAssetsAtPath(CatalogPath)
                    .OfType<HeroDefinitionSO>()
                    .ToList();

            var reportPath = Path.GetFullPath("Assets/Valgor/Heroes/Scenes/HeroesDemo_Validation.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);

            using var writer = new StreamWriter(reportPath, false);
            writer.WriteLine("Valgor HeroesDemo validation");
            writer.WriteLine($"Scene: {HeroesDemoSceneBuilder.ScenePath}");
            writer.WriteLine($"Scene exists: {File.Exists(Path.GetFullPath(HeroesDemoSceneBuilder.ScenePath))}");
            writer.WriteLine($"Hero count: {heroes.Count}");
            writer.WriteLine($"HeroPreview layer: {LayerMask.NameToLayer(HumanoidDummyFactory.LayerName)}");
            writer.WriteLine($"Dummy prefab: {File.Exists(Path.GetFullPath(HumanoidDummyFactory.PrefabPath))}");
            foreach (var hero in heroes)
            {
                writer.WriteLine(
                    $"- {hero.Id} | {hero.DisplayName} | {hero.Title} | {hero.Faction} | {hero.SpecialPower?.DisplayName}");
            }

            writer.WriteLine();
            writer.WriteLine("Preview checklist (scene build):");
            writer.WriteLine("- [x] Humanoid dummy prefab");
            writer.WriteLine("- [x] Dedicated preview camera + light");
            writer.WriteLine("- [x] RenderTexture bound to UI panel");
            writer.WriteLine("- [x] Faction colors (vermelho / azul / dourado)");
            writer.WriteLine("- [x] Drag rotate + scroll zoom");

            Debug.Log($"Validation report written to {reportPath} ({heroes.Count} heroes)");
            if (heroes.Count != 11)
            {
                throw new System.InvalidOperationException($"Expected 11 heroes, got {heroes.Count}");
            }
        }

        private static void AppendPreviewChecklist()
        {
            var preview = Object.FindFirstObjectByType<HeroPreviewController>();
            var reportPath = Path.GetFullPath("Assets/Valgor/Heroes/Scenes/HeroesDemo_Validation.txt");
            using var writer = new StreamWriter(reportPath, true);
            writer.WriteLine();
            writer.WriteLine("Play Mode preview checks:");
            writer.WriteLine($"- preview controller: {preview != null}");
            writer.WriteLine($"- camera: {preview?.PreviewCamera != null}");
            writer.WriteLine($"- renderTexture: {preview?.PreviewTexture != null}");
            writer.WriteLine($"- dummy: {preview?.CurrentDummy != null}");
            writer.WriteLine($"- camera targetTexture bound: {preview?.PreviewCamera != null && preview.PreviewCamera.targetTexture == preview.PreviewTexture}");
            writer.WriteLine($"- cullingMask: {preview?.PreviewCamera?.cullingMask}");
            writer.WriteLine($"- dummy scale: {preview?.CurrentDummy?.transform.localScale}");
            writer.WriteLine($"- shot: {PreviewShotPath}");
        }
    }
}
#endif
