using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.Core.Modules;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
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
        private DateTime _lastTickUtc;
        private readonly double _tickIntervalSeconds;

        public WorldMapSession(
            WorldMapSettings settings,
            IWorldMapClock clock,
            IHeroesGateway heroes,
            IWorldMapRepository repository)
        {
            Settings = settings;
            Clock = clock;
            _heroes = heroes;
            Repository = repository;
            Energy = settings.StartingEnergy;
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
        public int Energy { get; private set; }
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
            if (snapshot != null)
            {
                Energy = Math.Clamp(snapshot.Energy, 0, Settings.MaxEnergy);
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

            if (!Marches.TryDispatch(Selection.Selected.DefinitionId, Settings.DefaultPlayerId, out error))
            {
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

            var energy = Energy;
            if (!Encounters.TryEngage(Selection.Selected.DefinitionId, ref energy, out error))
            {
                return false;
            }

            Energy = energy;
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
            var snapshot = new WorldMapSnapshot
            {
                SavedAtUtc = Clock.UtcNow,
                LastAdvanceUtc = Marches.LastAdvanceUtc,
                Energy = Energy,
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

        private void DepositCompletedMarch(MarchOrder? marchBeforeAdvance)
        {
            if (marchBeforeAdvance == null || marchBeforeAdvance.State != MarchState.Completed)
            {
                return;
            }

            if (BoundWallet == null)
            {
                return;
            }

            TryDepositMarchLoad(marchBeforeAdvance, BoundWallet, out _);
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
