namespace Valgor.Core.Modules
{
    /// <summary>
    /// Contrato futuro da cidade do jogador.
    /// </summary>
    public interface IPlayerCityModule
    {
        bool IsLoaded { get; }
        void Enter();
        void Exit();
    }
}
