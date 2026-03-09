using Features.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WorldHealthBar : MonoBehaviour, IHealthFeedback
    {
        [SerializeField] private Slider _healthSlider;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        public void OnHealthChanged(float current, float max)
        {
            _healthSlider.value = current / max;
            gameObject.SetActive(current > 0);
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + _mainCamera.transform.forward);
        }
    }
}