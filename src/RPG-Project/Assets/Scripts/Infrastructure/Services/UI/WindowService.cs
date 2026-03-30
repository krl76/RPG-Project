using Core.Gameplay.State;
using Infrastructure.Factories.UI;
using Infrastructure.Providers.Configs;
using UnityEngine;

namespace Infrastructure.Services.UI
{
    /// <summary>
    /// Открывает, закрывает и выдаёт UI-окна по идентификатору.
    /// </summary>
    public class WindowService : IWindowService
    {
        private readonly IUIFactory _uiFactory;
        private readonly IConfigDataProvider _configDataProvider;
        private readonly IGameStateService _gameStateService;

        public WindowService(
            IUIFactory uiFactory,
            IConfigDataProvider configDataProvider,
            IGameStateService gameStateService)
        {
            _uiFactory = uiFactory;
            _configDataProvider = configDataProvider;
            _gameStateService = gameStateService;

            _gameStateService.StateChanged += OnGameStateChanged;
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

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                Open(WindowID.GameOver);
                return;
            }

            if (IsWindowOpened(WindowID.GameOver))
            {
                Close(WindowID.GameOver);
            }
        }
    }
}
