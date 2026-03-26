namespace UI.MVC.Views
{
    public interface IHUDView
    {
        void SetHealth(float currentHealth, float maxHealth);
        void StartMagicCooldown(float cooldownDuration);
        void CompleteMagicCooldown();
    }
}
