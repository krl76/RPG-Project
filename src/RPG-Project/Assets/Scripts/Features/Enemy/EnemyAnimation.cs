using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features.Enemy
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimation : MonoBehaviour
    {
        private static readonly int MoveSpeedHash = Animator.StringToHash(AnimationID.WALK_SPEED);
        private static readonly int RunSpeedHash = Animator.StringToHash(AnimationID.RUN_SPEED);
        private static readonly int AttackSpeedHash = Animator.StringToHash(AnimationID.ATTACK_SPEED);
        
        private static readonly int WalkHash = Animator.StringToHash(AnimationID.WALK);
        private static readonly int RunHash = Animator.StringToHash(AnimationID.RUN);
        private static readonly int IdleHash = Animator.StringToHash(AnimationID.IDLE);
        private static readonly int AttackHash = Animator.StringToHash(AnimationID.ATTACK);
        private static readonly int MagicAttackHash = Animator.StringToHash(AnimationID.MAGIC_ATTACK);
        private static readonly int HitHash = Animator.StringToHash(AnimationID.HIT);
        private static readonly int DeathHash = Animator.StringToHash(AnimationID.DEATH);

        public event Action OnAttackReachEnd;
        public event Action OnAttackExit;

        private Animator Animator { get; set; }
        private Dictionary<string, AnimationStateMachine> States { get; set; }

        private void Awake() => 
            Initialize();

        private void OnDestroy()
        {
            if (States == null)
                return;
            if (States.TryGetValue(AnimationID.ATTACK, out var state))
            {
                state.OnExit -= AttackAnimationExit;
                state.OnReachEnd -= AttackAnimationReachEnd;
            }
        }

        private void Initialize()
        {
            Animator = GetComponent<Animator>();
            var stateMachineBehaviours = Animator.GetBehaviours<AnimationStateMachine>();
            States = stateMachineBehaviours.ToDictionary(x => x.StateName, x => x);
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            if (States.TryGetValue(AnimationID.ATTACK, out var state))
            {
                state.OnExit += AttackAnimationExit;
                state.OnReachEnd += AttackAnimationReachEnd;
            }
        }

        public void SetIsWalking(bool isWalking)
        {
            Animator.SetBool(WalkHash, isWalking);
        }
        
        public void SetIsRunning(bool isRunning)
        {
            Animator.SetBool(RunHash, isRunning);
        }
        
        public void PlayIdle()
        {
            Animator.SetTrigger(IdleHash);
        }

        public void PlayAttack()
        {
            Animator.SetTrigger(AttackHash);
        }
        
        public void PlayMagicAttack()
        {
            Animator.SetTrigger(MagicAttackHash);
        }

        public void PlayHit()
        {
            Animator.SetTrigger(HitHash);
        }

        public void PlayDeath()
        {
            Animator.SetTrigger(DeathHash);
        }

        public void SetWalkSpeed(float speed)
        {
            Animator.SetFloat(MoveSpeedHash, speed);
        }
        
        public void SetRunSpeed(float speed)
        {
            Animator.SetFloat(RunSpeedHash, speed);
        }
        
        public void SetAttackSpeed(float speed)
        {
            Animator.SetFloat(AttackSpeedHash, speed);
        }

        public void ResetIdleTrigger()
        {
            Animator.ResetTrigger(IdleHash);
        }
        
        public void ResetAttackTrigger()
        {
            Animator.ResetTrigger(AttackHash);
        }
        
        public void ResetRunTrigger()
        {
            Animator.ResetTrigger(RunSpeedHash);
        }
        
        public void ResetWalkTrigger()
        {
            Animator.ResetTrigger(WalkHash);
        }
        
        private void AttackAnimationExit()
        {
            OnAttackExit?.Invoke();
        }
        
        private void AttackAnimationReachEnd()
        {
            OnAttackReachEnd?.Invoke();
        }
    }
}