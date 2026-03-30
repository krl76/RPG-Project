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
    /// <summary>
    /// Окно HUD, отображающее здоровье, магию и очки.
    /// </summary>
    public class HUDWindow : WindowBase, IHUDView
    {
        public override WindowID Id => WindowID.HUD;

        [SerializeField] private Slider _hpSlider;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Image _magicIcon;
        [SerializeField] private RectTransform _scoreRoot;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField, Min(1f)] private float _scorePunchScale = 1.12f;
        [SerializeField, Min(0.01f)] private float _scorePunchDuration = 0.5f;

        private bool _isCooldownActive;
        private bool _hasInitializedScore;
        private float _cooldownTimer;
        private float _cooldownDuration;
        private float _scorePunchElapsed = float.MaxValue;
        private Vector3 _scoreBaseScale = Vector3.one;
        private HUDWindowController _controller;

        [Inject]
        private void Construct(HUDWindowController controller)
        {
            _controller = controller;
        }

        protected override void Awake()
        {
            base.Awake();

            if (_scoreRoot != null)
            {
                _scoreBaseScale = _scoreRoot.localScale;
            }
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

        public void SetMagicCooldown(float remainingTime, float totalDuration)
        {
            if (remainingTime <= 0f || totalDuration <= 0f)
            {
                CompleteMagicCooldown();
                return;
            }

            _isCooldownActive = true;
            _cooldownDuration = totalDuration;
            _cooldownTimer = Mathf.Clamp(totalDuration - remainingTime, 0f, totalDuration);
            _magicIcon.fillAmount = Mathf.Clamp01(_cooldownTimer / _cooldownDuration);
        }

        public void CompleteMagicCooldown()
        {
            _isCooldownActive = false;
            _cooldownTimer = _cooldownDuration;
            _magicIcon.fillAmount = 1f;
        }

        public void SetScore(int currentScore, bool animated)
        {
            if (_scoreText != null)
            {
                _scoreText.text = currentScore.ToString();
            }

            if (_scoreRoot == null)
            {
                _hasInitializedScore = true;
                return;
            }

            if (animated && _hasInitializedScore)
            {
                _scorePunchElapsed = 0f;
            }
            else
            {
                _scorePunchElapsed = _scorePunchDuration;
                _scoreRoot.localScale = _scoreBaseScale;
            }

            _hasInitializedScore = true;
        }

        private void Update()
        {
            if (_isCooldownActive && _magicIcon != null)
            {
                _cooldownTimer += Time.deltaTime;
                _magicIcon.fillAmount = Mathf.Clamp01(_cooldownTimer / _cooldownDuration);
            }

            UpdateScorePunch();
        }

        private void UpdateScorePunch()
        {
            if (_scoreRoot == null)
            {
                return;
            }

            if (_scorePunchElapsed >= _scorePunchDuration)
            {
                _scoreRoot.localScale = _scoreBaseScale;
                return;
            }

            _scorePunchElapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(_scorePunchElapsed / _scorePunchDuration);
            float pulse = Mathf.Sin(normalizedTime * Mathf.PI);
            float scaleMultiplier = Mathf.Lerp(1f, _scorePunchScale, pulse);
            _scoreRoot.localScale = _scoreBaseScale * scaleMultiplier;
        }
    }
}
