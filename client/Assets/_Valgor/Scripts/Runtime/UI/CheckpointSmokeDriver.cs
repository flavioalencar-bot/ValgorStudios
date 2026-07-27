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
    /// QA da build Beta 0.1. Ativa só com -checkpointSmoke.
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

            Application.runInBackground = true;
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
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture("00-main-menu");

            EnsureLocalProfile();
            LocalPlayerProfile.MarkIntroDone();
            LocalPlayerProfile.TutorialStep = LocalPlayerProfile.TutorialSteps.Complete;
            PlayerPrefs.Save();
            yield return new WaitForSecondsRealtime(0.4f);

            yield return Navigate(n => n.GoToCity());
            yield return WaitForScene(SceneIds.City, 90f);
            Debug.Log("[CheckpointSmoke] City OK");
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Capture("01-city");

            // —— Sprint UX contextual: Castelo / Fazenda / Armazém ——
            yield return CaptureBuildingUxEvidence();

            yield return Navigate(n => n.GoToHeroes());
            yield return WaitForScene(SceneIds.Heroes, 90f);
            Debug.Log("[CheckpointSmoke] HeroesDemo OK");
            yield return new WaitForSecondsRealtime(3.5f);
            yield return Capture("02-heroes-vortex");

            yield return Navigate(n => n.GoToDragonTower());
            yield return WaitForScene(SceneIds.City, 90f);
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Capture("03-dragons-tower");

            yield return Navigate(n => n.GoToWorldMap());
            yield return WaitForScene(SceneIds.WorldMap, 90f);
            Debug.Log("[CheckpointSmoke] WorldMap OK");
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Capture("04-worldmap");

            TrySelectFirstWorldNode();
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture("05-worldmap-node");

            TryDispatchSelected();
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture("06-worldmap-march");

            yield return Navigate(n => n.GoToCity());
            yield return WaitForScene(SceneIds.City, 90f);
            yield return Capture("07-city-return");
            Debug.Log("[CheckpointSmoke] Jornada mínima concluída.");
            yield return new WaitForSecondsRealtime(0.8f);
            Application.Quit(0);
        }

        private IEnumerator Capture(string name)
        {
            if (!_captureEvidence)
            {
                yield break;
            }

            yield return new WaitForEndOfFrame();
            var path = Path.GetFullPath(Path.Combine(_evidenceDir, name + ".png"));
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[CheckpointSmoke] Captura: {path}");
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(2.2f);
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

        private IEnumerator CaptureBuildingUxEvidence()
        {
            InvokeCityController("DebugResetBuildingsToSeedLayout");
            yield return new WaitForSecondsRealtime(0.5f);

            TrySelectCityBuilding("castle");
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture("ux-01-castle-selected");
            yield return Capture("ux-02-castle-context-menu");

            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(1f);
            yield return Capture("ux-06-upgrade-panel");
            yield return Capture("ux-07-upgrade-requirements");

            // Cenário 1: PlayerLevel alto (perfil) NÃO libera Fazenda — Castelo cidade Nv.1.
            TrySelectCityBuilding("farm");
            yield return new WaitForSecondsRealtime(0.8f);
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(1f);
            TryCityUpgradeSelected();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Capture("ux-13-farm-blocked-by-city-castle");
            InvokeCityPresenter("DebugGoToFirstUnmetRequirement");
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture("ux-13b-prereq-go-castle");

            // Cenário 2: Armazém bloqueado pela Fazenda.
            TrySelectCityBuilding("warehouse");
            yield return new WaitForSecondsRealtime(0.8f);
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(1f);
            yield return Capture("ux-11-prereq-blocked");
            TryCityUpgradeSelected();
            yield return new WaitForSecondsRealtime(0.3f);
            InvokeCityPresenter("DebugGoToFirstUnmetRequirement");
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture("ux-12-prereq-go-farm");

            // Cenário 3: Castelo bloqueado por Fazenda e Armazém ao mesmo tempo.
            TrySelectCityBuilding("castle");
            yield return new WaitForSecondsRealtime(0.8f);
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(1f);
            TryCityUpgradeSelected();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Capture("ux-14-castle-blocked-by-farm-warehouse");

            // 2ª entrega UX: Serraria / Pedreira / Mina / Academia
            TrySelectCityBuilding("lumbermill");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-15-lumbermill-selected");
            InvokeCityPresenter("DebugOpenDetailsPanel");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-16-lumbermill-details");
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-17-lumbermill-upgrade");

            TrySelectCityBuilding("quarry");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-18-quarry-selected");
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-19-quarry-upgrade");

            TrySelectCityBuilding("mine");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-20-mine-selected");
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-21-mine-upgrade");

            TrySelectCityBuilding("academy");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-22-academy-selected");
            InvokeCityPresenter("DebugOpenDetailsPanel");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-23-academy-details");
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-24-academy-upgrade");

            TrySelectCityBuilding("farm");
            yield return new WaitForSecondsRealtime(0.8f);
            ForceFarmCollectable();
            yield return new WaitForSecondsRealtime(0.6f);
            yield return Capture("ux-03-farm-collectable");

            TryCityCollectSelected();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-04-farm-collected");

            TrySelectCityBuilding("warehouse");
            yield return new WaitForSecondsRealtime(1f);
            yield return Capture("ux-05-warehouse-selected");
            InvokeCityPresenter("DebugOpenDetailsPanel");
            yield return new WaitForSecondsRealtime(0.8f);

            // Construção: Mina Nv.0→1 (só Castelo ≥1) — Castelo/Fazenda/Armazém estão bloqueados.
            TrySelectCityBuilding("mine");
            yield return new WaitForSecondsRealtime(0.5f);
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(0.4f);
            TryCityUpgradeSelected();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-08-construction-progress");
            yield return Capture("ux-09-construction-timer");

            // Aguarda conclusão natural (~6–10s) ou força instantâneo.
            var waited = 0f;
            while (waited < 14f && IsSelectedUpgrading())
            {
                waited += 0.5f;
                yield return new WaitForSecondsRealtime(0.5f);
            }

            if (IsSelectedUpgrading())
            {
                TryCityInstantComplete();
                yield return new WaitForSecondsRealtime(0.8f);
            }

            yield return Capture("ux-10-upgrade-complete");
            Debug.Log("[CheckpointSmoke] Evidências UX edifícios OK");
        }

        private static void InvokeCityController(string methodName)
        {
            var controller = FindCityController();
            if (controller == null)
            {
                Debug.LogWarning($"[CheckpointSmoke] CityController ausente p/ {methodName}");
                return;
            }

            var method = controller.GetType().GetMethod(methodName);
            if (method == null)
            {
                Debug.LogWarning($"[CheckpointSmoke] CityController.{methodName} indisponível.");
                return;
            }

            method.Invoke(controller, null);
            Debug.Log($"[CheckpointSmoke] CityController.{methodName}");
        }

        private static void InvokeCityPresenter(string methodName)
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null || !string.Equals(mb.GetType().Name, "CityHudController", StringComparison.Ordinal))
                {
                    continue;
                }

                var presenterProp = mb.GetType().GetProperty("Presenter");
                var presenter = presenterProp?.GetValue(mb);
                var method = presenter?.GetType().GetMethod(methodName);
                method?.Invoke(presenter, null);
                Debug.Log($"[CheckpointSmoke] Presenter.{methodName}");
                return;
            }

            Debug.LogWarning($"[CheckpointSmoke] Presenter.{methodName} indisponível.");
        }

        private static object? FindCityController()
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null || !string.Equals(mb.GetType().Name, "CityBootstrap", StringComparison.Ordinal))
                {
                    continue;
                }

                return mb.GetType().GetProperty("Controller")?.GetValue(mb);
            }

            return null;
        }

        private static void TryCityUpgradeSelected()
        {
            var controller = FindCityController();
            var method = controller?.GetType().GetMethod("TryUpgradeSelected");
            var ok = method?.Invoke(controller, null);
            Debug.Log($"[CheckpointSmoke] TryUpgradeSelected → {ok}");
        }

        private static void TryCityInstantComplete()
        {
            var controller = FindCityController();
            var method = controller?.GetType().GetMethod("TryInstantCompleteSelected");
            if (method == null)
            {
                return;
            }

            var args = new object?[] { null };
            var ok = method.Invoke(controller, args);
            Debug.Log($"[CheckpointSmoke] InstantComplete → {ok} err={args[0]}");
        }

        private static void TryCityCollectSelected()
        {
            var controller = FindCityController();
            var method = controller?.GetType().GetMethod("CollectSelected");
            var amount = method?.Invoke(controller, null);
            Debug.Log($"[CheckpointSmoke] CollectSelected → {amount}");
        }

        private static void ForceFarmCollectable()
        {
            var controller = FindCityController();
            if (controller == null)
            {
                return;
            }

            try
            {
                var economy = controller.GetType().GetProperty("Economy")?.GetValue(controller);
                var production = economy?.GetType().GetProperty("Production")?.GetValue(economy);
                var tryGet = production?.GetType().GetMethod("TryGetState");
                if (tryGet == null)
                {
                    return;
                }

                var args = new object?[] { "farm", null };
                if (tryGet.Invoke(production, args) is not true || args[1] == null)
                {
                    Debug.LogWarning("[CheckpointSmoke] Farm production state ausente.");
                    return;
                }

                var accProp = args[1].GetType().GetProperty("Accumulated");
                accProp?.SetValue(args[1], 150L);
                controller.GetType().GetMethod("RefreshPresentation")?.Invoke(controller, null);
                Debug.Log("[CheckpointSmoke] Farm Accumulated=150");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CheckpointSmoke] ForceFarmCollectable: {ex.Message}");
            }
        }

        private static bool IsSelectedUpgrading()
        {
            var controller = FindCityController();
            var selection = controller?.GetType().GetProperty("Selection")?.GetValue(controller);
            var selected = selection?.GetType().GetProperty("Selected")?.GetValue(selection);
            if (selected == null)
            {
                return false;
            }

            var state = selected.GetType().GetProperty("State")?.GetValue(selected);
            return state != null && string.Equals(state.ToString(), "Upgrading", StringComparison.Ordinal);
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

        private static void TryDispatchSelected()
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null || !string.Equals(mb.GetType().Name, "WorldMapController", StringComparison.Ordinal))
                {
                    continue;
                }

                var sessionProp = mb.GetType().GetProperty("Session");
                var session = sessionProp?.GetValue(mb);
                var method = session?.GetType().GetMethod("TryDispatchToSelected");
                if (method == null)
                {
                    continue;
                }

                var args = new object[] { null! };
                var ok = method.Invoke(session, args);
                Debug.Log($"[CheckpointSmoke] Dispatch: {ok} err={args[0]}");
                return;
            }

            Debug.LogWarning("[CheckpointSmoke] Dispatch não encontrado.");
        }
    }
}
