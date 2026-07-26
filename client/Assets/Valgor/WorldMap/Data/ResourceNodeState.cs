using System;
using Valgor.City.Data;

namespace Valgor.WorldMap.Data
{
    /// <summary>
    /// Estados específicos de um nó de recurso no mapa.
    /// Locked permanece em <see cref="WorldNodeStatus"/> para regiões trancadas.
    /// </summary>
    public enum ResourceNodeState
    {
        Available,
        Occupied,
        Depleted,
        Respawning
    }
}
