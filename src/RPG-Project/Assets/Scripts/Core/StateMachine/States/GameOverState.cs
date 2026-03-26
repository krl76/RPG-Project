using Core.Bootstrap.Scenes.Game;
using Core.Gameplay.State;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine.States
{
    public sealed class GameOverState : IGameFlowState
    {
        private readonly IGameStateService _gameStateService;
        private readonly GameSceneBootstrap _gameSceneBootstrap;

        public GameOverState(
            IGameStateService gameStateService,
            GameSceneBootstrap gameSceneBootstrap)
        {
            _gameStateService = gameStateService;
            _gameSceneBootstrap = gameSceneBootstrap;
        }

        public UniTask Enter()
        {
            _gameSceneBootstrap.DisableGameplay();
            _gameStateService.Enter(GameState.GameOver);
            return UniTask.CompletedTask;
        }

        public void Exit()
        {
        }
    }
}
