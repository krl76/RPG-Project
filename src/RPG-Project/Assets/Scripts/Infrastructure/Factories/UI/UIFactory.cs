using System.Collections.Generic;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.UI;
using UI.Base;
using UnityEngine;

namespace Infrastructure.Factories.UI
{
    /// <summary>
    /// Фабрика создания, хранения и уничтожения окон UI.
    /// </summary>
    public class UIFactory : IUIFactory
    {
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly Dictionary<WindowID, GameObject> _screenInstances = new();

        public UIFactory(IGameObjectFactory gameObjectFactory)
        {
            _gameObjectFactory = gameObjectFactory;
        }

        public GameObject CreateScreen(GameObject prefab, WindowID windowId)
        {
            if (_screenInstances.TryGetValue(windowId, out var existingInstance) && existingInstance != null)
            {
                Debug.LogWarning($"[UIFactory] Screen with WindowID {windowId} already exists. Swapping screens.");
                DestroyScreen(windowId);
            }
            else if (_screenInstances.ContainsKey(windowId))
            {
                _screenInstances.Remove(windowId);
            }

            var instance = _gameObjectFactory.Instantiate(prefab);
            if (instance.TryGetComponent<WindowBase>(out var window) == false)
            {
                Debug.LogError($"[UIFactory] Screen prefab for {windowId} has no {nameof(WindowBase)}.");
                Object.Destroy(instance);
                return null;
            }

            if (_screenInstances.TryAdd(windowId, instance))
            {
                window.OnOpen();
                
                return instance;
            }
            
            Object.Destroy(instance);
            return null;
        }

        public T GetScreenComponent<T>(WindowID windowId) where T : Component
        {
            if (_screenInstances.TryGetValue(windowId, out var screenObject))
            {
                if (screenObject == null)
                {
                    _screenInstances.Remove(windowId);
                    Debug.LogError($"[UIFactory] Screen with WindowID {windowId} was destroyed unexpectedly.");
                    return null;
                }

                if (screenObject.TryGetComponent<T>(out var screenComponent))
                {
                    return screenComponent;
                }

                Debug.LogError($"[UIFactory] Component of screen by type {typeof(T)} not found on WindowID {windowId}.");
                return null;
            }

            Debug.LogError($"[UIFactory] Screen with WindowID {windowId} not found.");
            return null;
        }

        public void DestroyScreen(WindowID windowId)
        {
            if (!_screenInstances.Remove(windowId, out var screenObject))
            {
                Debug.LogWarning($"[UIFactory] Cannot destroy. Screen with WindowID {windowId} not found.");
                return;
            }

            if (screenObject == null)
            {
                return;
            }

            if (screenObject.TryGetComponent<WindowBase>(out var window))
            {
                window.OnClose();
            }
            
            _gameObjectFactory.Destroy(screenObject);
        }

        public bool Exists(WindowID windowId)
        {
            if (_screenInstances.TryGetValue(windowId, out var screenObject) == false)
            {
                return false;
            }

            if (screenObject != null)
            {
                return true;
            }

            _screenInstances.Remove(windowId);
            return false;
        }
    }
}
