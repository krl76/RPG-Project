using Infrastructure.Factories.UI;
using Infrastructure.Providers;
using Infrastructure.Providers.Configs;
using UnityEngine;

namespace Infrastructure.Services.UI
{
    public class WindowService : IWindowService
    {
        private readonly IUIFactory _uiFactory;
        private readonly IConfigDataProvider _configDataProvider;

        public WindowService(
            IUIFactory uiFactory, 
            IConfigDataProvider configDataProvider)
        {
            _uiFactory = uiFactory;
            _configDataProvider = configDataProvider;
        }

        public bool IsWindowOpened(WindowID windowID) => 
            _uiFactory.Exists(windowID);

        public void Open(WindowID windowID)
        {
            var prefab = _configDataProvider.GetWindowPrefab(windowID);
            if (prefab == null)
            {
                Debug.LogError($"[WindowService] Failed to open. Prefab not found for ID: {windowID}");
                return;
            }
            
            _uiFactory.CreateScreen(prefab, windowID);
        }

        public T OpenAndGet<T>(WindowID windowID) where T : Component
        {
            Open(windowID);
            return _uiFactory.GetScreenComponent<T>(windowID);
        }

        public T Get<T>(WindowID windowID) where T : Component => 
            _uiFactory.GetScreenComponent<T>(windowID);

        public void Close(WindowID windowID) => 
            _uiFactory.DestroyScreen(windowID);
    }
}