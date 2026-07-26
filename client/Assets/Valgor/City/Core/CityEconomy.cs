using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.City.Production;

namespace Valgor.City.Core
{
    /// <summary>
    /// Estado econômico da cidade que sobrevive City↔WorldMap via ServiceRegistry.
    /// </summary>
    public sealed class CityEconomy
    {
        public CityEconomy(
            ResourceWallet wallet,
            ResourceProductionService production,
            ResourceCollectionService collection,
            ProductionTickService tick,
            IProductionRepository repository,
            IGameClock clock)
        {
            Wallet = wallet;
            Production = production;
            Collection = collection;
            Tick = tick;
            Repository = repository;
            Clock = clock;
        }

        public ResourceWallet Wallet { get; }
        public ResourceProductionService Production { get; }
        public ResourceCollectionService Collection { get; }
        public ProductionTickService Tick { get; }
        public IProductionRepository Repository { get; }
        public IGameClock Clock { get; }

        public static CityEconomy Create(IGameClock? clock = null)
        {
            clock ??= new SystemGameClock();
            var settings = ProductionCatalog.Settings;
            var wallet = new ResourceWallet();
            var production = new ResourceProductionService(clock, settings);
            var collection = new ResourceCollectionService(production, wallet);
            var tick = new ProductionTickService(production, clock, settings);
            var repository = new LocalProductionRepository(settings.PersistenceKey);
            return new CityEconomy(wallet, production, collection, tick, repository, clock);
        }

        public void ApplyOfflineAndPersist(IEnumerable<BuildingInstance> buildings)
        {
            foreach (var building in buildings)
            {
                Production.RegisterBuilding(building);
            }

            var snapshot = Repository.Load();
            if (snapshot != null)
            {
                foreach (var pair in snapshot.Wallet)
                {
                    Wallet.SetAmount(pair.Key, pair.Value);
                }

                foreach (var pair in snapshot.Buildings)
                {
                    Production.RestoreState(pair.Value);
                }
            }
            else
            {
                SeedStarterWallet();
            }

            Tick.ForceApply();
            Persist(buildings);
        }

        public void Persist(IEnumerable<BuildingInstance> buildings)
        {
            var snapshot = new ProductionSnapshot { SavedAtUtc = Clock.UtcNow };
            foreach (var building in buildings)
            {
                if (Production.TryGetState(building.DefinitionId, out var state))
                {
                    snapshot.Buildings[building.DefinitionId] = state;
                }
            }

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                snapshot.Wallet[resource] = Wallet.Get(resource);
            }

            Repository.Save(snapshot);
        }

        private void SeedStarterWallet()
        {
            if (Wallet.Get(ResourceType.Gold) > 0)
            {
                return;
            }

            Wallet.Add(ResourceType.Gold, 5000);
            Wallet.Add(ResourceType.Food, 3000);
            Wallet.Add(ResourceType.Wood, 3000);
            Wallet.Add(ResourceType.Stone, 2000);
            Wallet.Add(ResourceType.Iron, 1000);
            Wallet.Add(ResourceType.DragonEssence, 100);
            Wallet.Add(ResourceType.Diamonds, 50);
        }
    }
}
