namespace Infrastructure.Services.Player.Input
{
    /// <summary>
    /// Контракт боевого ввода и отката магии игрока.
    /// </summary>
    public interface IFightInputService
    {
        float MagicCooldownRemaining { get; }
        float MagicCooldownDuration { get; }
        void InstallService();
        void UninstallService();
        void AttackEnd();
        void RestoreMagicCooldown(float remainingTime, float totalDuration);
    }
}
