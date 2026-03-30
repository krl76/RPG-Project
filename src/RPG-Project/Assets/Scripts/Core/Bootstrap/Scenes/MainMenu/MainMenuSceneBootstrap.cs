using Infrastructure.Services.UI;
using Input.PlayerInput;
using UnityEngine;

namespace Core.Bootstrap.Scenes.MainMenu
{
    /// <summary>
    /// Подготавливает главное меню после загрузки стартовой сцены.
    /// </summary>
    public sealed class MainMenuSceneBootstrap
    {
        private readonly InputManager _inputManager;
        private readonly IWindowService _windowService;

        public MainMenuSceneBootstrap(InputManager inputManager, IWindowService windowService)
        {
            _inputManager = inputManager;
            _windowService = windowService;
        }

        public void Initialize()
        {
            _inputManager.ChangeState(_inputManager.DisabledInputState);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_windowService.IsWindowOpened(WindowID.MainMenu) == false)
            {
                _windowService.Open(WindowID.MainMenu);
            }
        }

        public void Cleanup()
        {
            if (_windowService.IsWindowOpened(WindowID.Settings))
            {
                _windowService.Close(WindowID.Settings);
            }

            if (_windowService.IsWindowOpened(WindowID.MainMenu))
            {
                _windowService.Close(WindowID.MainMenu);
            }
        }
    }
}
