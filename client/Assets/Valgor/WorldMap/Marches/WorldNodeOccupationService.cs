using System;
using System.Collections.Generic;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Marches
{
    /// <summary>
    /// Ocupação exclusiva de nós (especialmente recursos) por marcha.
    /// </summary>
    public sealed class WorldNodeOccupationService
    {
        public bool IsOccupied(WorldNodeInstance node) =>
            !string.IsNullOrEmpty(node.OccupiedByMarchId);

        public bool TryOccupy(WorldNodeInstance node, MarchOrder march, out string error)
        {
            if (march.OccupyingNodeId != null &&
                !string.Equals(march.OccupyingNodeId, node.DefinitionId, StringComparison.Ordinal))
            {
                error = "Uma marcha não pode ocupar dois nós simultaneamente.";
                return false;
            }

            if (IsOccupied(node) &&
                !string.Equals(node.OccupiedByMarchId, march.MarchId, StringComparison.Ordinal))
            {
                error = "Nó já ocupado por outra marcha.";
                return false;
            }

            node.OccupiedByMarchId = march.MarchId;
            march.OccupyingNodeId = node.DefinitionId;
            if (node.RemainingAmount > 0 &&
                node.ResourceState is not (ResourceNodeState.Depleted or ResourceNodeState.Respawning))
            {
                node.SetResourceState(ResourceNodeState.Occupied);
            }

            error = string.Empty;
            return true;
        }

        public bool CanAcceptIncomingMarch(WorldNodeInstance node, string incomingMarchId, WorldNodeKind targetType)
        {
            if (!IsOccupied(node))
            {
                return true;
            }

            if (string.Equals(node.OccupiedByMarchId, incomingMarchId, StringComparison.Ordinal))
            {
                return true;
            }

            // Recurso ocupado rejeita marcha incompatível (outra marcha).
            if (targetType == WorldNodeKind.Resource)
            {
                return false;
            }

            // Demais tipos: também exclusivos enquanto ocupados.
            return false;
        }

        public void Release(WorldNodeInstance node, MarchOrder march)
        {
            if (string.Equals(node.OccupiedByMarchId, march.MarchId, StringComparison.Ordinal))
            {
                node.OccupiedByMarchId = null;
            }

            if (string.Equals(march.OccupyingNodeId, node.DefinitionId, StringComparison.Ordinal))
            {
                march.OccupyingNodeId = null;
            }

            if (node.ResourceState == ResourceNodeState.Occupied && node.RemainingAmount > 0)
            {
                node.SetResourceState(ResourceNodeState.Available);
            }
        }

        public void ReleaseAllForMarch(IEnumerable<WorldNodeInstance> nodes, MarchOrder march)
        {
            foreach (var node in nodes)
            {
                Release(node, march);
            }

            march.OccupyingNodeId = null;
        }
    }
}
