namespace Features.Enemy.States
{
    public sealed class BossEnemyAirAttackState : EnemyStateBase
    {
        public BossEnemyAirAttackState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.AirAttack;

        public override void Enter()
        {
            if (Enemy.CanStartAirAttack() == false)
            {
                StateMachine.Enter(EnemyStateId.Chase);
                return;
            }

            Enemy.StartAirAttack();
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
