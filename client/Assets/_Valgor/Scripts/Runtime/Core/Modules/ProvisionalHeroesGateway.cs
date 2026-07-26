using System;

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
    }
}
