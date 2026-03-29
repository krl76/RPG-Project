namespace Features.Enemy.States
{
    public sealed class BossEnemyAggressionState : EnemyStateBase
    {
        public BossEnemyAggressionState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Aggression;

        public override void Enter()
        {
            Enemy.StopMovement();
            Enemy.StartAggressionAction();
        }

        public override void Tick()
        {
            Enemy.LookAtTarget();

            if (Enemy.IsActionInProgress || Enemy.IsBossActionAnimationStillPlaying())
            {
                return;
            }

            if (Enemy.ShouldStartEnrage())
            {
                StateMachine.Enter(EnemyStateId.Enrage);
                return;
            }

            StateMachine.Enter(EnemyStateId.Chase);
        }
    }
}
