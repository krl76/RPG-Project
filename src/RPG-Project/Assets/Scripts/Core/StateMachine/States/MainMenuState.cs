using Core.Bootstrap.Scenes.MainMenu;
using Core.Gameplay.State;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine.States
{
    public sealed class MainMenuState : IGameFlowState
    {
        private readonly IGameStateService _gameStateService;
        private readonly MainMenuSceneBootstrap _mainMenuSceneBootstrap;

        public MainMenuState(
            IGameStateService gameStateService,
            MainMenuSceneBootstrap mainMenuSceneBootstrap)
        {
            _gameStateService = gameStateService;
            _mainMenuSceneBootstrap = mainMenuSceneBootstrap;
        }

        public UniTask Enter()
        {
            _mainMenuSceneBootstrap.Initialize();
            _gameStateService.Enter(GameState.MainMenu);
            return UniTask.CompletedTask;
        }

        public void Exit()
        {
            _mainMenuSceneBootstrap.Cleanup();
        }
    }
}
