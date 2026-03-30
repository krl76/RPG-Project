using Features.Combat;
using UnityEngine;

namespace Features.Enemy
{
    /// <summary>
    /// Эффект периодического урона по области для атак врага.
    /// </summary>
    public sealed class EnemyPersistentDamageEffect : MonoBehaviour
    {
        private float _damagePerTick;
        private float _tickInterval;
        private float _radius;
        private LayerMask _targetLayer;
        private float _nextDamageTime;

        public void Setup(float damagePerSecond, float tickInterval, float radius, LayerMask targetLayer)
        {
            _tickInterval = Mathf.Max(0.05f, tickInterval);
            _damagePerTick = Mathf.Max(0f, damagePerSecond) * _tickInterval;
            _radius = Mathf.Max(0f, radius);
            _targetLayer = targetLayer;
            _nextDamageTime = 0f;
        }

        private void Update()
        {
            if (Time.time < _nextDamageTime)
            {
                return;
            }

            _nextDamageTime = Time.time + _tickInterval;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _radius, _targetLayer);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<IDamageable>(out IDamageable victim) && victim.IsAlive)
                {
                    victim.TakeDamage(_damagePerTick);
                    break;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
