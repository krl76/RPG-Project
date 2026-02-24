using Infrastructure.Factories.Objects;
using Infrastructure.Factories.UI;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Scene;
using Infrastructure.Services.UI;
using prototype_Roma.Scripts;
using UnityEngine.Rendering;
using Zenject;

namespace Infrastructure.Installers
{
    public class InfrastructureInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindProviders();
            BindServices();
            BindFactories();
        }
        private void BindProviders()
        {
            Container.Bind<IConfigDataProvider>().To<ConfigDataProvider>().AsSingle();
        }
        private void BindServices()
        {
            Container.Bind<TestInitialize>().AsSingle().NonLazy();

            Container.Bind<IInitializable>().To<TestInitialize>().FromResolve();
            
            Container.Bind<PlayerInput>().AsSingle();
            Container.Bind<InputManager>().AsSingle();
            Container.Bind<IPlayerService>().To<PlayerService>().AsSingle();
            Container.Bind<ICameraService>().To<CameraService>().AsSingle();
            Container.Bind<IFightInputService>().To<FightInputService>().AsSingle();
            Container.Bind<IMovementInputService>().To<MovementInputService>().AsSingle();
            Container.Bind<IPlayerAnimatorService>().To<PlayerAnimatorService>().AsSingle();
            Container.Bind<ISceneLoaderService>().To<SceneLoaderService>().AsSingle();
            Container.Bind<IWindowService>().To<WindowService>().AsSingle();
        }
        private void BindFactories()
        {
            Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsSingle();
            Container.Bind<IUIFactory>().To<UIFactory>().AsSingle();
        }
    }
}