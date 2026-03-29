using UnityEngine;

namespace Features.Enemy
{
    [RequireComponent(typeof(EnemyAnimation))]
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
