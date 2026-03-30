namespace Features.Enemy.States
{
    /// <summary>
    /// Основное боевое состояние босса с выбором дальнейшей атаки.
    /// </summary>
    public sealed class BossEnemyChaseState : EnemyStateBase
    {
        public BossEnemyChaseState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Chase;

        public override void Enter()
        {
        }

        public override void Tick()
        {
            if (Enemy.ShouldStayInBossFight() == false)
            {
                Enemy.ClearProvocation();
                StateMachine.Enter(EnemyStateId.Rest);
                return;
            }

            if (Enemy.ShouldStartEnrage())
            {
                StateMachine.Enter(EnemyStateId.Enrage);
                return;
            }

            if (Enemy.ShouldPauseBossOffense())
            {
                Enemy.StopMovement();
                Enemy.LookAtTarget();
                return;
            }

            if (Enemy.CanStartAirAttack())
            {
                StateMachine.Enter(EnemyStateId.AirAttack);
                return;
            }

            if (Enemy.IsPlayerInStrongAttackRange() && Enemy.CanStartStrongAttack())
            {
                StateMachine.Enter(EnemyStateId.StrongAttack);
                return;
            }

            if (Enemy.IsPlayerInAttackRange() && Enemy.CanStartPrimaryAttack())
            {
                StateMachine.Enter(EnemyStateId.Attack);
                return;
            }

            if (Enemy.IsPlayerInAttackRange())
            {
                Enemy.StopMovement();
                Enemy.LookAtTarget();
                return;
            }

            Enemy.MoveTowardsTarget(Enemy.GetBossChaseSpeedMultiplier());
        }
    }
}
