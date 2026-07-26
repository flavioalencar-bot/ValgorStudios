using System;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Marches
{
    public enum MarchState
    {
        Preparing,
        Marching,
        Arrived,
        Gathering,
        Returning,
        Completed,
        Cancelled
    }

    public sealed class MarchChangedEvent : EventArgs
    {
        public MarchChangedEvent(MarchOrder? march, MarchState? previousState = null)
        {
            March = march;
            PreviousState = previousState;
        }

        public MarchOrder? March { get; }
        public MarchState? PreviousState { get; }
    }

    public sealed class MarchOrder
    {
        public MarchOrder(
            string marchId,
            string playerId,
            string originNodeId,
            string targetNodeId,
            string selectedTeamId,
            DateTime departureAt,
            DateTime arrivalAt,
            MarchState state,
            float speed,
            long capacity,
            WorldNodeKind targetType,
            DateTime? returnAt = null,
            long resourceLoad = 0)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (resourceLoad < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resourceLoad));
            }

            if (speed <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            MarchId = marchId ?? throw new ArgumentNullException(nameof(marchId));
            PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
            OriginNodeId = originNodeId ?? throw new ArgumentNullException(nameof(originNodeId));
            TargetNodeId = targetNodeId ?? throw new ArgumentNullException(nameof(targetNodeId));
            SelectedTeamId = selectedTeamId ?? throw new ArgumentNullException(nameof(selectedTeamId));
            DepartureAt = departureAt;
            ArrivalAt = arrivalAt;
            ReturnAt = returnAt;
            State = state;
            Speed = speed;
            Capacity = capacity;
            ResourceLoad = resourceLoad;
            TargetType = targetType;
        }

        public string MarchId { get; }
        public string PlayerId { get; }
        public string OriginNodeId { get; }
        public string TargetNodeId { get; }
        public string SelectedTeamId { get; }
        public DateTime DepartureAt { get; set; }
        public DateTime ArrivalAt { get; set; }
        public DateTime? ReturnAt { get; set; }
        public MarchState State { get; set; }
        public float Speed { get; }
        public long Capacity { get; }
        public long ResourceLoad { get; set; }
        public WorldNodeKind TargetType { get; }
        public bool RewardsDelivered { get; set; }
        public string? OccupyingNodeId { get; set; }
        public string? RewardDeliveryId { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public bool IsCommitted { get; set; }

        /// <summary>Compatibilidade com consumidores existentes.</summary>
        public string Id => MarchId;

        public MarchOrder Clone() =>
            new(
                MarchId,
                PlayerId,
                OriginNodeId,
                TargetNodeId,
                SelectedTeamId,
                DepartureAt,
                ArrivalAt,
                State,
                Speed,
                Capacity,
                TargetType,
                ReturnAt,
                ResourceLoad)
            {
                RewardsDelivered = RewardsDelivered,
                OccupyingNodeId = OccupyingNodeId,
                RewardDeliveryId = RewardDeliveryId,
                DeliveredAt = DeliveredAt,
                IsCommitted = IsCommitted
            };
    }
}
