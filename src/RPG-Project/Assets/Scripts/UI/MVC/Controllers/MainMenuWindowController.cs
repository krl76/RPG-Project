using Core.Gameplay.Save;
using Core.StateMachine;
using Core.StateMachine.States;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.UI;
using UI.MVC.Views;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI.MVC.Controllers
{
    /// <summary>
    /// Контроллер главного меню, запускающий игровые переходы.
    /// </summary>
    public sealed class MainMenuWindowController
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IGameSaveInteractor _gameSaveInteractor;
        private readonly IWindowService _windowService;

        private IMainMenuView _view;

        public MainMenuWindowController(
            IGameStateMachine gameStateMachine,
            IGameSaveInteractor gameSaveInteractor,
            IWindowService windowService)
        {
            _gameStateMachine = gameStateMachine;
            _gameSaveInteractor = gameSaveInteractor;
            _windowService = windowService;
        }

        public void Attach(IMainMenuView view)
        {
            Detach();

            _view = view;
            _view.PlayRequested += OnPlayRequested;
            _view.SettingsRequested += OnSettingsRequested;
            _view.ExitRequested += OnExitRequested;
        }

        public void Detach()
        {
            if (_view == null)
            {
                return;
            }

            _view.PlayRequested -= OnPlayRequested;
            _view.SettingsRequested -= OnSettingsRequested;
            _view.ExitRequested -= OnExitRequested;
            _view = null;
        }

        private void OnPlayRequested()
        {
            StartGameAsync().Forget();
        }

        private void OnSettingsRequested()
        {
            if (_windowService.IsWindowOpened(WindowID.Settings))
            {
                return;
            }

            _windowService.Open(WindowID.Settings);
        }

        private static void OnExitRequested()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private async UniTask StartGameAsync()
        {
            _gameSaveInteractor.ClearPendingRestore();
            await _gameStateMachine.Enter<LoadGameState>();
        }
    }
}
