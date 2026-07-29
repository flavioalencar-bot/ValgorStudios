using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valgor.City.Buildings;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.Visual;
using Valgor.Core;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Operações de homologação: top-up, atender requisito, evoluir castelo, save/reload.
    /// Usa o fluxo real de upgrade (BeginUpgrade → CompleteUpgrade / InstantComplete).
    /// </summary>
    public sealed class CityProgressionQaController : MonoBehaviour
    {
        private CityController _city = null!;
        private bool _busy;
        private string _status = string.Empty;

        public bool IsBusy => _busy;
        public string Status => _status;

        public void Bind(CityController city)
        {
            _city = city ?? throw new ArgumentNullException(nameof(city));
        }

        public void TopUpNow()
        {
            if (!CityProgressionQa.IsActive || _city == null)
            {
                return;
            }

            CityProgressionQaBootstrap.TopUpWallet(_city.Economy.Wallet);
            CityProgressionQaBootstrap.TopUpEnergyPrefs();
            _city.Economy.PersistWallet();
            _city.RefreshPresentation();
        }

        public int GetCastleLevel() => _city?.GetCastleLevel() ?? 1;

        public int GetCastleVisualTier()
        {
            if (_city == null || !_city.TryGetBuildingByDefinitionId("castle", out var castle))
            {
                return 1;
            }

            if (!_city.TryGetView(castle, out var view))
            {
                return CastleRealVisualLoader.ResolveTier(castle.Level);
            }

            var visual = view.transform.Find("Visual");
            var attached = visual != null ? CastleRealVisualLoader.FindAttachedTier(visual) : 0;
            return attached > 0 ? attached : CastleRealVisualLoader.ResolveTier(castle.Level);
        }

        public void RequestEvolvePlusOne() => StartSafe(EvolveCastleToLevel(GetCastleLevel() + 1));

        public void RequestEvolveToNextTier()
        {
            var level = GetCastleLevel();
            var tier = CastleRealVisualLoader.ResolveTier(level);
            var target = Math.Min(30, tier * 5 + 1);
            if (target <= level)
            {
                target = Math.Min(30, level + 1);
            }

            StartSafe(EvolveCastleToLevel(target));
        }

        public void RequestEvolveTo30() => StartSafe(EvolveCastleToLevel(30));

        public void RequestResetTo1()
        {
            if (_busy || _city == null)
            {
                return;
            }

            _city.DebugResetBuildingsToSeedLayout();
            TopUpNow();
            _city.SyncCastleVisuals(animate: false);
            _status = "Reset → Nv.1 / Tier 1";
            Debug.Log($"[Valgor.QA] {_status}");
        }

        public void RequestSave()
        {
            if (_city == null)
            {
                return;
            }

            TopUpNow();
            _city.Persist();
            PlayerPrefs.Save();
            _status = $"Save OK ({CityProgressionQa.SaveSlotId})";
            Debug.Log($"[Valgor.QA] {_status} key={CityProgressionQa.PersistenceKey}");
        }

        public void RequestReload()
        {
            if (_busy)
            {
                return;
            }

            StartSafe(ReloadSaveRoutine());
        }

        /// <summary>Evolui o edifício exigido até o nível mínimo via fluxo normal.</summary>
        public void RequestSatisfyRequirement(string definitionId, int minimumLevel) =>
            StartSafe(UpgradeBuildingToLevel(definitionId, minimumLevel));

        public IEnumerator EvolveCastleToLevel(int targetLevel)
        {
            targetLevel = Math.Clamp(targetLevel, 1, 30);
            _status = $"Evoluindo Castelo → Nv.{targetLevel}…";
            yield return UpgradeBuildingToLevel("castle", targetLevel);
        }

        public IEnumerator UpgradeBuildingToLevel(string definitionId, int targetLevel)
        {
            if (_city == null || string.IsNullOrEmpty(definitionId))
            {
                yield break;
            }

            if (!_city.TryGetBuildingByDefinitionId(definitionId, out _))
            {
                _status = $"Edifício ausente: {definitionId}";
                yield break;
            }

            var guard = 0;
            while (guard++ < 400)
            {
                TopUpNow();

                if (!_city.TryGetBuildingByDefinitionId(definitionId, out var building))
                {
                    yield break;
                }

                if (building.Level >= targetLevel)
                {
                    _status = $"{definitionId} OK Nv.{building.Level}";
                    yield break;
                }

                if (building.State == BuildingState.Upgrading)
                {
                    if (!_city.TrySelectByDefinitionId(definitionId))
                    {
                        yield return null;
                        continue;
                    }

                    if (_city.TryInstantCompleteSelected(out var instantError))
                    {
                        TopUpNow();
                        yield return null;
                        continue;
                    }

                    // Espera conclusão natural (timer QA ~2s).
                    var deadline = Time.unscaledTime + CityProgressionQa.HomologDurationSeconds + 2f;
                    while (building.State == BuildingState.Upgrading && Time.unscaledTime < deadline)
                    {
                        yield return null;
                    }

                    TopUpNow();
                    continue;
                }

                if (GetActiveConstructionCount() > 0)
                {
                    yield return WaitForConstructionIdle();
                    continue;
                }

                var definition = _city.GetDefinition(building);
                if (building.Level >= definition.MaxLevel)
                {
                    _status = $"{definitionId} no MaxLevel={definition.MaxLevel} (alvo {targetLevel})";
                    Debug.LogWarning($"[Valgor.QA] {_status}");
                    yield break;
                }

                // Satisfaz primeiro pré-requisito bloqueado (recursivo).
                if (TryFindFirstUnmet(building, out var blocked))
                {
                    if (!string.IsNullOrEmpty(blocked.JumpToDefinitionId) &&
                        blocked.RequiredMinimumLevel > 0 &&
                        !string.Equals(blocked.JumpToDefinitionId, definitionId, StringComparison.Ordinal))
                    {
                        var depId = blocked.JumpToDefinitionId!;
                        var depMin = blocked.RequiredMinimumLevel;

                        // Deadlock catálogo (ex.: Castelo→2 precisa Fazenda 2 e Fazenda→2 precisa Castelo 2).
                        if (_city.TryGetBuildingByDefinitionId(depId, out var depBuilding) &&
                            TryFindFirstUnmet(depBuilding, out var depBlock) &&
                            string.Equals(depBlock.JumpToDefinitionId, definitionId, StringComparison.Ordinal))
                        {
                            Debug.Log(
                                $"[Valgor.QA] Deadlock {definitionId}↔{depId} — forçando passo em {depId}");
                            if (!TryForceOneUpgradeLevel(depBuilding))
                            {
                                _status = $"Falha forçar {depId}";
                                yield break;
                            }

                            TopUpNow();
                            yield return null;
                            continue;
                        }

                        Debug.Log($"[Valgor.QA] Atendendo requisito {depId}→Nv.{depMin} para {definitionId}");
                        yield return UpgradeBuildingToLevel(depId, depMin);
                        continue;
                    }

                    if (string.IsNullOrEmpty(blocked.JumpToDefinitionId))
                    {
                        _status = $"Bloqueado (unlock): {blocked.Detail}";
                        Debug.LogWarning($"[Valgor.QA] {_status}");
                        yield break;
                    }
                }

                if (!_city.TrySelectByDefinitionId(definitionId))
                {
                    yield return null;
                    continue;
                }

                if (!_city.TryUpgradeSelected())
                {
                    var reason = _city.GetUpgradeBlockReason(building, definition) ?? "upgrade falhou";
                    // Tenta instant se já estiver upgrading por race.
                    if (building.State == BuildingState.Upgrading)
                    {
                        _city.TryInstantCompleteSelected(out _);
                        TopUpNow();
                        yield return null;
                        continue;
                    }

                    _status = $"Falha upgrade {definitionId}: {reason}";
                    Debug.LogWarning($"[Valgor.QA] {_status}");
                    yield return new WaitForSecondsRealtime(0.25f);
                    // Se o bloqueio for dependência, tenta de novo após pequeno wait.
                    continue;
                }

                // Conclusão via InstantComplete (ainda passa por CompleteUpgrade).
                yield return null;
                if (!_city.TryInstantCompleteSelected(out var err))
                {
                    // Fallback: espera timer QA.
                    var waitUntil = Time.unscaledTime + CityProgressionQa.HomologDurationSeconds + 1.5f;
                    while (building.State == BuildingState.Upgrading && Time.unscaledTime < waitUntil)
                    {
                        yield return null;
                    }
                }

                TopUpNow();
                yield return null;
            }

            _status = $"Timeout evoluindo {definitionId} → {targetLevel}";
            Debug.LogError($"[Valgor.QA] {_status}");
        }

        private bool TryForceOneUpgradeLevel(BuildingInstance building)
        {
            if (_city == null || building == null)
            {
                return false;
            }

            TopUpNow();
            var definition = _city.GetDefinition(building);
            if (building.Level >= definition.MaxLevel)
            {
                return false;
            }

            if (building.State == BuildingState.Upgrading)
            {
                _city.TrySelectByDefinitionId(building.DefinitionId);
                return _city.TryInstantCompleteSelected(out _);
            }

            // Paga custos e conclui via BeginUpgrade + CompleteUpgrade (sem pular eventos).
            foreach (var cost in definition.BaseCosts)
            {
                var amount = definition.GetUpgradeCost(cost.Key, building.Level);
                if (amount > 0)
                {
                    _city.Economy.Wallet.TrySpend(cost.Key, amount);
                }
            }

            var completesAt = _city.Economy.Clock.UtcNow;
            building.BeginUpgrade(completesAt);
            building.CompleteUpgrade();
            _city.Economy.Production.OnBuildingUpgraded(building);
            if (string.Equals(building.DefinitionId, "castle", StringComparison.Ordinal))
            {
                BetaProgress.SyncCastleLevel(building.Level);
            }

            if (_city.TryGetView(building, out var view))
            {
                view.RefreshStateColor();
                view.RefreshLabel(definition);
                view.SetConstructionProgress(0f, string.Empty, false);
                if (string.Equals(building.DefinitionId, "castle", StringComparison.Ordinal))
                {
                    view.SyncCastleVisual(animate: !CityProgressionQa.IsActive);
                }
            }

            _city.Persist();
            _city.NotifyBuildingChanged();
            TopUpNow();
            return true;
        }

        private bool TryFindFirstUnmet(BuildingInstance building, out BuildingDependencyCheck check)
        {
            foreach (var item in _city.GetDependencyChecks(building))
            {
                if (!item.Satisfied)
                {
                    check = item;
                    return true;
                }
            }

            check = default;
            return false;
        }

        private int GetActiveConstructionCount() => _city.GetActiveConstructionCount();

        private IEnumerator WaitForConstructionIdle()
        {
            var deadline = Time.unscaledTime + 30f;
            while (GetActiveConstructionCount() > 0 && Time.unscaledTime < deadline)
            {
                // Tenta concluir o que estiver selecionável.
                foreach (var b in _city.Buildings)
                {
                    if (b.State != BuildingState.Upgrading)
                    {
                        continue;
                    }

                    _city.TrySelectByDefinitionId(b.DefinitionId);
                    _city.TryInstantCompleteSelected(out _);
                    break;
                }

                TopUpNow();
                yield return null;
            }
        }

        private IEnumerator ReloadSaveRoutine()
        {
            _status = "Recarregando save QA…";
            _city.Persist();
            PlayerPrefs.Save();

            // Reaplica snapshot do repositório QA nos edifícios em memória.
            var snapshot = _city.Economy.Repository.Load();
            if (snapshot == null)
            {
                _status = "Save QA vazio";
                yield break;
            }

            foreach (var building in _city.Buildings)
            {
                if (snapshot.BuildingProgress.TryGetValue(building.DefinitionId, out var progress))
                {
                    building.ApplyPersisted(progress.Level, progress.State, progress.UpgradeCompletesAtUtc);
                }
            }

            foreach (var pair in snapshot.Wallet)
            {
                _city.Economy.Wallet.SetAmount(pair.Key, pair.Value);
            }

            TopUpNow();
            if (_city.TryGetBuildingByDefinitionId("castle", out var castle))
            {
                BetaProgress.SyncCastleLevel(castle.Level);
            }

            _city.SyncCastleVisuals(animate: false);
            _city.RefreshPresentation();
            _city.NotifyBuildingChanged();
            _status =
                $"Reload OK — Castelo Nv.{GetCastleLevel()} Tier {GetCastleVisualTier()}";
            Debug.Log($"[Valgor.QA] {_status}");
            yield return null;
        }

        private void StartSafe(IEnumerator routine)
        {
            if (!CityProgressionQa.IsActive)
            {
                return;
            }

            if (_busy)
            {
                _status = "QA ocupado…";
                return;
            }

            StartCoroutine(RunGuarded(routine));
        }

        private IEnumerator RunGuarded(IEnumerator routine)
        {
            _busy = true;
            try
            {
                yield return routine;
            }
            finally
            {
                _busy = false;
            }
        }
    }
}
