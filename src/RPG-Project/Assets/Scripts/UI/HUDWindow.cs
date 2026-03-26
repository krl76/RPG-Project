using Infrastructure.Services.Events;
using Infrastructure.Services.UI;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HUDWindow : WindowBase, IPlayerHealthSubscriber, IPlayerMagicSubscriber
    {
        public override WindowID Id => WindowID.HUD;

        [SerializeField] private Slider _hpSlider;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Image _magicIcon;

        private bool _isCooldownActive;
        private float _cooldownTimer;
        private float _cooldownDuration;

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);
            EventBus.Subscribe(this);
        }

        public override void OnClose()
        {
            base.OnClose();
            EventBus.Unsubscribe(this);
        }

        public void OnPlayerHealthChanged(float currentHealth, float maxHealth)
        {
            _hpSlider.value = currentHealth / maxHealth;
            _hpText.text = $"{Mathf.CeilToInt(currentHealth)} / {maxHealth}";
        }

        public void OnPlayerDied()
        {
            //TODO: выключение HUD при смерти
        }

        public void OnMagicUsed(float cooldownDuration)
        {
            _isCooldownActive = true;
            _cooldownTimer = 0f;
            _cooldownDuration = cooldownDuration;
            _magicIcon.fillAmount = 0f;
        }

        public void OnMagicReady()
        {
            _isCooldownActive = false;
            _magicIcon.fillAmount = 1f;
        }

        private void Update()
        {
            if (_isCooldownActive && _magicIcon != null)
            {
                _cooldownTimer += Time.deltaTime;
                _magicIcon.fillAmount = Mathf.Clamp01(_cooldownTimer / _cooldownDuration);
            }
        }
    }
}