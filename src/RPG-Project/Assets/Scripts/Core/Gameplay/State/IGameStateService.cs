using System;

namespace Core.Gameplay.State
{
    /// <summary>
    /// Сервис текущего high-level состояния игры.
    /// </summary>
    public interface IGameStateService
    {
        event Action<GameState> StateChanged;

        GameState CurrentState { get; }

        void Enter(GameState state);
    }
}
