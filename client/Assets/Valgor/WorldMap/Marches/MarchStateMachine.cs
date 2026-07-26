using System;
using System.Collections.Generic;

namespace Valgor.WorldMap.Marches
{
    public sealed class MarchStateMachine
    {
        private static readonly Dictionary<MarchState, HashSet<MarchState>> Transitions = new()
        {
            [MarchState.Preparing] = new() { MarchState.Marching, MarchState.Cancelled },
            [MarchState.Marching] = new() { MarchState.Arrived, MarchState.Cancelled },
            [MarchState.Arrived] = new() { MarchState.Gathering, MarchState.Returning, MarchState.Cancelled },
            [MarchState.Gathering] = new() { MarchState.Returning, MarchState.Cancelled },
            [MarchState.Returning] = new() { MarchState.Completed },
            [MarchState.Completed] = new(),
            [MarchState.Cancelled] = new()
        };

        private static readonly HashSet<MarchState> Cancellable =
            new() { MarchState.Preparing, MarchState.Marching, MarchState.Arrived, MarchState.Gathering };

        public bool CanTransition(MarchState from, MarchState to) =>
            Transitions.TryGetValue(from, out var next) && next.Contains(to);

        public bool CanCancel(MarchState state) => Cancellable.Contains(state);

        public bool TryTransition(MarchOrder march, MarchState to, out string error)
        {
            if (!CanTransition(march.State, to))
            {
                error = $"Transição inválida: {march.State} → {to}.";
                return false;
            }

            march.State = to;
            error = string.Empty;
            return true;
        }
    }
}
