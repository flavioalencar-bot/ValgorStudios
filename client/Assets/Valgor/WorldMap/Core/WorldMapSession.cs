using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.WorldMap.Camera;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Energy;
using Valgor.WorldMap.Filters;
using Valgor.WorldMap.Locate;
using Valgor.WorldMap.Marches;
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
        private readonly Dictionary<string, WorldTerritoryRuntime> _territories = new();
        private readonly IHeroesGateway _heroes;
        private IDragonGateway _dragons = new ProvisionalDragonGateway();
        private readonly WorldNodeOccupationService _occupation = new();
        private readonly IEnergyPersistenceRepository _energyRepository;
        private DateTime _lastTickUtc;
        private readonly double _tickIntervalSeconds;

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
            SeedTerritories();
            Marches.Changed += (_, _) => Persist();
            Encounters.Changed += () =>
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
        public IReadOnlyDictionary<string, WorldTerritoryRuntime> Territories => _territories;
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

        public void BindDragons(IDragonGateway dragons)
        {
            _dragons = dragons ?? throw new ArgumentNullException(nameof(dragons));
        }

        public WorldMapNodeDefinition GetDefinition(string id) => WorldNodeCatalog.Get(id);

        public WorldNodeInstance GetNode(string id) => _nodes[id];

        public bool TryGetCreature(string id, out WorldCreatureInstance instance) =>
            _creatures.TryGetValue(id, out instance!);

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

                Marches.Restore(snapshot.March, snapshot.LastAdvanceUtc, snapshot.LastCompletedMarch);
                DepositCompletedMarch(Marches.LastCompleted);
                Selection.RestoreFromId(snapshot.SelectedNodeId, _nodes);
            }
            else
            {
                Marches.Advance(Clock.UtcNow);
                Selection.Deselect();
            }

            AdvanceCreatures(Clock.UtcNow);
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
                    LastDispatchDetail =
                        $"{_heroes.DescribeFormation()} · Dragão destacado · Poder {GetAttackerPower()}";
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
                LastCompletedMarch = Marches.LastCompleted?.Clone()
            };

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

        private void SeedTerritories()
        {
            foreach (var definition in WorldTerritoryCatalog.All.Values)
            {
                _territories[definition.Id] = new WorldTerritoryRuntime(definition.Id, definition.DefaultState);
            }
        }
    }
}
