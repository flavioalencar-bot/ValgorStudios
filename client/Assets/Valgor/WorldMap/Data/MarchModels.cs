using System;

namespace Valgor.WorldMap.Data
{
    public enum MarchPhase
    {
        Idle,
        TravelingOutbound,
        Arrived,
        Returning,
        Completed
    }

    public sealed class MarchOrder
    {
        public MarchOrder(
            string id,
            string reservationId,
            string originNodeId,
            string targetNodeId,
            DateTime departedAtUtc,
            DateTime arrivesAtUtc,
            MarchPhase phase)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ReservationId = reservationId ?? throw new ArgumentNullException(nameof(reservationId));
            OriginNodeId = originNodeId ?? throw new ArgumentNullException(nameof(originNodeId));
            TargetNodeId = targetNodeId ?? throw new ArgumentNullException(nameof(targetNodeId));
            DepartedAtUtc = departedAtUtc;
            ArrivesAtUtc = arrivesAtUtc;
            Phase = phase;
        }

        public string Id { get; }
        public string ReservationId { get; }
        public string OriginNodeId { get; }
        public string TargetNodeId { get; }
        public DateTime DepartedAtUtc { get; set; }
        public DateTime ArrivesAtUtc { get; set; }
        public MarchPhase Phase { get; set; }
        public string? CurrentNodeId { get; set; }
    }

    public sealed class MarchChangedEventArgs : EventArgs
    {
        public MarchChangedEventArgs(MarchOrder? march) => March = march;
        public MarchOrder? March { get; }
    }
}
