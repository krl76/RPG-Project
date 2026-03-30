namespace Features.Enemy.States
{
    /// <summary>
    /// Универсальное состояние выполнения обычной или сильной атаки врага.
    /// </summary>
    public sealed class EnemyAttackState : EnemyStateBase
    {
        private readonly EnemyActionType _actionType;
        private readonly EnemyStateId _nextStateId;

        public EnemyAttackState(
            EnemyAI enemy,
            EnemyStateMachine stateMachine,
            EnemyActionType actionType,
            EnemyStateId nextStateId) : base(enemy, stateMachine)
        {
            _actionType = actionType;
            _nextStateId = nextStateId;
        }

        public override EnemyStateId Id =>
            _actionType == EnemyActionType.StrongAttack ? EnemyStateId.StrongAttack : EnemyStateId.Attack;

        public override void Enter()
        {
            if (_actionType == EnemyActionType.StrongAttack)
            {
                if (Enemy.CanStartStrongAttack() == false)
                {
                    StateMachine.Enter(_nextStateId);
                    return;
                }

                Enemy.StartStrongAttack();
                return;
            }

            if (Enemy.CanStartPrimaryAttack() == false)
            {
                StateMachine.Enter(_nextStateId);
                return;
            }

            Enemy.StartPrimaryAttack();
        }

        public override void Tick()
        {
            Enemy.LookAtTarget();

            if (Enemy.IsActionInProgress || Enemy.IsBossActionAnimationStillPlaying())
            {
                return;
            }

            if (Enemy.ShouldFlee())
            {
                StateMachine.Enter(EnemyStateId.Flee);
                return;
            }

            StateMachine.Enter(_nextStateId);
        }
    }
}
