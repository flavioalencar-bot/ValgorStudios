using System;
using Valgor.City.Data;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Marches;

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

            if (march == null ||
                march.State is not (MarchState.Arrived or MarchState.Gathering) ||
                !string.Equals(march.TargetNodeId, node.DefinitionId, StringComparison.Ordinal))
            {
                return false;
            }

            if (march.RewardsDelivered)
            {
                return false;
            }

            return true;
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
