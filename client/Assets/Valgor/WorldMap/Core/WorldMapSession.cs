using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.Core.Modules;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;

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
            Marches = new MarchService(clock, settings, heroes, id => WorldNodeCatalog.Get(id));
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
        }

        public WorldMapSettings Settings { get; }
        public IWorldMapClock Clock { get; }
        public IWorldMapRepository Repository { get; }
        public WorldNodeSelectionService Selection { get; }
        public RegionSelectionService RegionSelection { get; }
        public MarchService Marches { get; }
        public WorldResourceHarvestService Harvest { get; }
        public CreatureEncounterService Encounters { get; }
        public int Energy { get; private set; }
        public IReadOnlyDictionary<string, WorldNodeInstance> Nodes => _nodes;
        public IReadOnlyDictionary<string, WorldCreatureInstance> Creatures => _creatures;
        public event Action? Changed;

        public static WorldMapSession Create(IHeroesGateway heroes, IWorldMapClock? clock = null)
        {
            clock ??= new SystemWorldMapClock();
            var settings = WorldMapSettings.Default;
            return new WorldMapSession(settings, clock, heroes, new LocalWorldMapRepository(settings.PersistenceKey));
        }

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
                        pair.Value.RemainingAmount);
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
            }
            else
            {
                Marches.Advance(Clock.UtcNow);
            }

            AdvanceCreatures(Clock.UtcNow);
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

            if (!Marches.TryDispatch(Selection.Selected.DefinitionId, out error))
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

        public bool TryCollectSelected(ResourceWallet? wallet, out string error, out long collected)
        {
            collected = 0;
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

            var definition = GetDefinition(Selection.Selected.DefinitionId);
            if (!Harvest.TryCollect(Selection.Selected, definition, Marches.Active, wallet, out collected))
            {
                error = "Coleta indisponível.";
                return false;
            }

            Persist();
            Changed?.Invoke();
            error = string.Empty;
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
            var snapshot = new WorldMapSnapshot
            {
                SavedAtUtc = Clock.UtcNow,
                LastAdvanceUtc = Clock.UtcNow,
                Energy = Energy,
                March = Marches.Active
            };

            foreach (var pair in _nodes)
            {
                snapshot.Nodes[pair.Key] = new WorldNodeInstance(
                    pair.Value.DefinitionId,
                    pair.Value.Status,
                    pair.Value.RemainingAmount);
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
