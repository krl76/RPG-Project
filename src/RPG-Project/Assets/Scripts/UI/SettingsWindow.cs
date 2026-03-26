using Infrastructure.Services.Player.Input;
using Infrastructure.Services.UI;
using TMPro;
using UI.Base;
using UI.MVC.Controllers;
using UI.MVC.Views;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public sealed class SettingsWindow : WindowBase, ISettingsView
    {
        public override WindowID Id => WindowID.Settings;
        public override bool IsPopup => true;

        [SerializeField] private Button _closeButton;
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _effectsVolumeSlider;
        [SerializeField] private Button _jumpButton;
        [SerializeField] private Button _sprintButton;
        [SerializeField] private Button _swordAttackButton;
        [SerializeField] private Button _magicAttackButton;

        public event System.Action CloseRequested;
        public event System.Action<float> MasterVolumeChanged;
        public event System.Action<float> MusicVolumeChanged;
        public event System.Action<float> EffectsVolumeChanged;
        public event System.Action<InputBindingKey> RebindRequested;

        private SettingsWindowController _controller;

        [Inject]
        private void Construct(SettingsWindowController controller)
        {
            _controller = controller;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);

            BindViewEvents();
            _controller.Attach(this);
        }

        public override void OnClose()
        {
            _controller.Detach();
            UnbindViewEvents();
            base.OnClose();
        }

        public void SetVolumes(float master, float music, float effects)
        {
            _masterVolumeSlider.SetValueWithoutNotify(master);
            _musicVolumeSlider.SetValueWithoutNotify(music);
            _effectsVolumeSlider.SetValueWithoutNotify(effects);
        }

        public void SetBindingDisplay(InputBindingKey bindingKey, string displayValue)
        {
            SetButtonText(GetButton(bindingKey), $"{GetBindingLabel(bindingKey)}: {displayValue}");
        }

        public void ShowRebindPrompt(InputBindingKey bindingKey)
        {
            SetButtonText(GetButton(bindingKey), $"{GetBindingLabel(bindingKey)}: press key...");
        }

        public void SetRebindButtonsInteractable(bool isInteractable)
        {
            _jumpButton.interactable = isInteractable;
            _sprintButton.interactable = isInteractable;
            _swordAttackButton.interactable = isInteractable;
            _magicAttackButton.interactable = isInteractable;
        }

        private void BindViewEvents()
        {
            _closeButton.onClick.AddListener(RaiseCloseRequested);
            _masterVolumeSlider.onValueChanged.AddListener(RaiseMasterVolumeChanged);
            _musicVolumeSlider.onValueChanged.AddListener(RaiseMusicVolumeChanged);
            _effectsVolumeSlider.onValueChanged.AddListener(RaiseEffectsVolumeChanged);
            _jumpButton.onClick.AddListener(RaiseJumpRebindRequested);
            _sprintButton.onClick.AddListener(RaiseSprintRebindRequested);
            _swordAttackButton.onClick.AddListener(RaiseSwordAttackRebindRequested);
            _magicAttackButton.onClick.AddListener(RaiseMagicAttackRebindRequested);
        }

        private void UnbindViewEvents()
        {
            _closeButton.onClick.RemoveListener(RaiseCloseRequested);
            _masterVolumeSlider.onValueChanged.RemoveListener(RaiseMasterVolumeChanged);
            _musicVolumeSlider.onValueChanged.RemoveListener(RaiseMusicVolumeChanged);
            _effectsVolumeSlider.onValueChanged.RemoveListener(RaiseEffectsVolumeChanged);
            _jumpButton.onClick.RemoveListener(RaiseJumpRebindRequested);
            _sprintButton.onClick.RemoveListener(RaiseSprintRebindRequested);
            _swordAttackButton.onClick.RemoveListener(RaiseSwordAttackRebindRequested);
            _magicAttackButton.onClick.RemoveListener(RaiseMagicAttackRebindRequested);
        }

        private Button GetButton(InputBindingKey bindingKey)
        {
            return bindingKey switch
            {
                InputBindingKey.Jump => _jumpButton,
                InputBindingKey.Sprint => _sprintButton,
                InputBindingKey.SwordAttack => _swordAttackButton,
                InputBindingKey.MagicAttack => _magicAttackButton,
                _ => null
            };
        }

        private static void SetButtonText(Button button, string value)
        {
            TextMeshProUGUI text = button != null ? button.GetComponentInChildren<TextMeshProUGUI>() : null;
            if (text != null)
            {
                text.text = value;
            }
        }

        private static string GetBindingLabel(InputBindingKey bindingKey)
        {
            return bindingKey switch
            {
                InputBindingKey.MoveUp => "Move Up",
                InputBindingKey.MoveDown => "Move Down",
                InputBindingKey.MoveLeft => "Move Left",
                InputBindingKey.MoveRight => "Move Right",
                InputBindingKey.Jump => "Jump",
                InputBindingKey.Sprint => "Sprint",
                InputBindingKey.SwordAttack => "Sword Attack",
                InputBindingKey.MagicAttack => "Magic Attack",
                _ => bindingKey.ToString()
            };
        }

        private void RaiseCloseRequested() => CloseRequested?.Invoke();
        private void RaiseMasterVolumeChanged(float value) => MasterVolumeChanged?.Invoke(value);
        private void RaiseMusicVolumeChanged(float value) => MusicVolumeChanged?.Invoke(value);
        private void RaiseEffectsVolumeChanged(float value) => EffectsVolumeChanged?.Invoke(value);
        private void RaiseJumpRebindRequested() => RebindRequested?.Invoke(InputBindingKey.Jump);
        private void RaiseSprintRebindRequested() => RebindRequested?.Invoke(InputBindingKey.Sprint);
        private void RaiseSwordAttackRebindRequested() => RebindRequested?.Invoke(InputBindingKey.SwordAttack);
        private void RaiseMagicAttackRebindRequested() => RebindRequested?.Invoke(InputBindingKey.MagicAttack);
    }
}
