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
            yield return Capture("vis-01-main-menu");
            // Splash/Loading já passou quando o smoke chega ao menu — tenta captura residual se a cena ainda existir.
            yield return TryCaptureSplashIfPresent();

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
            yield return Capture("vis-02-city-full");
            yield return Capture("art-01-city-full");
            TryOpenMissionsPanel();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("08-missions-panel");
            TryOpenMissionsPanel(); // toggle fecha
            yield return new WaitForSecondsRealtime(0.3f);
            TrySelectCityBuilding("castle");
            yield return new WaitForSecondsRealtime(1f);
            yield return Capture("vis-03-castle");
            yield return Capture("vis-07-building-selected");
            yield return Capture("art-02-castle");
            yield return Capture("art-08-building-selected");
            TrySelectCityBuilding("dragon-tower");
            yield return new WaitForSecondsRealtime(0.9f);
            yield return Capture("vis-06-dragon-tower");
            yield return Capture("art-03-dragon-tower");
            TrySelectCityBuilding("farm");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("vis-04-economy");
            yield return Capture("art-04-farm");
            TrySelectCityBuilding("warehouse");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("art-05-warehouse");
            TrySelectCityBuilding("academy");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("art-06-academy");
            TrySelectCityBuilding("arena");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("vis-05-military");
            TrySelectCityBuilding("castle");
            yield return new WaitForSecondsRealtime(0.5f);
            // Visão com iluminação quente (dia reforçado).
            InvokeCityVisualLighting("ApplyDayLighting");
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture("art-07-lighting");
            InvokeCityPresenter("DebugOpenDetailsPanel");
            yield return new WaitForSecondsRealtime(0.7f);
            yield return Capture("vis-08-details");
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(0.7f);
            yield return Capture("vis-09-upgrade");

            // Noite provisória (evidência extra).
            InvokeCityVisualLighting("ApplyNightLighting");
            yield return new WaitForSecondsRealtime(0.6f);
            yield return Capture("art-09-city-night");
            InvokeCityVisualLighting("ApplyDayLighting");
            yield return new WaitForSecondsRealtime(0.3f);

            // —— Sprint UX contextual: Castelo / Fazenda / Armazém ——
            yield return CaptureBuildingUxEvidence();

            yield return Navigate(n => n.GoToHeroes());
            yield return WaitForScene(SceneIds.Heroes, 90f);
            Debug.Log("[CheckpointSmoke] HeroesDemo OK — Vortex selecionado por padrão no OnEnable");
            yield return new WaitForSecondsRealtime(3.5f);
            yield return Capture("02-heroes-vortex");
            yield return Capture("vis-10-heroes");

            yield return Navigate(n => n.GoToDragonTower());
            yield return WaitForScene(SceneIds.City, 90f);
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Capture("03-dragons-tower");
            yield return Capture("vis-11-dragons");

            yield return Navigate(n => n.GoToWorldMap());
            yield return WaitForScene(SceneIds.WorldMap, 90f);
            Debug.Log("[CheckpointSmoke] WorldMap OK — energia deve carregar sem throw");
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Capture("04-worldmap");
            yield return Capture("vis-12-worldmap");
            TrySelectFirstWorldNode();
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture("05-worldmap-node");

            TryDispatchSelected();
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture("06-worldmap-march");

            yield return Navigate(n => n.GoToCity());
            yield return WaitForScene(SceneIds.City, 90f);
            yield return Capture("07-city-return");
            Debug.Log($"[CheckpointSmoke] Jornada mínima concluída. missões chapter={BetaMissions.ActiveChapter}");
            yield return new WaitForSecondsRealtime(0.8f);
            Application.Quit(0);
        }

        private IEnumerator TryCaptureSplashIfPresent()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() &&
                    (string.Equals(scene.name, SceneIds.Loading, StringComparison.Ordinal) ||
                     string.Equals(scene.name, "Splash", StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.Log($"[CheckpointSmoke] Splash/Loading residual: {scene.name}");
                    yield return Capture("00-splash");
                    yield break;
                }
            }

            Debug.Log("[CheckpointSmoke] Splash/Loading já encerrado — captura 00-splash omitida.");
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

            // 3ª entrega UX: Arena / Hospital / Torre / Templo / Mercado / Laboratório
            yield return CaptureSupportBuilding("arena", "ux-25");
            yield return CaptureSupportBuilding("hospital", "ux-26");
            yield return CaptureSupportBuilding("dragon-tower", "ux-27", feedAction: true);
            yield return CaptureSupportBuilding("temple", "ux-28");
            yield return CaptureSupportBuilding("market", "ux-29");
            yield return CaptureSupportBuilding("laboratory", "ux-30");

            // Muralha (evolutiva): Detalhes + Atualizar (padrão Academia).
            TrySelectCityBuilding("wall");
            yield return new WaitForSecondsRealtime(0.9f);
            yield return Capture("ux-31-wall-selected");
            TryClickWallSegmentProxy();
            yield return new WaitForSecondsRealtime(0.7f);
            yield return Capture("ux-31b-wall-segment-click");
            InvokeCityPresenter("DebugOpenDetailsPanel");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-32-wall-details");
            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-33-wall-upgrade");
            yield return Capture("ux-34-wall-blocked");
            InvokeCityPresenter("DebugGoToFirstUnmetRequirement");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("ux-35-wall-go");

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

        private IEnumerator CaptureSupportBuilding(string definitionId, string prefix, bool feedAction = false)
        {
            TrySelectCityBuilding(definitionId);
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture($"{prefix}-{definitionId}-selected");

            InvokeCityPresenter("DebugOpenDetailsPanel");
            yield return new WaitForSecondsRealtime(0.7f);
            yield return Capture($"{prefix}-{definitionId}-details");

            InvokeCityPresenter("DebugOpenOpenPanel");
            yield return new WaitForSecondsRealtime(0.7f);
            yield return Capture($"{prefix}-{definitionId}-open");

            if (feedAction)
            {
                InvokeCityPresenter("DebugFeedDragon");
                yield return new WaitForSecondsRealtime(0.5f);
                yield return Capture($"{prefix}-{definitionId}-feed");
            }

            InvokeCityPresenter("DebugOpenUpgradePanel");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture($"{prefix}-{definitionId}-upgrade");
            TryCityUpgradeSelected();
            yield return new WaitForSecondsRealtime(0.25f);
            yield return Capture($"{prefix}-{definitionId}-blocked");
            InvokeCityPresenter("DebugGoToFirstUnmetRequirement");
            yield return new WaitForSecondsRealtime(1.0f);
            yield return Capture($"{prefix}-{definitionId}-go");
        }

        private static void TryOpenMissionsPanel()
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null || !string.Equals(mb.GetType().Name, "BetaNavigationBar", StringComparison.Ordinal))
                {
                    continue;
                }

                var flags = System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.NonPublic;
                var method = mb.GetType().GetMethod("ToggleMissions", flags);
                if (method == null)
                {
                    Debug.LogWarning("[CheckpointSmoke] BetaNavigationBar.ToggleMissions ausente.");
                    return;
                }

                method.Invoke(mb, null);
                Debug.Log("[CheckpointSmoke] Missões: ToggleMissions");
                return;
            }

            Debug.LogWarning("[CheckpointSmoke] BetaNavigationBar não encontrado — 08-missions-panel pode ficar vazio.");
            Debug.Log($"[CheckpointSmoke] missões chapter={BetaMissions.ActiveChapter} claimed={BetaMissions.ClaimedMask}");
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

        /// <summary>Simula clique em segmento da muralha (proxy → BuildingView wall).</summary>
        private static void TryClickWallSegmentProxy()
        {
            var proxies = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < proxies.Length; i++)
            {
                var mb = proxies[i];
                if (mb == null ||
                    !string.Equals(mb.GetType().Name, "BuildingSelectionClickProxy", StringComparison.Ordinal))
                {
                    continue;
                }

                var targetProp = mb.GetType().GetProperty("Target");
                var target = targetProp?.GetValue(mb);
                var instanceProp = target?.GetType().GetProperty("Instance");
                var instance = instanceProp?.GetValue(target);
                var defIdProp = instance?.GetType().GetProperty("DefinitionId");
                var defId = defIdProp?.GetValue(instance) as string;
                if (!string.Equals(defId, "wall", StringComparison.Ordinal))
                {
                    continue;
                }

                var notify = mb.GetType().GetMethod("NotifyClicked");
                notify?.Invoke(mb, null);
                Debug.Log("[CheckpointSmoke] Clique proxy muralha → wall");
                return;
            }

            Debug.LogWarning("[CheckpointSmoke] Nenhum BuildingSelectionClickProxy da muralha encontrado.");
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
        /// <summary>
        /// Reflection evita referência Valgor.Runtime → Valgor.City (ciclo com asmdef).
        /// </summary>
        private static void InvokeCityVisualLighting(string methodName)
        {
            var type = Type.GetType("Valgor.City.Visual.CityEnvironmentBuilder, Valgor.City");
            type?.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, null);
        }

    }
}
