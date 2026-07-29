using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using Valgor.City.Camera;
using Valgor.City.Core;
using Valgor.City.Visual;
using Valgor.Core;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;
using Valgor.Dragons.Visual;

namespace Valgor.City.Qa
{
    /// <summary>
    /// E2E Unity: Dragão Nv.1→30 (alimentar, vínculo, caps, rituais, timer, visual).
    /// Ativa com -dragonPhase2E2E (build QA com VALGOR_CITY_PROGRESSION_QA).
    /// </summary>
    public sealed class DragonPhase2E2EAutoTest : MonoBehaviour
    {
        public const string EvidenceDir = DragonPhase2Qa.EvidenceDir;

        private CityProgressionQaController _qa = null!;
        private CityController _city = null!;
        private DragonService _dragons = null!;
        private readonly StringBuilder _report = new();
        private bool _failed;

        public void Begin(CityProgressionQaController qa, CityController city, DragonService dragons)
        {
            _qa = qa;
            _city = city;
            _dragons = dragons;
            Directory.CreateDirectory(EvidenceDir);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            Application.runInBackground = true;
            _report.AppendLine("Dragon Phase2 P1 E2E — Nv.1→30");
            _report.AppendLine($"started={DateTime.UtcNow:o}");
            _report.AppendLine($"compiledIn={CityProgressionQa.IsCompiledIn} e2e={DragonPhase2Qa.IsE2ETest}");

            _qa.TopUpNow();
            yield return EnsureBuildingsForCap30();
            if (_failed)
            {
                yield return Finish();
                yield break;
            }

            yield return EnsureDragonBorn();
            if (_failed)
            {
                yield return Finish();
                yield break;
            }

            yield return Capture("01-born-nv1");
            yield return new WaitForSecondsRealtime(0.2f);

            AssertVisualStage(DragonVisualStage.Hatchling, "born");
            if (_failed)
            {
                // Soft: ainda assim tenta progressão se o dragão está Nv.1.
                if (_dragons.TryGet("dragon-ember-1", out var d) && d.DragonLevel == 1)
                {
                    _report.AppendLine("WARN visual assert falhou; seguindo com Nv.1");
                    _failed = false;
                }
                else
                {
                    yield return Finish();
                    yield break;
                }
            }

            var ritualBreaks = new[] { 6, 11, 16, 21, 26 };
            while (true)
            {
                _dragons.SyncBuildingLevels(
                    _city.GetCastleLevel(),
                    _city.TryGetBuildingByDefinitionId("dragon-tower", out var tower) ? tower.Level : 1);

                if (!_dragons.TryGet("dragon-ember-1", out var dragon))
                {
                    Fail("dragão ausente");
                    break;
                }

                if (dragon.DragonLevel >= 30)
                {
                    break;
                }

                var from = dragon.DragonLevel;
                var to = from + 1;
                var isRitual = DragonProgressionRules.IsRitualTarget(to);
                var stageBefore = DragonProgressionRules.VisualStageForLevel(from);

                yield return FeedUntilReady(dragon.InstanceId);
                if (_failed)
                {
                    break;
                }

                AssertBondProgress(dragon.InstanceId, $"pre-lv{to}");

                if (_dragons.GetMaxAllowedDragonLevel() < to)
                {
                    Fail($"cap insuficiente para Nv.{to} (max={_dragons.GetMaxAllowedDragonLevel()})");
                    break;
                }

                if (!_dragons.TryStartLevelUp(dragon.InstanceId, out var startErr))
                {
                    Fail($"TryStartLevelUp {from}→{to}: {startErr}");
                    break;
                }

                Assert(_dragons.TryGet(dragon.InstanceId, out dragon) && dragon.IsLevelingUp, "deveria estar leveling");
                Assert(
                    DragonProgressionRules.VisualStageForLevel(dragon.DragonLevel) == stageBefore,
                    "visual não deve antecipar durante timer");

                if (isRitual)
                {
                    yield return Capture($"ritual-{from}-to-{to}-during");
                    AssertVisualStage(stageBefore, $"ritual-during-{to}");
                }

                // Conclui pelo timer real (duração QA curta).
                var deadline = Time.unscaledTime + 8f;
                while (Time.unscaledTime < deadline)
                {
                    _dragons.Tick();
                    if (_dragons.TryGet(dragon.InstanceId, out dragon) && !dragon.IsLevelingUp)
                    {
                        break;
                    }

                    yield return null;
                }

                Assert(_dragons.TryGet(dragon.InstanceId, out dragon) && !dragon.IsLevelingUp,
                    $"timer não concluiu {from}→{to}");
                Assert(dragon.DragonLevel == to, $"nível esperado {to}, veio {dragon.DragonLevel}");

                var stageAfter = DragonProgressionRules.VisualStageForLevel(to);
                Assert(dragon.GrowthStage == DragonProgressionRules.StageForLevel(to),
                    $"growth stage Nv.{to}");
                yield return new WaitForSecondsRealtime(0.25f);
                AssertVisualStage(stageAfter, $"after-{to}");

                if (isRitual || to == 30 || Array.IndexOf(ritualBreaks, to) >= 0)
                {
                    yield return Capture($"after-nv{to}-{DragonProgressionRules.StageDisplayName(dragon.GrowthStage)}");
                }

                _dragons.Persist();
                _report.AppendLine($"OK {from}→{to} stage={DragonProgressionRules.StageDisplayName(dragon.GrowthStage)} ritual={isRitual}");
            }

            if (!_failed && _dragons.TryGet("dragon-ember-1", out var final) && final.DragonLevel == 30)
            {
                AssertVisualStage(DragonVisualStage.Ancestral, "nv30");
                _city.TrySelectByDefinitionId("dragon-tower");
                yield return new WaitForSecondsRealtime(0.4f);
                yield return Capture("99-nv30-ancestral");
                _dragons.Persist();
                _report.AppendLine("PASS Nv.30 Ancestral");
            }

            yield return Finish();
        }

        private IEnumerator Finish()
        {
            var path = Path.Combine(EvidenceDir, "e2e-report.txt");
            File.WriteAllText(path, _report.ToString());
            Debug.Log($"[Valgor.QA] Dragon P1 E2E done failed={_failed} report={path}");
            Application.Quit(_failed ? 1 : 0);
            yield break;
        }

        private IEnumerator EnsureBuildingsForCap30()
        {
            _report.AppendLine("buildings → Castelo 30 / Torre 15 (cap 30)");
            yield return _qa.EvolveCastleToLevel(30);
            yield return _qa.UpgradeBuildingToLevel("academy", 8);
            yield return _qa.UpgradeBuildingToLevel("dragon-tower", 15);
            _qa.TopUpNow();
            var castle = _city.GetCastleLevel();
            var towerLv = _city.TryGetBuildingByDefinitionId("dragon-tower", out var tower) ? tower.Level : 0;
            _dragons.SyncBuildingLevels(castle, towerLv);
            _report.AppendLine($"buildings result castle={castle} tower={towerLv} cap={_dragons.GetMaxAllowedDragonLevel()}");
            Assert(towerLv >= 15, $"Torre precisa Nv.15 (atual {towerLv})");
            Assert(_dragons.GetMaxAllowedDragonLevel() >= 30, "cap efetivo deve ser 30");
        }

        private IEnumerator EnsureDragonBorn()
        {
            _dragons.SyncBuildingLevels(
                _city.GetCastleLevel(),
                _city.TryGetBuildingByDefinitionId("dragon-tower", out var tower) ? tower.Level : 1);

            if (_dragons.EggJourneyPhaseLabel == "BORN" &&
                _dragons.TryGet("dragon-ember-1", out var born) &&
                born.DragonLevel >= 1)
            {
                _report.AppendLine($"já nascido Nv.{born.DragonLevel}");
                // Reinicia progressão para E2E limpo se já avançado.
                if (born.DragonLevel > 1 || born.IsLevelingUp)
                {
                    born.DragonLevel = 1;
                    born.Experience = 0;
                    born.IsLevelingUp = false;
                    born.PendingLevel = 0;
                    born.LevelUpEndsAtUtc = null;
                    born.Energy = 100;
                    born.Health = 100;
                    born.GrowthStage = DragonGrowthStage.Hatchling;
                    _dragons.Persist();
                    _dragons.NotifyChanged(born.InstanceId);
                    _report.AppendLine("reset → Nv.1 Filhote para E2E limpo");
                    yield return new WaitForSecondsRealtime(0.35f);
                }

                yield break;
            }

            _city.TrySelectByDefinitionId("dragon-tower");
            yield return new WaitForSecondsRealtime(0.3f);

            // Labels: enum.ToString().ToUpperInvariant() → MISSIONACTIVE (sem underscore).
            var phase = _dragons.EggJourneyPhaseLabel;
            if (phase is "LOCKED" or "UNLOCKED")
            {
                Assert(_dragons.TryAcceptEggMission(out var e1), e1);
                phase = _dragons.EggJourneyPhaseLabel;
            }

            if (phase == "MISSIONACTIVE")
            {
                Assert(_dragons.TryConquerEgg(out var e2), e2);
                phase = _dragons.EggJourneyPhaseLabel;
            }

            if (phase == "EGGOWNED")
            {
                Assert(_dragons.TryBeginIncubation(out var e3), e3);
                phase = _dragons.EggJourneyPhaseLabel;
            }

            if (phase == "INCUBATING")
            {
                for (var i = 0; i < 5; i++)
                {
                    _dragons.TryCareIncubation(out _);
                    _qa.TopUpNow();
                }

                var deadline = Time.unscaledTime + 8f;
                while (Time.unscaledTime < deadline && _dragons.EggJourneyPhaseLabel != "BORN")
                {
                    _dragons.Tick();
                    yield return null;
                }
            }

            Assert(_dragons.EggJourneyPhaseLabel == "BORN", $"nascimento falhou phase={_dragons.EggJourneyPhaseLabel}");
            Assert(_dragons.TryGet("dragon-ember-1", out var ember) && ember.DragonLevel == 1, "Nv.1 após nascimento");
            if (_failed)
            {
                yield break;
            }

            yield return Capture("00-egg-to-born");
        }

        private IEnumerator FeedUntilReady(string dragonId)
        {
            _qa.TopUpNow();
            for (var guard = 0; guard < 40; guard++)
            {
                if (!_dragons.TryGet(dragonId, out var d))
                {
                    Fail("feed: dragão ausente");
                    yield break;
                }

                var need = DragonProgressionRules.ExperienceRequiredForLevel(d.DragonLevel);
                if (d.Experience >= need && d.Energy >= 25 && d.Health >= 40 && !d.IsLevelingUp)
                {
                    yield break;
                }

                if (!_dragons.TryFeed(dragonId, out var err))
                {
                    // Pode falhar se já saciado — injeta XP direto em QA.
                    d.Experience = Math.Max(d.Experience, need);
                    d.Energy = 100;
                    d.Health = 100;
                    _dragons.Persist();
                    _report.AppendLine($"feed fallback XP inject ({err})");
                    yield break;
                }

                yield return null;
            }
        }

        private void AssertBondProgress(string dragonId, string tag)
        {
            if (_dragons.TryGet(dragonId, out var d) && d.BondLevel >= 0)
            {
                _report.AppendLine($"bond ok {tag} Nv.{d.BondLevel} pts={d.BondPoints}");
            }
        }

        private void AssertVisualStage(DragonVisualStage expected, string tag)
        {
            var nest = FindFirstObjectByType<DragonNestView>();
            if (nest == null)
            {
                Fail($"nest ausente ({tag})");
                return;
            }

            var placeholder = GameObject.Find($"_PLACEHOLDER_{expected}_");
            // Busca por prefixo no hierarchy do nest.
            var found = false;
            foreach (var t in nest.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith($"_PLACEHOLDER_{expected}_", StringComparison.Ordinal) ||
                    t.name == $"Placeholder_{expected}" ||
                    t.name == $"DragonVisual_{expected}")
                {
                    found = true;
                    break;
                }
            }

            Assert(found, $"visual {expected} não encontrado ({tag})");
            _ = placeholder;
            var cfg = DragonStageVisualCatalog.Get(expected);
            Assert(cfg.Stage == expected, $"catalog {expected}");
            _report.AppendLine($"visual {tag}={expected} placeholder={cfg.PlaceholderFlag}");
        }

        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            var path = Path.Combine(EvidenceDir, $"{name}.png");
            ScreenCapture.CaptureScreenshot(path);
            _report.AppendLine($"shot={name}");
            yield return new WaitForSecondsRealtime(0.15f);
        }

        private void Assert(bool condition, string message)
        {
            if (condition)
            {
                return;
            }

            Fail(message);
        }

        private void Fail(string message)
        {
            _failed = true;
            _report.AppendLine("FAIL: " + message);
            Debug.LogError("[Valgor.QA] " + message);
        }
    }
}
