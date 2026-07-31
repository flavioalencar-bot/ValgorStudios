using System;

namespace Valgor.WorldMap.Raids
{
    /// <summary>Baixas e capacidade do Hospital (puro).</summary>
    public static class CasualtyResolver
    {
        public static CasualtyResult Resolve(int totalCasualties, int freeHospitalCapacity)
        {
            var total = Math.Max(0, totalCasualties);
            var capacityBefore = Math.Max(0, freeHospitalCapacity);
            var candidateWounded = (int)Math.Floor(total * (double)RaidCombatCatalog.WoundedCandidateRatio);
            var baseLosses = total - candidateWounded;
            var accepted = Math.Min(candidateWounded, capacityBefore);
            var overflow = candidateWounded - accepted;
            var lost = baseLosses + overflow;
            var capacityAfter = Math.Max(0, capacityBefore - accepted);

            return new CasualtyResult
            {
                TotalCasualties = total,
                Wounded = accepted,
                Lost = lost,
                HospitalOverflow = overflow,
                HospitalCapacityBefore = capacityBefore,
                HospitalCapacityAfter = capacityAfter
            };
        }

        /// <summary>Heróis e Dragão nunca ocupam leitos — só tropas.</summary>
        public static int TroopsOnlyCasualties(int troopCasualties) => Math.Max(0, troopCasualties);
    }

    public static class HospitalCapacityResolver
    {
        public static int FreeCapacity(int hospitalLevel, int unitsInCare)
        {
            var capacity = Math.Max(0, hospitalLevel) * 10;
            return Math.Max(0, capacity - Math.Max(0, unitsInCare));
        }

        public static int CapacityFromCatalog(int hospitalCapacity, int unitsInCare) =>
            Math.Max(0, hospitalCapacity - Math.Max(0, unitsInCare));
    }
}
