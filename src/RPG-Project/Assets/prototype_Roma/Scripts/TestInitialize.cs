using System;
using Infrastructure.Providers.Configs;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace prototype_Roma.Scripts
{
    /*public class BootStrapTest : MonoInstaller
    {
        public override void InstallBindings()
        {
            Debug.Log("init 1");
            
            Container.Bind<TestInitialize>().AsSingle().NonLazy();

            Container.Bind<IInitializable>().To<TestInitialize>().FromResolve();
        }
    }*/
    
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