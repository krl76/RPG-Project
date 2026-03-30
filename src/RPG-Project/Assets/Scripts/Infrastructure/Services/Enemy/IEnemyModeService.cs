namespace Infrastructure.Services.Enemy
{
    /// <summary>
    /// Контракт переключения мирного режима врагов.
    /// </summary>
    public interface IEnemyModeService
    {
        bool IsPeacefulModeEnabled { get; }
        void SetPeacefulMode(bool isEnabled);
    }
}
