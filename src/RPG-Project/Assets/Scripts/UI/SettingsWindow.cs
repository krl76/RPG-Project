using Infrastructure.Services.Audio;
using Infrastructure.Services.Player.Input;
using Infrastructure.Services.UI;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public sealed class SettingsWindow : WindowBase
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

        private IWindowService _windowService;
        private IAudioService _audioService;
        private IInputBindingService _inputBindingService;

        [Inject]
        private void Construct(
            IWindowService windowService,
            IAudioService audioService,
            IInputBindingService inputBindingService)
        {
            _windowService = windowService;
            _audioService = audioService;
            _inputBindingService = inputBindingService;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);

            BindButtons();
            BindSliders();
            RefreshVolumeSliders();
            RefreshBindingTexts();
        }

        public override void OnClose()
        {
            _inputBindingService.CancelRebind();

            _closeButton.onClick.RemoveListener(CloseWindow);

            _masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            _effectsVolumeSlider.onValueChanged.RemoveListener(OnEffectsVolumeChanged);

            _jumpButton.onClick.RemoveListener(RebindJump);
            _sprintButton.onClick.RemoveListener(RebindSprint);
            _swordAttackButton.onClick.RemoveListener(RebindSwordAttack);
            _magicAttackButton.onClick.RemoveListener(RebindMagicAttack);

            base.OnClose();
        }

        private void BindButtons()
        {
            _closeButton.onClick.AddListener(CloseWindow);
            _jumpButton.onClick.AddListener(RebindJump);
            _sprintButton.onClick.AddListener(RebindSprint);
            _swordAttackButton.onClick.AddListener(RebindSwordAttack);
            _magicAttackButton.onClick.AddListener(RebindMagicAttack);
        }

        private void BindSliders()
        {
            _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            _effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);
        }

        private void RefreshVolumeSliders()
        {
            _masterVolumeSlider.SetValueWithoutNotify(_audioService.MasterVolume);
            _musicVolumeSlider.SetValueWithoutNotify(_audioService.MusicVolume);
            _effectsVolumeSlider.SetValueWithoutNotify(_audioService.EffectsVolume);
        }

        private void RefreshBindingTexts()
        {
            SetBindingText(_jumpButton, InputBindingKey.Jump);
            SetBindingText(_sprintButton, InputBindingKey.Sprint);
            SetBindingText(_swordAttackButton, InputBindingKey.SwordAttack);
            SetBindingText(_magicAttackButton, InputBindingKey.MagicAttack);
        }

        private void CloseWindow()
        {
            _windowService.Close(WindowID.Settings);
        }

        private void OnMasterVolumeChanged(float value) => _audioService.SetMasterVolume(value);
        private void OnMusicVolumeChanged(float value) => _audioService.SetMusicVolume(value);
        private void OnEffectsVolumeChanged(float value) => _audioService.SetEffectsVolume(value);

        private void RebindJump() => StartRebind(InputBindingKey.Jump, _jumpButton);
        private void RebindSprint() => StartRebind(InputBindingKey.Sprint, _sprintButton);
        private void RebindSwordAttack() => StartRebind(InputBindingKey.SwordAttack, _swordAttackButton);
        private void RebindMagicAttack() => StartRebind(InputBindingKey.MagicAttack, _magicAttackButton);

        private void StartRebind(InputBindingKey bindingKey, Button button)
        {
            if (_inputBindingService.StartRebind(bindingKey, OnRebindCompleted, OnRebindCompleted) == false)
            {
                return;
            }

            SetAllRebindButtonsInteractable(false);
            SetButtonText(button, $"{GetBindingLabel(bindingKey)}: нажмите клавишу...");
        }

        private void OnRebindCompleted()
        {
            SetAllRebindButtonsInteractable(true);
            RefreshBindingTexts();
        }

        private void SetAllRebindButtonsInteractable(bool isInteractable)
        {
            _jumpButton.interactable = isInteractable;
            _sprintButton.interactable = isInteractable;
            _swordAttackButton.interactable = isInteractable;
            _magicAttackButton.interactable = isInteractable;
        }

        private void SetBindingText(Button button, InputBindingKey bindingKey)
        {
            SetButtonText(button, $"{GetBindingLabel(bindingKey)}: {_inputBindingService.GetBindingDisplay(bindingKey)}");
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
                InputBindingKey.MoveUp => "Движение вверх",
                InputBindingKey.MoveDown => "Движение вниз",
                InputBindingKey.MoveLeft => "Движение влево",
                InputBindingKey.MoveRight => "Движение вправо",
                InputBindingKey.Jump => "Прыжок",
                InputBindingKey.Sprint => "Бег",
                InputBindingKey.SwordAttack => "Удар мечом",
                InputBindingKey.MagicAttack => "Магия",
                _ => bindingKey.ToString()
            };
        }
    }
}
