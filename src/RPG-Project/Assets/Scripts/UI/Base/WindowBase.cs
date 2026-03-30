using Infrastructure.Services.UI;
using UnityEngine;
using Zenject;

namespace UI.Base
{
    [RequireComponent(typeof(CanvasGroup))]
    /// <summary>
    /// Базовый класс для всех UI-окон проекта.
    /// </summary>
    public abstract class WindowBase : MonoBehaviour
    {
        public abstract WindowID Id { get; }
        public virtual bool IsPopup => false;

        private CanvasGroup _canvasGroup;

        [Inject] public virtual void Construct() { }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void OnOpen(object payload = null)
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            if (IsPopup) transform.SetAsLastSibling();
        }

        public virtual void OnClose()
        {
            gameObject.SetActive(false);
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
