using UnityEngine;

namespace Features.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public class MagicProjectile : MonoBehaviour
    {
        private float _speed;
        private float _damage;
        private Vector3 _moveDirection;
        private bool _hasCustomMoveDirection;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private LayerMask _targetLayer = ~0;
        [SerializeField] private Rigidbody _rigidbody;
        private bool _isMoving;

        private void OnTriggerEnter(Collider other)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                return;
            }

            int targetLayer = ResolveTargetLayer(other, damageable);
            if (((1 << targetLayer) & _targetLayer.value) == 0)
            {
                return;
            }

            damageable.TakeDamage(_damage);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (_isMoving)
            {
                Vector3 moveDirection = _hasCustomMoveDirection ? _moveDirection : transform.forward;
                transform.position += moveDirection * (_speed * Time.deltaTime);
            }
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

        private static int ResolveTargetLayer(Collider other, IDamageable damageable)
        {
            if (damageable is Component damageableComponent)
            {
                return damageableComponent.gameObject.layer;
            }

            Transform root = other.transform.root;
            return root != null ? root.gameObject.layer : other.gameObject.layer;
        }
    }
}
