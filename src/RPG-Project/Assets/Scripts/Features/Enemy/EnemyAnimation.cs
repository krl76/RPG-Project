using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features.Enemy
{
    [RequireComponent(typeof(Animator))]
    /// <summary>
    /// Адаптер работы с параметрами и событиями аниматора врага.
    /// </summary>
    public class EnemyAnimation : MonoBehaviour
    {
        private static readonly int WalkSpeedHash = Animator.StringToHash(AnimationID.WALK_SPEED);
        private static readonly int RunSpeedHash = Animator.StringToHash(AnimationID.RUN_SPEED);
        private static readonly int AttackSpeedHash = Animator.StringToHash(AnimationID.ATTACK_SPEED);

        private static readonly int WalkHash = Animator.StringToHash(AnimationID.WALK);
        private static readonly int RunHash = Animator.StringToHash(AnimationID.RUN);
        private static readonly int FleeHash = Animator.StringToHash(AnimationID.FLEE);

        private static readonly int AttackHash = Animator.StringToHash(AnimationID.ATTACK);
        private static readonly int MagicAttackHash = Animator.StringToHash(AnimationID.MAGIC_ATTACK);
        private static readonly int StrongAttackHash = Animator.StringToHash(AnimationID.STRONG_ATTACK);
        private static readonly int AirAttackHash = Animator.StringToHash(AnimationID.AIR_ATTACK);
        private static readonly int AggressionHash = Animator.StringToHash(AnimationID.AGGRESSION);
        private static readonly int EnrageHash = Animator.StringToHash(AnimationID.ENRAGE);
        private static readonly int HitHash = Animator.StringToHash(AnimationID.HIT);
        private static readonly int DeathHash = Animator.StringToHash(AnimationID.DEATH);

        public event Action OnAttackImpact;
        public event Action OnStrongAttackImpact;
        public event Action OnAirAttackImpact;
        public event Action OnAttackEffectCompleted;
        public event Action OnActionCompleted;

        private readonly int[] _idleStateHashes =
        {
            Animator.StringToHash("Base Layer.Idle01"),
            Animator.StringToHash("Base Layer.Idle"),
            Animator.StringToHash("Base Layer.Sleep")
        };
        private readonly int[] _idleShortStateHashes =
        {
            Animator.StringToHash("Idle01"),
            Animator.StringToHash("Idle"),
            Animator.StringToHash("Sleep")
        };

        private readonly int[] _runStateHashes =
        {
            Animator.StringToHash("Base Layer.Run"),
            Animator.StringToHash("Base Layer.Walk")
        };
        private readonly int[] _runShortStateHashes =
        {
            Animator.StringToHash("Run"),
            Animator.StringToHash("Walk")
        };

        private Animator _animator;
        private HashSet<int> _availableParameters;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void SetIsWalking(bool isWalking)
        {
            EnsureInitialized();
            SetBoolIfExists(WalkHash, isWalking);
        }

        public void SetIsRunning(bool isRunning)
        {
            EnsureInitialized();
            SetBoolIfExists(RunHash, isRunning);
        }

        public void SetIsFleeing(bool isFleeing)
        {
            EnsureInitialized();
            SetBoolIfExists(FleeHash, isFleeing);
        }

        public void PlayAttack(bool isRangedAttack)
        {
            EnsureInitialized();
            ResetActionTriggers();
            int preferredHash = isRangedAttack ? MagicAttackHash : AttackHash;
            int fallbackHash = isRangedAttack ? AttackHash : MagicAttackHash;

            if (SetTriggerIfExists(preferredHash))
            {
                return;
            }

            SetTriggerIfExists(fallbackHash);
        }

        public void PlayStrongAttack()
        {
            EnsureInitialized();
            ResetActionTriggers();
            if (SetTriggerIfExists(StrongAttackHash))
            {
                return;
            }

            if (SetTriggerIfExists(MagicAttackHash))
            {
                return;
            }

            SetTriggerIfExists(AttackHash);
        }

        public void PlayAirAttack()
        {
            EnsureInitialized();
            ResetActionTriggers();
            if (SetTriggerIfExists(AirAttackHash))
            {
                return;
            }

            if (SetTriggerIfExists(StrongAttackHash))
            {
                return;
            }

            SetTriggerIfExists(MagicAttackHash);
        }

        public void PlayAggression()
        {
            EnsureInitialized();
            ResetActionTriggers();
            SetTriggerIfExists(AggressionHash);
        }

        public void PlayEnrage()
        {
            EnsureInitialized();
            ResetActionTriggers();
            SetTriggerIfExists(EnrageHash);
        }

        public void PlayHit()
        {
            EnsureInitialized();
            ResetActionTriggers();
            SetTriggerIfExists(HitHash);
        }

        public void PlayDeath()
        {
            EnsureInitialized();
            ResetActionTriggers();
            SetTriggerIfExists(DeathHash);
        }

        public void SetWalkSpeed(float speed)
        {
            EnsureInitialized();
            SetFloatIfExists(WalkSpeedHash, speed);
        }

        public void SetRunSpeed(float speed)
        {
            EnsureInitialized();
            SetFloatIfExists(RunSpeedHash, speed);
        }

        public void SetAttackSpeed(float speed)
        {
            EnsureInitialized();
            SetFloatIfExists(AttackSpeedHash, speed);
        }

        public void ProcessAnimationEvent(string eventId)
        {
            switch (eventId)
            {
                case "AttackImpact":
                case "OnAttackImpact":
                    OnAttackImpact?.Invoke();
                    break;
                case "StrongAttackImpact":
                case "OnStrongAttackImpact":
                    OnStrongAttackImpact?.Invoke();
                    break;
                case "AirAttackImpact":
                case "OnAirAttackImpact":
                    OnAirAttackImpact?.Invoke();
                    break;
                case "AttackEffectCompleted":
                case "AttaclEffectCompleted":
                case "StrongAttackCompleted":
                case "AirAttackCompleted":
                case "OnAttackEffectCompleted":
                case "OnAttaclEffectCompleted":
                case "OnStrongAttackCompleted":
                case "OnAirAttackCompleted":
                    OnAttackEffectCompleted?.Invoke();
                    break;
                case "ActionCompleted":
                case "AggressionCompleted":
                case "EnrageCompleted":
                case "OnActionCompleted":
                case "OnAggressionCompleted":
                case "OnEnrageCompleted":
                    OnActionCompleted?.Invoke();
                    break;
            }
        }

        public void SyncLocomotionState(bool isMoving)
        {
            EnsureInitialized();

            if (UsesAnimatorDrivenLocomotion())
            {
                return;
            }

            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            if (ShouldDelayLocomotionSync(currentState))
            {
                return;
            }

            int[] shortStateHashes = isMoving ? _runShortStateHashes : _idleShortStateHashes;
            for (int i = 0; i < shortStateHashes.Length; i++)
            {
                if (currentState.shortNameHash == shortStateHashes[i])
                {
                    return;
                }
            }

            if (_animator.IsInTransition(0))
            {
                AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
                if (ShouldDelayLocomotionSync(nextState))
                {
                    return;
                }

                for (int i = 0; i < shortStateHashes.Length; i++)
                {
                    if (nextState.shortNameHash == shortStateHashes[i])
                    {
                        return;
                    }
                }
            }

            int[] stateHashes = isMoving ? _runStateHashes : _idleStateHashes;
            for (int i = 0; i < stateHashes.Length; i++)
            {
                int stateHash = stateHashes[i];
                if (_animator.HasState(0, stateHash) == false)
                {
                    continue;
                }

                _animator.CrossFade(stateHash, 0.12f, 0);
                return;
            }
        }

        private bool UsesAnimatorDrivenLocomotion()
        {
            return _availableParameters.Contains(RunHash)
                || _availableParameters.Contains(WalkHash);
        }

        private bool ShouldDelayLocomotionSync(AnimatorStateInfo stateInfo)
        {
            if (IsLocomotionState(stateInfo.shortNameHash))
            {
                return false;
            }

            return stateInfo.loop == false && stateInfo.normalizedTime < 0.98f;
        }

        private bool IsLocomotionState(int shortStateHash)
        {
            for (int i = 0; i < _idleShortStateHashes.Length; i++)
            {
                if (_idleShortStateHashes[i] == shortStateHash)
                {
                    return true;
                }
            }

            for (int i = 0; i < _runShortStateHashes.Length; i++)
            {
                if (_runShortStateHashes[i] == shortStateHash)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsCurrentStateOrTransitioningTo(string shortStateName)
        {
            EnsureInitialized();

            int stateHash = Animator.StringToHash(shortStateName);
            if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash)
            {
                return true;
            }

            return _animator.IsInTransition(0)
                && _animator.GetNextAnimatorStateInfo(0).shortNameHash == stateHash;
        }

        public bool IsAnyCurrentStateOrTransitioningTo(params string[] shortStateNames)
        {
            EnsureInitialized();

            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            for (int i = 0; i < shortStateNames.Length; i++)
            {
                if (currentState.shortNameHash == Animator.StringToHash(shortStateNames[i]))
                {
                    return true;
                }
            }

            if (_animator.IsInTransition(0) == false)
            {
                return false;
            }

            AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
            for (int i = 0; i < shortStateNames.Length; i++)
            {
                if (nextState.shortNameHash == Animator.StringToHash(shortStateNames[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureInitialized()
        {
            if (_animator != null && _availableParameters != null)
            {
                return;
            }

            _animator = GetComponent<Animator>();
            _availableParameters = new HashSet<int>(_animator.parameters.Select(parameter => parameter.nameHash));
        }

        private void SetBoolIfExists(int hash, bool value)
        {
            if (_availableParameters.Contains(hash))
            {
                _animator.SetBool(hash, value);
            }
        }

        private void SetFloatIfExists(int hash, float value)
        {
            if (_availableParameters.Contains(hash))
            {
                _animator.SetFloat(hash, value);
            }
        }

        private bool SetTriggerIfExists(int hash)
        {
            if (_availableParameters.Contains(hash) == false)
            {
                return false;
            }

            _animator.SetTrigger(hash);
            return true;
        }

        private void ResetActionTriggers()
        {
            ResetTriggerIfExists(AttackHash);
            ResetTriggerIfExists(MagicAttackHash);
            ResetTriggerIfExists(StrongAttackHash);
            ResetTriggerIfExists(AirAttackHash);
            ResetTriggerIfExists(AggressionHash);
            ResetTriggerIfExists(EnrageHash);
            ResetTriggerIfExists(HitHash);
        }

        private void ResetTriggerIfExists(int hash)
        {
            if (_availableParameters.Contains(hash))
            {
                _animator.ResetTrigger(hash);
            }
        }

    }
}
