using Infrastructure.Providers.Configs;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Player;
using Infrastructure.Services.Player.Animator;
using Infrastructure.Services.Player.Input;
using Input.PlayerInput;
using JetBrains.Annotations;
using prototype_Roma.Scripts;
using UnityEngine;
using Zenject;

namespace Infrastructure.Installers
{
    
    [UsedImplicitly]
    public class TestInitialize : IInitializable

    {
        private readonly IPlayerService _playerService;
        private readonly IPlayerAnimatorService _playerAnimatorService;
        private readonly IFightInputService _fightInputService;
        private readonly IMovementInputService _movementInputService;
        private readonly ICameraService _cameraService;
        private readonly IConfigDataProvider _configDataProvider;
        private readonly InputManager _inputManager;
        
        [Inject]
        public TestInitialize(IPlayerService playerService, IPlayerAnimatorService playerAnimatorService,
            IFightInputService fightInputService, IMovementInputService movementInputService,
            ICameraService cameraService, IConfigDataProvider configDataProvider,
            InputManager inputManager)
        {
            _playerService = playerService;
            _playerAnimatorService = playerAnimatorService;
            _fightInputService = fightInputService;
            _movementInputService = movementInputService;
            _cameraService = cameraService;
            _configDataProvider = configDataProvider;
            _inputManager = inputManager;
        }

        public void Initialize()
        {
            Debug.Log("init");
            _configDataProvider.Load();
            _playerService.InstallService();
            _playerAnimatorService.InstallService();
            _fightInputService.InstallService();
            _cameraService.InstallService();
            _movementInputService.InstallService();
            _inputManager.ChangeState(_inputManager.GameplayInputState);
        }
    }
}