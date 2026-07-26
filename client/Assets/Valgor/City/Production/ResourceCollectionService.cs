using System;
using Valgor.City.Data;

namespace Valgor.City.Production
{
    public sealed class ResourceCollectionService
    {
        private readonly ResourceProductionService _production;
        private readonly ResourceWallet _wallet;

        public ResourceCollectionService(ResourceProductionService production, ResourceWallet wallet)
        {
            _production = production ?? throw new ArgumentNullException(nameof(production));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        /// <summary>
        /// Coleta o acumulado do edifício, zera apenas o buffer local e credita a carteira.
        /// </summary>
        public long Collect(BuildingInstance building)
        {
            if (!ProductionCatalog.TryGet(building.DefinitionId, out var definition))
            {
                return 0;
            }

            if (!_production.TryGetState(building.DefinitionId, out var state) || state.Accumulated <= 0)
            {
                return 0;
            }

            var amount = state.Accumulated;
            state.Accumulated = 0;
            _wallet.Add(definition.Resource, amount);
            _production.NotifyCollected(
                building.DefinitionId,
                definition.Resource,
                definition.GetCapacity(building.Level));
            return amount;
        }
    }
}
