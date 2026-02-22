using System.Collections.Generic;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.UI;
using UnityEngine;

namespace Infrastructure.Factories.UI
{
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
            if (_screenInstances.ContainsKey(windowId))
            {
                Debug.LogWarning($"[UIFactory] Screen with WindowID {windowId} already exists. Swapping screens.");
                DestroyScreen(windowId);
            }

            var instance = _gameObjectFactory.Instantiate(prefab);

            if (_screenInstances.TryAdd(windowId, instance))
            {
                return instance;
            }

            Object.Destroy(instance);
            return null;
        }

        public T GetScreenComponent<T>(WindowID windowId) where T : Component
        {
            if (_screenInstances.TryGetValue(windowId, out var screenObject))
            {
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

            _gameObjectFactory.Destroy(screenObject);
        }

        public bool Exists(WindowID windowId) => _screenInstances.ContainsKey(windowId);
    }
}