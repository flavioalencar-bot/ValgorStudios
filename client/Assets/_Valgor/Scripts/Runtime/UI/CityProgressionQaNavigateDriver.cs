using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valgor.Bootstrap;
using Valgor.Core;

namespace Valgor.UI
{
    /// <summary>
    /// Sessão QA: na build com VALGOR_CITY_PROGRESSION_QA (ou -cityProgressionQA)
    /// navega até a City após o MainMenu para homologação imediata.
    /// </summary>
    public sealed class CityProgressionQaNavigateDriver : MonoBehaviour
    {
        public static void EnsureFromCommandLine()
        {
            if (!CityProgressionQa.IsActive)
            {
                return;
            }

            if (FindFirstObjectByType<CityProgressionQaNavigateDriver>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(CityProgressionQaNavigateDriver));
            DontDestroyOnLoad(host);
            host.AddComponent<CityProgressionQaNavigateDriver>();
            Debug.Log(
                $"[Valgor.QA] Session ativa (compiledIn={CityProgressionQa.IsCompiledIn} " +
                $"autoTest={CityProgressionQa.IsAutoTest}).");
        }

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            Debug.Log("[Valgor.QA] Aguardando MainMenu…");
            yield return WaitForScene("MainMenu", 120f);
            EnsureLocalProfile();
            LocalPlayerProfile.MarkIntroDone();
            LocalPlayerProfile.TutorialStep = LocalPlayerProfile.TutorialSteps.Complete;
            PlayerPrefs.Save();
            yield return new WaitForSecondsRealtime(0.6f);

            if (CityProgressionQa.IsResponsiveUiTest)
            {
                Directory.CreateDirectory(CityProgressionQa.ResponsiveEvidenceDir);
                Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(0.6f);
                yield return CaptureMenu("00-main-menu-1080x640");
                Screen.SetResolution(1600, 900, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(0.35f);
            }

            var nav = GameBootstrap.Game?.Navigator;
            if (nav == null)
            {
                Debug.LogError("[Valgor.QA] Navigator indisponível.");
                if (CityProgressionQa.IsAutoTest || CityProgressionQa.IsResponsiveUiTest)
                {
                    Application.Quit(1);
                }

                yield break;
            }

            // Evita foco pendente (ex.: Torre) roubar o overview da Capital no boot QA.
            BetaFocusHints.PendingBuildingDefinitionId = null;
            yield return nav.GoToCity();
            yield return WaitForScene("City", 90f);
            yield return new WaitForSecondsRealtime(1.2f);
            yield return CaptureCityOverview();
            Debug.Log("[Valgor.QA] City carregada — modo homologação pronto.");
        }

        private static IEnumerator CaptureCityOverview()
        {
            var dir = @"C:\Valgor_Studio\docs\releases\capital-final-ui\screenshots";
            Directory.CreateDirectory(dir);
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.7f);
            FrameLookAt(Vector3.zero, 17.6f);
            yield return CaptureNamed(dir, "01-overview-1920x1080");
            yield return CaptureNamed(dir, "20-full-ui");
            yield return CaptureNamed(dir, "03-player-panel");
            yield return CaptureNamed(dir, "04-resource-bar");
            yield return CaptureNamed(dir, "05-build-queues");
            yield return CaptureNamed(dir, "06-top-menus");
            yield return CaptureNamed(dir, "07-bottom-menu");
            yield return CaptureNamed(dir, "08-chat");

            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.6f);
            FrameLookAt(Vector3.zero, 17.6f);
            yield return CaptureNamed(dir, "02-overview-1080x640");

            yield return CaptureLookAt(dir, "09-building-labels", new Vector3(0f, 0f, -2f), 12.5f);
            yield return CaptureLookAt(dir, "10-castle", new Vector3(0f, 0f, 0f), 14.5f);
            yield return CaptureLookAt(dir, "11-dragon-tower", new Vector3(0f, 1.3f, 17.6f), 12.5f);
            yield return CaptureLookAt(dir, "12-wall-gate", new Vector3(0f, 0f, -18f), 11f);
            yield return CaptureLookAt(dir, "13-military", new Vector3(12f, 0f, -3f), 11f);
            yield return CaptureLookAt(dir, "14-economic", new Vector3(-8f, 0f, -7f), 11f);
            yield return CaptureLookAt(dir, "15-industrial", new Vector3(13f, 0f, -12f), 11f);
            yield return CaptureLookAt(dir, "16-sacred", new Vector3(9f, 0f, 4.8f), 11f);
            yield return CaptureLookAt(dir, "17-rural", new Vector3(-14f, 0f, -7.5f), 11f);
            yield return CaptureLookAt(dir, "18-dragon-visible", new Vector3(0f, 1.3f, 17.6f), 11.5f);
            yield return CaptureNamed(dir, "19-dragon-deployed");
            FrameLookAt(Vector3.zero, 17.6f);
            Debug.Log($"[Valgor.QA] Capital Final captures → {dir}");
        }

        private static IEnumerator CaptureLookAt(string dir, string name, Vector3 worldTarget, float ortho)
        {
            FrameLookAt(worldTarget, ortho);
            yield return new WaitForSecondsRealtime(0.35f);
            yield return CaptureNamed(dir, name);
        }

        private static void FrameLookAt(Vector3 worldTarget, float ortho)
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null)
            {
                return;
            }

            cam.orthographic = true;
            cam.orthographicSize = ortho;
            var rot = Quaternion.Euler(46f, 40f, 0f);
            cam.transform.rotation = rot;
            var forward = rot * Vector3.forward;
            const float height = 19.2f;
            var t = height / Mathf.Max(0.05f, -forward.y);
            cam.transform.position = worldTarget - forward * t + Vector3.up * height;
        }

        private static IEnumerator CaptureNamed(string dir, string name)
        {
            var path = Path.Combine(dir, name + ".png");
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSecondsRealtime(1.2f);
            Debug.Log($"[Valgor.QA] Capture {path}");
        }

        private static IEnumerator CaptureMenu(string name)
        {
            yield return new WaitForEndOfFrame();
            var path = Path.Combine(CityProgressionQa.ResponsiveEvidenceDir, name + ".png");
            var usedFallback = false;
            try
            {
                var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                tex.Apply(false);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.Destroy(tex);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Valgor.QA] Menu capture: {ex.Message}");
                ScreenCapture.CaptureScreenshot(path);
                usedFallback = true;
            }

            if (usedFallback)
            {
                yield return new WaitForSecondsRealtime(2f);
            }
            else
            {
                yield return new WaitForSecondsRealtime(0.25f);
            }

            Debug.Log($"[Valgor.QA] Capture {path}");
        }

        private static void EnsureLocalProfile()
        {
            if (!LocalPlayerProfile.HasProfile)
            {
                if (!LocalPlayerProfile.Create("CityProgressionQA", out var error))
                {
                    Debug.LogError($"[Valgor.QA] Perfil: {error}");
                    return;
                }
            }

            LocalPlayerProfile.MarkIntroDone();
            LocalPlayerProfile.ApplyToSession(GameBootstrap.Game?.Session);
        }

        private static IEnumerator WaitForScene(string sceneName, float timeoutSec)
        {
            var t = 0f;
            while (t < timeoutSec)
            {
                if (string.Equals(SceneManager.GetActiveScene().name, sceneName, System.StringComparison.Ordinal))
                {
                    yield break;
                }

                t += 0.25f;
                yield return new WaitForSecondsRealtime(0.25f);
            }

            Debug.LogError($"[Valgor.QA] Timeout aguardando cena {sceneName}");
        }
    }
}
