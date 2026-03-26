using Core.Bootstrap.Scenes.Game;
using Core.Gameplay.State;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.StateMachine.States
{
    public sealed class GameplayState : IGameFlowState
    {
        private readonly IGameStateService _gameStateService;
        private readonly GameSceneBootstrap _gameSceneBootstrap;

        public GameplayState(
            IGameStateService gameStateService,
            GameSceneBootstrap gameSceneBootstrap)
        {
            _gameStateService = gameStateService;
            _gameSceneBootstrap = gameSceneBootstrap;
        }

        public UniTask Enter()
        {
            if (_gameSceneBootstrap.Initialize())
            {
                _gameStateService.Enter(GameState.Gameplay);
            }
            else
            {
                Debug.LogError("[GameplayState] Game scene bootstrap failed.");
            }

            return UniTask.CompletedTask;
        }

        public void Exit()
        {
        }
    }
}
