using System;
using System.Collections.Generic;
using Valgor.City.Buildings;
using Valgor.City.Data;
using Valgor.City.Production;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;

namespace Valgor.City.Core
{
    public sealed class CityController
    {
        public const int ConstructionQueueSlots = 1;
        public const int ResearchQueueSlots = 1;

        private readonly ResourceWallet _wallet;
        private readonly CityEconomy _economy;
        private readonly Dictionary<BuildingInstance, BuildingDefinition> _definitions = new();
        private readonly Dictionary<BuildingInstance, BuildingView> _views = new();
        private readonly List<BuildingInstance> _buildings = new();
        private DragonService? _dragons;

        public CityController(CityEconomy economy, BuildingSelectionService selection)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _wallet = economy.Wallet;
            Selection = selection;
            Selection.SelectionChanged += OnSelectionChanged;
            _economy.Production.Changed += (_, __) =>
            {
                RefreshWorldIndicators();
                BuildingChanged?.Invoke();
            };
        }

        public BuildingSelectionService Selection { get; }
        public CityEconomy Economy => _economy;
        public IDragonGateway? Dragons => _dragons;
        public IReadOnlyList<BuildingInstance> Buildings => _buildings;
        public event Action? BuildingChanged;

        public void BindDragons(DragonService dragons)
        {
            _dragons = dragons ?? throw new ArgumentNullException(nameof(dragons));
            _dragons.Changed += (_, __) => BuildingChanged?.Invoke();
        }

        public void Add(BuildingSlot slot, BuildingInstance instance, BuildingDefinition definition, BuildingView view)
        {
            slot.Initialize(slot.SlotId, definition.Id, instance);
            _definitions.Add(instance, definition);
            _views.Add(instance, view);
            _buildings.Add(instance);
            _economy.Production.RegisterBuilding(instance);
            view.Clicked += _ => Selection.Select(instance);
            view.RefreshLabel(definition);
        }

        public BuildingDefinition GetDefinition(BuildingInstance instance) => _definitions[instance];

        public bool TryGetView(BuildingInstance instance, out BuildingView view) =>
            _views.TryGetValue(instance, out view!);

        public bool TrySelectByDefinitionId(string definitionId)
        {
            foreach (var building in _buildings)
            {
                if (string.Equals(building.DefinitionId, definitionId, StringComparison.Ordinal))
                {
                    Selection.Select(building);
                    return true;
                }
            }

            return false;
        }

        public bool TryUpgradeSelected()
        {
            var building = Selection.Selected;
            if (building == null || !_definitions.TryGetValue(building, out var definition) || !CanUpgrade(building, definition))
            {
                return false;
            }

            if (GetActiveConstructionCount() >= ConstructionQueueSlots)
            {
                return false;
            }

            foreach (var cost in definition.BaseCosts)
            {
                var amount = definition.GetUpgradeCost(cost.Key, building.Level);
                if (_wallet.Get(cost.Key) < amount)
                {
                    return false;
                }
            }

            foreach (var cost in definition.BaseCosts)
            {
                _wallet.TrySpend(cost.Key, definition.GetUpgradeCost(cost.Key, building.Level));
            }

            var completesAt = _economy.Clock.UtcNow + definition.GetUpgradeDuration(building.Level);
            building.BeginUpgrade(completesAt);
            if (_views.TryGetValue(building, out var view))
            {
                view.RefreshStateColor();
                view.RefreshLabel(definition);
            }

            _economy.Persist(_buildings);
            BuildingChanged?.Invoke();
            RefreshWorldIndicators();
            return true;
        }

        public int GetCastleLevel()
        {
            var fromBuildings = 1;
            foreach (var building in _buildings)
            {
                if (string.Equals(building.DefinitionId, "castle", StringComparison.Ordinal))
                {
                    fromBuildings = Math.Max(1, building.Level);
                    break;
                }
            }

            return Math.Max(fromBuildings, Valgor.Core.BetaProgress.CastleLevel);
        }

        public void SyncBetaProgress()
        {
            Valgor.Core.BetaProgress.SyncCastleLevel(GetCastleLevel());
            foreach (var building in _buildings)
            {
                if (string.Equals(building.DefinitionId, "laboratory", StringComparison.Ordinal) &&
                    building.Level >= 1 &&
                    building.State != BuildingState.Locked)
                {
                    Valgor.Core.BetaProgress.UnlockGatherResearch();
                    break;
                }
            }
        }

        public bool CanUpgrade(BuildingInstance building, BuildingDefinition definition)
        {
            if (!building.CanUpgrade(definition))
            {
                return false;
            }

            if (GetActiveConstructionCount() >= ConstructionQueueSlots &&
                building.State != BuildingState.Upgrading)
            {
                return false;
            }

            if (string.Equals(building.DefinitionId, "castle", StringComparison.Ordinal))
            {
                return true;
            }

            return building.Level < GetCastleLevel();
        }

        public string? GetUpgradeBlockReason(BuildingInstance building, BuildingDefinition definition)
        {
            if (building.State == BuildingState.Upgrading)
            {
                return "Melhoria em andamento.";
            }

            if (!building.CanUpgrade(definition))
            {
                return building.Level >= definition.MaxLevel ? "Nível máximo." : "Indisponível.";
            }

            if (GetActiveConstructionCount() >= ConstructionQueueSlots)
            {
                return "Fila de construção cheia (1/1).";
            }

            if (!string.Equals(building.DefinitionId, "castle", StringComparison.Ordinal) &&
                building.Level >= GetCastleLevel())
            {
                return $"Requer Castelo Nv.{building.Level + 1}";
            }

            return null;
        }

        public BuildingInstance? GetActiveConstruction()
        {
            foreach (var building in _buildings)
            {
                if (building.State == BuildingState.Upgrading)
                {
                    return building;
                }
            }

            return null;
        }

        public int GetActiveConstructionCount()
        {
            var count = 0;
            foreach (var building in _buildings)
            {
                if (building.State == BuildingState.Upgrading)
                {
                    count++;
                }
            }

            return count;
        }

        public string DescribeConstructionQueue()
        {
            var active = GetActiveConstruction();
            if (active == null)
            {
                return $"Construção {GetActiveConstructionCount()}/{ConstructionQueueSlots} · livre";
            }

            var def = _definitions[active];
            var remaining = active.UpgradeCompletesAtUtc.HasValue
                ? active.UpgradeCompletesAtUtc.Value - _economy.Clock.UtcNow
                : TimeSpan.Zero;
            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            return $"Construção 1/{ConstructionQueueSlots} · {def.DisplayName} → Nv.{active.Level + 1} ({FormatDuration(remaining)})";
        }

        public string DescribeResearchQueue()
        {
            if (Valgor.Core.BetaProgress.ResearchGatherBoost)
            {
                return $"Pesquisa 1/{ResearchQueueSlots} · Coleta + ativa";
            }

            return $"Pesquisa 0/{ResearchQueueSlots} · Lab desbloqueia Coleta +";
        }

        public long CollectSelected()
        {
            var building = Selection.Selected;
            if (building == null)
            {
                return 0;
            }

            _economy.Tick.ForceApply();
            var amount = _economy.Collection.Collect(building);
            if (amount > 0)
            {
                _economy.Persist(_buildings);
                BuildingChanged?.Invoke();
                RefreshWorldIndicators();
            }

            return amount;
        }

        public void Tick()
        {
            _economy.Tick.Update();
            _dragons?.Tick();
            AdvanceConstruction();
            RefreshWorldIndicators();
        }

        public void Persist()
        {
            _economy.Persist(_buildings);
            _dragons?.Persist();
        }

        public void RefreshPresentation() => RefreshWorldIndicators();

        private void AdvanceConstruction()
        {
            var now = _economy.Clock.UtcNow;
            var completed = false;
            foreach (var building in _buildings)
            {
                if (building.State != BuildingState.Upgrading ||
                    !building.UpgradeCompletesAtUtc.HasValue ||
                    now < building.UpgradeCompletesAtUtc.Value)
                {
                    continue;
                }

                building.CompleteUpgrade();
                _economy.Production.OnBuildingUpgraded(building);
                if (string.Equals(building.DefinitionId, "castle", StringComparison.Ordinal))
                {
                    Valgor.Core.BetaProgress.SyncCastleLevel(building.Level);
                }

                if (string.Equals(building.DefinitionId, "laboratory", StringComparison.Ordinal) &&
                    building.Level >= 1)
                {
                    Valgor.Core.BetaProgress.UnlockGatherResearch();
                }

                if (_views.TryGetValue(building, out var view) &&
                    _definitions.TryGetValue(building, out var definition))
                {
                    view.RefreshStateColor();
                    view.RefreshLabel(definition);
                }

                completed = true;
            }

            if (completed)
            {
                _economy.Persist(_buildings);
                BuildingChanged?.Invoke();
            }
        }

        private void RefreshWorldIndicators()
        {
            foreach (var pair in _views)
            {
                var building = pair.Key;
                var view = pair.Value;
                var definition = _definitions[building];
                view.RefreshLabel(definition);

                long amount = 0;
                ResourceType? resource = null;
                if (_economy.Production.TryGetState(building.DefinitionId, out var state) && state.HasCollectable)
                {
                    amount = state.Accumulated;
                    if (ProductionCatalog.TryGet(building.DefinitionId, out var prod))
                    {
                        resource = prod.Resource;
                    }
                }

                view.SetCollectable(amount, resource);
                view.SetUpgradeAvailable(CanUpgrade(building, definition) &&
                                         GetUpgradeBlockReason(building, definition) == null &&
                                         HasUpgradeFunds(building, definition));
            }
        }

        private bool HasUpgradeFunds(BuildingInstance building, BuildingDefinition definition)
        {
            foreach (var cost in definition.BaseCosts)
            {
                if (_wallet.Get(cost.Key) < definition.GetUpgradeCost(cost.Key, building.Level))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnSelectionChanged(BuildingInstance? selected)
        {
            foreach (var pair in _views)
            {
                pair.Value.SetSelected(ReferenceEquals(pair.Key, selected));
            }

            if (selected == null)
            {
                return;
            }

            if (string.Equals(selected.DefinitionId, "castle", StringComparison.Ordinal))
            {
                Valgor.UI.BetaJourneyGuide.NotifyCastleSelected();
            }
            else if (string.Equals(selected.DefinitionId, "farm", StringComparison.Ordinal))
            {
                Valgor.UI.BetaJourneyGuide.NotifyFarmSelected();
            }
        }

        private static string FormatDuration(TimeSpan value)
        {
            if (value.TotalHours >= 1)
            {
                return $"{(int)value.TotalHours}h{value.Minutes:00}m";
            }

            if (value.TotalMinutes >= 1)
            {
                return $"{value.Minutes}m{value.Seconds:00}s";
            }

            return $"{Math.Max(0, (int)value.TotalSeconds)}s";
        }
    }
}
