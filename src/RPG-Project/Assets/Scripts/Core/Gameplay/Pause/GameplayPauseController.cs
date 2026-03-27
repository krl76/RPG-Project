using System;
using Core.Gameplay.Save;
using Core.Gameplay.State;
using Core.StateMachine;
using Core.StateMachine.States;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.UI;
using Input.PlayerInput;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Core.Gameplay.Pause
{
    public sealed class GameplayPauseController : ITickable, IDisposable
    {
        private readonly IGameStateService _gameStateService;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IWindowService _windowService;
        private readonly InputManager _inputManager;
        private readonly IGameSaveInteractor _gameSaveInteractor;

        public GameplayPauseController(
            IGameStateService gameStateService,
            IGameStateMachine gameStateMachine,
            IWindowService windowService,
            InputManager inputManager,
            IGameSaveInteractor gameSaveInteractor)
        {
            _gameStateService = gameStateService;
            _gameStateMachine = gameStateMachine;
            _windowService = windowService;
            _inputManager = inputManager;
            _gameSaveInteractor = gameSaveInteractor;

            _gameStateService.StateChanged += OnGameStateChanged;
        }

        public void Tick()
        {
            if (Keyboard.current == null || Keyboard.current.escapeKey.wasPressedThisFrame == false)
            {
                return;
            }

            if (_gameStateService.CurrentState == GameState.Paused)
            {
                if (_windowService.IsWindowOpened(WindowID.Settings))
                {
                    _windowService.Close(WindowID.Settings);
                    return;
                }

                Resume();
                return;
            }

            if (_gameStateService.CurrentState == GameState.Gameplay)
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (_gameStateService.CurrentState != GameState.Gameplay || _windowService.IsWindowOpened(WindowID.Pause))
            {
                return;
            }

            _inputManager.ChangeState(_inputManager.DisabledInputState);
            _windowService.Open(WindowID.Pause);

            Time.timeScale = 0f;
            _gameStateService.Enter(GameState.Paused);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            if (_gameStateService.CurrentState != GameState.Paused)
            {
                return;
            }

            if (_windowService.IsWindowOpened(WindowID.Settings))
            {
                _windowService.Close(WindowID.Settings);
            }

            if (_windowService.IsWindowOpened(WindowID.Pause))
            {
                _windowService.Close(WindowID.Pause);
            }

            Time.timeScale = 1f;
            _inputManager.ChangeState(_inputManager.GameplayInputState);
            _gameStateService.Enter(GameState.Gameplay);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SaveGame()
        {
            if (_gameStateService.CurrentState != GameState.Paused)
            {
                return;
            }

            _gameSaveInteractor.SaveGame();
        }

        public void LoadGame()
        {
            LoadGameAsync().Forget();
        }

        public void OpenSettings()
        {
            if (_gameStateService.CurrentState != GameState.Paused || _windowService.IsWindowOpened(WindowID.Settings))
            {
                return;
            }

            _windowService.Open(WindowID.Settings);
        }

        public void ExitToMainMenu()
        {
            ExitToMainMenuAsync().Forget();
        }

        public void Cleanup()
        {
            PrepareForSceneTransition();
        }

        public void Dispose()
        {
            _gameStateService.StateChanged -= OnGameStateChanged;
        }

        private async UniTask ExitToMainMenuAsync()
        {
            PrepareForSceneTransition();
            await _gameStateMachine.Enter<LoadMainMenuState>();
        }

        private async UniTask LoadGameAsync()
        {
            if (_gameStateService.CurrentState != GameState.Paused)
            {
                return;
            }

            if (_gameSaveInteractor.PrepareLoadGame() == false)
            {
                return;
            }

            PrepareForSceneTransition();
            await _gameStateMachine.Enter<LoadGameState>();
        }

        private void PrepareForSceneTransition()
        {
            if (_windowService.IsWindowOpened(WindowID.Settings))
            {
                _windowService.Close(WindowID.Settings);
            }

            if (_windowService.IsWindowOpened(WindowID.Pause))
            {
                _windowService.Close(WindowID.Pause);
            }

            Time.timeScale = 1f;
            _inputManager.ChangeState(_inputManager.DisabledInputState);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Gameplay || state == GameState.Paused)
            {
                return;
            }

            if (_windowService.IsWindowOpened(WindowID.Pause) == false &&
                _windowService.IsWindowOpened(WindowID.Settings) == false)
            {
                return;
            }

            PrepareForSceneTransition();
        }
    }
}
