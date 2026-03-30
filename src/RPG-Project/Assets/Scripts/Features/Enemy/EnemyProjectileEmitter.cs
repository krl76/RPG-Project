using Features.Combat;
using Infrastructure.Factories.Objects;
using UnityEngine;

namespace Features.Enemy
{
    /// <summary>
    /// Выпускает пачки снарядов или визуальных прожектайлов по цели.
    /// </summary>
    public sealed class EnemyProjectileEmitter : MonoBehaviour
    {
        private IGameObjectFactory _gameObjectFactory;
        private GameObject _projectilePrefab;
        private Transform _target;
        private CharacterController _targetCharacterController;
        private Collider _targetCollider;
        private float _projectileDamage;
        private float _projectileSpeed;
        private float _emissionInterval;
        private int _projectileCount;
        private float _spreadAngle;
        private float _downwardTiltAngle;
        private float _visualProjectileLifetime;
        private Vector3 _visualRotationOffset;
        private float _nextEmitTime;

        public void Setup(
            IGameObjectFactory gameObjectFactory,
            GameObject projectilePrefab,
            Transform target,
            float damagePerSecond,
            float projectileSpeed,
            float emissionInterval,
            int projectileCount = 1,
            float spreadAngle = 0f,
            float downwardTiltAngle = 0f,
            float visualProjectileLifetime = 0.75f,
            Vector3 visualRotationOffset = default)
        {
            _gameObjectFactory = gameObjectFactory;
            _projectilePrefab = projectilePrefab;
            _target = target;
            CacheTargetComponents();
            _projectileSpeed = Mathf.Max(0f, projectileSpeed);
            _emissionInterval = Mathf.Max(0.05f, emissionInterval);
            _projectileCount = Mathf.Max(1, projectileCount);
            _spreadAngle = Mathf.Max(0f, spreadAngle);
            _downwardTiltAngle = downwardTiltAngle;
            _visualProjectileLifetime = Mathf.Max(0.05f, visualProjectileLifetime);
            _visualRotationOffset = visualRotationOffset;
            _projectileDamage = Mathf.Max(0f, damagePerSecond) * _emissionInterval / _projectileCount;
            _nextEmitTime = 0f;
        }

        private void Update()
        {
            if (_projectilePrefab == null || _gameObjectFactory == null)
            {
                return;
            }

            if (Time.time < _nextEmitTime)
            {
                return;
            }

            _nextEmitTime = Time.time + _emissionInterval;
            EmitBurst();
        }

        private void EmitBurst()
        {
            float startAngle = -_spreadAngle * (_projectileCount - 1) * 0.5f;

            for (int i = 0; i < _projectileCount; i++)
            {
                float angleOffset = startAngle + _spreadAngle * i;
                Vector3 moveDirection = GetMoveDirection(angleOffset);
                Quaternion movementRotation = moveDirection.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(moveDirection, Vector3.up)
                    : transform.rotation;

                GameObject projectile = _gameObjectFactory.Instantiate(
                    _projectilePrefab,
                    transform.position,
                    movementRotation * Quaternion.Euler(_visualRotationOffset));

                if (projectile.TryGetComponent(out MagicProjectile magicProjectile))
                {
                    magicProjectile.SetMoveDirection(moveDirection);
                    magicProjectile.Setup(_projectileDamage, _projectileSpeed, _downwardTiltAngle);
                    continue;
                }

                if (projectile.TryGetComponent(out VisualProjectileMover visualProjectileMover) == false)
                {
                    visualProjectileMover = projectile.AddComponent<VisualProjectileMover>();
                }

                visualProjectileMover.Setup(moveDirection, _projectileSpeed, _visualProjectileLifetime);
            }
        }

        private Vector3 GetMoveDirection(float angleOffset)
        {
            Vector3 direction = _target != null
                ? (GetTargetAimPoint() - transform.position)
                : transform.forward;

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = transform.forward;
            }

            return Quaternion.Euler(0f, angleOffset, 0f) * direction.normalized;
        }

        private void CacheTargetComponents()
        {
            _targetCharacterController = null;
            _targetCollider = null;

            if (_target == null)
            {
                return;
            }

            Transform targetRoot = _target.root != null ? _target.root : _target;
            _targetCharacterController = targetRoot.GetComponentInChildren<CharacterController>();
            if (_targetCharacterController == null)
            {
                _targetCollider = targetRoot.GetComponentInChildren<Collider>();
            }
        }

        private Vector3 GetTargetAimPoint()
        {
            if (_targetCharacterController != null)
            {
                return _targetCharacterController.bounds.center;
            }

            if (_targetCollider != null)
            {
                return _targetCollider.bounds.center;
            }

            return _target != null ? _target.position : transform.position + transform.forward;
        }
    }
}
