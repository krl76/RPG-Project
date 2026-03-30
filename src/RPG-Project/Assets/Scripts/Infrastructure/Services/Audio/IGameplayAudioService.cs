namespace Infrastructure.Services.Audio
{
    /// <summary>
    /// Контракт звуков прогрессии и победы.
    /// </summary>
    public interface IGameplayAudioService
    {
        void PlayScoreGained();
        void PlayVictoryMusic();
        void StopVictoryMusic();
    }
}
