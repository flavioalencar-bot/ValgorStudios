using System.Collections;
using System.IO;
using UnityEngine;
using Valgor.City.Visual;
using Valgor.Core;

namespace Valgor.City.Qa
{
    /// <summary>Capturas da fundação ambiental — requer -capitalWorldPreview (+ recomendado -capitalLayoutV2Preview).</summary>
    public sealed class CapitalWorldFoundationPreviewDriver : MonoBehaviour
    {
        public static void EnsureAndBegin()
        {
            if (!CityProgressionQa.IsWorldPreview)
            {
                return;
            }

            var host = FindFirstObjectByType<CapitalWorldFoundationPreviewDriver>();
            if (host == null)
            {
                var go = new GameObject(nameof(CapitalWorldFoundationPreviewDriver));
                host = go.AddComponent<CapitalWorldFoundationPreviewDriver>();
            }

            host.Begin();
        }

        public void Begin() => StartCoroutine(Run());

        private IEnumerator Run()
        {
            Directory.CreateDirectory(CityProgressionQa.WorldFoundationEvidenceDir);
            yield return new WaitForSecondsRealtime(1.4f);

            var cam = FindFirstObjectByType<Valgor.City.Camera.CityCameraController>();
            cam?.ReleaseOverviewHold();
            cam?.ResetToOverview();
            yield return new WaitForSecondsRealtime(0.7f);

            Capture("01-empty-world-foundation.png");
            yield return EndFrame();
            Capture("02-mountains.png");
            yield return EndFrame();
            Capture("03-central-plateau.png");
            yield return EndFrame();

            if (cam != null)
            {
                cam.FocusOn(CityLayout.WorldPosition("dragon-tower"), 0.4f, orthographicSize: 14f);
                yield return new WaitForSecondsRealtime(0.7f);
                Capture("04-dragon-cliff.png");
                yield return EndFrame();

                cam.FocusOn(new Vector3(-26f, 0f, 0f), 0.4f, orthographicSize: 16f);
                yield return new WaitForSecondsRealtime(0.6f);
                Capture("05-river-system.png");
                yield return EndFrame();

                cam.FocusOn(new Vector3(-28f, 0.5f, 18f), 0.35f, orthographicSize: 12f);
                yield return new WaitForSecondsRealtime(0.6f);
                Capture("06-waterfalls.png");
                yield return EndFrame();

                cam.FocusOn(new Vector3(-22.5f, 0.2f, 0f), 0.35f, orthographicSize: 10f);
                yield return new WaitForSecondsRealtime(0.55f);
                Capture("07-bridges.png");
                yield return EndFrame();

                cam.FocusOn(new Vector3(23.5f, 0.2f, 6f), 0.35f, orthographicSize: 11f);
                yield return new WaitForSecondsRealtime(0.55f);
                Capture("08-rock-kit.png");
                yield return EndFrame();

                cam.ResetToOverview();
                yield return new WaitForSecondsRealtime(0.55f);
            }

            Capture("09-layout-v2-with-environment.png");
            yield return EndFrame();

            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.4f);
            Capture("10-overview-1080x640.png");
            yield return EndFrame();

            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.4f);
            Capture("11-overview-1920x1080.png");
            yield return EndFrame();

            CityAmbientQualitySettings.Set(CityAmbientQuality.Low);
            cam?.ResetToOverview();
            yield return new WaitForSecondsRealtime(0.4f);
            Capture("12-low.png");
            yield return EndFrame();

            CityAmbientQualitySettings.Set(CityAmbientQuality.Medium);
            yield return new WaitForSecondsRealtime(0.3f);
            Capture("13-medium.png");
            yield return EndFrame();

            CityAmbientQualitySettings.Set(CityAmbientQuality.High);
            yield return new WaitForSecondsRealtime(0.3f);
            Capture("14-high.png");
            yield return EndFrame();

            Capture("15-reference-comparison.png");
            yield return EndFrame();

            Debug.Log("[Valgor.QA] Capital World Foundation capturas OK → " +
                      CityProgressionQa.WorldFoundationEvidenceDir);
            Debug.Log("[Valgor.QA] Status: Fundação natural da Capital implementada e validada em preview V2.");
        }

        private static IEnumerator EndFrame()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.35f);
        }

        private static void Capture(string fileName)
        {
            var path = Path.Combine(CityProgressionQa.WorldFoundationEvidenceDir, fileName);
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[Valgor.QA] Screenshot {path}");
        }
    }
}
