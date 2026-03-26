using Infrastructure.Providers.Configs;
using Infrastructure.Services.UI;
using UI;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Infrastructure.Services.Scene
{
    public sealed class LoadingScreenService : ILoadingScreenService
    {
        private readonly DiContainer _container;
        private readonly IConfigDataProvider _configDataProvider;

        private LoadingBarView _loadingBarView;

        public LoadingScreenService(DiContainer container, IConfigDataProvider configDataProvider)
        {
            _container = container;
            _configDataProvider = configDataProvider;
        }

        public void Show()
        {
            if (_loadingBarView != null)
            {
                _loadingBarView.SetProgress(0f);
                return;
            }

            var prefab = _configDataProvider.GetWindowPrefab(WindowID.Loading);
            if (prefab == null)
            {
                Debug.LogError("[LoadingScreenService] LoadingBar prefab is not configured.");
                return;
            }

            var instance = _container.InstantiatePrefab(prefab);
            Object.DontDestroyOnLoad(instance);

            _loadingBarView = instance.GetComponent<LoadingBarView>();
            if (_loadingBarView == null)
            {
                Debug.LogError("[LoadingScreenService] LoadingBarView component is missing on prefab.");
                Object.Destroy(instance);
                return;
            }

            _loadingBarView.SetProgress(0f);
        }

        public void SetProgress(float progress)
        {
            _loadingBarView?.SetProgress(progress);
        }

        public void Hide()
        {
            if (_loadingBarView == null)
            {
                return;
            }

            Object.Destroy(_loadingBarView.gameObject);
            _loadingBarView = null;
        }
    }
}
