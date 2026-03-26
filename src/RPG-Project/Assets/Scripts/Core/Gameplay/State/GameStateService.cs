using System;

namespace Core.Gameplay.State
{
    public sealed class GameStateService : IGameStateService
    {
        public event Action<GameState> StateChanged;

        public GameState CurrentState { get; private set; } = GameState.None;

        public void Enter(GameState state)
        {
            if (CurrentState == state)
            {
                return;
            }

            CurrentState = state;
            StateChanged?.Invoke(CurrentState);
        }
    }
}
