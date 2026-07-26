using System;
using System.Collections.Generic;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Core
{
    /// <summary>
    /// Grafo oficial de estados do dragão (complemento 02).
    /// </summary>
    public sealed class DragonStateMachine
    {
        private static readonly Dictionary<DragonState, HashSet<DragonState>> Transitions = new()
        {
            [DragonState.Locked] = new() { DragonState.Egg },
            [DragonState.Egg] = new() { DragonState.Hatching },
            [DragonState.Hatching] = new() { DragonState.Juvenile },
            [DragonState.Juvenile] = new() { DragonState.Resting, DragonState.Hungry },
            [DragonState.Ready] = new() { DragonState.Deployed, DragonState.Hungry, DragonState.Resting },
            [DragonState.Deployed] = new() { DragonState.Exhausted, DragonState.Injured },
            [DragonState.Hungry] = new() { DragonState.Resting, DragonState.Ready },
            [DragonState.Exhausted] = new() { DragonState.Recovering },
            [DragonState.Injured] = new() { DragonState.Recovering },
            [DragonState.Recovering] = new() { DragonState.Resting },
            [DragonState.Resting] = new() { DragonState.Ready, DragonState.Hungry }
        };

        public bool CanTransition(DragonState from, DragonState to) =>
            Transitions.TryGetValue(from, out var next) && next.Contains(to);

        public bool TryTransition(DragonInstance dragon, DragonState to, out string error)
        {
            if (!CanTransition(dragon.State, to))
            {
                error = $"Transição inválida: {dragon.State} → {to}.";
                return false;
            }

            dragon.State = to;
            dragon.LastUpdatedUtc = DateTime.UtcNow;
            error = string.Empty;
            return true;
        }
    }
}
