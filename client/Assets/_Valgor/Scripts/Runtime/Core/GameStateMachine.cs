using System;

namespace Valgor.Core
{
    public sealed class GameStateMachine
    {
        public GameState Current { get; private set; } = GameState.None;

        public event Action<GameState, GameState> StateChanged;

        public void TransitionTo(GameState next)
        {
            if (Current == next)
            {
                return;
            }

            if (!IsValidTransition(Current, next))
            {
                throw new InvalidOperationException($"Invalid game state transition: {Current} -> {next}");
            }

            var previous = Current;
            Current = next;
            StateChanged?.Invoke(previous, next);
        }

        private static bool IsValidTransition(GameState from, GameState to)
        {
            return (from, to) switch
            {
                (GameState.None, GameState.Bootstrapping) => true,
                (GameState.Bootstrapping, GameState.Loading) => true,
                (GameState.Loading, GameState.MainMenu) => true,
                (GameState.MainMenu, GameState.PlayerCity) => true,
                (GameState.MainMenu, GameState.WorldMap) => true,
                (GameState.PlayerCity, GameState.WorldMap) => true,
                (GameState.PlayerCity, GameState.MainMenu) => true,
                (GameState.WorldMap, GameState.PlayerCity) => true,
                (GameState.WorldMap, GameState.MainMenu) => true,
                _ => false
            };
        }
    }
}
