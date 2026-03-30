using UnityEngine;

namespace Features.Enemy
{
    [RequireComponent(typeof(EnemyAnimation))]
    /// <summary>
    /// Принимает animation events и передаёт их в систему анимации врага.
    /// </summary>
    public sealed class EnemyEventReader : MonoBehaviour
    {
        private EnemyAnimation _enemyAnimation;

        private void Awake()
        {
            _enemyAnimation = GetComponent<EnemyAnimation>();
        }

        public void OnAnimationEvent(string eventId)
        {
            _enemyAnimation.ProcessAnimationEvent(eventId);
        }
    }
}
