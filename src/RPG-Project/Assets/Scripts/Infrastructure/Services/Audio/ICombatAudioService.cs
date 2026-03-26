namespace Infrastructure.Services.Audio
{
    public interface ICombatAudioService
    {
        void PlayPlayerMeleeAttack();
        void PlayPlayerShot();
        void PlayPlayerHit();
        void PlayEnemyMeleeAttack();
        void PlayEnemyMagicAttack();
        void PlayEnemyHit();
    }
}
