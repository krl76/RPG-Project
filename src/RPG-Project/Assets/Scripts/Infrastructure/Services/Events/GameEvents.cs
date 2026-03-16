namespace Infrastructure.Services.Events
{
    public interface IPlayerHealthSubscriber : IGlobalSubscriber
    {
        void OnPlayerHealthChanged(float currentHealth, float maxHealth);
        void OnPlayerDied();
    }

    public interface IPlayerMagicSubscriber : IGlobalSubscriber
    {
        void OnMagicUsed(float cooldownDuration);
        void OnMagicReady();
    }
    
    public interface IGameStateSubscriber : IGlobalSubscriber
    {
        void OnGameOver();
        void OnGameRestarted();
    }
}