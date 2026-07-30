using System;
using System.Collections.Generic;
using Valgor.Core;

namespace Valgor.Core.Modules
{
    /// <summary>
    /// Gateway de heróis da beta: Vortex + progressão por Castelo/pesquisa.
    /// </summary>
    public sealed class BetaHeroesGateway : IHeroesGateway
    {
        public const string VortexHeroId = "HERO_VORTEX_000";
        public const string VortexDisplayName = "Vortex";
        public const string ElyraHeroId = "HERO_ELYRA_001";
        public const string VesperaHeroId = "HERO_VESPERA_010";
        public const int VortexMarchPower = 280;
        public const int PowerPerCastleLevel = 20;
        public const float BaseGatherMultiplier = 1.10f;
        public const float ResearchGatherExtra = 1.05f;

        private static readonly string[] RiderIds =
        {
            VortexHeroId,
            ElyraHeroId,
            VesperaHeroId
        };

        private string? _activeReservationId;
        private string? _activeTargetNodeId;

        public bool IsAvailable => true;

        public bool TryReserveMarchSlot(string targetNodeId, out string reservationId)
        {
            if (string.IsNullOrWhiteSpace(targetNodeId))
            {
                reservationId = string.Empty;
                return false;
            }

            _activeTargetNodeId = targetNodeId;
            _activeReservationId = $"vortex-{VortexHeroId}-{Guid.NewGuid():N}";
            reservationId = _activeReservationId;
            return true;
        }

        public int GetProvisionalMarchPower() =>
            VortexMarchPower + Math.Max(1, BetaProgress.CastleLevel) * PowerPerCastleLevel;

        public float GetGatherRateMultiplier()
        {
            var multiplier = BaseGatherMultiplier;
            if (BetaProgress.ResearchGatherBoost)
            {
                multiplier *= ResearchGatherExtra;
            }

            return multiplier;
        }

        public string DescribeFormation()
        {
            var power = GetProvisionalMarchPower();
            var baseLine = string.IsNullOrEmpty(_activeReservationId)
                ? $"{VortexDisplayName} (pronto · poder {power})"
                : $"{VortexDisplayName} → {_activeTargetNodeId} · poder {power}";
            return baseLine;
        }

        public IReadOnlyList<string> GetCompatibleRiderHeroIds() => RiderIds;

        public bool TryGetHeroDisplayName(string heroId, out string displayName)
        {
            displayName = heroId switch
            {
                VortexHeroId => VortexDisplayName,
                ElyraHeroId => "Elyra",
                VesperaHeroId => "Vespera",
                _ => string.Empty
            };
            return !string.IsNullOrEmpty(displayName);
        }
    }
}
