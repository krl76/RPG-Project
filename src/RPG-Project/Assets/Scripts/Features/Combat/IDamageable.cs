using System;
using UnityEngine;

namespace Features.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(float amount);
    }
}