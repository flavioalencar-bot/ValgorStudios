using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Valgor.City.Visual;
using Valgor.Core;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Capturas da Capital restaurada (sem World Foundation / sem galeria).
    /// Ativo com -capitalLayoutV2Preview e sem -capitalWorldPreview.
    /// </summary>
    public sealed class CapitalRestoredCaptureDriver : MonoBehaviour
    {
        public static void EnsureAndBegin()
        {
            if (!CityProgressionQa.IsLayoutV2Preview || CityProgressionQa.IsWorldPreview)
            {
                return;
            }

            if (CityCapitalWorldConfig.Enabled)
            {
                Debug.LogError(
                    "[Valgor.QA] Capital Restored: World Foundation ainda ativo — abortando captura.");
                return;
            }

            var host = FindFirstObjectByType<CapitalRestoredCaptureDriver>();
            if (host == null)
            {
                var go = new GameObject(nameof(CapitalRestoredCaptureDriver));
                host = go.AddComponent<CapitalRestoredCaptureDriver>();
            }

            host.Begin();
        }

        public void Begin() => StartCoroutine(Run());

        private IEnumerator Run()
        {
            CityCapitalWorldNoTripo.NeverCallTripoApi("restored-capture");
            CityCapitalWorldNoTripo.AssertZeroTripoUsage();

            var dir = CityProgressionQa.RestoredEvidenceDir;
            Directory.CreateDirectory(dir);

            yield return new WaitForSecondsRealtime(1.8f);

            var cam = FindFirstObjectByType<Valgor.City.Camera.CityCameraController>();
            cam?.ReleaseOverviewHold();
            cam?.ResetToOverview();
            cam?.ReleaseOverviewHold();
            yield return new WaitForSecondsRealtime(0.35f);

            // Força enquadramento restored (Capital ~70%) — evita SerializeField antigo.
            var unityCam = global::UnityEngine.Camera.main;
            if (unityCam != null)
            {
                unityCam.orthographic = true;
                unityCam.orthographicSize = CityCapitalLayoutRuntime.IsPreviewV2 ? 13.5f : 14f;
                Debug.Log(
                    $"[Valgor.QA] Restored camera orthoSize={unityCam.orthographicSize} " +
                    $"pos={unityCam.transform.position} WorldEnabled={CityCapitalWorldConfig.Enabled}");
            }

            yield return new WaitForSecondsRealtime(0.65f);

            // 02 — overview principal (Capital ~70% da tela).
            yield return Capture(dir, "02-restored-capital.png");

            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.5f);
            cam?.ResetToOverview();
            cam?.ReleaseOverviewHold();
            yield return new WaitForSecondsRealtime(0.45f);
            yield return Capture(dir, "03-restored-1080x640.png");

            Screen.SetResolution(1600, 900, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.5f);
            cam?.ResetToOverview();
            cam?.ReleaseOverviewHold();
            yield return new WaitForSecondsRealtime(0.45f);
            yield return Capture(dir, "04-restored-1600x900.png");

            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.5f);
            cam?.ResetToOverview();
            cam?.ReleaseOverviewHold();
            yield return new WaitForSecondsRealtime(0.45f);
            yield return Capture(dir, "05-restored-1920x1080.png");

            CityCapitalWorldNoTripo.AssertZeroTripoUsage();
            Debug.Log(
                $"[Valgor.QA] Capital Restored capturas OK → {dir} " +
                $"(TripoApiInvocationCount={CityCapitalWorldNoTripo.TripoApiInvocationCount})");
            Debug.Log(
                "[Valgor.QA] Zero Tripo: calls=0 tokens=0 jobs=0 retries=0 | " +
                $"WorldEnabled={CityCapitalWorldConfig.Enabled}");
        }

        private static IEnumerator Capture(string dir, string fileName)
        {
            yield return new WaitForEndOfFrame();
            var path = Path.Combine(dir, fileName);
            try
            {
                var w = Screen.width;
                var h = Screen.height;
                var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply(false);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                UnityEngine.Object.Destroy(tex);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Valgor.QA] ReadPixels falhou ({ex.Message}); fallback CaptureScreenshot");
                ScreenCapture.CaptureScreenshot(path);
            }

            Debug.Log($"[Valgor.QA] Screenshot {path}");
            yield return new WaitForSecondsRealtime(0.25f);
        }
    }
}
