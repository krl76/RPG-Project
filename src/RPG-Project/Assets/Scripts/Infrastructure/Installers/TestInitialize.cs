using System;
using Cysharp.Threading.Tasks;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Events;
using Infrastructure.Services.Player;
using Infrastructure.Services.Player.Animator;
using Infrastructure.Services.Player.Input;
using Infrastructure.Services.Scene;
using Infrastructure.Services.UI;
using Input.PlayerInput;
using JetBrains.Annotations;
using prototype_Roma.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Infrastructure.Installers
{
    
    [UsedImplicitly] 
    public class TestInitialize : IInitializable, IGameStateSubscriber

    {
        private readonly IPlayerService _playerService;
        private readonly IPlayerAnimatorService _playerAnimatorService;
        private readonly IFightInputService _fightInputService;
        private readonly IMovementInputService _movementInputService;
        private readonly ICameraService _cameraService;
        private readonly IConfigDataProvider _configDataProvider;
        private readonly InputManager _inputManager;
        private readonly IWindowService _windowService;
        private readonly ISceneLoaderService _sceneLoaderService;

        [Inject]
        public TestInitialize(IPlayerService playerService, IPlayerAnimatorService playerAnimatorService,
            IFightInputService fightInputService, IMovementInputService movementInputService,
            ICameraService cameraService, IConfigDataProvider configDataProvider,
            InputManager inputManager, IWindowService windowService, ISceneLoaderService sceneLoaderService)
        {
            _playerService = playerService;
            _playerAnimatorService = playerAnimatorService;
            _fightInputService = fightInputService;
            _movementInputService = movementInputService;
            _cameraService = cameraService;
            _configDataProvider = configDataProvider;
            _inputManager = inputManager;
            _windowService = windowService;
            _sceneLoaderService = sceneLoaderService;
        }

        public void Initialize()
        {
            Debug.Log("init");
            _configDataProvider.Load();
            
            InitializeServices();
            
            EventBus.Subscribe(this);
        }

        public void InitializeServices()
        {
            _playerService.InstallService();
            _playerAnimatorService.InstallService();
            _fightInputService.InstallService();
            _cameraService.InstallService();
            _movementInputService.InstallService();
            _inputManager.ChangeState(_inputManager.GameplayInputState);
            _windowService.Open(WindowID.HUD);
        }

        public void OnGameOver()
        {
            //
            _fightInputService.UninstallService();
            _movementInputService.UninstallService();

            _playerAnimatorService.UninstallService();
        }

        public async void OnGameRestarted()
        {
            _windowService.Close(WindowID.HUD);
            
            _windowService.Close(WindowID.GameOver);
            
            await _sceneLoaderService.LoadSceneAsync("Game", LoadSceneMode.Single);

            await UniTask.DelayFrame(2); // ожидание повторной инициализации
            
            InitializeServices();
            
            Debug.Log("initialized");
        }
    }
}