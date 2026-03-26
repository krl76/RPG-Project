using Core.Bootstrap.Scenes.Game;
using Core.Bootstrap.Scenes.MainMenu;
using Core.Gameplay.State;
using Core.StateMachine;
using Core.StateMachine.States;
using Infrastructure.Factories.Objects;
using Infrastructure.Factories.UI;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Enemy;
using Infrastructure.Services.Player;
using Infrastructure.Services.Player.Animator;
using Infrastructure.Services.Player.Input;
using Infrastructure.Services.Scene;
using Infrastructure.Services.UI;
using Input.PlayerInput;
using Zenject;

namespace Infrastructure.Installers
{
    public class InfrastructureInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindProviders();
            BindCoreState();
            BindInput();
            BindAudio();
            BindGameplayServices();
            BindUIServices();
            BindFactories();
            BindStateMachine();
            BindSceneBootstraps();
        }

        private void BindProviders()
        {
            Container.Bind<IConfigDataProvider>().To<ConfigDataProvider>().AsSingle();
            Container.Bind<ISceneLoaderService>().To<SceneLoaderService>().AsSingle();
        }

        private void BindCoreState()
        {
            Container.Bind<IGameStateService>().To<GameStateService>().AsSingle();
            Container.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle();
        }

        private void BindInput()
        {
            Container.Bind<PlayerInput>().AsSingle();
            Container.Bind<InputManager>().AsSingle();
            Container.Bind<IInputBindingService>().To<InputBindingService>().AsSingle();
        }

        private void BindAudio()
        {
            Container.Bind<IAudioService>().To<AudioService>().AsSingle();
        }

        private void BindGameplayServices()
        {
            Container.Bind<IPlayerService>().To<PlayerService>().AsSingle();
            Container.Bind<ICameraService>().To<CameraService>().AsSingle();
            Container.Bind<IFightInputService>().To<FightInputService>().AsSingle();
            Container.Bind<IMovementInputService>().To<MovementInputService>().AsSingle();
            Container.Bind<IPlayerAnimatorService>().To<PlayerAnimatorService>().AsSingle();
            Container.Bind<IEnemyService>().To<EnemyService>().AsSingle();
        }

        private void BindUIServices()
        {
            Container.Bind<IWindowService>().To<WindowService>().AsSingle();
        }

        private void BindFactories()
        {
            Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsSingle();
            Container.Bind<IUIFactory>().To<UIFactory>().AsSingle();
        }

        private void BindStateMachine()
        {
            Container.Bind<BootstrapState>().AsTransient();
            Container.Bind<LoadMainMenuState>().AsTransient();
            Container.Bind<MainMenuState>().AsTransient();
            Container.Bind<LoadGameState>().AsTransient();
            Container.Bind<GameplayState>().AsTransient();
            Container.Bind<GameOverState>().AsTransient();
        }

        private void BindSceneBootstraps()
        {
            Container.Bind<MainMenuSceneBootstrap>().AsSingle();
            Container.Bind<GameSceneBootstrap>().AsSingle();
        }
    }
}
