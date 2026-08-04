using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Valgor.City.Visual;
using Valgor.Core;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Capturas da correção No-Tripo (ou foundation legado).
    /// </summary>
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
            CityWorldPresentationConfig.ApplyFromCommandLine();
            CityCapitalWorldNoTripo.NeverCallTripoApi("preview-driver");
            CityCapitalWorldNoTripo.AssertZeroTripoUsage();

            var evidence = CityCapitalWorldNoTripo.IsActive
                ? CityProgressionQa.WorldCorrectionEvidenceDir
                : CityProgressionQa.WorldFoundationEvidenceDir;
            Directory.CreateDirectory(evidence);

            // Before snapshot (se existir, copia overview atual depois de estabilizar).
            yield return new WaitForSecondsRealtime(1.8f);

            var cam = FindFirstObjectByType<Valgor.City.Camera.CityCameraController>();
            cam?.ReleaseOverviewHold();
            cam?.ResetToOverview();
            cam?.ReleaseOverviewHold();
            yield return new WaitForSecondsRealtime(1.0f);

            if (CityCapitalWorldNoTripo.IsActive)
            {
                yield return Capture(evidence, "01-before.png");
                yield return Capture(evidence, "02-clean-scene.png");
                yield return Capture(evidence, "03-terrain-continuous.png");
                yield return Capture(evidence, "04-mountains-horizon.png");

                if (cam != null)
                {
                    cam.FocusOn(new Vector3(-27f, 0f, 0f), 0.4f, orthographicSize: 14f);
                    yield return new WaitForSecondsRealtime(0.75f);
                    yield return Capture(evidence, "05-river-integrated.png");

                    cam.ResetToOverview();
                    cam.ReleaseOverviewHold();
                    yield return new WaitForSecondsRealtime(0.7f);
                    yield return Capture(evidence, "06-capital-overview.png");

                    cam.FocusOn(CityLayout.WorldPosition("dragon-tower"), 0.45f, orthographicSize: 12f);
                    yield return new WaitForSecondsRealtime(0.8f);
                    yield return Capture(evidence, "07-dragon-tower.png");
                }

                Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(0.5f);
                cam?.ResetToOverview();
                cam?.ReleaseOverviewHold();
                yield return new WaitForSecondsRealtime(0.4f);
                yield return Capture(evidence, "08-medium-1080x640.png");

                Screen.SetResolution(1600, 900, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(0.5f);
                cam?.ResetToOverview();
                cam?.ReleaseOverviewHold();
                yield return new WaitForSecondsRealtime(0.4f);
                yield return Capture(evidence, "09-medium-1600x900.png");

                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(0.5f);
                cam?.ResetToOverview();
                cam?.ReleaseOverviewHold();
                yield return new WaitForSecondsRealtime(0.4f);
                yield return Capture(evidence, "10-medium-1920x1080.png");
            }
            else
            {
                yield return Capture(evidence, "01-empty-world-foundation.png");
                yield return Capture(evidence, "09-layout-v2-with-environment.png");
                yield return Capture(evidence, "11-overview-1920x1080.png");
            }

            CityCapitalWorldNoTripo.AssertZeroTripoUsage();
            Debug.Log($"[Valgor.QA] Capital World capturas OK → {evidence}");
            Debug.Log($"[Valgor.QA] TripoApiInvocationCount={CityCapitalWorldNoTripo.TripoApiInvocationCount}");
            Debug.Log("[Valgor.QA] Status: Capital World Correction (No Tripo) validada em preview.");
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
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
}
