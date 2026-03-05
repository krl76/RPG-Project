using System;
using UnityEngine;

namespace Features.Combat
{
    public interface IDamageable
    {
        event Action OnHealthChanged;
        bool IsAlive { get; }
        void TakeDamage(float amount);
    }
}