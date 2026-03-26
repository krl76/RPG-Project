using Infrastructure.Services.UI;
using UI.Base;
using UI.MVC.Controllers;
using UI.MVC.Views;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class GameOverWindow : WindowBase, IGameOverView
    {
        public override WindowID Id => WindowID.GameOver;
        public override bool IsPopup => true;

        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _backToMenuButton;

        public event System.Action RestartRequested;
        public event System.Action BackToMenuRequested;

        private GameOverWindowController _controller;

        [Inject]
        private void Construct(GameOverWindowController controller)
        {
            _controller = controller;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);

            _restartButton.onClick.AddListener(RaiseRestartRequested);
            _backToMenuButton.onClick.AddListener(RaiseBackToMenuRequested);
            _controller.Attach(this);
        }

        public override void OnClose()
        {
            _controller.Detach();
            _restartButton.onClick.RemoveListener(RaiseRestartRequested);
            _backToMenuButton.onClick.RemoveListener(RaiseBackToMenuRequested);
            base.OnClose();
        }

        private void RaiseRestartRequested() => RestartRequested?.Invoke();
        private void RaiseBackToMenuRequested() => BackToMenuRequested?.Invoke();
    }
}
