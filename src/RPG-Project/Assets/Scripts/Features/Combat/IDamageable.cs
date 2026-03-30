using System;
using UnityEngine;

namespace Features.Combat
{
    /// <summary>
    /// Контракт объекта, который может получать урон.
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(float amount);
    }
}
