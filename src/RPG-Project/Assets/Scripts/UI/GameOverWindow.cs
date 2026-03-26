using System;
using Infrastructure.Services.Events;
using Infrastructure.Services.Scene;
using Infrastructure.Services.UI;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class GameOverWindow : WindowBase
    {
        public override WindowID Id => WindowID.GameOver;
        public override bool IsPopup => true;

        [SerializeField] private Button _restartButton;
        private ISceneLoaderService _sceneLoader;

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);
            
            _restartButton.onClick.AddListener(RestartGame);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public override void OnClose()
        {
            base.OnClose();
            _restartButton.onClick.RemoveListener(RestartGame);
        }

        private void RestartGame()
        {
            EventBus.RaiseEvent<IGameStateSubscriber>(sub => sub.OnGameRestarted());
        }
    }
}