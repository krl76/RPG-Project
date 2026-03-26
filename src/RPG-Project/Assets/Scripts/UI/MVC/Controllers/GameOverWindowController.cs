using Core.StateMachine;
using Core.StateMachine.States;
using Cysharp.Threading.Tasks;
using UI.MVC.Views;
using UnityEngine;

namespace UI.MVC.Controllers
{
    public sealed class GameOverWindowController
    {
        private readonly IGameStateMachine _gameStateMachine;

        private IGameOverView _view;

        public GameOverWindowController(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public void Attach(IGameOverView view)
        {
            Detach();

            _view = view;
            _view.RestartRequested += OnRestartRequested;
            _view.BackToMenuRequested += OnBackToMenuRequested;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Detach()
        {
            if (_view == null)
            {
                return;
            }

            _view.RestartRequested -= OnRestartRequested;
            _view.BackToMenuRequested -= OnBackToMenuRequested;
            _view = null;
        }

        private void OnRestartRequested()
        {
            RestartGameAsync().Forget();
        }

        private void OnBackToMenuRequested()
        {
            BackToMenuAsync().Forget();
        }

        private async UniTask RestartGameAsync()
        {
            await _gameStateMachine.Enter<LoadGameState>();
        }

        private async UniTask BackToMenuAsync()
        {
            await _gameStateMachine.Enter<LoadMainMenuState>();
        }
    }
}
