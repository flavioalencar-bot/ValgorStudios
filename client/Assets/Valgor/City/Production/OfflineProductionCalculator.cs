using System;

namespace Valgor.City.Production
{
    using Valgor.City.Data;

    /// <summary>
    /// Cálculo determinístico de produção offline/online baseado em timestamp.
    /// </summary>
    public static class OfflineProductionCalculator
    {
        public static long CalculateProduced(
            double ratePerHour,
            long currentAccumulated,
            long capacity,
            DateTime lastUpdatedUtc,
            DateTime nowUtc,
            TimeSpan maxOfflineDuration)
        {
            if (ratePerHour <= 0 || capacity <= 0)
            {
                return 0;
            }

            if (nowUtc <= lastUpdatedUtc)
            {
                return 0;
            }

            var room = capacity - currentAccumulated;
            if (room <= 0)
            {
                return 0;
            }

            var elapsed = nowUtc - lastUpdatedUtc;
            if (elapsed > maxOfflineDuration)
            {
                elapsed = maxOfflineDuration;
            }

            if (elapsed <= TimeSpan.Zero)
            {
                return 0;
            }

            var produced = (long)Math.Floor(ratePerHour * elapsed.TotalHours);
            if (produced <= 0)
            {
                return 0;
            }

            return produced > room ? room : produced;
        }

        public static TimeSpan? EstimateTimeToFill(
            double ratePerHour,
            long currentAccumulated,
            long capacity)
        {
            if (ratePerHour <= 0 || capacity <= 0)
            {
                return null;
            }

            var room = capacity - currentAccumulated;
            if (room <= 0)
            {
                return TimeSpan.Zero;
            }

            var hours = room / ratePerHour;
            return TimeSpan.FromHours(hours);
        }
    }
}
