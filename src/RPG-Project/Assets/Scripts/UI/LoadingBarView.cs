using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class LoadingBarView : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;

        private void Awake()
        {
            SetProgress(0f);
        }

        public void SetProgress(float progress)
        {
            if (_fillImage == null)
            {
                return;
            }

            _fillImage.fillAmount = Mathf.Clamp01(progress);
        }
    }
}
