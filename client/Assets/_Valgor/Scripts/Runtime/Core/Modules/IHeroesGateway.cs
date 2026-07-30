using System;
using System.Collections.Generic;

namespace Valgor.Core.Modules
{
    /// <summary>
    /// Ponto de integração com o módulo de heróis.
    /// </summary>
    public interface IHeroesGateway
    {
        bool IsAvailable { get; }

        bool TryReserveMarchSlot(string targetNodeId, out string reservationId);

        int GetProvisionalMarchPower();

        string DescribeFormation();

        /// <summary>Multiplicador de coleta no mapa (1.0 = base).</summary>
        float GetGatherRateMultiplier();

        /// <summary>Heróis elegíveis como montadores (Fase 4).</summary>
        IReadOnlyList<string> GetCompatibleRiderHeroIds();

        bool TryGetHeroDisplayName(string heroId, out string displayName);
    }
}
