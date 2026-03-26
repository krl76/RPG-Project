using Core.Gameplay.State;
using Cysharp.Threading.Tasks;
using Infrastructure.Providers.Configs;
using Input.PlayerInput;

namespace Core.StateMachine.States
{
    public sealed class BootstrapState : IGameFlowState
    {
        private readonly IGameStateService _gameStateService;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IConfigDataProvider _configDataProvider;
        private readonly InputManager _inputManager;

        public BootstrapState(
            IGameStateService gameStateService,
            IGameStateMachine gameStateMachine,
            IConfigDataProvider configDataProvider,
            InputManager inputManager)
        {
            _gameStateService = gameStateService;
            _gameStateMachine = gameStateMachine;
            _configDataProvider = configDataProvider;
            _inputManager = inputManager;
        }

        public async UniTask Enter()
        {
            _gameStateService.Enter(GameState.Bootstrapping);
            _configDataProvider.Load();
            _inputManager.ChangeState(_inputManager.DisabledInputState);

            await _gameStateMachine.Enter<LoadMainMenuState>();
        }

        public void Exit()
        {
        }
    }
}
