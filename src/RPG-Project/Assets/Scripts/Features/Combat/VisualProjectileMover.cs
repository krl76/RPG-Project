using UnityEngine;

namespace Features.Combat
{
    public sealed class VisualProjectileMover : MonoBehaviour
    {
        private Vector3 _moveDirection;
        private float _speed;
        private float _lifetime;

        public void Setup(Vector3 moveDirection, float speed, float lifetime)
        {
            _moveDirection = moveDirection.sqrMagnitude > 0.0001f
                ? moveDirection.normalized
                : transform.forward;
            _speed = Mathf.Max(0f, speed);
            _lifetime = Mathf.Max(0.05f, lifetime);
            Destroy(gameObject, _lifetime);
        }

        private void Update()
        {
            transform.position += _moveDirection * (_speed * Time.deltaTime);
        }
    }
}
