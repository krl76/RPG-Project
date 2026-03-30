using UnityEngine;
using UnityEngine.Rendering;

namespace Features.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    /// <summary>
    /// Простой боевой снаряд, который летит вперёд и наносит урон при столкновении.
    /// </summary>
    public class MagicProjectile : MonoBehaviour
    {
        private const float FallbackCollisionRadius = 0.05f;

        private float _speed;
        private float _damage;
        private Vector3 _moveDirection;
        private bool _hasCustomMoveDirection;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private LayerMask _targetLayer = ~0;
        [SerializeField] private LayerMask _selfLayer = 0;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _collisionTrigger;
        private bool _isMoving;
        private float _cachedCollisionRadius = -1f;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[8];

        private void Awake()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            if (_collisionTrigger == null)
            {
                _collisionTrigger = GetComponent<Collider>();
            }
        }

        private void OnTriggerEnter(Collider other)
        { 
            int otherLayer = other.gameObject.layer;
            if (((1 << otherLayer) & _selfLayer.value) != 0)
            {
                return;
            }

            IDamageable damageable = other.GetComponentInChildren<IDamageable>();
            if (damageable == null)
            {
                damageable = other.GetComponent<IDamageable>();
                if (damageable == null)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            if (((1 << otherLayer) & _targetLayer.value) == 0)
            {
                Destroy(gameObject);
                return;
            }

            damageable.TakeDamage(_damage);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (_isMoving == false)
            {
                return;
            }

            Vector3 moveDirection = _hasCustomMoveDirection ? _moveDirection : transform.forward;
            float travelDistance = _speed * Time.deltaTime;
            if (travelDistance <= 0f)
            {
                return;
            }

            Vector3 currentPosition = transform.position;

            transform.position = currentPosition + moveDirection * travelDistance;
        }

        public void Setup(float damage, float speed, float downwardTiltAngle = 0)
        {
            _damage = damage;
            _speed = speed;

            if (downwardTiltAngle != 0)
                transform.Rotate(Vector3.right, downwardTiltAngle);

            _isMoving = true;

            _rigidbody.isKinematic = true;
            Destroy(gameObject, _lifetime);
        }

        public void SetMoveDirection(Vector3 moveDirection)
        {
            if (moveDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            _moveDirection = moveDirection.normalized;
            _hasCustomMoveDirection = true;
        }
    }
}
