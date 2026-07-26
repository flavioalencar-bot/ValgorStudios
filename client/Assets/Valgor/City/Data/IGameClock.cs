using System;

namespace Valgor.City.Data
{
    /// <summary>
    /// Relógio do jogo. Fonte oficial será o servidor quando conectado.
    /// </summary>
    public interface IGameClock
    {
        DateTime UtcNow { get; }
    }

    public sealed class SystemGameClock : IGameClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
