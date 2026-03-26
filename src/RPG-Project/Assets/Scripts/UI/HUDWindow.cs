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
    public class HUDWindow : WindowBase, IHUDView
    {
        public override WindowID Id => WindowID.HUD;

        [SerializeField] private Slider _hpSlider;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Image _magicIcon;

        private bool _isCooldownActive;
        private float _cooldownTimer;
        private float _cooldownDuration;
        private HUDWindowController _controller;

        [Inject]
        private void Construct(HUDWindowController controller)
        {
            _controller = controller;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);
            _controller.Attach(this);
        }

        public override void OnClose()
        {
            _controller.Detach();
            base.OnClose();
        }

        public void SetHealth(float currentHealth, float maxHealth)
        {
            _hpSlider.value = currentHealth / maxHealth;
            _hpText.text = $"{Mathf.CeilToInt(currentHealth)} / {maxHealth}";
        }

        public void StartMagicCooldown(float cooldownDuration)
        {
            _isCooldownActive = true;
            _cooldownTimer = 0f;
            _cooldownDuration = cooldownDuration;
            _magicIcon.fillAmount = 0f;
        }

        public void CompleteMagicCooldown()
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
