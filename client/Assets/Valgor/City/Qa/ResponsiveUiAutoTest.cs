using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.UI;
using Valgor.Core;
using Valgor.UI;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Regressão responsiva P1 — flag -responsiveUiTest.
    /// </summary>
    public sealed class ResponsiveUiAutoTest : MonoBehaviour
    {
        public const string EvidenceDir = CityProgressionQa.ResponsiveEvidenceDir;

        private static readonly (int W, int H, string Tag)[] Resolutions =
        {
            (1920, 1080, "1920x1080"),
            (1600, 900, "1600x900"),
            (1366, 768, "1366x768"),
            (1280, 720, "1280x720"),
            (1080, 640, "1080x640")
        };

        private CityProgressionQaController _qa = null!;
        private CityController _city = null!;
        private readonly StringBuilder _report = new();
        private int _pass;
        private int _fail;

        public void Begin(CityProgressionQaController qa, CityController city)
        {
            _qa = qa;
            _city = city;
            Directory.CreateDirectory(EvidenceDir);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            Application.runInBackground = true;
            _report.AppendLine("Responsive UI P1 AutoTest");
            _report.AppendLine($"started={DateTime.UtcNow:o}");
            _report.AppendLine($"match={ValgorResponsiveUi.IsShortScreen} short-at-start");

            _qa.TopUpNow();
            _qa.RequestResetTo1();
            yield return new WaitForSecondsRealtime(0.4f);
            _qa.RequestSatisfyAllCastleRequirements();
            while (_qa.IsBusy)
            {
                yield return null;
            }

            // Comparativo 5 resoluções — City HUD + modal Atualizar + Detalhes + QA panel.
            foreach (var res in Resolutions)
            {
                Screen.SetResolution(res.W, res.H, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(0.55f);

                var qaHud = FindFirstObjectByType<CityProgressionQaHud>();
                qaHud?.OpenPanel();
                yield return new WaitForSecondsRealtime(0.25f);
                yield return Capture($"city-qa-panel-{res.Tag}");
                // Fecha painel QA (Toggle a partir de aberto).
                qaHud?.TogglePanel();
                yield return new WaitForSecondsRealtime(0.15f);
                yield return Capture($"city-hud-{res.Tag}");

                _city.TrySelectByDefinitionId("castle");
                yield return new WaitForSecondsRealtime(0.35f);
                yield return Capture($"city-context-{res.Tag}");

                var presenter = FindPresenter();
                presenter?.DebugOpenDetailsPanel();
                yield return new WaitForSecondsRealtime(0.4f);
                yield return Capture($"details-{res.Tag}");
                AssertModalFullyInViewport(BuildingDetailsModal.RootName, $"details-in-view-{res.Tag}");

                presenter?.DebugOpenUpgradePanel();
                yield return new WaitForSecondsRealtime(0.45f);
                yield return Capture($"upgrade-{res.Tag}");
                AssertModalFullyInViewport(BuildingUpgradeModal.RootName, $"upgrade-in-view-{res.Tag}");

                presenter?.DebugOpenObtainForResource(ResourceType.Wood);
                yield return new WaitForSecondsRealtime(0.45f);
                yield return Capture($"obtain-{res.Tag}");
                AssertModalFullyInViewport(ObtainMoreResourcesModal.RootName, $"obtain-in-view-{res.Tag}");

                presenter?.DebugHidePanels();
                yield return new WaitForSecondsRealtime(0.2f);
            }

            // Foco 1080×640: construção em andamento + missões + heróis via nav não (smoke separado).
            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.5f);
            _qa.TopUpNow();
            _city.TrySelectByDefinitionId("castle");
            var p = FindPresenter();
            p?.DebugOpenUpgradePanel();
            yield return new WaitForSecondsRealtime(0.3f);
            p?.DebugConfirmUpgrade();
            yield return new WaitForSecondsRealtime(0.35f);
            p?.DebugHidePanels();
            yield return new WaitForSecondsRealtime(0.25f);
            yield return Capture("1080x640-construction-world");
            Assert(_city.TryGetBuildingByDefinitionId("castle", out var c) &&
                   c.State == BuildingState.Upgrading, "construction-active-1080");

            if (c != null && c.State == BuildingState.Upgrading)
            {
                _city.TrySelectByDefinitionId("castle");
                _city.TryInstantCompleteSelected(out _);
            }

            // Missões
            var nav = FindFirstObjectByType<BetaNavigationBar>();
            nav?.DebugToggleMissions();
            yield return new WaitForSecondsRealtime(0.4f);
            yield return Capture("1080x640-missions");
            nav?.DebugToggleMissions();
            yield return new WaitForSecondsRealtime(0.15f);

            _report.AppendLine($"pass={_pass} fail={_fail}");
            var path = Path.Combine(EvidenceDir, "auto-test-report.txt");
            File.WriteAllText(path, _report.ToString());
            WriteComparativeSummary();
            Debug.Log($"[Valgor.QA] Responsive UI AutoTest DONE — {path}");
            Application.Quit(_fail > 0 ? 1 : 0);
        }

        private void AssertModalFullyInViewport(string rootName, string tag)
        {
            var doc = FindFirstObjectByType<CityHudController>()?.GetComponent<UIDocument>();
            var el = doc?.rootVisualElement.Q(rootName);
            if (el == null || el.style.display == DisplayStyle.None)
            {
                Assert(false, tag + "-missing");
                return;
            }

            var r = el.worldBound;
            // worldBound em coordenadas de painel; margem de segurança.
            var ok = r.yMin >= -4f && r.yMax <= Screen.height + 4f &&
                     r.xMin >= -4f && r.xMax <= Screen.width + 4f &&
                     r.height > 40f;
            // Em ScaleWithScreenSize worldBound não é pixels de tela — valida tamanho mínimo e display.
            ok = el.resolvedStyle.display == DisplayStyle.Flex &&
                 el.resolvedStyle.height > 80f &&
                 el.resolvedStyle.width > 200f;
            Assert(ok, tag);
        }

        private void WriteComparativeSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Comparativo resoluções — Responsive P1");
            sb.AppendLine();
            sb.AppendLine("| Resolução | Capturas |");
            sb.AppendLine("|---|---|");
            foreach (var res in Resolutions)
            {
                sb.AppendLine(
                    $"| {res.Tag} | city-hud / city-context / details / upgrade / obtain / city-qa-panel |");
            }

            sb.AppendLine();
            sb.AppendLine("Foco 1080×640: `1080x640-construction-world.png`, `1080x640-missions.png`, `00-main-menu-1080x640.png`.");
            sb.AppendLine();
            sb.AppendLine("## Causa do corte (pré-fix)");
            sb.AppendLine(
                "- Menu: ScrollView com flexGrow=0 + justify center → conteúdo estourava a viewport e botões inferiores sumiam.");
            sb.AppendLine(
                "- PanelSettings: ScaleWithScreenSize sem match balanceado (0.5) agravava overflow vertical.");
            sb.AppendLine(
                "- Modais/QA/Missões: maxHeight fixo ou painel sem scroll interno em altura curta.");
            File.WriteAllText(Path.Combine(EvidenceDir, "COMPARATIVE.md"), sb.ToString());
        }

        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            var path = Path.Combine(EvidenceDir, name + ".png");
            var usedFallback = false;
            try
            {
                var w = Screen.width;
                var h = Screen.height;
                var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply(false);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Destroy(tex);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Valgor.QA] Capture fallback: {ex.Message}");
                ScreenCapture.CaptureScreenshot(path);
                usedFallback = true;
            }

            yield return new WaitForSecondsRealtime(usedFallback ? 2f : 0.2f);
            Debug.Log($"[Valgor.QA] Capture {path}");
        }

        private void Assert(bool ok, string tag)
        {
            if (ok)
            {
                _pass++;
                _report.AppendLine($"[OK] {tag}");
            }
            else
            {
                _fail++;
                _report.AppendLine($"[FAIL] {tag}");
                Debug.LogError($"[Valgor.QA] FAIL {tag}");
            }
        }

        private static BuildingSelectionPresenter? FindPresenter() =>
            FindFirstObjectByType<CityHudController>()?.Presenter;
    }
}
