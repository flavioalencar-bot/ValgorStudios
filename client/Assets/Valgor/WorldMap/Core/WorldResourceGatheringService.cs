using System;
using Valgor.City.Data;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Marches;

namespace Valgor.WorldMap.Core
{
    /// <summary>
    /// Coleta completa no mapa: taxa por hora → carga da marcha → depleção → respawn.
    /// Não altera ocupação nem a máquina de estados da marcha além de GATHERING.
    /// </summary>
    public sealed class WorldResourceGatheringService
    {
        private readonly IWorldMapClock _clock;

        public WorldResourceGatheringService(IWorldMapClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public event Action? Changed;

        public bool CanStart(WorldNodeInstance node, WorldMapNodeDefinition definition, MarchOrder? march)
        {
            if (definition is not WorldResourceNode)
            {
                return false;
            }

            if (node.ResourceState is ResourceNodeState.Depleted or ResourceNodeState.Respawning)
            {
                return false;
            }

            if (node.Status == WorldNodeStatus.Locked || node.RemainingAmount <= 0)
            {
                return false;
            }

            return march != null &&
                   march.State == MarchState.Arrived &&
                   string.Equals(march.TargetNodeId, node.DefinitionId, StringComparison.Ordinal) &&
                   !march.RewardsDelivered &&
                   march.ResourceLoad < march.Capacity;
        }

        public bool TryStart(WorldNodeInstance node, WorldMapNodeDefinition definition, MarchOrder march, MarchStateMachine stateMachine, out string error)
        {
            if (!CanStart(node, definition, march))
            {
                error = "Coleta indisponível.";
                return false;
            }

            if (!stateMachine.TryTransition(march, MarchState.Gathering, out error))
            {
                return false;
            }

            node.SetResourceState(ResourceNodeState.Occupied);
            node.OccupiedByMarchId = march.MarchId;
            node.LastGatherUpdatedUtc = _clock.UtcNow;
            march.OccupyingNodeId = node.DefinitionId;
            Changed?.Invoke();
            error = string.Empty;
            return true;
        }

        public long ApplyGathering(
            WorldNodeInstance node,
            WorldResourceNode definition,
            MarchOrder march,
            DateTime nowUtc)
        {
            if (march.State != MarchState.Gathering ||
                !string.Equals(march.TargetNodeId, node.DefinitionId, StringComparison.Ordinal))
            {
                return 0;
            }

            if (node.ResourceState is ResourceNodeState.Depleted or ResourceNodeState.Respawning)
            {
                return 0;
            }

            var last = node.LastGatherUpdatedUtc ?? nowUtc;
            var gathered = ResourceGatherCalculator.CalculateGathered(
                definition.GetGatherRatePerHour(),
                node.RemainingAmount,
                march.ResourceLoad,
                march.Capacity,
                last,
                nowUtc);

            node.LastGatherUpdatedUtc = nowUtc;

            if (gathered <= 0)
            {
                return 0;
            }

            node.RemainingAmount -= gathered;
            march.ResourceLoad += gathered;

            if (node.RemainingAmount <= 0)
            {
                node.RemainingAmount = 0;
                BeginRespawn(node, definition, nowUtc);
            }
            else
            {
                node.SetResourceState(ResourceNodeState.Occupied);
                node.OccupiedByMarchId = march.MarchId;
            }

            Changed?.Invoke();
            return gathered;
        }

        public void AdvanceRespawn(WorldNodeInstance node, WorldResourceNode definition, DateTime nowUtc)
        {
            if (node.ResourceState != ResourceNodeState.Respawning || !node.RespawnAt.HasValue)
            {
                return;
            }

            if (nowUtc < node.RespawnAt.Value)
            {
                return;
            }

            node.RemainingAmount = definition.MaxAmount;
            node.RespawnAt = null;
            node.LastGatherUpdatedUtc = null;
            node.OccupiedByMarchId = null;
            node.SetResourceState(ResourceNodeState.Available);
            Changed?.Invoke();
        }

        public bool TryDepositLoad(MarchOrder march, WorldResourceNode definition, ResourceWallet wallet, out long deposited)
        {
            deposited = 0;
            if (march.ResourceLoad <= 0)
            {
                return false;
            }

            if (march.RewardsDelivered)
            {
                return false;
            }

            deposited = march.ResourceLoad;
            wallet.Add(definition.ResourceType, deposited);
            march.ResourceLoad = 0;
            march.RewardsDelivered = true;
            Changed?.Invoke();
            return true;
        }

        private static void BeginRespawn(WorldNodeInstance node, WorldResourceNode definition, DateTime nowUtc)
        {
            node.OccupiedByMarchId = null;
            node.RespawnAt = nowUtc.Add(definition.RespawnDuration);
            node.LastGatherUpdatedUtc = null;
            node.SetResourceState(ResourceNodeState.Respawning);
        }
    }
}
