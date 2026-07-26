using System;

namespace Valgor.WorldMap.Core
{
    /// <summary>
    /// Cálculo determinístico de coleta no mapa por timestamp.
    /// </summary>
    public static class ResourceGatherCalculator
    {
        public static long CalculateGathered(
            double ratePerHour,
            long nodeRemaining,
            long marchLoad,
            long marchCapacity,
            DateTime lastUpdatedUtc,
            DateTime nowUtc)
        {
            if (ratePerHour <= 0 || nodeRemaining <= 0 || marchCapacity <= 0)
            {
                return 0;
            }

            if (nowUtc <= lastUpdatedUtc)
            {
                return 0;
            }

            var room = marchCapacity - marchLoad;
            if (room <= 0)
            {
                return 0;
            }

            var elapsed = nowUtc - lastUpdatedUtc;
            if (elapsed <= TimeSpan.Zero)
            {
                return 0;
            }

            var gathered = (long)Math.Floor(ratePerHour * elapsed.TotalHours);
            if (gathered <= 0)
            {
                return 0;
            }

            if (gathered > nodeRemaining)
            {
                gathered = nodeRemaining;
            }

            return gathered > room ? room : gathered;
        }

        public static TimeSpan? EstimateTimeToFillOrDeplete(
            double ratePerHour,
            long nodeRemaining,
            long marchLoad,
            long marchCapacity)
        {
            if (ratePerHour <= 0)
            {
                return null;
            }

            var room = marchCapacity - marchLoad;
            if (room <= 0 || nodeRemaining <= 0)
            {
                return TimeSpan.Zero;
            }

            var amount = room < nodeRemaining ? room : nodeRemaining;
            return TimeSpan.FromHours(amount / ratePerHour);
        }
    }
}
