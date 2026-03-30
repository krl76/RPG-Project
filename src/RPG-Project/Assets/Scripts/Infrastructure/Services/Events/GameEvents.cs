namespace Infrastructure.Services.Events
{
    /// <summary>
    /// Подписчик на события изменения здоровья игрока.
    /// </summary>
    public interface IPlayerHealthSubscriber : IGlobalSubscriber
    {
        void OnPlayerHealthChanged(float currentHealth, float maxHealth);
        void OnPlayerDied();
    }

    /// <summary>
    /// Подписчик на события использования магии игроком.
    /// </summary>
    public interface IPlayerMagicSubscriber : IGlobalSubscriber
    {
        void OnMagicUsed(float remainingTime, float totalDuration);
        void OnMagicReady();
    }

    /// <summary>
    /// Подписчик на изменения количества очков.
    /// </summary>
    public interface IScoreSubscriber : IGlobalSubscriber
    {
        void OnScoreChanged(int currentScore, bool animated);
    }

    /// <summary>
    /// Подписчик на запрос появления босса.
    /// </summary>
    public interface IBossSpawnSubscriber : IGlobalSubscriber
    {
        void OnBossSpawnRequested();
    }
}
