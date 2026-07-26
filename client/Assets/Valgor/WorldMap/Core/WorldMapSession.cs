using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.Core.Modules;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Core
{
    /// <summary>
    /// Estado do mapa mundial que sobrevive City↔WorldMap via ServiceRegistry.
    /// </summary>
    public sealed class WorldMapSession
    {
        private readonly Dictionary<string, WorldNodeInstance> _nodes = new();
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
            Repository = repository;
            Selection = new WorldNodeSelectionService();
            RegionSelection = new RegionSelectionService();
            Harvest = new WorldResourceHarvestService();
            Marches = new MarchService(clock, settings, heroes, id => WorldNodeCatalog.Get(id));
            _tickIntervalSeconds = settings.MarchTickIntervalSeconds;
            _lastTickUtc = clock.UtcNow;
            SeedNodes();
            Marches.Changed += (_, _) => Persist();
        }

        public WorldMapSettings Settings { get; }
        public IWorldMapClock Clock { get; }
        public IWorldMapRepository Repository { get; }
        public WorldNodeSelectionService Selection { get; }
        public RegionSelectionService RegionSelection { get; }
        public MarchService Marches { get; }
        public WorldResourceHarvestService Harvest { get; }
        public IReadOnlyDictionary<string, WorldNodeInstance> Nodes => _nodes;
        public event Action? Changed;

        public static WorldMapSession Create(IHeroesGateway heroes, IWorldMapClock? clock = null)
        {
            clock ??= new SystemWorldMapClock();
            var settings = WorldMapSettings.Default;
            return new WorldMapSession(settings, clock, heroes, new LocalWorldMapRepository(settings.PersistenceKey));
        }

        public WorldMapNodeDefinition GetDefinition(string id) => WorldNodeCatalog.Get(id);

        public WorldNodeInstance GetNode(string id) => _nodes[id];

        public void LoadOrInitialize()
        {
            var snapshot = Repository.Load();
            if (snapshot != null)
            {
                foreach (var pair in snapshot.Nodes)
                {
                    _nodes[pair.Key] = new WorldNodeInstance(
                        pair.Value.DefinitionId,
                        pair.Value.Status,
                        pair.Value.RemainingAmount);
                }

                Marches.Restore(snapshot.March, snapshot.LastAdvanceUtc);
            }
            else
            {
                Marches.Advance(Clock.UtcNow);
            }

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

        public void Persist()
        {
            var snapshot = new WorldMapSnapshot
            {
                SavedAtUtc = Clock.UtcNow,
                LastAdvanceUtc = Clock.UtcNow,
                March = Marches.Active
            };

            foreach (var pair in _nodes)
            {
                snapshot.Nodes[pair.Key] = new WorldNodeInstance(
                    pair.Value.DefinitionId,
                    pair.Value.Status,
                    pair.Value.RemainingAmount);
            }

            Repository.Save(snapshot);
        }

        private void SeedNodes()
        {
            foreach (var definition in WorldNodeCatalog.All.Values)
            {
                var amount = definition is WorldResourceNode resource ? resource.Amount : 0;
                _nodes[definition.Id] = new WorldNodeInstance(definition.Id, definition.DefaultStatus, amount);
            }
        }
    }
}
