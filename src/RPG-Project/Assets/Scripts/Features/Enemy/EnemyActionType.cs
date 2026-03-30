namespace Features.Enemy
{
    /// <summary>
    /// Типы действий врага, используемые в логике состояний и анимаций.
    /// </summary>
    public enum EnemyActionType
    {
        None = 0,
        Aggression = 1,
        Attack = 2,
        StrongAttack = 3,
        AirAttack = 4,
        Enrage = 5,
        Hit = 6,
        Death = 7
    }
}
