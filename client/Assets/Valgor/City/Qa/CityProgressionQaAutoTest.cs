using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using Valgor.City.Core;
using Valgor.City.Visual;
using Valgor.Core;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Teste automático: Castelo 1→30 com trocas de tier, save/reload e evidências.
    /// Ativa com -cityProgressionQA -cityProgressionQATest
    /// </summary>
    public sealed class CityProgressionQaAutoTest : MonoBehaviour
    {
        public const string EvidenceDir =
            @"C:\Valgor_Studio\docs\releases\context-menu-final-evidence";

        private CityProgressionQaController _qa = null!;
        private CityController _city = null!;
        private readonly StringBuilder _report = new();

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
            _report.AppendLine("City Progression QA AutoTest");
            _report.AppendLine($"started={DateTime.UtcNow:o}");
            _report.AppendLine($"compiledIn={CityProgressionQa.IsCompiledIn} active={CityProgressionQa.IsActive}");

            // Banner + recursos QA no HUD
            _qa.TopUpNow();
            var hud = FindFirstObjectByType<CityProgressionQaHud>();
            // Garante estado inicial Nv.1
            _qa.RequestResetTo1();
            yield return new WaitForSecondsRealtime(0.5f);
            _city.TrySelectByDefinitionId("castle");
            yield return new WaitForSecondsRealtime(0.45f);
            yield return Capture("tier1-menu");
            AssertLevelTier(1, 1, "start");

            yield return EvolveAndCapture(5, 1, "tier1-nv5");
            yield return EvolveAndCapture(6, 2, "tier2-nv6");
            yield return EvolveAndCapture(11, 3, "tier3-nv11");
            yield return EvolveAndCapture(16, 4, "tier4-nv16");
            yield return EvolveAndCapture(21, 5, "tier5-nv21");
            yield return EvolveAndCapture(26, 6, "tier6-nv26");
            yield return EvolveAndCapture(30, 6, "tier6-nv30-max");

            // Decoração toast + nível máximo
            InvokeDecoration();
            yield return new WaitForSecondsRealtime(0.7f);
            yield return Capture("menu-decoration-toast");
            yield return Capture("menu-nivel-maximo");

            hud?.OpenPanel();
            yield return new WaitForSecondsRealtime(0.4f);
            yield return Capture("qa-panel");

            _qa.RequestSave();
            yield return new WaitForSecondsRealtime(0.3f);

            _qa.RequestReload();
            while (_qa.IsBusy)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.5f);
            _city.TrySelectByDefinitionId("castle");
            yield return new WaitForSecondsRealtime(0.4f);
            yield return Capture("reload-nv30-menu");
            AssertLevelTier(30, 6, "reload");

            _qa.RequestResetTo1();
            yield return new WaitForSecondsRealtime(0.4f);
            _qa.RequestSave();
            _qa.RequestReload();
            while (_qa.IsBusy)
            {
                yield return null;
            }

            AssertLevelTier(1, 1, "reset-reload");
            yield return Capture("reset-nv1");

            yield return _qa.EvolveCastleToLevel(30);
            _city.SyncCastleVisuals(animate: false);
            yield return new WaitForSecondsRealtime(0.35f);
            _qa.RequestSave();
            AssertLevelTier(30, 6, "final");

            var path = Path.Combine(EvidenceDir, "auto-test-report.txt");
            File.WriteAllText(path, _report.ToString());
            Debug.Log($"[Valgor.QA] AutoTest DONE — report={path}");
            Application.Quit(0);
        }

        private static void InvokeDecoration()
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null || !string.Equals(mb.GetType().Name, "CityHudController", StringComparison.Ordinal))
                {
                    continue;
                }

                var presenterProp = mb.GetType().GetProperty("Presenter");
                var presenter = presenterProp?.GetValue(mb);
                var method = presenter?.GetType().GetMethod("DebugShowDecorationPlaceholder");
                method?.Invoke(presenter, null);
                return;
            }
        }

        private IEnumerator EvolveAndCapture(int level, int expectTier, string captureName)
        {
            _report.AppendLine($"evolve→{level} expectTier={expectTier}");
            yield return _qa.EvolveCastleToLevel(level);
            yield return new WaitForSecondsRealtime(0.6f);
            _city.TrySelectByDefinitionId("castle");
            yield return new WaitForSecondsRealtime(0.35f);
            AssertLevelTier(level, expectTier, captureName);
            yield return Capture(captureName);
        }

        private void AssertLevelTier(int level, int tier, string tag)
        {
            var gotLevel = _qa.GetCastleLevel();
            var gotTier = _qa.GetCastleVisualTier();
            var resolved = CastleRealVisualLoader.ResolveTier(gotLevel);
            var ok = gotLevel == level && (gotTier == tier || resolved == tier);
            var line =
                $"[{tag}] level={gotLevel} (want {level}) tierAttached={gotTier} tierResolved={resolved} (want {tier}) {(ok ? "OK" : "FAIL")}";
            _report.AppendLine(line);
            if (ok)
            {
                Debug.Log($"[Valgor.QA] {line}");
            }
            else
            {
                Debug.LogError($"[Valgor.QA] {line}");
            }
        }

        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            var path = Path.Combine(EvidenceDir, name + ".png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[Valgor.QA] Capture {path}");
            yield return new WaitForSecondsRealtime(1.2f);
        }
    }
}
