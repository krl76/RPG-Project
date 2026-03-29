namespace Features.Enemy.States
{
    public sealed class BossEnemyEnrageState : EnemyStateBase
    {
        public BossEnemyEnrageState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Enrage;

        public override void Enter()
        {
            Enemy.StopMovement();
            Enemy.StartEnrageAction();
        }

        public override void Tick()
        {
            Enemy.LookAtTarget();

            if (Enemy.IsActionInProgress || Enemy.IsBossActionAnimationStillPlaying())
            {
                return;
            }

            StateMachine.Enter(EnemyStateId.Chase);
        }
    }
}
