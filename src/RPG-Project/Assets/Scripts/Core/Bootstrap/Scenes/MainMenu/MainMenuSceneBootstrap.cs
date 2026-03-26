using Input.PlayerInput;
using UnityEngine;

namespace Core.Bootstrap.Scenes.MainMenu
{
    public sealed class MainMenuSceneBootstrap
    {
        private readonly InputManager _inputManager;

        public MainMenuSceneBootstrap(InputManager inputManager)
        {
            _inputManager = inputManager;
        }

        public void Initialize()
        {
            _inputManager.ChangeState(_inputManager.DisabledInputState);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
