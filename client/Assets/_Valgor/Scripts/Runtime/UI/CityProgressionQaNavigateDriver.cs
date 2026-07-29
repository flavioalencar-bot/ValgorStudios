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

            yield return nav.GoToCity();
            yield return WaitForScene("City", 90f);
            Debug.Log("[Valgor.QA] City carregada — modo homologação pronto.");
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
