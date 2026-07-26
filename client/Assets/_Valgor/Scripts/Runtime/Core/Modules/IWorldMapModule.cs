namespace Valgor.Core.Modules
{
    /// <summary>
    /// Contrato futuro do mapa mundial.
    /// </summary>
    public interface IWorldMapModule
    {
        bool IsLoaded { get; }
        void Enter();
        void Exit();
    }
}
