using System;
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
        public const int VortexMarchPower = 280;
        public const int PowerPerCastleLevel = 20;
        public const float BaseGatherMultiplier = 1.10f;
        public const float ResearchGatherExtra = 1.05f;

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
    }
}
