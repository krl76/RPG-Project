namespace Infrastructure.Services.Audio
{
    /// <summary>
    /// Контракт звуков боевых действий игрока.
    /// </summary>
    public interface ICombatAudioService
    {
        void PlayPlayerMeleeAttack();
        void PlayPlayerShot();
        void PlayPlayerHit();
    }
}
