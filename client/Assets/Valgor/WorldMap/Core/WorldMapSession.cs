using System;
using System.Collections.Generic;
using Valgor.City.Buildings;
using Valgor.City.Data;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.Dragons.Data;
using Valgor.WorldMap.Camera;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Energy;
using Valgor.WorldMap.Filters;
using Valgor.WorldMap.Locate;
using Valgor.WorldMap.Marches;
using Valgor.WorldMap.Raids;
using Valgor.WorldMap.Territory;

namespace Valgor.WorldMap.Core
{
    /// <summary>
    /// Estado do mapa mundial que sobrevive City↔WorldMap via ServiceRegistry.
    /// </summary>
    public sealed class WorldMapSession
    {
        private readonly Dictionary<string, WorldNodeInstance> _nodes = new();
        private readonly Dictionary<string, WorldCreatureInstance> _creatures = new();
        private readonly Dictionary<string, CityRaidInstance> _raids = new();
        private readonly Dictionary<string, WorldTerritoryRuntime> _territories = new();
        private readonly ProcessedOperationRegistry _processedOperations = new();
        private readonly LocalDefenseScheduler _defenseScheduler;
        private readonly PendingDefenseProcessor _defenseProcessor;
        private readonly RaidResultApplicationService _raidApplication;
        private readonly IHeroesGateway _heroes;
        private IDragonGateway _dragons = new ProvisionalDragonGateway();
        private readonly WorldNodeOccupationService _occupation = new();
        private readonly IEnergyPersistenceRepository _energyRepository;
        private DateTime _lastTickUtc;
        private readonly double _tickIntervalSeconds;
        private int _boundWallLevel;
        private int _boundHospitalLevel;
        private int _boundWarehouseLevel;
        private bool _requestDominatorWrath;

        public WorldMapSession(
            WorldMapSettings settings,
            IWorldMapClock clock,
            IHeroesGateway heroes,
            IWorldMapRepository repository,
            PlayerEnergyWallet? energyWallet = null,
            IEnergyPersistenceRepository? energyRepository = null,
            IWorldMapFilterPersistenceRepository? filterRepository = null)
        {
            Settings = settings;
            Clock = clock;
            _heroes = heroes;
            Repository = repository;
            _energyRepository = energyRepository ?? new EnergyPersistenceRepository(settings.EnergyPersistenceKey);
            EnergyWallet = energyWallet ?? CreateEnergyWallet(settings, clock);
            EnergyRegen = new EnergyRegenerationService(EnergyWallet, clock);
            EnergyCosts = new EnergyCostResolver(EnergyWallet.Settings);
            Filters = new WorldMapFilterService(
                filterRepository ?? new WorldMapFilterPersistenceRepository(settings.FilterPersistenceKey));
            CameraPersistence = new WorldCameraPersistenceService(
                new WorldCameraStateRepository(settings.CameraPersistenceKey),
                defaultZoom: settings.DefaultCameraZoom);
            Selection = new WorldNodeSelectionService();
            RegionSelection = new RegionSelectionService();
            Harvest = new WorldResourceHarvestService();
            Gathering = new WorldResourceGatheringService(clock);
            Marches = new MarchService(
                clock,
                settings,
                heroes,
                id => WorldNodeCatalog.Get(id),
                id => _nodes[id],
                _occupation);
            Encounters = new CreatureEncounterService(
                id => WorldCreatureCatalog.TryGet(id, out var def) ? def : null,
                id => _creatures.TryGetValue(id, out var inst) ? inst : null,
                () => Marches.Active);
            CityRaids = new CityRaidEncounterService(
                id => CityRaidCatalog.TryGet(id, out var def) ? def : null,
                id => _raids.TryGetValue(id, out var inst) ? inst : null,
                id => WorldNodeCatalog.All.TryGetValue(id, out var node) ? node : null,
                () => Marches.Active);
            _defenseScheduler = new LocalDefenseScheduler(_processedOperations);
            _defenseProcessor = new PendingDefenseProcessor(_defenseScheduler, _processedOperations);
            _raidApplication = new RaidResultApplicationService(_processedOperations);
            Locator = new WorldMapLocatorService(
                settings,
                id => WorldNodeCatalog.Get(id),
                id => _nodes.TryGetValue(id, out var node) ? node : null,
                () => Selection.Selected,
                () => Marches.Active?.TargetNodeId);
            _tickIntervalSeconds = settings.MarchTickIntervalSeconds;
            _lastTickUtc = clock.UtcNow;
            SeedNodes();
            SeedCreatures();
            SeedRaids();
            SeedTerritories();
            Marches.Changed += (_, _) => Persist();
            Encounters.Changed += () =>
            {
                Persist();
                Changed?.Invoke();
            };
            CityRaids.Changed += () =>
            {
                Persist();
                Changed?.Invoke();
            };
            Gathering.Changed += () =>
            {
                Persist();
                Changed?.Invoke();
            };
            EnergyWallet.Changed += (_, _) =>
            {
                PersistEnergy();
                Changed?.Invoke();
            };
            Filters.Changed += () => Changed?.Invoke();
            Selection.SelectionChanged += _ => Persist();
        }

        public WorldMapSettings Settings { get; }
        public IWorldMapClock Clock { get; }
        public IWorldMapRepository Repository { get; }
        public WorldNodeSelectionService Selection { get; }
        public RegionSelectionService RegionSelection { get; }
        public MarchService Marches { get; }
        public WorldResourceHarvestService Harvest { get; }
        public WorldResourceGatheringService Gathering { get; }
        public CreatureEncounterService Encounters { get; }
        public CityRaidEncounterService CityRaids { get; }
        public WorldNodeOccupationService Occupation => _occupation;
        public PlayerEnergyWallet EnergyWallet { get; }
        public EnergyRegenerationService EnergyRegen { get; }
        public EnergyCostResolver EnergyCosts { get; }
        public WorldMapFilterService Filters { get; }
        public WorldMapLocatorService Locator { get; }
        public int Energy => EnergyWallet.CurrentEnergy;
        public ResourceWallet? BoundWallet { get; private set; }
        private Action? _persistBoundWallet;
        public IDragonGateway Dragons => _dragons;
        public WorldCameraPersistenceService CameraPersistence { get; }
        public IReadOnlyDictionary<string, WorldNodeInstance> Nodes => _nodes;
        public IReadOnlyDictionary<string, WorldCreatureInstance> Creatures => _creatures;
        public IReadOnlyDictionary<string, CityRaidInstance> Raids => _raids;
        public IReadOnlyDictionary<string, WorldTerritoryRuntime> Territories => _territories;
        public CityBattleReport? LastBattleReport { get; private set; }
        public ProcessedOperationRegistry ProcessedOperations => _processedOperations;
        public LocalDefenseScheduler DefenseScheduler => _defenseScheduler;
        public event Action? Changed;

        /// <summary>
        /// Disparado quando loot de marcha é depositado na carteira da cidade.
        /// Camada de apresentação (HUD/tutorial) assina — sem dependência de UI na lógica.
        /// </summary>
        public event Action? RewardDeposited;

        public static WorldMapSession Create(IHeroesGateway heroes, IWorldMapClock? clock = null)
        {
            clock ??= new SystemWorldMapClock();
            var settings = WorldMapSettings.Default;
            return new WorldMapSession(settings, clock, heroes, new LocalWorldMapRepository(settings.PersistenceKey));
        }

        public void BindWallet(ResourceWallet? wallet, Action? persistWallet = null)
        {
            BoundWallet = wallet;
            _persistBoundWallet = persistWallet;
        }

        public void BindCityDefense(int wallLevel)
        {
            BindCityDefenseStats(wallLevel);
        }

        public void BindCityDefenseStats(int wallLevel, int hospitalLevel = 1, int warehouseLevel = 1)
        {
            _boundWallLevel = Math.Max(0, wallLevel);
            _boundHospitalLevel = Math.Max(0, hospitalLevel);
            _boundWarehouseLevel = Math.Max(0, warehouseLevel);
        }

        public void SetDominatorWrathRequested(bool active) => _requestDominatorWrath = active;

        public string DescribeSelectedRaidInspection()
        {
            if (Selection.Selected == null ||
                !CityRaidCatalog.TryGet(Selection.Selected.DefinitionId, out var def))
            {
                return "Nenhuma cidade NPC de raid selecionada.";
            }

            var wall = _raids.TryGetValue(def.CityNodeId, out var inst) && inst.WallCurrentDurability > 0
                ? inst.WallCurrentDurability
                : def.Fortification.WallCurrentDurability;
            return def.DescribeInspection() + $"\nDurabilidade runtime: {wall}/{def.Fortification.WallMaxDurability}";
        }

        public string DescribeRaidPreparation()
        {
            var role = DescribeSelectedDragonBattleRole();
            var wrath = CanRequestDominatorWrath(out var wrathMsg);
            return
                $"Formação: {_heroes.DescribeFormation()}\n" +
                $"Poder marcha: {GetAttackerPower()}\n" +
                $"Dragão: {role}\n" +
                $"Ira do Dominador: {(wrath ? "disponível" : wrathMsg)}";
        }

        public bool CanRequestDominatorWrath(out string reason)
        {
            reason = string.Empty;
            var statuses = _dragons.GetDragonStatuses();
            if (statuses.Count == 0)
            {
                reason = "Sem dragão.";
                return false;
            }

            var st = statuses[0];
            if (!string.Equals(_dragons.GetBattleRoleLabel(st.DragonId), "Assault", StringComparison.OrdinalIgnoreCase))
            {
                reason = "Somente em Assault.";
                return false;
            }

            if (st.DragonLevel < RaidCombatCatalog.DominatorWrathUnlockLevel)
            {
                reason = $"Requer Nv.{RaidCombatCatalog.DominatorWrathUnlockLevel}+.";
                return false;
            }

            return true;
        }

        public void BindDragons(IDragonGateway dragons)
        {
            _dragons = dragons ?? throw new ArgumentNullException(nameof(dragons));
        }

        public WorldMapNodeDefinition GetDefinition(string id) => WorldNodeCatalog.Get(id);

        public WorldNodeInstance GetNode(string id) => _nodes[id];

        public bool TryGetCreature(string id, out WorldCreatureInstance instance) =>
            _creatures.TryGetValue(id, out instance!);

        public bool TryGetRaid(string cityNodeId, out CityRaidInstance instance) =>
            _raids.TryGetValue(cityNodeId, out instance!);

        public void LoadOrInitialize()
        {
            Filters.LoadOrInitialize();
            var snapshot = Repository.Load();
            LoadEnergy(snapshot);
            EnergyRegen.ApplyUntil(Clock.UtcNow);

            if (snapshot != null)
            {
                foreach (var pair in snapshot.Nodes)
                {
                    _nodes[pair.Key] = new WorldNodeInstance(
                        pair.Value.DefinitionId,
                        pair.Value.Status,
                        pair.Value.RemainingAmount)
                    {
                        OccupiedByMarchId = pair.Value.OccupiedByMarchId,
                        RespawnAt = pair.Value.RespawnAt,
                        LastGatherUpdatedUtc = pair.Value.LastGatherUpdatedUtc,
                        ResourceState = pair.Value.ResourceState
                    };
                }

                foreach (var pair in snapshot.Creatures)
                {
                    _creatures[pair.Key] = new WorldCreatureInstance(
                        pair.Value.DefinitionId,
                        pair.Value.State,
                        pair.Value.RegionId,
                        pair.Value.X,
                        pair.Value.Z,
                        pair.Value.RespawnAtUtc)
                    {
                        EngagedMarchId = pair.Value.EngagedMarchId
                    };
                }

                foreach (var pair in snapshot.Raids)
                {
                    _raids[pair.Key] = pair.Value.Clone();
                }

                MergeProcessed(snapshot.ProcessedOperations);
                _defenseScheduler.ReplaceAll(snapshot.DefenseEvents);
                if (snapshot.BoundWallLevel > 0 || snapshot.BoundHospitalLevel > 0 || snapshot.BoundWarehouseLevel > 0)
                {
                    BindCityDefenseStats(
                        snapshot.BoundWallLevel,
                        snapshot.BoundHospitalLevel,
                        snapshot.BoundWarehouseLevel);
                }

                Marches.Restore(snapshot.March, snapshot.LastAdvanceUtc, snapshot.LastCompletedMarch);
                DepositCompletedMarch(Marches.LastCompleted);
                Selection.RestoreFromId(snapshot.SelectedNodeId, _nodes);
                LastBattleReport = snapshot.LastBattleReport;
            }
            else
            {
                Marches.Advance(Clock.UtcNow);
                Selection.Deselect();
            }

            AdvanceCreatures(Clock.UtcNow);
            AdvanceRaids(Clock.UtcNow);
            AdvanceResourceRespawns(Clock.UtcNow);
            Persist();
            Changed?.Invoke();
        }

        public void Tick()
        {
            var now = Clock.UtcNow;
            if ((now - _lastTickUtc).TotalSeconds < _tickIntervalSeconds)
            {
                return;
            }

            _lastTickUtc = now;
            EnergyRegen.ApplyUntil(now);
            Marches.Advance(now);
            ApplyResourceGathering(now);
            AdvanceResourceRespawns(now);
            DepositCompletedMarch(Marches.LastCompleted);
            AdvanceCreatures(now);
            AdvanceRaids(now);
            _dragons.Tick();
            Persist();
            Changed?.Invoke();
        }

        public bool TryDispatchToSelected(out string error)
        {
            LastDispatchDetail = null;
            LastDispatchWasQueued = false;
            if (Selection.Selected == null)
            {
                error = "Nenhum nó selecionado.";
                return false;
            }

            if (Selection.Selected.Status == WorldNodeStatus.Locked)
            {
                error = "Nó bloqueado.";
                return false;
            }

            var selectedId = Selection.Selected.DefinitionId;
            var hadLiveMarch = Marches.Active != null;

            EnergyRegen.ApplyUntil(Clock.UtcNow);
            var dispatchCost = EnergyCosts.ResolveMarchDispatch();
            if (dispatchCost > 0 && !EnergyWallet.TrySpend(dispatchCost, out error))
            {
                return false;
            }

            if (!Marches.TryDispatch(selectedId, Settings.DefaultPlayerId, out error))
            {
                if (dispatchCost > 0)
                {
                    EnergyWallet.Add(dispatchCost);
                }

                return false;
            }

            if (hadLiveMarch &&
                Marches.HasQueuedMarch &&
                string.Equals(Marches.QueuedTargetNodeId, selectedId, StringComparison.Ordinal))
            {
                LastDispatchWasQueued = true;
                LastDispatchDetail = $"Fila: próxima → {selectedId}";
            }
            else if (Marches.Active != null)
            {
                if (_dragons.TryDeployFirstReadyToMarch(Marches.Active.Id, out var dragonError))
                {
                    var mountHint = string.Empty;
                    if (_dragons.TryGetMarchDragonPresence(
                            Marches.Active.Id,
                            out _,
                            out var stage,
                            out var mounted,
                            out var heroId) &&
                        mounted)
                    {
                        mountHint = $" · montaria {stage}" +
                                    (string.IsNullOrEmpty(heroId) ? string.Empty : $"/{heroId}");
                    }

                    LastDispatchDetail =
                        $"{_heroes.DescribeFormation()} · Dragão destacado{mountHint} · Poder {GetAttackerPower()}";
                }
                else
                {
                    LastDispatchDetail =
                        $"{_heroes.DescribeFormation()} · Sem dragão READY ({dragonError}) · Poder {_heroes.GetProvisionalMarchPower()}";
                }
            }

            Changed?.Invoke();
#if UNITY_5_3_OR_NEWER
            BetaMissions.Notify(MissionEvent.SendMarch);
#endif
            return true;
        }

        public bool TryReturnMarch(out string error)
        {
            if (!Marches.TryReturn(out error))
            {
                return false;
            }

            Changed?.Invoke();
            return true;
        }

        public bool TryCancelMarch(out string error)
        {
            var marchId = Marches.Active?.Id;
            if (!Marches.TryCancel(out error))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(marchId))
            {
                _dragons.TryRecallFromMarch(marchId, out _);
            }

            Changed?.Invoke();
            return true;
        }

        public bool TryCollectSelected(ResourceWallet? wallet, out string error, out long collected)
        {
            collected = 0;
            if (Selection.Selected == null)
            {
                error = "Nenhum nó selecionado.";
                return false;
            }

            if (wallet != null)
            {
                BoundWallet = wallet;
                // Mantém callback de persistência já vinculado, se houver.
            }

            var definition = GetDefinition(Selection.Selected.DefinitionId);
            if (definition is not WorldResourceNode resource || Marches.Active == null)
            {
                error = "Coleta indisponível.";
                return false;
            }

            if (!Gathering.TryStart(
                    Selection.Selected,
                    resource,
                    Marches.Active,
                    Marches.StateMachine,
                    out error))
            {
                return false;
            }

            collected = Gathering.ApplyGathering(
                Selection.Selected,
                resource,
                Marches.Active,
                Clock.UtcNow,
                _heroes.GetGatherRateMultiplier());
            Persist();
            Changed?.Invoke();
            return true;
        }

        public bool TryDepositMarchLoad(MarchOrder march, ResourceWallet? wallet, out long deposited)
        {
            deposited = 0;
            wallet ??= BoundWallet;
            if (wallet == null)
            {
                return false;
            }

            if (GetDefinition(march.TargetNodeId) is not WorldResourceNode resource)
            {
                return false;
            }

            // Retry: já entregue na memória, falta commit da carteira.
            if (march.RewardsDelivered && !march.IsCommitted)
            {
                CommitRewardDelivery(march);
                return true;
            }

            if (!Gathering.TryDepositLoad(march, resource, wallet, out deposited))
            {
                return false;
            }

            CommitRewardDelivery(march);
            Changed?.Invoke();
            return true;
        }

        private void CommitRewardDelivery(MarchOrder march)
        {
            WorldResourceGatheringService.MarkDeliveryCommitted(march);
            Persist();
            _persistBoundWallet?.Invoke();
            // Re-persist após carteira para garantir IsCommitted no disco.
            Persist();
        }

        public bool TryEngageSelectedCreature(out string error)
        {
            if (Selection.Selected == null)
            {
                error = "Nenhum nó selecionado.";
                return false;
            }

            EnergyRegen.ApplyUntil(Clock.UtcNow);
            var available = EnergyWallet.CurrentEnergy;
            if (!Encounters.TryEngage(Selection.Selected.DefinitionId, ref available, out error))
            {
                return false;
            }

            EnergyWallet.SyncFromExternal(available, EnergyWallet.LastUpdatedAt);
            if (Marches.Active != null)
            {
                if (!_dragons.TryEnterCombatForMarch(Marches.Active.Id, out var dragonCombatError))
                {
                    // Combate de criatura segue sem dragão se ele não puder apoiar.
                    LastDispatchDetail = $"Criatura engajada · dragão fora: {dragonCombatError}";
                }
            }

            Persist();
            Changed?.Invoke();
            return true;
        }

        public bool IsNodeVisible(string nodeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var instance))
            {
                return false;
            }

            return WorldNodeVisibilityResolver.IsVisible(GetDefinition(nodeId), instance, Filters.State);
        }

        public bool TryGetTerritory(string territoryId, out WorldTerritoryRuntime runtime) =>
            _territories.TryGetValue(territoryId, out runtime!);

        public bool TryGetTerritoryByRegion(string regionId, out WorldTerritoryRuntime runtime)
        {
            runtime = null!;
            if (!WorldTerritoryCatalog.TryGetByRegion(regionId, out var definition))
            {
                return false;
            }

            return _territories.TryGetValue(definition.Id, out runtime!);
        }

        public bool TryResolveSelectedCreature(ResourceWallet? wallet, out string error, out CreatureDifficultyBand band)
        {
            band = CreatureDifficultyBand.Impossible;
            if (Selection.Selected == null)
            {
                error = "Nenhum nó selecionado.";
                return false;
            }

            if (wallet == null)
            {
                error = "Carteira da cidade indisponível.";
                return false;
            }

            var attackerPower = GetAttackerPower();
            var victory = Encounters.TryResolveProvisional(
                Selection.Selected.DefinitionId,
                attackerPower,
                wallet,
                Clock.UtcNow,
                out error,
                out band);

            if (Marches.Active != null)
            {
                var difficultyBand = band switch
                {
                    CreatureDifficultyBand.Trivial => 0,
                    CreatureDifficultyBand.Easy => 1,
                    CreatureDifficultyBand.Fair => 2,
                    CreatureDifficultyBand.Hard => 3,
                    _ => 4
                };
                if (_dragons.TryApplyCombatOutcomeForMarch(
                        Marches.Active.Id,
                        victory,
                        difficultyBand,
                        out _,
                        out var dragonSummary) &&
                    !string.IsNullOrEmpty(dragonSummary))
                {
                    LastDispatchDetail = dragonSummary;
                }
            }

            Persist();
            Changed?.Invoke();
            return victory;
        }

        public bool TryEngageSelectedCityRaid(out string error)
        {
            if (Selection.Selected == null)
            {
                error = "Nenhum nó selecionado.";
                return false;
            }

            EnergyRegen.ApplyUntil(Clock.UtcNow);
            var available = EnergyWallet.CurrentEnergy;
            if (!CityRaids.TryEngage(Selection.Selected.DefinitionId, ref available, out error))
            {
                return false;
            }

            EnergyWallet.SyncFromExternal(available, EnergyWallet.LastUpdatedAt);
            if (Marches.Active != null)
            {
                if (!_dragons.TryEnterCombatForMarch(Marches.Active.Id, out var dragonCombatError))
                {
                    LastDispatchDetail = $"Raid engajado · dragão fora: {dragonCombatError}";
                }
            }

            Persist();
            Changed?.Invoke();
            return true;
        }

        public bool TryResolveSelectedCityRaid(
            ResourceWallet? wallet,
            out string error,
            out CreatureDifficultyBand band)
        {
            band = CreatureDifficultyBand.Impossible;
            if (Selection.Selected == null)
            {
                error = "Nenhum nó selecionado.";
                return false;
            }

            if (wallet == null)
            {
                error = "Carteira da cidade indisponível.";
                return false;
            }

            var cityId = Selection.Selected.DefinitionId;
            if (!CityRaidCatalog.TryGet(cityId, out var definition))
            {
                error = "Cidade não é alvo de raid.";
                return false;
            }

            if (_raids.TryGetValue(cityId, out var raidInst) &&
                !string.IsNullOrEmpty(raidInst.ActiveBattleId) &&
                _processedOperations.BattleIds.Contains(raidInst.ActiveBattleId!))
            {
                error = "BattleId já processado.";
                band = CreatureDifficultyBand.Fair;
                return false;
            }

            var attackerPower = GetAttackerPower();
            var dragonSnap = BuildDragonBattleSnapshot(forDefense: false);
            var victory = CityRaids.TryResolveDetailed(
                cityId,
                attackerPower,
                troopCount: 1000,
                dragonSnap,
                wallet,
                Clock.UtcNow,
                out error,
                out band,
                out _,
                out var raidReport,
                out var battleResult);

            var dragonSummary = string.Empty;
            if (Marches.Active != null)
            {
                var difficultyBand = band switch
                {
                    CreatureDifficultyBand.Trivial => 0,
                    CreatureDifficultyBand.Easy => 1,
                    CreatureDifficultyBand.Fair => 2,
                    CreatureDifficultyBand.Hard => 3,
                    _ => 4
                };
                if (_dragons.TryApplyCombatOutcomeForMarch(
                        Marches.Active.Id,
                        victory,
                        difficultyBand,
                        out _,
                        out dragonSummary) &&
                    !string.IsNullOrEmpty(dragonSummary))
                {
                    LastDispatchDetail = dragonSummary;
                }
            }

            var wounded = battleResult?.AttackerCasualties.TotalCasualties > 0
                ? (int)Math.Round(
                    100.0 * battleResult.AttackerCasualties.Wounded /
                    Math.Max(1, battleResult.AttackerCasualties.TotalCasualties))
                : (victory ? 22 : 45);

            var battleId = battleResult?.BattleId ?? $"raid:{cityId}:{Clock.UtcNow.Ticks}";
            var reportId = battleId + ":report";
            LastBattleReport = new CityBattleReport
            {
                CreatedUtc = Clock.UtcNow,
                TargetCityId = cityId,
                TargetDisplayName = definition.DisplayName,
                Victory = victory,
                IsDefense = false,
                AttackerPower = battleResult?.AttackerPowerFinal ?? attackerPower,
                DefensePower = battleResult?.DefenderPowerFinal ?? definition.DefensePower,
                Band = band,
                DragonRoleLabel = DescribeSelectedDragonBattleRole(),
                TroopWoundedPercent = wounded,
                LootGranted = victory,
                DragonSummary = dragonSummary,
                Summary = string.IsNullOrEmpty(raidReport)
                    ? error
                    : $"{raidReport} Feridos ~{wounded}%.",
                BattleId = battleId,
                ReportId = reportId,
                WallDurabilityAfter = battleResult?.WallDurabilityAfter ?? 0,
                DominatorWrathUsed = battleResult?.DominatorWrathUsed ?? false
            };

            if (!_processedOperations.BattleIds.Contains(battleId))
            {
                _processedOperations.BattleIds.Add(battleId);
                _processedOperations.ReportIds.Add(reportId);
                if (battleResult?.Loot != null &&
                    !string.IsNullOrEmpty(battleResult.Loot.LootTransactionId))
                {
                    _processedOperations.LootTransactionIds.Add(battleResult.Loot.LootTransactionId);
                }
            }

            LastDispatchDetail = LastBattleReport.Summary;
            _requestDominatorWrath = false;
            if (victory)
            {
                _persistBoundWallet?.Invoke();
            }

            Persist();
            Changed?.Invoke();
            return victory;
        }

        public DefenseEvent ScheduleNpcInvasion(
            string defenseEventId,
            int attackerPower,
            DateTime? scheduledAtUtc = null,
            int troopCount = 800)
        {
            var battleId = defenseEventId + ":battle";
            var evt = _defenseScheduler.Schedule(
                defenseEventId,
                battleId,
                scheduledAtUtc ?? Clock.UtcNow,
                new NpcAttackerSnapshot
                {
                    AttackerId = "npc-raider",
                    DisplayName = "Invasor NPC",
                    Power = Math.Max(1, attackerPower),
                    TroopCount = Math.Max(1, troopCount)
                });
            Persist();
            return evt;
        }

        public int ProcessPendingDefenses(out string log)
        {
            return _defenseProcessor.ProcessDue(
                Clock.UtcNow,
                evt => LocalDefenseResolver.Resolve(
                    evt,
                    _boundWallLevel,
                    wallMax: Math.Max(1000, _boundWallLevel * 500L),
                    wallCurrent: Math.Max(1000, _boundWallLevel * 500L),
                    wallBonus: 0.10m,
                    wallReduction: 0.05m,
                    hospitalFree: HospitalCapacityResolver.FreeCapacity(_boundHospitalLevel, 0),
                    cityResources: ResourceBundle.From(
                        (ResourceType.Food, 5000),
                        (ResourceType.Gold, 5000),
                        (ResourceType.Wood, 4000),
                        (ResourceType.Stone, 3000),
                        (ResourceType.Iron, 2000)),
                    warehouseProtection: ResourceBundle.From(
                        (ResourceType.Food, WarehouseRules.GetProtection(_boundWarehouseLevel)),
                        (ResourceType.Gold, WarehouseRules.GetProtection(_boundWarehouseLevel)),
                        (ResourceType.Wood, WarehouseRules.GetProtection(_boundWarehouseLevel)),
                        (ResourceType.Stone, WarehouseRules.GetProtection(_boundWarehouseLevel)),
                        (ResourceType.Iron, WarehouseRules.GetProtection(_boundWarehouseLevel))),
                    minimumReserve: ResourceBundle.From((ResourceType.Food, 200), (ResourceType.Gold, 200)),
                    guardDragon: BuildDragonBattleSnapshot(forDefense: true)),
                (evt, result) =>
                {
                    LastBattleReport = new CityBattleReport
                    {
                        CreatedUtc = Clock.UtcNow,
                        TargetCityId = Settings.PlayerHomeNodeId,
                        TargetDisplayName = "Sua cidade",
                        Victory = result.Outcome == BattleOutcome.Victory,
                        IsDefense = true,
                        AttackerPower = result.AttackerPowerFinal,
                        DefensePower = result.DefenderPowerFinal,
                        DragonRoleLabel = "Guard",
                        TroopWoundedPercent = result.DefenderCasualties.TotalCasualties <= 0
                            ? 0
                            : (int)Math.Round(
                                100.0 * result.DefenderCasualties.Wounded /
                                Math.Max(1, result.DefenderCasualties.TotalCasualties)),
                        LootGranted = false,
                        Summary = result.Summary,
                        BattleId = evt.BattleId,
                        ReportId = evt.ReportId,
                        DefenseEventId = evt.DefenseEventId,
                        WallDurabilityAfter = result.WallDurabilityAfter
                    };
                    Persist();
                },
                out log);
        }

        private DragonBattleSnapshot BuildDragonBattleSnapshot(bool forDefense)
        {
            var statuses = _dragons.GetDragonStatuses();
            if (statuses.Count == 0)
            {
                return new DragonBattleSnapshot();
            }

            var st = statuses[0];
            var roleLabel = _dragons.GetBattleRoleLabel(st.DragonId);
            Enum.TryParse<DragonBattleRole>(roleLabel, true, out var role);
            if (forDefense)
            {
                role = string.Equals(roleLabel, "Guard", StringComparison.OrdinalIgnoreCase)
                    ? DragonBattleRole.Guard
                    : DragonBattleRole.Support;
            }
            else if (role == DragonBattleRole.Guard)
            {
                role = DragonBattleRole.Support;
            }

            var wrathOk = !forDefense &&
                          role == DragonBattleRole.Assault &&
                          st.DragonLevel >= RaidCombatCatalog.DominatorWrathUnlockLevel &&
                          _requestDominatorWrath;

            return new DragonBattleSnapshot
            {
                DragonId = st.DragonId,
                DragonLevel = st.DragonLevel,
                Role = role,
                SupportPower = forDefense
                    ? Math.Max(0, _dragons.GetHomeGuardPower())
                    : Math.Max(0, GetAttackerPower() / 12),
                Energy = 100,
                DominatorWrathUnlocked = st.DragonLevel >= RaidCombatCatalog.DominatorWrathUnlockLevel,
                DominatorWrathRequested = wrathOk,
                DominatorWrathAlreadyUsedInBattle = false
            };
        }

        public bool TrySimulateHomeDefense(int attackerPower, out string error, out string report)
        {
            var defense = CityHomeDefenseService.ComputeDefensePower(
                _boundWallLevel,
                _heroes.GetProvisionalMarchPower(),
                _dragons.GetHomeGuardPower());
            CityHomeDefenseService.TryResolve(
                attackerPower,
                defense,
                out var band,
                out var defended,
                out var wounded,
                out report);

            LastBattleReport = new CityBattleReport
            {
                CreatedUtc = Clock.UtcNow,
                TargetCityId = Settings.PlayerHomeNodeId,
                TargetDisplayName = "Cidade do Jogador",
                Victory = defended,
                IsDefense = true,
                AttackerPower = attackerPower,
                DefensePower = defense,
                Band = band,
                DragonRoleLabel = "Guard",
                TroopWoundedPercent = wounded,
                LootGranted = false,
                DragonSummary = _dragons.GetHomeGuardPower() > 0
                    ? $"Guarda dracônica +{_dragons.GetHomeGuardPower()}"
                    : "Sem dragão em Guard",
                Summary = report
            };
            LastDispatchDetail = report;
            error = string.Empty;
            Persist();
            Changed?.Invoke();
            return defended;
        }

        public string DescribeSelectedDragonBattleRole()
        {
            var statuses = _dragons.GetDragonStatuses();
            return statuses.Count == 0 ? "Support" : _dragons.GetBattleRoleLabel(statuses[0].DragonId);
        }

        public int GetAttackerPower()
        {
            var heroes = _heroes.GetProvisionalMarchPower();
            var dragons = Marches.Active != null
                ? Math.Max(_dragons.GetSupportPowerForMarch(Marches.Active.Id), _dragons.GetProvisionalDragonPower())
                : _dragons.GetProvisionalDragonPower();
            return heroes + dragons;
        }

        public int GetHeroMarchPower() => _heroes.GetProvisionalMarchPower();

        public string DescribeHeroFormation() => _heroes.DescribeFormation();

        public bool TryGetMarchDragonPresence(
            string marchId,
            out string dragonId,
            out string stageLabel,
            out bool isMounted,
            out string bondedHeroId) =>
            _dragons.TryGetMarchDragonPresence(marchId, out dragonId, out stageLabel, out isMounted, out bondedHeroId);

        public string? LastDispatchDetail { get; private set; }

        public bool LastDispatchWasQueued { get; private set; }

        public float GetGatherRateMultiplier() => _heroes.GetGatherRateMultiplier();
        public string? LastDepositMessage { get; private set; }
        public long LastDepositAmount { get; private set; }

        public string? ConsumeDepositMessage()
        {
            var message = LastDepositMessage;
            LastDepositMessage = null;
            return message;
        }

        public void Persist()
        {
            Marches.PersistMarch();
            PersistEnergy();
            Filters.Persist();
            var snapshot = new WorldMapSnapshot
            {
                SavedAtUtc = Clock.UtcNow,
                LastAdvanceUtc = Marches.LastAdvanceUtc,
                Energy = EnergyWallet.CurrentEnergy,
                SelectedNodeId = Selection.SelectedNodeId,
                March = Marches.Active?.Clone(),
                LastCompletedMarch = Marches.LastCompleted?.Clone(),
                LastBattleReport = LastBattleReport,
                ProcessedOperations = _processedOperations.Clone(),
                BoundWallLevel = _boundWallLevel,
                BoundHospitalLevel = _boundHospitalLevel,
                BoundWarehouseLevel = _boundWarehouseLevel
            };
            foreach (var evt in _defenseScheduler.CloneEvents())
            {
                snapshot.DefenseEvents.Add(evt);
            }

            foreach (var pair in _nodes)
            {
                snapshot.Nodes[pair.Key] = new WorldNodeInstance(
                    pair.Value.DefinitionId,
                    pair.Value.Status,
                    pair.Value.RemainingAmount)
                {
                    OccupiedByMarchId = pair.Value.OccupiedByMarchId,
                    RespawnAt = pair.Value.RespawnAt,
                    LastGatherUpdatedUtc = pair.Value.LastGatherUpdatedUtc,
                    ResourceState = pair.Value.ResourceState
                };
            }

            foreach (var pair in _creatures)
            {
                snapshot.Creatures[pair.Key] = new WorldCreatureInstance(
                    pair.Value.DefinitionId,
                    pair.Value.State,
                    pair.Value.RegionId,
                    pair.Value.X,
                    pair.Value.Z,
                    pair.Value.RespawnAtUtc)
                {
                    EngagedMarchId = pair.Value.EngagedMarchId
                };
            }

            foreach (var pair in _raids)
            {
                snapshot.Raids[pair.Key] = pair.Value.Clone();
            }

            Repository.Save(snapshot);
        }

        private void LoadEnergy(WorldMapSnapshot? mapSnapshot)
        {
            var snapshot = _energyRepository.Load();
            if (snapshot != null)
            {
                EnergyWallet.Configure(snapshot.MaxEnergy, snapshot.RegenIntervalSec, snapshot.RegenAmount);
                EnergyWallet.SyncFromExternal(snapshot.CurrentEnergy, snapshot.LastUpdatedAt);
                return;
            }

            // Fallback legado: snapshot do mapa só tinha currentEnergy.
            if (mapSnapshot != null)
            {
                EnergyWallet.SyncFromExternal(mapSnapshot.Energy, Clock.UtcNow);
            }
        }

        private void PersistEnergy()
        {
            _energyRepository.Save(new EnergySnapshot
            {
                CurrentEnergy = EnergyWallet.CurrentEnergy,
                MaxEnergy = EnergyWallet.MaxEnergy,
                LastUpdatedAt = EnergyWallet.LastUpdatedAt,
                RegenIntervalSec = EnergyWallet.RegenIntervalSec,
                RegenAmount = EnergyWallet.RegenAmount
            });
        }

        private void MergeProcessed(ProcessedOperationRegistry? source)
        {
            if (source == null)
            {
                return;
            }

            foreach (var id in source.RaidIds)
            {
                _processedOperations.RaidIds.Add(id);
            }

            foreach (var id in source.MarchIds)
            {
                _processedOperations.MarchIds.Add(id);
            }

            foreach (var id in source.BattleIds)
            {
                _processedOperations.BattleIds.Add(id);
            }

            foreach (var id in source.LootTransactionIds)
            {
                _processedOperations.LootTransactionIds.Add(id);
            }

            foreach (var id in source.ReportIds)
            {
                _processedOperations.ReportIds.Add(id);
            }

            foreach (var id in source.WallRepairIds)
            {
                _processedOperations.WallRepairIds.Add(id);
            }

            foreach (var id in source.DefenseEventIds)
            {
                _processedOperations.DefenseEventIds.Add(id);
            }
        }

        private static PlayerEnergyWallet CreateEnergyWallet(WorldMapSettings settings, IWorldMapClock clock)
        {
            var energySettings = new EnergySettings
            {
                CurrentEnergy = settings.StartingEnergy,
                MaxEnergy = settings.MaxEnergy,
                LastUpdatedAt = clock.UtcNow,
                RegenIntervalSec = settings.EnergyRegenIntervalSec,
                RegenAmount = settings.EnergyRegenAmount,
                MarchDispatchCost = settings.MarchDispatchEnergyCost,
                PersistenceKey = settings.EnergyPersistenceKey
            };
            return new PlayerEnergyWallet(energySettings);
        }

        private void ApplyResourceGathering(DateTime nowUtc)
        {
            var march = Marches.Active;
            if (march == null || march.State != MarchState.Gathering)
            {
                return;
            }

            if (GetDefinition(march.TargetNodeId) is not WorldResourceNode resource)
            {
                return;
            }

            Gathering.ApplyGathering(
                GetNode(march.TargetNodeId),
                resource,
                march,
                nowUtc,
                _heroes.GetGatherRateMultiplier());
        }

        private void AdvanceResourceRespawns(DateTime nowUtc)
        {
            foreach (var pair in _nodes)
            {
                if (GetDefinition(pair.Key) is WorldResourceNode resource)
                {
                    Gathering.AdvanceRespawn(pair.Value, resource, nowUtc);
                }
            }
        }

        private void DepositCompletedMarch(MarchOrder? completed)
        {
            if (completed == null || completed.State != MarchState.Completed)
            {
                return;
            }

            _dragons.TryRecallFromMarch(completed.Id, out _);

            if (completed.RewardsDelivered && completed.IsCommitted)
            {
                return;
            }

            if (BoundWallet == null)
            {
                LastDepositMessage = "Loot pendente — abra a Cidade para sincronizar a carteira.";
                LastDepositAmount = 0;
                return;
            }

            if (TryDepositMarchLoad(completed, BoundWallet, out var deposited) && deposited > 0)
            {
                LastDepositAmount = deposited;
                var resourceName = GetDefinition(completed.TargetNodeId) is WorldResourceNode resource
                    ? resource.ResourceType.ToString()
                    : "recursos";
                LastDepositMessage = $"+{deposited} {resourceName} depositados na cidade!";
                RewardDeposited?.Invoke();
#if UNITY_5_3_OR_NEWER
                BetaMissions.Notify(MissionEvent.ReceiveReward);
#endif
            }
        }

        private void AdvanceCreatures(DateTime utcNow)
        {
            foreach (var pair in _creatures)
            {
                if (WorldCreatureCatalog.TryGet(pair.Key, out var definition))
                {
                    Encounters.AdvanceInstance(pair.Value, definition, utcNow);
                }
            }
        }

        private void AdvanceRaids(DateTime utcNow)
        {
            foreach (var pair in _raids)
            {
                if (CityRaidCatalog.TryGet(pair.Key, out var definition))
                {
                    CityRaids.AdvanceInstance(pair.Value, definition, utcNow);
                }
            }

            ProcessPendingDefenses(out _);
        }

        private void SeedNodes()
        {
            foreach (var definition in WorldNodeCatalog.All.Values)
            {
                var amount = definition is WorldResourceNode resource ? resource.Amount : 0;
                _nodes[definition.Id] = new WorldNodeInstance(definition.Id, definition.DefaultStatus, amount);
            }
        }

        private void SeedCreatures()
        {
            foreach (var definition in WorldCreatureCatalog.All.Values)
            {
                _creatures[definition.Id] = new WorldCreatureInstance(
                    definition.Id,
                    WorldCreatureState.Available,
                    definition.RegionId,
                    definition.X,
                    definition.Z);
            }
        }

        private void SeedRaids()
        {
            foreach (var definition in CityRaidCatalog.All.Values)
            {
                if (!_raids.ContainsKey(definition.CityNodeId))
                {
                    _raids[definition.CityNodeId] = new CityRaidInstance(definition.CityNodeId)
                    {
                        WallCurrentDurability = definition.Fortification.WallCurrentDurability
                    };
                }
            }
        }

        private void SeedTerritories()
        {
            foreach (var definition in WorldTerritoryCatalog.All.Values)
            {
                _territories[definition.Id] = new WorldTerritoryRuntime(definition.Id, definition.DefaultState);
            }
        }
    }
}
