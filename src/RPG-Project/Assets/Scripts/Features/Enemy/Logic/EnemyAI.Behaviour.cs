using Data.Configs;
using UnityEngine;

namespace Features.Enemy
{
    public partial class EnemyAI
    {
        public bool ShouldActivateRegularEnemy()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Regular || HasActiveTarget() == false)
            {
                return false;
            }

            if (_enemyModeService.IsPeacefulModeEnabled)
            {
                return false;
            }

            return _isProvoked || DistanceToTarget() <= Config.ChaseRange;
        }

        public bool ShouldActivateBoss()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss || HasActiveTarget() == false)
            {
                return false;
            }

            if (_isProvoked)
            {
                return true;
            }

            return _enemyModeService.IsPeacefulModeEnabled == false && DistanceToTarget() <= Config.ChaseRange;
        }

        public bool ShouldStayInRegularAggression()
        {
            if (HasActiveTarget() == false || _enemyModeService.IsPeacefulModeEnabled)
            {
                return false;
            }

            return _isProvoked || DistanceToTarget() <= GetDisengageRange();
        }

        public bool ShouldStayInBossFight()
        {
            if (HasActiveTarget() == false)
            {
                return false;
            }

            if (_isProvoked == false && _enemyModeService.IsPeacefulModeEnabled)
            {
                return false;
            }

            return _isProvoked || DistanceToTarget() <= GetDisengageRange();
        }

        public bool ShouldFlee()
        {
            return Config.BehaviourType == EnemyBehaviourType.Regular
                && HasActiveTarget()
                && DistanceToTarget() <= GetDisengageRange()
                && GetHealthRatio() <= Config.FleeHealthThreshold;
        }

        public bool ShouldStartEnrage()
        {
            return Config.BehaviourType == EnemyBehaviourType.Boss
                && _isProvoked
                && _isEnraged == false
                && GetHealthRatio() <= Config.EnrageHealthThreshold;
        }

        public bool IsPlayerInAttackRange()
        {
            return HasActiveTarget() && DistanceToTarget() <= Config.AttackRange;
        }

        public bool IsPlayerInStrongAttackRange()
        {
            return HasActiveTarget() && DistanceToTarget() <= Config.StrongAttackRange;
        }

        public bool IsPlayerInAirAttackRange()
        {
            return HasActiveTarget() && DistanceToTarget() <= Config.AirAttackRange;
        }

        public bool CanStartPrimaryAttack()
        {
            return HasActiveTarget()
                && IsActionInProgress == false
                && Time.time >= _nextPrimaryAttackTime
                && CanBossStartOffensiveAction()
                && IsPlayerInAttackRange();
        }

        public bool CanStartStrongAttack()
        {
            return Config.BehaviourType == EnemyBehaviourType.Boss
                && HasActiveTarget()
                && IsActionInProgress == false
                && Time.time >= _nextStrongAttackTime
                && CanBossStartOffensiveAction()
                && IsPlayerInAttackRange() == false
                && IsPlayerInStrongAttackRange();
        }

        public bool CanStartAirAttack()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss
                || HasActiveTarget() == false
                || IsActionInProgress
                || Time.time < _nextAirAttackTime
                || CanBossStartOffensiveAction() == false
                || IsPlayerInAirAttackRange() == false)
            {
                return false;
            }

            return DistanceToTarget() > Config.StrongAttackRange;
        }

        public float GetChaseSpeedMultiplier() => Mathf.Max(0.01f, Config.ChaseSpeedMultiplier);

        public float GetBossChaseSpeedMultiplier()
        {
            float speedMultiplier = Config.ChaseSpeedMultiplier;
            if (_isEnraged)
            {
                speedMultiplier *= Mathf.Max(1f, Config.EnragedSpeedMultiplier);
            }

            return Mathf.Max(0.01f, speedMultiplier);
        }

        public void ClearProvocation()
        {
            _isProvoked = false;
        }

        public bool ShouldPauseBossOffense()
        {
            return Config.BehaviourType == EnemyBehaviourType.Boss && Time.time < _bossAttackUnlockAt;
        }

        public bool IsBossActionAnimationStillPlaying()
        {
            return IsCurrentActionAnimationLocked();
        }

        private float GetDisengageRange() =>
            Mathf.Max(Config.ChaseRange, Config.DisengageRange);

        private float GetHealthRatio() =>
            Config.MaxHealth <= 0f ? 0f : _currentHealth / Config.MaxHealth;

        private float DistanceToTarget()
        {
            if (HasActiveTarget() == false)
            {
                return float.MaxValue;
            }

            return Vector3.Distance(transform.position, _playerTransform.position);
        }

        private bool HasActiveTarget()
        {
            return _playerTransform != null && (_playerDamageable == null || _playerDamageable.IsAlive);
        }
    }
}
