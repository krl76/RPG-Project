using Infrastructure.Services.UI;
using UnityEngine;

namespace Infrastructure.Factories.UI
{
    /// <summary>
    /// Контракт фабрики экранов UI.
    /// </summary>
    public interface IUIFactory
    {
        GameObject CreateScreen(GameObject prefab, WindowID windowId);
        T GetScreenComponent<T>(WindowID windowId) where T : Component;
        void DestroyScreen(WindowID windowId);
        bool Exists(WindowID windowId);
    }
}
