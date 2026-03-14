using UnityEngine;

namespace Features.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public class MagicProjectile : MonoBehaviour
    {
        private float _speed;
        private float _damage;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private Rigidbody _rigidbody;
        private bool _isMoving;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (_isMoving)
            {
                transform.position += transform.forward * (_speed * Time.deltaTime);
            }
        }

        public void Setup(float damage, float speed)
        {
            _damage = damage;
            _speed = speed;
            _isMoving = true;

            _rigidbody.isKinematic = true;
            Destroy(gameObject, _lifetime);
        }
    }
}