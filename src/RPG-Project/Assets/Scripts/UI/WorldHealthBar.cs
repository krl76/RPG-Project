using Features.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WorldHealthBar : MonoBehaviour, IHealthFeedback
    {
        private const float PunchDuration = 0.5f;

        [SerializeField] private Slider _healthSlider;
        [SerializeField, Range(0.7f, 1f)] private float _minPunchScale = 0.75f;
        private Camera _mainCamera;
        private Vector3 _baseScale;
        private float _punchElapsed = PunchDuration;
        private bool _hasInitializedHealth;

        private void Start()
        {
            _mainCamera = Camera.main;
            _baseScale = transform.localScale;
        }

        public void OnHealthChanged(float current, float max)
        {
            _healthSlider.value = current / max;
            gameObject.SetActive(current > 0);

            if (_hasInitializedHealth)
            {
                _punchElapsed = 0f;
            }
            else
            {
                _hasInitializedHealth = true;
            }
        }

        private void LateUpdate()
        {
            if (_mainCamera != null)
            {
                transform.LookAt(transform.position + _mainCamera.transform.forward);
            }

            UpdatePunchScale();
        }

        private void UpdatePunchScale()
        {
            if (_punchElapsed >= PunchDuration)
            {
                transform.localScale = _baseScale;
                return;
            }

            _punchElapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(_punchElapsed / PunchDuration);
            float pulse = Mathf.Sin(normalizedTime * Mathf.PI);
            float scaleMultiplier = Mathf.Lerp(1f, _minPunchScale, pulse);
            transform.localScale = _baseScale * scaleMultiplier;
        }
    }
}
