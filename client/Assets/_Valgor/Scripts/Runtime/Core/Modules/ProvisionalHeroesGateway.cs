using System;
using System.Collections.Generic;

namespace Valgor.Core.Modules
{
    /// <summary>
    /// Stub local até o módulo de heróis registrar a implementação real.
    /// </summary>
    public sealed class ProvisionalHeroesGateway : IHeroesGateway
    {
        public bool IsAvailable => false;

        public bool TryReserveMarchSlot(string targetNodeId, out string reservationId)
        {
            if (string.IsNullOrWhiteSpace(targetNodeId))
            {
                reservationId = string.Empty;
                return false;
            }

            reservationId = "provisional-" + Guid.NewGuid().ToString("N");
            return true;
        }

        public int GetProvisionalMarchPower() => 100;

        public float GetGatherRateMultiplier() => 1f;

        public string DescribeFormation() => "Formação provisional";

        public IReadOnlyList<string> GetCompatibleRiderHeroIds() => Array.Empty<string>();

        public bool TryGetHeroDisplayName(string heroId, out string displayName)
        {
            displayName = string.Empty;
            return false;
        }
    }
}
