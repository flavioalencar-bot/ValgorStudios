namespace Valgor.Core.Modules
{
    /// <summary>
    /// Ponto de integração com o módulo de heróis.
    /// A implementação concreta permanece sob responsabilidade do agente de heróis.
    /// </summary>
    public interface IHeroesGateway
    {
        bool IsAvailable { get; }
    }
}
