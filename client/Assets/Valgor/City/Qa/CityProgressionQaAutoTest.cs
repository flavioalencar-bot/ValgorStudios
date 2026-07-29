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
            @"C:\Valgor_Studio\docs\releases\city-progression-qa-evidence";

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

            // Garante estado inicial Nv.1
            _qa.RequestResetTo1();
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture("01-castle-nv1-tier1");
            AssertLevelTier(1, 1, "start");

            yield return EvolveAndCapture(5, 1, "02-castle-nv5-tier1");
            yield return EvolveAndCapture(6, 2, "03-castle-nv6-tier2");
            yield return EvolveAndCapture(11, 3, "04-castle-nv11-tier3");
            yield return EvolveAndCapture(16, 4, "05-castle-nv16-tier4");
            yield return EvolveAndCapture(21, 5, "06-castle-nv21-tier5");
            yield return EvolveAndCapture(26, 6, "07-castle-nv26-tier6");
            yield return EvolveAndCapture(30, 6, "08-castle-nv30-tier6");

            // Painel QA aberto
            var hud = FindFirstObjectByType<CityProgressionQaHud>();
            hud?.OpenPanel();
            yield return new WaitForSecondsRealtime(0.4f);
            yield return Capture("09-qa-panel");

            _qa.RequestSave();
            yield return new WaitForSecondsRealtime(0.3f);

            // Recarrega
            _qa.RequestReload();
            while (_qa.IsBusy)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture("10-reload-nv30");
            AssertLevelTier(30, 6, "reload");

            // Voltar Nv.1 no save QA
            _qa.RequestResetTo1();
            yield return new WaitForSecondsRealtime(0.4f);
            _qa.RequestSave();
            _qa.RequestReload();
            while (_qa.IsBusy)
            {
                yield return null;
            }

            AssertLevelTier(1, 1, "reset-reload");
            yield return Capture("11-reset-nv1");

            // Restaura Nv.30 para evidência final de save (re-evolve rápido)
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
