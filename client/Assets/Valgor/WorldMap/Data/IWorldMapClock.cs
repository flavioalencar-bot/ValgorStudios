using System;

namespace Valgor.WorldMap.Data
{
    public interface IWorldMapClock
    {
        DateTime UtcNow { get; }
    }

    public sealed class SystemWorldMapClock : IWorldMapClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public sealed class FixedWorldMapClock : IWorldMapClock
    {
        public FixedWorldMapClock(DateTime utcNow) => UtcNow = utcNow;
        public DateTime UtcNow { get; set; }
    }
}
