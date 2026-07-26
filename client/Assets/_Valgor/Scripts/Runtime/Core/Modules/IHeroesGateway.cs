namespace Valgor.Core.Modules
{
    /// <summary>
    /// Ponto de integração com o módulo de heróis.
    /// A implementação concreta permanece sob responsabilidade do agente de heróis.
    /// </summary>
    public interface IHeroesGateway
    {
        bool IsAvailable { get; }

        /// <summary>
        /// Reserva um slot de marcha provisório sem expor roster, combate ou progressão.
        /// </summary>
        bool TryReserveMarchSlot(string targetNodeId, out string reservationId);

        /// <summary>
        /// Poder provisório da formação para encontros no mapa (sem expor o sistema interno).
        /// </summary>
        int GetProvisionalMarchPower();
    }
}
