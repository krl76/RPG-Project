namespace Features.Enemy.States
{
    public sealed class RegularEnemyAggressionState : EnemyStateBase
    {
        public RegularEnemyAggressionState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Aggression;

        public override void Enter()
        {
        }

        public override void Tick()
        {
            if (Enemy.ShouldFlee())
            {
                StateMachine.Enter(EnemyStateId.Flee);
                return;
            }

            if (Enemy.ShouldStayInRegularAggression() == false)
            {
                Enemy.ClearProvocation();
                StateMachine.Enter(EnemyStateId.Rest);
                return;
            }

            if (Enemy.IsPlayerInAttackRange())
            {
                Enemy.StopMovement();
                Enemy.LookAtTarget();

                if (Enemy.CanStartPrimaryAttack())
                {
                    StateMachine.Enter(EnemyStateId.Attack);
                }

                return;
            }

            Enemy.MoveTowardsTarget(Enemy.GetChaseSpeedMultiplier());
        }
    }
}
