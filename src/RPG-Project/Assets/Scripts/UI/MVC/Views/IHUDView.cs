namespace UI.MVC.Views
{
    /// <summary>
    /// Контракт HUD для отображения здоровья, отката магии и очков.
    /// </summary>
    public interface IHUDView
    {
        void SetHealth(float currentHealth, float maxHealth);
        void SetMagicCooldown(float remainingTime, float totalDuration);
        void CompleteMagicCooldown();
        void SetScore(int currentScore, bool animated);
    }
}
