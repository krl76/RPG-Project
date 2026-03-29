namespace Infrastructure.Services.Audio
{
    public interface ICombatAudioService
    {
        void PlayPlayerMeleeAttack();
        void PlayPlayerShot();
        void PlayPlayerHit();
    }
}
