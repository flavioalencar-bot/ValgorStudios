using System;
using Valgor.City.Data;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Core
{
    public sealed class WorldResourceHarvestService
    {
        public bool CanCollect(WorldNodeInstance node, WorldMapNodeDefinition definition, MarchOrder? march)
        {
            if (definition is not WorldResourceNode)
            {
                return false;
            }

            if (node.Status is WorldNodeStatus.Locked or WorldNodeStatus.Depleted)
            {
                return false;
            }

            if (node.RemainingAmount <= 0)
            {
                return false;
            }

            return march != null &&
                   march.Phase == MarchPhase.Arrived &&
                   string.Equals(march.TargetNodeId, node.DefinitionId, StringComparison.Ordinal);
        }

        public bool TryCollect(WorldNodeInstance node, WorldMapNodeDefinition definition, MarchOrder? march, ResourceWallet wallet, out long collected)
        {
            collected = 0;
            if (!CanCollect(node, definition, march) || definition is not WorldResourceNode resource)
            {
                return false;
            }

            collected = node.RemainingAmount;
            wallet.Add(resource.Resource, collected);
            node.RemainingAmount = 0;
            node.Status = WorldNodeStatus.Depleted;
            return true;
        }
    }
}
