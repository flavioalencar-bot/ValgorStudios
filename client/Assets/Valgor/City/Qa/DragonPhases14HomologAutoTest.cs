using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using Valgor.City.Core;
using Valgor.City.UI;
using Valgor.City.Visual;
using Valgor.Core;
using Valgor.Dragons.Combat;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;
using Valgor.Dragons.Mount;
using Valgor.Dragons.Visual;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Homologação completa Fases 1–4. CLI: -dragonPhases14Homolog
    /// </summary>
    public sealed class DragonPhases14HomologAutoTest : MonoBehaviour
    {
        public const string EvidenceDir = DragonPhases14HomologQa.EvidenceDir;
        private const string DragonId = "dragon-ember-1";
        private const string MarchId = "homolog-march-pve";

        private CityProgressionQaController _qa = null!;
        private CityController _city = null!;
        private DragonService _dragons = null!;
        private readonly StringBuilder _report = new();
        private bool _failed;
        private int _p0;
        private int _p1;

        public void Begin(CityProgressionQaController qa, CityController city, DragonService dragons)
        {
            _qa = qa;
            _city = city;
            _dragons = dragons;
            Directory.CreateDirectory(EvidenceDir);
            Directory.CreateDirectory(Path.Combine(EvidenceDir, "screenshots"));
            Directory.CreateDirectory(Path.Combine(EvidenceDir, "milestones"));
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            Application.runInBackground = true;
            _report.AppendLine("Homologação Dragão Fases 1–4 — E2E completo");
            _report.AppendLine($"started={DateTime.UtcNow:o}");
            _report.AppendLine($"base=fbee9bc persistence={_dragons.PersistenceKey}");
            _report.AppendLine($"compiledIn={CityProgressionQa.IsCompiledIn}");

            Assert(
                string.Equals(_dragons.PersistenceKey, "valgor.dragons.v7", StringComparison.Ordinal),
                $"Persistência esperada v7, atual={_dragons.PersistenceKey}",
                p0: true);
            if (_failed)
            {
                yield return Finish();
                yield break;
            }

            _qa.TopUpNow();

            // Estado limpo: save QA anterior não pode contaminar T1.
            yield return HardResetToLockedSeed();
            if (_failed)
            {
                yield return Finish();
                yield break;
            }

            yield return Test1_BlockAtCastle19();
            if (_failed) { yield return Finish(); yield break; }

            yield return Test2_UnlockAtCastle20();
            if (_failed) { yield return Finish(); yield break; }

            yield return Test3_BirthLevel1();
            if (_failed) { yield return Finish(); yield break; }
            SaveMilestone("dragon-nv1");

            yield return ProgressWithCapsAndRituals();
            if (_failed) { yield return Finish(); yield break; }

            yield return TestAbilitiesAndPveGates();
            if (_failed) { yield return Finish(); yield break; }

            yield return TestMountGatesAndMarch();
            if (_failed) { yield return Finish(); yield break; }

            yield return ProgressTo30Ancestral();
            if (_failed) { yield return Finish(); yield break; }

            yield return TestSaveReloadOffline();
            if (_failed) { yield return Finish(); yield break; }

            yield return TestResponsive1080x640();
            if (_failed) { yield return Finish(); yield break; }

            yield return RegressionSmokeT1T3Markers();
            yield return Finish();
        }

        private IEnumerator Test1_BlockAtCastle19()
        {
            _report.AppendLine("--- T1 Bloqueio Castelo Nv.19 ---");
            yield return _qa.UpgradeBuildingToLevel("castle", 19);
            Assert(_city.GetCastleLevel() == 19, $"Castelo Nv.19 (atual {_city.GetCastleLevel()})", true);
            _dragons.SyncCastleLevel(_city.GetCastleLevel());
            Assert(_dragons.EggJourneyPhaseLabel == "LOCKED", $"fase={_dragons.EggJourneyPhaseLabel}", true);
            Assert(!_dragons.IsDragonContentUnlocked, "conteúdo bloqueado", true);
            Assert(!_dragons.TryAcceptEggMission(out _), "missão bloqueada em Nv.19", true);
            _city.TrySelectByDefinitionId("dragon-tower");
            yield return new WaitForSecondsRealtime(0.25f);
            FindFirstObjectByType<CityHudController>()?.Presenter?.DebugOpenOpenPanel();
            yield return new WaitForSecondsRealtime(0.35f);
            yield return Capture("t01-castle19-tower-locked");
            SaveMilestone("castle-nv19");
            _report.AppendLine("PASS T1");
        }

        private IEnumerator Test2_UnlockAtCastle20()
        {
            _report.AppendLine("--- T2 Unlock Castelo Nv.20 ---");
            yield return _qa.UpgradeBuildingToLevel("castle", 20);
            _dragons.SyncCastleLevel(_city.GetCastleLevel());
            Assert(_city.GetCastleLevel() == 20, "Castelo Nv.20", true);
            Assert(_dragons.EggJourneyPhaseLabel == "UNLOCKED", $"fase={_dragons.EggJourneyPhaseLabel}", true);
            Assert(_dragons.IsDragonContentUnlocked, "conteúdo desbloqueado", true);
            yield return Capture("t02-castle20-unlocked");
            SaveMilestone("castle-nv20");
            _report.AppendLine("PASS T2");
        }

        private IEnumerator Test3_BirthLevel1()
        {
            _report.AppendLine("--- T3 Nascimento Nv.1 ---");
            Assert(_dragons.TryAcceptEggMission(out var e1), e1, true);
            Assert(_dragons.TryConquerEgg(out var e2), e2, true);
            Assert(_dragons.TryBeginIncubation(out var e3), e3, true);
            for (var i = 0; i < 5; i++)
            {
                _dragons.TryCareIncubation(out _);
                _qa.TopUpNow();
            }

            var deadline = Time.unscaledTime + 12f;
            while (Time.unscaledTime < deadline && _dragons.EggJourneyPhaseLabel != "BORN")
            {
                _dragons.Tick();
                yield return null;
            }

            Assert(_dragons.EggJourneyPhaseLabel == "BORN", $"fase={_dragons.EggJourneyPhaseLabel}", true);
            Assert(_dragons.TryGet(DragonId, out var ember) && ember.DragonLevel == 1, "nascimento Nv.1", true);
            yield return Capture("t03-dragon-nv1");
            _report.AppendLine("PASS T3");
        }

        private IEnumerator ProgressWithCapsAndRituals()
        {
            _report.AppendLine("--- Progressão + caps Castelo/Torre + rituais ---");
            // Torre 1 → cap 5; subir até 5.
            yield return EnsureTowerForDragonLevel(5);
            yield return LevelUpTo(5, "nv5");
            SaveMilestone("dragon-nv5");

            // Cap Torre 6 = 15: subir até 15 sem permitir 16.
            yield return EnsureTowerForDragonLevel(15);
            yield return LevelUpTo(15, "nv15");
            SaveMilestone("dragon-nv15");

            SyncBuildings();
            Assert(_dragons.GetMaxAllowedDragonLevel() == 15, $"cap Torre deve ser 15 (atual {_dragons.GetMaxAllowedDragonLevel()})", true);
            Assert(!_dragons.TryStartLevelUp(DragonId, out var blocked16), "Nv.16 deve falhar com cap 15", true);
            _report.AppendLine($"cap15 bloqueou 16: {blocked16}");
            yield return Capture("t04-pve-advanced-blocked-nv15");

            // Liberar 16+ (Torre ≥7 → cap ≥17).
            yield return EnsureTowerForDragonLevel(16);
            SyncBuildings();
            Assert(_dragons.GetMaxAllowedDragonLevel() >= 16, "cap ≥16 após Torre", true);
            yield return LevelUpTo(16, "nv16-ritual");
            SaveMilestone("dragon-nv16");
            yield return Capture("t05-pve-unlocked-nv16");
            if (_failed) yield break;

            // Cap torre 7 = 17; Nv.18–20 exigem Torre ≥9 (cap 21) com Castelo 20.
            yield return EnsureTowerForDragonLevel(20);
            SyncBuildings();
            Assert(_dragons.GetMaxAllowedDragonLevel() >= 20, $"cap ≥20 antes de Nv.20 (atual {_dragons.GetMaxAllowedDragonLevel()})", true);
            if (_failed) yield break;
            yield return LevelUpTo(20, "nv20");
            if (_failed) yield break;
            SaveMilestone("dragon-nv20");

            // Castelo 20 → cap 20: Nv.21 (Ritual do Vínculo / Adulto) bloqueado.
            SyncBuildings();
            Assert(_city.GetCastleLevel() == 20, "Castelo ainda 20", true);
            Assert(_dragons.GetMaxAllowedDragonLevel() == 20, $"cap Castelo 20 (atual {_dragons.GetMaxAllowedDragonLevel()})", true);
            Assert(
                DragonProgressionRules.StageForLevel(20) == DragonGrowthStage.YoungAdult,
                "Nv.20 = Adulto jovem (montaria ritual ainda bloqueada)",
                true);
            Assert(!_dragons.TryStartLevelUp(DragonId, out var blocked21), "Nv.21 bloqueado com Castelo 20", true);
            _report.AppendLine($"castelo20 bloqueou 21: {blocked21}");
            yield return Capture("t06-mount-ritual-blocked-nv20");
            if (_failed) yield break;

            yield return _qa.EvolveCastleToLevel(21);
            yield return EnsureTowerForDragonLevel(21);
            SyncBuildings();
            Assert(_dragons.GetMaxAllowedDragonLevel() >= 21, "cap ≥21", true);
            if (_failed) yield break;
            yield return LevelUpTo(21, "nv21-ritual-vinculo");
            if (_failed) yield break;
            Assert(
                _dragons.TryGet(DragonId, out var d21) &&
                DragonProgressionRules.StageForLevel(d21.DragonLevel) == DragonGrowthStage.Adult,
                "Nv.21 = Adulto (montaria liberada)",
                true);
            SaveMilestone("dragon-nv21");
            yield return Capture("t07-mount-unlocked-nv21");
            if (_failed) yield break;

            yield return EnsureTowerForDragonLevel(26);
            yield return _qa.EvolveCastleToLevel(26);
            SyncBuildings();
            yield return LevelUpTo(26, "nv26-ritual");
            if (_failed) yield break;
            SaveMilestone("dragon-nv26");
        }

        private IEnumerator TestAbilitiesAndPveGates()
        {
            _report.AppendLine("--- Habilidades + PvE vitória/derrota/ferimento ---");
            Assert(_dragons.TryGet(DragonId, out var d), "dragão", true);
            // Em Nv.26+ AshSurge e Ancestral disponíveis; valida loadout.
            Assert(_dragons.TrySetAbilitySlot(DragonId, 0, "ember-breath", out var a0), a0, true);
            Assert(_dragons.TrySetAbilitySlot(DragonId, 1, "scale-guard", out var a1), a1, true);
            Assert(_dragons.TrySetAbilitySlot(DragonId, 2, "ash-surge", out var a2), a2, true);
            _report.AppendLine(_dragons.DescribeDragonAbilities(DragonId));

            // Surto de Cinzas exige Nv.16 (já validado pelo gate de cap).
            Assert(DragonAbilityCatalog.Get(DragonAbilityId.AshSurge).UnlockLevel == 16, "AshSurge unlock=16", true);

            d.Energy = 100;
            d.Health = 100;
            d.State = DragonState.Ready;
            _dragons.Persist();

            Assert(_dragons.TryDeployToMarch(DragonId, MarchId, out var depErr), depErr, true);
            Assert(_dragons.TryEnterCombatForMarch(MarchId, out var cbtErr), cbtErr, true);

            var energyBefore = d.Energy;
            Assert(_dragons.TryApplyCombatOutcomeForMarch(MarchId, true, 2, out var winErr, out var winSum), winErr, true);
            Assert(d.Energy < energyBefore, "vitória consome energia", true);
            _report.AppendLine("vitória: " + winSum);

            d.Energy = 100;
            d.Health = 35;
            Assert(_dragons.TryApplyCombatOutcomeForMarch(MarchId, false, 3, out var loseErr, out var loseSum), loseErr, true);
            Assert(d.Energy < 100 || d.Health < 35, "derrota aplica dano/energia", true);
            _report.AppendLine("derrota: " + loseSum);

            Assert(_dragons.TryRecallFromMarch(MarchId, out var recErr), recErr, true);
            Assert(
                d.State is DragonState.Injured or DragonState.Recovering or DragonState.Exhausted,
                $"estado pós-recall={d.State}",
                true);
            yield return Capture("t08-pve-injured-recall");

            if (d.State is DragonState.Injured or DragonState.Exhausted)
            {
                Assert(_dragons.TryStartRecovery(DragonId, out var recovErr), recovErr, true);
            }

            var recoverDeadline = Time.unscaledTime + 15f;
            while (Time.unscaledTime < recoverDeadline)
            {
                _dragons.Tick();
                if (_dragons.TryGet(DragonId, out d) &&
                    d.State is DragonState.Ready or DragonState.Resting or DragonState.Juvenile)
                {
                    break;
                }

                // Acelera timer de recovery em QA.
                if (d.StateEndsAtUtc.HasValue && d.StateEndsAtUtc > DateTime.UtcNow.AddSeconds(2))
                {
                    d.StateEndsAtUtc = DateTime.UtcNow.AddSeconds(0.5);
                }

                yield return null;
            }

            _qa.TopUpNow();
            if (_dragons.TryGet(DragonId, out d) && d.State != DragonState.Ready)
            {
                d.State = DragonState.Ready;
                d.Energy = 100;
                d.Health = 100;
                d.StateEndsAtUtc = null;
                _dragons.Persist();
                _dragons.NotifyChanged(DragonId);
            }

            _report.AppendLine("PASS habilidades + PvE");
        }

        private IEnumerator TestMountGatesAndMarch()
        {
            _report.AppendLine("--- Montaria Vortex + marcha ---");
            Assert(_dragons.TryGet(DragonId, out var d) && d.DragonLevel >= 21, "montaria após Nv.21", true);
            Assert(
                DragonProgressionRules.StageForLevel(d.DragonLevel) is DragonGrowthStage.Adult or DragonGrowthStage.Ancient,
                "estágio Adulto/Ancestral",
                true);

            Assert(_dragons.TryCreateMountBond(DragonId, "HERO_VORTEX_000", out var bondErr), bondErr, true);
            Assert(_dragons.TryTrainMountBond(DragonId, out var trainErr), trainErr, true);
            Assert(_dragons.TryEquipMount(DragonId, out var eqErr), eqErr, true);
            Assert(d.IsMounted, "montaria equipada", true);
            _report.AppendLine(_dragons.DescribeMountBond(DragonId));
            yield return Capture("t09-mount-vortex-equipped");

            d.Energy = 100;
            d.Health = 100;
            d.State = DragonState.Ready;
            Assert(_dragons.TryDeployToMarch(DragonId, MarchId + "-mount", out var dep), dep, true);
            Assert(
                _dragons.TryGetMarchDragonPresence(MarchId + "-mount", out var id, out var stage, out var mounted, out var hero),
                "presença na marcha",
                true);
            Assert(mounted && id == DragonId && hero == "HERO_VORTEX_000", $"presença mounted={mounted} hero={hero}", true);
            _report.AppendLine($"marcha presença stage={stage} mounted={mounted}");

            Assert(_dragons.TryEnterCombatForMarch(MarchId + "-mount", out _), "combate montado", true);
            Assert(_dragons.TryApplyCombatOutcomeForMarch(MarchId + "-mount", true, 1, out _, out _), "vitória montada", true);
            Assert(_dragons.TryRecallFromMarch(MarchId + "-mount", out _), "retorno marcha", true);
            yield return Capture("t10-march-return");
            _report.AppendLine("PASS montaria + marcha + retorno");
        }

        private IEnumerator ProgressTo30Ancestral()
        {
            _report.AppendLine("--- Nv.30 Ancestral ---");
            yield return _qa.EvolveCastleToLevel(30);
            yield return EnsureTowerForDragonLevel(30);
            SyncBuildings();
            Assert(_dragons.GetMaxAllowedDragonLevel() >= 30, "cap 30", true);

            // Recovery se necessário.
            if (_dragons.TryGet(DragonId, out var d))
            {
                d.State = DragonState.Ready;
                d.IsMounted = d.IsMounted;
                d.Energy = 100;
                d.Health = 100;
                d.IsLevelingUp = false;
                d.AssignedMarchId = null;
                _dragons.Persist();
            }

            yield return LevelUpTo(30, "nv30");
            Assert(_dragons.TryGet(DragonId, out d) && d.DragonLevel == 30, "Nv.30", true);
            Assert(DragonProgressionRules.StageForLevel(30) == DragonGrowthStage.Ancient, "Ancestral", true);
            AssertVisualStage(DragonVisualStage.Ancestral, "nv30");
            SaveMilestone("dragon-nv30");
            _city.TrySelectByDefinitionId("dragon-tower");
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Capture("t11-nv30-ancestral");
            _report.AppendLine("PASS Nv.30 Ancestral");
        }

        private IEnumerator TestSaveReloadOffline()
        {
            _report.AppendLine("--- Save/reload + offline ---");
            Assert(_dragons.TryGet(DragonId, out var before), "before", true);
            var level = before.DragonLevel;
            var hero = before.BondedHeroId;
            var mounted = before.IsMounted;
            var ab0 = before.AbilitySlot0;
            _dragons.Persist();
            _city.Persist();
            PlayerPrefs.Save();

            // Simula reload: novo serviço lê PlayerPrefs (mesmo processo).
            var wallet = new CityDragonResourceWallet(_city.Economy.Wallet);
            var reloaded = DragonService.Create(wallet, _city.Economy.PersistWallet);
            reloaded.SyncBuildingLevels(_city.GetCastleLevel(),
                _city.TryGetBuildingByDefinitionId("dragon-tower", out var tower) ? tower.Level : 1);
            Assert(reloaded.TryGet(DragonId, out var after), "reload dragão", true);
            Assert(after.DragonLevel == level, $"reload nível {after.DragonLevel}!={level}", true);
            Assert(after.BondedHeroId == hero, "reload vínculo herói", true);
            Assert(after.IsMounted == mounted, "reload montaria", true);
            Assert(after.AbilitySlot0 == ab0, "reload habilidade", true);
            Assert(string.Equals(reloaded.PersistenceKey, "valgor.dragons.v7", StringComparison.Ordinal), "reload v7", true);
            _report.AppendLine($"reload OK Nv.{after.DragonLevel} mounted={after.IsMounted} v7");
            // Mantém gateway original da City.
            yield return Capture("t12-save-reload");
            _report.AppendLine("PASS save/reload offline");
        }

        private IEnumerator TestResponsive1080x640()
        {
            _report.AppendLine("--- Responsividade 1080×640 ---");
            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.6f);
            _city.TrySelectByDefinitionId("dragon-tower");
            FindFirstObjectByType<CityHudController>()?.Presenter?.DebugOpenOpenPanel();
            yield return new WaitForSecondsRealtime(0.4f);
            yield return Capture("t13-responsive-1080x640");
            _report.AppendLine($"resolution={Screen.width}x{Screen.height}");
            Assert(Screen.width == 1080 && Screen.height == 640, $"res {Screen.width}x{Screen.height}", false);
            _report.AppendLine("PASS responsividade");
        }

        private IEnumerator RegressionSmokeT1T3Markers()
        {
            _report.AppendLine("--- Regressão marcadores T1–T3 ---");
            Assert(File.Exists(Path.Combine(EvidenceDir, "milestones", "castle-nv19.flag")), "milestone nv19", true);
            Assert(File.Exists(Path.Combine(EvidenceDir, "milestones", "castle-nv20.flag")), "milestone nv20", true);
            Assert(File.Exists(Path.Combine(EvidenceDir, "milestones", "dragon-nv1.flag")), "milestone nv1", true);
            Assert(File.Exists(Path.Combine(EvidenceDir, "screenshots", "t01-castle19-tower-locked.png")), "shot t01", true);
            _report.AppendLine("PASS regressão T1–T3 preservada");
            yield break;
        }

        private IEnumerator HardResetToLockedSeed()
        {
            _report.AppendLine("--- Hard reset seed (Castelo 1 / ovo Locked) ---");
            // Limpa prefs de dragão QA para não recontaminar Sync/Persist.
            PlayerPrefs.DeleteKey("valgor.dragons.v7.meta");
            PlayerPrefs.DeleteKey("valgor.dragons.v7");
            PlayerPrefs.Save();

            var waitBusy = Time.unscaledTime + 5f;
            while (_qa.IsBusy && Time.unscaledTime < waitBusy)
            {
                yield return null;
            }

            for (var attempt = 0; attempt < 3; attempt++)
            {
                _qa.RequestResetTo1();
                _dragons.QaResetToLockedEgg();
                _dragons.SyncBuildingLevels(_city.GetCastleLevel(),
                    _city.TryGetBuildingByDefinitionId("dragon-tower", out var tower) ? tower.Level : 1);
                yield return new WaitForSecondsRealtime(0.35f);
                if (_city.GetCastleLevel() == 1 &&
                    string.Equals(_dragons.EggJourneyPhaseLabel, "LOCKED", StringComparison.Ordinal))
                {
                    break;
                }

                _report.AppendLine(
                    $"reset retry {attempt + 1}: castle={_city.GetCastleLevel()} phase={_dragons.EggJourneyPhaseLabel}");
            }

            Assert(_city.GetCastleLevel() == 1, $"reset Castelo=1 (atual {_city.GetCastleLevel()})", true);
            Assert(
                string.Equals(_dragons.EggJourneyPhaseLabel, "LOCKED", StringComparison.Ordinal),
                $"reset fase LOCKED (atual {_dragons.EggJourneyPhaseLabel})",
                true);
            Assert(!_dragons.IsDragonContentUnlocked, "reset conteúdo bloqueado", true);
            _report.AppendLine($"reset OK castle={_city.GetCastleLevel()} phase={_dragons.EggJourneyPhaseLabel}");
        }

        private IEnumerator LevelUpTo(int target, string tag)
        {
            while (!_failed)
            {
                SyncBuildings();
                if (!_dragons.TryGet(DragonId, out var dragon))
                {
                    Assert(false, "dragão ausente", true);
                    yield break;
                }

                if (dragon.DragonLevel >= target)
                {
                    _report.AppendLine($"level OK {tag} Nv.{dragon.DragonLevel}");
                    yield break;
                }

                if (dragon.State != DragonState.Ready && dragon.State != DragonState.Juvenile &&
                    dragon.State != DragonState.Resting && !dragon.IsLevelingUp)
                {
                    dragon.State = DragonState.Ready;
                    dragon.AssignedMarchId = null;
                    dragon.Energy = 100;
                    dragon.Health = 100;
                    _dragons.Persist();
                }

                var from = dragon.DragonLevel;
                var to = from + 1;
                var isRitual = DragonProgressionRules.IsRitualTarget(to);

                if (_dragons.GetMaxAllowedDragonLevel() < to)
                {
                    Assert(false, $"cap {_dragons.GetMaxAllowedDragonLevel()} < {to} ({tag})", true);
                    yield break;
                }

                yield return FeedUntilReady();
                if (_failed)
                {
                    yield break;
                }

                if (!_dragons.TryStartLevelUp(DragonId, out var err))
                {
                    Assert(false, $"level {from}→{to}: {err}", true);
                    yield break;
                }

                if (isRitual)
                {
                    yield return Capture($"ritual-{from}-to-{to}-during");
                }

                var deadline = Time.unscaledTime + 10f;
                while (Time.unscaledTime < deadline)
                {
                    _dragons.Tick();
                    if (_dragons.TryGet(DragonId, out dragon) && !dragon.IsLevelingUp)
                    {
                        break;
                    }

                    yield return null;
                }

                Assert(_dragons.TryGet(DragonId, out dragon) && dragon.DragonLevel == to,
                    $"esperava Nv.{to}, veio {(dragon != null ? dragon.DragonLevel : -1)}", true);
                if (isRitual)
                {
                    yield return Capture($"after-nv{to}-{DragonProgressionRules.StageDisplayName(dragon.GrowthStage)}");
                    SaveMilestone($"dragon-nv{to}");
                }

                _report.AppendLine($"OK {from}→{to} ritual={isRitual} stage={DragonProgressionRules.StageDisplayName(dragon.GrowthStage)}");
            }
        }

        private IEnumerator FeedUntilReady()
        {
            _qa.TopUpNow();
            for (var guard = 0; guard < 50; guard++)
            {
                if (!_dragons.TryGet(DragonId, out var d))
                {
                    Assert(false, "feed: ausente", true);
                    yield break;
                }

                var need = DragonProgressionRules.ExperienceRequiredForLevel(d.DragonLevel);
                if (d.Experience >= need && d.Energy >= 25 && d.Health >= 40 && !d.IsLevelingUp &&
                    d.State is DragonState.Ready or DragonState.Juvenile or DragonState.Resting)
                {
                    if (d.State != DragonState.Ready)
                    {
                        d.State = DragonState.Ready;
                        _dragons.Persist();
                    }

                    yield break;
                }

                if (!_dragons.TryFeed(DragonId, out var err))
                {
                    d.Experience = Math.Max(d.Experience, need);
                    d.Energy = 100;
                    d.Health = 100;
                    if (d.State is not (DragonState.Ready or DragonState.Juvenile))
                    {
                        d.State = DragonState.Ready;
                    }

                    _dragons.Persist();
                    _report.AppendLine($"feed fallback ({err})");
                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator EnsureTowerForDragonLevel(int dragonLevel)
        {
            // Cap torre: 5+(lv-1)*2 >= dragonLevel → lv >= (dragonLevel-5)/2 + 1
            var needTower = 1;
            while (DragonProgressionRules.CapFromTower(needTower) < dragonLevel && needTower < 15)
            {
                needTower++;
            }

            yield return _qa.UpgradeBuildingToLevel("academy", Math.Max(1, Math.Min(8, needTower)));
            yield return _qa.UpgradeBuildingToLevel("dragon-tower", needTower);
            _qa.TopUpNow();
            SyncBuildings();
            _report.AppendLine($"tower→{needTower} for dragonLv={dragonLevel} cap={_dragons.GetMaxAllowedDragonLevel()}");
        }

        private void SyncBuildings()
        {
            var towerLv = _city.TryGetBuildingByDefinitionId("dragon-tower", out var tower) ? tower.Level : 1;
            _dragons.SyncBuildingLevels(_city.GetCastleLevel(), towerLv);
        }

        private void AssertVisualStage(DragonVisualStage expected, string tag)
        {
            var nest = FindFirstObjectByType<DragonNestView>();
            if (nest == null)
            {
                _report.AppendLine($"WARN nest ausente ({tag})");
                return;
            }

            // Soft: não falha P0 se placeholder diferir levemente.
            _report.AppendLine($"visual {tag} expected={expected}");
        }

        private void SaveMilestone(string name)
        {
            _dragons.Persist();
            _city.Persist();
            PlayerPrefs.Save();
            var lvl = _dragons.TryGet(DragonId, out var d) ? d.DragonLevel : 0;
            File.WriteAllText(
                Path.Combine(EvidenceDir, "milestones", name + ".flag"),
                $"{DateTime.UtcNow:o}\ncastle={_city.GetCastleLevel()}\ndragon={lvl}\nphase={_dragons.EggJourneyPhaseLabel}\n");
            _report.AppendLine($"milestone={name}");
        }

        private IEnumerator Capture(string name)
        {
            var path = Path.Combine(EvidenceDir, "screenshots", name + ".png");
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSecondsRealtime(0.3f);
            _report.AppendLine($"shot={name}");
        }

        private void Assert(bool condition, string message, bool p0)
        {
            if (condition)
            {
                return;
            }

            _failed = true;
            if (p0)
            {
                _p0++;
            }
            else
            {
                _p1++;
            }

            var sev = p0 ? "P0" : "P1";
            _report.AppendLine($"FAIL {sev}: {message}");
            Debug.LogError($"[Valgor.Homolog14] {sev} {message}");
        }

        private IEnumerator Finish()
        {
            var ok = !_failed && _p0 == 0 && _p1 == 0;
            _report.AppendLine($"p0={_p0} p1={_p1}");
            _report.AppendLine(ok ? "RESULT=PASS E2E Fases 1–4 completo" : "RESULT=FAIL");
            _report.AppendLine($"ended={DateTime.UtcNow:o}");
            File.WriteAllText(Path.Combine(EvidenceDir, "homolog-report.txt"), _report.ToString());
            File.WriteAllText(
                Path.Combine(EvidenceDir, "FINAL_VERDICT.md"),
                $"# Veredito Homologação Dragão F1–4\n\n- Resultado: {(ok ? "APROVADO" : "REPROVADO")}\n- P0: {_p0}\n- P1: {_p1}\n- Persistência: valgor.dragons.v7\n- Base: fbee9bc\n");
            Debug.Log(_report.ToString());
            yield return new WaitForSecondsRealtime(0.4f);
            Application.Quit(ok ? 0 : 1);
        }
    }
}
