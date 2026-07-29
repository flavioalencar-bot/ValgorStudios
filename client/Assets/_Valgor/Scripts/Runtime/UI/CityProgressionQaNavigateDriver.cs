using System.Collections;
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

            var nav = GameBootstrap.Game?.Navigator;
            if (nav == null)
            {
                Debug.LogError("[Valgor.QA] Navigator indisponível.");
                if (CityProgressionQa.IsAutoTest)
                {
                    Application.Quit(1);
                }

                yield break;
            }

            yield return nav.GoToCity();
            yield return WaitForScene("City", 90f);
            Debug.Log("[Valgor.QA] City carregada — modo homologação pronto.");
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
