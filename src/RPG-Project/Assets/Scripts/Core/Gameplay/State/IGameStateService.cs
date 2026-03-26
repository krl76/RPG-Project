using System;

namespace Core.Gameplay.State
{
    public interface IGameStateService
    {
        event Action<GameState> StateChanged;

        GameState CurrentState { get; }

        void Enter(GameState state);
    }
}
