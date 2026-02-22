using Infrastructure.Factories.Objects;
using Infrastructure.Factories.UI;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Scene;
using Infrastructure.Services.UI;
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