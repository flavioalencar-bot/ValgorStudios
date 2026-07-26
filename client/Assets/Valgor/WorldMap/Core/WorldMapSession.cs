using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.Core.Modules;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Energy;
using Valgor.WorldMap.Marches;

namespace Valgor.WorldMap.Core
{
    /// <summary>
    /// Estado do mapa mundial que sobrevive City↔WorldMap via ServiceRegistry.
    /// </summary>
    public sealed class WorldMapSession
    {
        private readonly Dictionary<string, WorldNodeInstance> _nodes = new();
        private readonly Dictionary<string, WorldCreatureInstance> _creatures = new();
        private readonly IHeroesGateway _heroes;
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
            IEnergyPersistenceRepository? energyRepository = null)
        {
            Settings = settings;
            Clock = clock;
            _heroes = heroes;
            Repository = repository;
            _energyRepository = energyRepository ?? new EnergyPersistenceRepository(settings.EnergyPersistenceKey);
            EnergyWallet = energyWallet ?? CreateEnergyWallet(settings, clock);
            EnergyRegen = new EnergyRegenerationService(EnergyWallet, clock);
            EnergyCosts = new EnergyCostResolver(EnergyWallet.Settings);
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
            _tickIntervalSeconds = settings.MarchTickIntervalSeconds;
            _lastTickUtc = clock.UtcNow;
            SeedNodes();
            SeedCreatures();
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
        public int Energy => EnergyWallet.CurrentEnergy;
        public ResourceWallet? BoundWallet { get; private set; }
        public IReadOnlyDictionary<string, WorldNodeInstance> Nodes => _nodes;
        public IReadOnlyDictionary<string, WorldCreatureInstance> Creatures => _creatures;
        public event Action? Changed;

        public static WorldMapSession Create(IHeroesGateway heroes, IWorldMapClock? clock = null)
        {
            clock ??= new SystemWorldMapClock();
            var settings = WorldMapSettings.Default;
            return new WorldMapSession(settings, clock, heroes, new LocalWorldMapRepository(settings.PersistenceKey));
        }

        public void BindWallet(ResourceWallet? wallet) => BoundWallet = wallet;

        public WorldMapNodeDefinition GetDefinition(string id) => WorldNodeCatalog.Get(id);

        public WorldNodeInstance GetNode(string id) => _nodes[id];

        public bool TryGetCreature(string id, out WorldCreatureInstance instance) =>
            _creatures.TryGetValue(id, out instance!);

        public void LoadOrInitialize()
        {
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

                Marches.Restore(snapshot.March, snapshot.LastAdvanceUtc);
                DepositCompletedMarch(Marches.LastCompleted);
            }
            else
            {
                Marches.Advance(Clock.UtcNow);
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
            Persist();
            Changed?.Invoke();
        }

        public bool TryDispatchToSelected(out string error)
        {
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

            EnergyRegen.ApplyUntil(Clock.UtcNow);
            var dispatchCost = EnergyCosts.ResolveMarchDispatch();
            if (dispatchCost > 0 && !EnergyWallet.TrySpend(dispatchCost, out error))
            {
                return false;
            }

            if (!Marches.TryDispatch(Selection.Selected.DefinitionId, Settings.DefaultPlayerId, out error))
            {
                if (dispatchCost > 0)
                {
                    EnergyWallet.Add(dispatchCost);
                }

                return false;
            }

            Changed?.Invoke();
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
            if (!Marches.TryCancel(out error))
            {
                return false;
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

            collected = Gathering.ApplyGathering(Selection.Selected, resource, Marches.Active, Clock.UtcNow);
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

            if (!Gathering.TryDepositLoad(march, resource, wallet, out deposited))
            {
                return false;
            }

            Persist();
            Changed?.Invoke();
            return true;
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
            Persist();
            Changed?.Invoke();
            return true;
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

            if (!Encounters.TryResolveProvisional(
                    Selection.Selected.DefinitionId,
                    _heroes.GetProvisionalMarchPower(),
                    wallet,
                    Clock.UtcNow,
                    out error,
                    out band))
            {
                Persist();
                Changed?.Invoke();
                return false;
            }

            Persist();
            Changed?.Invoke();
            return true;
        }

        public void Persist()
        {
            Marches.PersistMarch();
            PersistEnergy();
            var snapshot = new WorldMapSnapshot
            {
                SavedAtUtc = Clock.UtcNow,
                LastAdvanceUtc = Marches.LastAdvanceUtc,
                Energy = EnergyWallet.CurrentEnergy,
                March = Marches.Active?.Clone()
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

            Gathering.ApplyGathering(GetNode(march.TargetNodeId), resource, march, nowUtc);
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

            if (BoundWallet == null)
            {
                return;
            }

            TryDepositMarchLoad(completed, BoundWallet, out _);
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
    }
}
