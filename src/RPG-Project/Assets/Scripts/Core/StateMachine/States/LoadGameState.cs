using Core.Bootstrap.Scenes.Game;
using Core.Gameplay.State;
using Cysharp.Threading.Tasks;
using Data.Paths;
using Infrastructure.Services.Scene;
using UnityEngine.SceneManagement;

namespace Core.StateMachine.States
{
    public sealed class LoadGameState : IGameFlowState
    {
        private readonly IGameStateService _gameStateService;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly GameSceneBootstrap _gameSceneBootstrap;

        public LoadGameState(
            IGameStateService gameStateService,
            IGameStateMachine gameStateMachine,
            ISceneLoaderService sceneLoaderService,
            GameSceneBootstrap gameSceneBootstrap)
        {
            _gameStateService = gameStateService;
            _gameStateMachine = gameStateMachine;
            _sceneLoaderService = sceneLoaderService;
            _gameSceneBootstrap = gameSceneBootstrap;
        }

        public async UniTask Enter()
        {
            _gameStateService.Enter(GameState.Loading);
            _gameSceneBootstrap.Cleanup();

            await _sceneLoaderService.LoadSceneAsync(ScenePaths.GAME_SCENE_PATH, LoadSceneMode.Single);
            await _gameStateMachine.Enter<GameplayState>();
        }

        public void Exit()
        {
        }
    }
}
