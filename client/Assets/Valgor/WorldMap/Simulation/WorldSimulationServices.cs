using System;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Simulation
{
    /// <summary>
    /// Relógio compartilhado da simulação mundial (mesmo instante para City e WorldMap).
    /// </summary>
    public sealed class WorldSimulationClock : IWorldMapClock
    {
        private readonly IWorldMapClock _inner;

        public WorldSimulationClock(IWorldMapClock? inner = null)
        {
            _inner = inner ?? new SystemWorldMapClock();
        }

        public DateTime UtcNow => _inner.UtcNow;
    }

    /// <summary>
    /// Coordena o avanço da sessão mundial sem depender da cena aberta.
    /// </summary>
    public sealed class WorldSimulationCoordinator
    {
        private WorldMapSession? _session;

        public WorldMapSession? Session => _session;

        public void Bind(WorldMapSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public void Tick()
        {
            _session?.Tick();
        }
    }

    /// <summary>
    /// Serviço de tick global registrado no ServiceRegistry.
    /// Avança marchas/coleta por timestamp mesmo com a City aberta.
    /// </summary>
    public sealed class GlobalMarchTickService
    {
        private readonly WorldSimulationCoordinator _coordinator;

        public GlobalMarchTickService(WorldSimulationCoordinator coordinator)
        {
            _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        }

        public WorldSimulationCoordinator Coordinator => _coordinator;

        public void Tick() => _coordinator.Tick();
    }
}
