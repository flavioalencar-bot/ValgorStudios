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
            FrameLookAt(Vector3.zero, 22f);
            yield return CaptureNamed(dir, "01-overview-1920x1080");

            // Sprint 3 — capturar cedo (antes do AutoTest encerrar o processo).
            var sprint3Dir = @"C:\Valgor_Studio\docs\releases\capital-sprint3\screenshots";
            System.IO.Directory.CreateDirectory(sprint3Dir);
            yield return CaptureLookAt(sprint3Dir, "01-overview", new Vector3(0f, 0.6f, -2f), 24f);
            yield return CaptureLookAt(sprint3Dir, "02-castelo", new Vector3(0f, 0.2f, 0f), 13f);
            yield return CaptureLookAt(sprint3Dir, "03-mercado", new Vector3(-8.6f, 0.3f, -6.4f), 10f);
            yield return CaptureLookAt(sprint3Dir, "04-hospital", new Vector3(-9.2f, 0.3f, 4.2f), 10f);
            yield return CaptureLookAt(sprint3Dir, "05-arena", new Vector3(11.2f, 0.3f, 0.8f), 11f);
            yield return CaptureLookAt(sprint3Dir, "06-templo", new Vector3(9.0f, 0.3f, 4.8f), 10f);
            yield return CaptureLookAt(sprint3Dir, "07-herohall", new Vector3(-6.8f, 0.3f, 5.2f), 10f);
            yield return CaptureLookAt(sprint3Dir, "08-fundicao", new Vector3(12.8f, 0.3f, -13.0f), 10f);
            yield return CaptureLookAt(sprint3Dir, "09-fazenda", new Vector3(-14.2f, 0.3f, -7.5f), 11f);
            yield return CaptureLookAt(sprint3Dir, "10-mina", new Vector3(-15.0f, 1.0f, 11.8f), 10f);
            yield return CaptureLookAt(sprint3Dir, "11-pedreira", new Vector3(15.2f, 1.0f, 12.2f), 10f);
            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return null;
            yield return CaptureNamed(sprint3Dir, "12-1080x640");
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            yield return null;
            yield return CaptureNamed(sprint3Dir, "13-1920x1080");

            yield return CaptureNamed(dir, "20-full-ui");
            yield return CaptureNamed(dir, "03-player-panel");
            yield return CaptureNamed(dir, "04-resource-bar");
            yield return CaptureNamed(dir, "05-build-queues");
            yield return CaptureNamed(dir, "06-top-menus");
            yield return CaptureNamed(dir, "07-bottom-menu");
            yield return CaptureNamed(dir, "08-chat");

            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.6f);
            FrameLookAt(Vector3.zero, 22f);
            yield return CaptureNamed(dir, "02-overview-1080x640");

            yield return CaptureLookAt(dir, "09-building-labels", new Vector3(0f, 0f, -2f), 12.5f);
            yield return CaptureLookAt(dir, "10-castle", new Vector3(0f, 0f, 0f), 14.5f);
            yield return CaptureLookAt(dir, "11-dragon-tower", new Vector3(0f, 1.3f, 25.5f), 11.5f);

            // Evidência apresentação Torre (escala/platô/norte).
            var fixDir = @"C:\Valgor_Studio\docs\releases\capital-dragon-tower-presentation\screenshots";
            Directory.CreateDirectory(fixDir);
            yield return CaptureLookAt(fixDir, "01-castle-dominant", new Vector3(0f, 0f, 0f), 14.5f);
            yield return CaptureLookAt(fixDir, "02-dragon-tower-plateau", new Vector3(0f, 1.3f, 25.5f), 11.5f);
            yield return CaptureLookAt(fixDir, "03-overview-separated", new Vector3(0f, 0.5f, 11f), 24f);
            yield return CaptureLookAt(fixDir, "04-axis-castle-to-tower", new Vector3(0f, 0.8f, 13f), 20f);

            yield return CaptureLookAt(dir, "12-wall-gate", new Vector3(0f, 0f, -19.2f), 11f);

            // Evidência muralha HF.
            var wallDir = @"C:\Valgor_Studio\docs\releases\capital-wall-high-fidelity\screenshots";
            Directory.CreateDirectory(wallDir);
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.4f);
            yield return CaptureLookAt(wallDir, "01-overview", new Vector3(0f, 0.5f, -4f), 22f);
            yield return CaptureLookAt(wallDir, "02-portao", new Vector3(0f, 0f, -19.2f), 11f);
            yield return CaptureLookAt(wallDir, "03-closeup", new Vector3(0f, 1.2f, -19.2f), 8.5f);
            yield return CaptureLookAt(wallDir, "04-isometrica", new Vector3(0f, 0.8f, -12f), 16f);
            yield return CaptureLookAt(wallDir, "05-selecao", new Vector3(0f, 0.5f, -19.2f), 10.5f);
            yield return CaptureLookAt(wallDir, "06-painel", new Vector3(0f, 0.5f, -19.2f), 10.5f);
            yield return CaptureLookAt(wallDir, "07-castelo", new Vector3(0f, 0f, 0f), 14.5f);
            yield return CaptureLookAt(wallDir, "08-torre", new Vector3(0f, 1.3f, 25.5f), 11.5f);
            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.45f);
            FrameLookAt(new Vector3(0f, 0.5f, -4f), 22f);
            yield return CaptureNamed(wallDir, "09-overview1080x640");

            var art3Dir = @"C:\Valgor_Studio\docs\releases\city-capital-artpass3\screenshots";
            System.IO.Directory.CreateDirectory(art3Dir);
            yield return CaptureLookAt(art3Dir, "01-overview", new Vector3(0f, 0.5f, -2f), 24f);
            yield return CaptureLookAt(art3Dir, "02-muralha-fechada", new Vector3(0f, 0.8f, -8f), 20f);
            yield return CaptureLookAt(art3Dir, "03-portao-hf", new Vector3(0f, 0.5f, -19.2f), 11f);
            yield return CaptureLookAt(art3Dir, "04-distrito-real", new Vector3(0f, 0f, 0f), 14f);
            yield return CaptureLookAt(art3Dir, "05-distrito-dragoes", new Vector3(0f, 1.3f, 25.5f), 12f);
            yield return CaptureLookAt(art3Dir, "06-distrito-economico", new Vector3(-8f, 0f, -7f), 11f);
            yield return CaptureLookAt(art3Dir, "07-distrito-militar", new Vector3(12f, 0f, -3f), 11f);
            yield return CaptureLookAt(art3Dir, "08-vegetacao", new Vector3(-14f, 0.5f, 4f), 14f);
            yield return CaptureLookAt(art3Dir, "09-estradas-agua", new Vector3(-18f, 0.5f, 0f), 16f);
            yield return CaptureLookAt(art3Dir, "10-atmosfera", new Vector3(0f, 0.5f, -10f), 13f);
            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return null;
            yield return CaptureNamed(art3Dir, "11-overview1080x640");

            var sprint1Dir = @"C:\Valgor_Studio\docs\releases\capital-wall-sprint1\screenshots";
            System.IO.Directory.CreateDirectory(sprint1Dir);
            yield return CaptureLookAt(sprint1Dir, "01-overview", new Vector3(0f, 0.6f, -2f), 24f);
            yield return CaptureLookAt(sprint1Dir, "02-portao", new Vector3(0f, 0.4f, -19.2f), 11f);
            yield return CaptureLookAt(sprint1Dir, "03-torre", new Vector3(14f, 1.2f, 0f), 12f);
            yield return CaptureLookAt(sprint1Dir, "04-canto", new Vector3(14f, 1.0f, 14f), 13f);
            yield return CaptureLookAt(sprint1Dir, "05-topo-da-muralha", new Vector3(10f, 4.5f, -8f), 10f);
            yield return CaptureLookAt(sprint1Dir, "06-perspectiva-norte", new Vector3(0f, 1.0f, 18f), 20f);
            yield return CaptureLookAt(sprint1Dir, "07-perspectiva-sul", new Vector3(0f, 0.8f, -14f), 18f);
            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return null;
            yield return CaptureNamed(sprint1Dir, "08-1080x640");
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            yield return null;
            yield return CaptureNamed(sprint1Dir, "09-1920x1080");

            var sprint2Dir = @"C:\Valgor_Studio\docs\releases\capital-sprint2\screenshots";
            System.IO.Directory.CreateDirectory(sprint2Dir);
            yield return CaptureLookAt(sprint2Dir, "01-overview", new Vector3(0f, 0.6f, -2f), 22f);
            yield return CaptureLookAt(sprint2Dir, "02-praca", new Vector3(0f, 0.4f, -3.2f), 12f);
            yield return CaptureLookAt(sprint2Dir, "03-fonte", new Vector3(0f, 0.8f, -3.2f), 8.5f);
            yield return CaptureLookAt(sprint2Dir, "04-castelo", new Vector3(0f, 0.2f, 0f), 13f);
            yield return CaptureLookAt(sprint2Dir, "05-jardins", new Vector3(-4f, 0.4f, -2f), 11f);
            yield return CaptureLookAt(sprint2Dir, "06-residencias", new Vector3(-6f, 0.5f, 2f), 11f);
            yield return CaptureLookAt(sprint2Dir, "07-portao", new Vector3(0f, 0.4f, -19.2f), 11f);
            yield return CaptureLookAt(sprint2Dir, "08-visao-aerea", new Vector3(0f, 1.0f, 2f), 26f);
            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return null;
            yield return CaptureNamed(sprint2Dir, "09-1080x640");
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            yield return null;
            yield return CaptureNamed(sprint2Dir, "10-1920x1080");

            yield return CaptureLookAt(dir, "13-military", new Vector3(12f, 0f, -3f), 11f);
            yield return CaptureLookAt(dir, "14-economic", new Vector3(-8f, 0f, -7f), 11f);
            yield return CaptureLookAt(dir, "15-industrial", new Vector3(13f, 0f, -12f), 11f);
            yield return CaptureLookAt(dir, "16-sacred", new Vector3(9f, 0f, 4.8f), 11f);
            yield return CaptureLookAt(dir, "17-rural", new Vector3(-14f, 0f, -7.5f), 11f);
            yield return CaptureLookAt(dir, "18-dragon-visible", new Vector3(0f, 1.3f, 25.5f), 11.5f);
            yield return CaptureNamed(dir, "19-dragon-deployed");
            FrameLookAt(Vector3.zero, 22f);
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

            var camGo = cam.gameObject;
            camGo.SendMessage("ReleaseOverviewHold", SendMessageOptions.DontRequireReceiver);
            camGo.SendMessage("SuppressFocus", 3f, SendMessageOptions.DontRequireReceiver);

            cam.orthographic = true;
            cam.orthographicSize = ortho;
            var rot = Quaternion.Euler(46f, 40f, 0f);
            cam.transform.rotation = rot;
            var forward = rot * Vector3.forward;
            var height = Mathf.Clamp(ortho * 1.05f, 14f, 28f);
            var t = height / Mathf.Max(0.05f, -forward.y);
            // Sem somar height de novo — isso deslocava a cidade para o rodapé da tela.
            cam.transform.position = worldTarget - forward * t;
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
