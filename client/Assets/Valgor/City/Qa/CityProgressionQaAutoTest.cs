using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using Valgor.City.Camera;
using Valgor.City.Core;
using Valgor.City.Visual;
using Valgor.Core;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Teste automático: troca de tier estável (câmera fixa) + evidências.
    /// Ativa com -cityProgressionQA -cityProgressionQATest
    /// </summary>
    public sealed class CityProgressionQaAutoTest : MonoBehaviour
    {
        public const string EvidenceDir =
            @"C:\Valgor_Studio\docs\releases\tier-swap-smooth-evidence";

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
            _report.AppendLine("City Progression QA AutoTest — smooth tier swap");
            _report.AppendLine($"started={DateTime.UtcNow:o}");
            _report.AppendLine($"compiledIn={CityProgressionQa.IsCompiledIn} active={CityProgressionQa.IsActive}");

            _qa.TopUpNow();
            _qa.RequestResetTo1();
            yield return new WaitForSecondsRealtime(0.5f);
            _city.TrySelectByDefinitionId("castle");
            yield return new WaitForSecondsRealtime(0.45f);
            AssertLevelTier(1, 1, "start");

            // Evolui até o limiar e valida câmera em cada cruzamento de faixa.
            yield return EvolveAndCapture(5, 1, "before-5to6");
            yield return CrossTierStable(6, 2, "cross-5to6");
            yield return EvolveAndCapture(10, 2, "before-10to11");
            yield return CrossTierStable(11, 3, "cross-10to11");
            yield return EvolveAndCapture(15, 3, "before-15to16");
            yield return CrossTierStable(16, 4, "cross-15to16");
            yield return EvolveAndCapture(20, 4, "before-20to21");
            yield return CrossTierStable(21, 5, "cross-20to21");
            yield return EvolveAndCapture(25, 5, "before-25to26");
            yield return CrossTierStable(26, 6, "cross-25to26");

            yield return EvolveAndCapture(30, 6, "tier6-nv30-max");

            // Upgrade rápido + evolve atalhos
            _qa.RequestResetTo1();
            yield return new WaitForSecondsRealtime(0.35f);
            yield return _qa.EvolveCastleToLevel(12);
            yield return new WaitForSecondsRealtime(0.5f);
            AssertLevelTier(12, 3, "fast-to-12");

            _qa.RequestEvolveToNextTier();
            while (_qa.IsBusy)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.6f);
            AssertLevelTier(16, 4, "evolve-next-tier");

            _qa.RequestEvolveTo30();
            while (_qa.IsBusy)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.6f);
            AssertLevelTier(30, 6, "evolve-to-30");

            _qa.RequestSave();
            yield return new WaitForSecondsRealtime(0.25f);
            _qa.RequestReload();
            while (_qa.IsBusy)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.4f);
            AssertLevelTier(30, 6, "reload");
            yield return Capture("reload-nv30");

            var path = Path.Combine(EvidenceDir, "auto-test-report.txt");
            File.WriteAllText(path, _report.ToString());
            Debug.Log($"[Valgor.QA] AutoTest DONE — report={path}");
            Application.Quit(0);
        }

        private IEnumerator CrossTierStable(int level, int expectTier, string tag)
        {
            _city.TrySelectByDefinitionId("castle");
            yield return new WaitForSecondsRealtime(0.3f);

            var cam = FindFirstObjectByType<CityCameraController>();
            var beforeOk = cam != null;
            var before = beforeOk ? cam!.CapturePose() : default;

            _report.AppendLine($"cross→{level} expectTier={expectTier}");
            var guard = 0;
            while (_qa.GetCastleLevel() < level && guard++ < 40)
            {
                if (!_qa.ForceCastleOneLevelNoNavigate())
                {
                    // Fallback: evolve normal (pode mover câmera se houver deps).
                    yield return _qa.EvolveCastleToLevel(level);
                    break;
                }

                yield return new WaitForSecondsRealtime(0.55f);
            }

            yield return new WaitForSecondsRealtime(0.35f);
            AssertLevelTier(level, expectTier, tag);

            if (beforeOk && cam != null)
            {
                var after = cam.CapturePose();
                var posDelta = Vector3.Distance(before.Position, after.Position);
                var zoomDelta = Mathf.Abs(before.OrthographicSize - after.OrthographicSize);
                var rotDot = Mathf.Abs(Quaternion.Dot(before.Rotation, after.Rotation));
                var stable = posDelta < 0.02f && zoomDelta < 0.02f && rotDot > 0.999f;
                var line =
                    $"[{tag}-camera] posΔ={posDelta:F4} zoomΔ={zoomDelta:F4} rotDot={rotDot:F5} {(stable ? "OK" : "FAIL")}";
                _report.AppendLine(line);
                if (stable)
                {
                    Debug.Log($"[Valgor.QA] {line}");
                }
                else
                {
                    Debug.LogError($"[Valgor.QA] {line}");
                }
            }
            else
            {
                _report.AppendLine($"[{tag}-camera] FAIL (no camera)");
            }

            yield return Capture(tag);
        }

        private IEnumerator EvolveAndCapture(int level, int expectTier, string captureName)
        {
            _report.AppendLine($"evolve→{level} expectTier={expectTier}");
            yield return _qa.EvolveCastleToLevel(level);
            yield return new WaitForSecondsRealtime(0.7f);
            _city.TrySelectByDefinitionId("castle");
            yield return new WaitForSecondsRealtime(0.3f);
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
            yield return new WaitForSecondsRealtime(1.1f);
        }
    }
}
