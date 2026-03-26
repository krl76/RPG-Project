using UnityEngine;

namespace Infrastructure.Services.UI
{
    public interface IWindowService
    {
        bool IsWindowOpened(WindowID windowID);
        void Open(WindowID windowID);
        T OpenAndGet<T>(WindowID windowID) where T : Component;
        T Get<T>(WindowID windowID) where T : Component;
        void Close(WindowID windowID);
    }
}