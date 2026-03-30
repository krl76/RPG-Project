namespace Features.Enemy.States
{
    /// <summary>
    /// Состояние отступления врага при низком здоровье.
    /// </summary>
    public sealed class EnemyFleeState : EnemyStateBase
    {
        public EnemyFleeState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Flee;

        public override void Enter()
        {
            Enemy.MoveAwayFromTarget();
        }

        public override void Tick()
        {
            Enemy.MoveAwayFromTarget();

            if (Enemy.HasReachedSafeDistance())
            {
                Enemy.ClearProvocation();
                StateMachine.Enter(EnemyStateId.Rest);
            }
        }
    }
}
