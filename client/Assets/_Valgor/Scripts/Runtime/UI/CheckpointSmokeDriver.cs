using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valgor.Bootstrap;
using Valgor.Core;
using Valgor.Navigation;

namespace Valgor.UI
{
    /// <summary>
    /// QA da build Checkpoint. Ativa só com -checkpointSmoke.
    /// Com -captureEvidence também grava PNGs das telas principais.
    /// </summary>
    public sealed class CheckpointSmokeDriver : MonoBehaviour
    {
        private bool _captureEvidence;
        private string _evidenceDir = string.Empty;

        public static void EnsureFromCommandLine()
        {
            if (!Environment.GetCommandLineArgs().Any(a =>
                    string.Equals(a, "-checkpointSmoke", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (FindFirstObjectByType<CheckpointSmokeDriver>() != null)
            {
                return;
            }

            var host = new GameObject("CheckpointSmokeDriver");
            DontDestroyOnLoad(host);
            host.AddComponent<CheckpointSmokeDriver>();
            Debug.Log("[CheckpointSmoke] Driver ativo.");
        }

        private IEnumerator Start()
        {
            _captureEvidence = Environment.GetCommandLineArgs().Any(a =>
                string.Equals(a, "-captureEvidence", StringComparison.OrdinalIgnoreCase));
            if (_captureEvidence)
            {
                _evidenceDir = Path.Combine(Application.dataPath, "..", "evidence");
                Directory.CreateDirectory(_evidenceDir);
                Debug.Log($"[CheckpointSmoke] Evidências → {_evidenceDir}");
            }

            Debug.Log("[CheckpointSmoke] Aguardando MainMenu…");
            yield return WaitForScene(SceneIds.MainMenu, 120f);
            Debug.Log("[CheckpointSmoke] MainMenu OK");
            yield return new WaitForSecondsRealtime(1.5f);

            EnsureLocalProfile();
            LocalPlayerProfile.TutorialStep = LocalPlayerProfile.TutorialSteps.Complete;
            PlayerPrefs.Save();
            yield return new WaitForSecondsRealtime(0.5f);

            yield return Navigate(n => n.GoToCity());
            yield return WaitForScene(SceneIds.City, 90f);
            Debug.Log("[CheckpointSmoke] City OK");
            yield return new WaitForSecondsRealtime(3f);
            yield return Capture("01-city");

            yield return Navigate(n => n.GoToHeroes());
            yield return WaitForScene(SceneIds.Heroes, 90f);
            Debug.Log("[CheckpointSmoke] HeroesDemo OK");
            yield return new WaitForSecondsRealtime(4f);
            yield return Capture("02-heroes-vortex");

            yield return Navigate(n => n.GoToWorldMap());
            yield return WaitForScene(SceneIds.WorldMap, 90f);
            Debug.Log("[CheckpointSmoke] WorldMap OK");
            yield return new WaitForSecondsRealtime(3f);
            yield return Capture("03-worldmap");

            TrySelectFirstWorldNode();
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Capture("04-worldmap-node-selected");

            TryExpandWorldMapFilters();
            yield return new WaitForSecondsRealtime(1f);
            yield return Capture("05-worldmap-filters-open");

            yield return Navigate(n => n.GoToCity());
            yield return WaitForScene(SceneIds.City, 90f);
            Debug.Log("[CheckpointSmoke] Jornada mínima concluída.");
            yield return new WaitForSecondsRealtime(1f);
            Application.Quit(0);
        }

        private IEnumerator Capture(string name)
        {
            if (!_captureEvidence)
            {
                yield break;
            }

            // Aguarda frames para ScreenCapture assíncrono não sobrescrever o PNG anterior.
            yield return new WaitForEndOfFrame();
            var path = Path.GetFullPath(Path.Combine(_evidenceDir, name + ".png"));
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[CheckpointSmoke] Captura: {path}");
            yield return new WaitForEndOfFrame();
            // ScreenCapture é assíncrono no player — aguarda flush do PNG.
            yield return new WaitForSecondsRealtime(2.5f);
            for (var i = 0; i < 20 && !File.Exists(path); i++)
            {
                yield return new WaitForSecondsRealtime(0.25f);
            }
        }

        private static void EnsureLocalProfile()
        {
            if (!LocalPlayerProfile.HasProfile)
            {
                if (!LocalPlayerProfile.Create("BetaSmoke", out var error))
                {
                    Debug.LogError($"[CheckpointSmoke] Falha ao criar perfil: {error}");
                    return;
                }

                Debug.Log("[CheckpointSmoke] Perfil local criado.");
            }

            LocalPlayerProfile.MarkIntroDone();
            LocalPlayerProfile.ApplyToSession(GameBootstrap.Game?.Session);
        }

        private static IEnumerator Navigate(Func<GameNavigator, IEnumerator> action)
        {
            var nav = GameBootstrap.Game?.Navigator;
            if (nav == null)
            {
                Debug.LogError("[CheckpointSmoke] Navigator indisponível.");
                yield break;
            }

            yield return action(nav);
        }

        private static IEnumerator WaitForScene(string sceneName, float timeoutSec)
        {
            var t = 0f;
            while (t < timeoutSec)
            {
                if (string.Equals(SceneManager.GetActiveScene().name, sceneName, StringComparison.Ordinal))
                {
                    yield break;
                }

                t += 0.25f;
                yield return new WaitForSecondsRealtime(0.25f);
            }

            Debug.LogError(
                $"[CheckpointSmoke] Timeout cena {sceneName}. Atual={SceneManager.GetActiveScene().name}");
        }

        private static void TrySelectCityBuilding(string definitionId)
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null || !string.Equals(mb.GetType().Name, "CityBootstrap", StringComparison.Ordinal))
                {
                    continue;
                }

                var controllerProp = mb.GetType().GetProperty("Controller");
                var controller = controllerProp?.GetValue(mb);
                var method = controller?.GetType().GetMethod("TrySelectByDefinitionId");
                if (method == null)
                {
                    continue;
                }

                var result = method.Invoke(controller, new object[] { definitionId });
                if (result is true)
                {
                    Debug.Log($"[CheckpointSmoke] Selecionado: {definitionId}");
                    return;
                }
            }

            Debug.LogWarning($"[CheckpointSmoke] Não foi possível selecionar {definitionId}.");
        }

        private static void TrySelectFirstWorldNode()
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null || !string.Equals(mb.GetType().Name, "WorldNodeView", StringComparison.Ordinal))
                {
                    continue;
                }

                mb.SendMessage("OnMouseUpAsButton", SendMessageOptions.DontRequireReceiver);
                Debug.Log("[CheckpointSmoke] Nó selecionado (OnMouseUpAsButton).");
                return;
            }

            Debug.LogWarning("[CheckpointSmoke] Nenhum WorldNodeView encontrado.");
        }

        private static void TryExpandWorldMapFilters()
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null || !string.Equals(mb.GetType().Name, "WorldMapHudController", StringComparison.Ordinal))
                {
                    continue;
                }

                var field = mb.GetType().GetField("_filterPanel",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var panel = field?.GetValue(mb);
                var expand = panel?.GetType().GetMethod("Expand");
                expand?.Invoke(panel, null);
                Debug.Log("[CheckpointSmoke] Filtros expandidos.");
                return;
            }

            Debug.LogWarning("[CheckpointSmoke] Filtros não encontrados.");
        }
    }
}
