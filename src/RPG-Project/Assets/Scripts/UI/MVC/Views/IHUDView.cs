namespace UI.MVC.Views
{
    public interface IHUDView
    {
        void SetHealth(float currentHealth, float maxHealth);
        void SetMagicCooldown(float remainingTime, float totalDuration);
        void CompleteMagicCooldown();
    }
}
