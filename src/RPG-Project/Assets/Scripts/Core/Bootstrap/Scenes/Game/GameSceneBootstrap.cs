using Core.Gameplay.Pause;
using Core.Gameplay.Save;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Player;
using Infrastructure.Services.Player.Animator;
using Infrastructure.Services.Player.Input;
using Infrastructure.Services.UI;
using Input.PlayerInput;

namespace Core.Bootstrap.Scenes.Game
{
    /// <summary>
    /// Собирает и запускает игровые системы после загрузки игровой сцены.
    /// </summary>
    public sealed class GameSceneBootstrap
    {
        private readonly IPlayerService _playerService;
        private readonly IPlayerAnimatorService _playerAnimatorService;
        private readonly IFightInputService _fightInputService;
        private readonly IMovementInputService _movementInputService;
        private readonly ICameraService _cameraService;
        private readonly InputManager _inputManager;
        private readonly IWindowService _windowService;
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly GameplayPauseController _gameplayPauseController;
        private readonly IGameSaveInteractor _gameSaveInteractor;

        private bool _isGameplayInstalled;

        public GameSceneBootstrap(
            IPlayerService playerService,
            IPlayerAnimatorService playerAnimatorService,
            IFightInputService fightInputService,
            IMovementInputService movementInputService,
            ICameraService cameraService,
            InputManager inputManager,
            IWindowService windowService,
            IGameObjectFactory gameObjectFactory,
            GameplayPauseController gameplayPauseController,
            IGameSaveInteractor gameSaveInteractor)
        {
            _playerService = playerService;
            _playerAnimatorService = playerAnimatorService;
            _fightInputService = fightInputService;
            _movementInputService = movementInputService;
            _cameraService = cameraService;
            _inputManager = inputManager;
            _windowService = windowService;
            _gameObjectFactory = gameObjectFactory;
            _gameplayPauseController = gameplayPauseController;
            _gameSaveInteractor = gameSaveInteractor;
        }

        public bool Initialize()
        {
            if (_isGameplayInstalled)
            {
                return true;
            }

            _gameplayPauseController.Cleanup();
            _playerService.InstallService();
            if (_playerService.PlayerObject == null || _playerService.PlayerTransform == null)
            {
                return false;
            }

            _playerAnimatorService.InstallService();
            _fightInputService.InstallService();
            _cameraService.InstallService();
            _movementInputService.InstallService();

            if (_windowService.IsWindowOpened(WindowID.HUD) == false)
            {
                _windowService.Open(WindowID.HUD);
            }

            _gameSaveInteractor.ApplyPendingGameState();
            _gameSaveInteractor.ClearPendingRestore();
            _inputManager.ChangeState(_inputManager.GameplayInputState);

            _isGameplayInstalled = true;
            return true;
        }

        public void DisableGameplay()
        {
            if (_isGameplayInstalled == false)
            {
                return;
            }

            _fightInputService.UninstallService();
            _movementInputService.UninstallService();
            _playerAnimatorService.UninstallService();
            _inputManager.ChangeState(_inputManager.DisabledInputState);

            _isGameplayInstalled = false;
        }

        public void Cleanup()
        {
            DisableGameplay();
            _gameplayPauseController.Cleanup();

            if (_windowService.IsWindowOpened(WindowID.HUD))
            {
                _windowService.Close(WindowID.HUD);
            }

            if (_windowService.IsWindowOpened(WindowID.GameOver))
            {
                _windowService.Close(WindowID.GameOver);
            }

            _gameObjectFactory.Cleanup();
        }
    }
}
