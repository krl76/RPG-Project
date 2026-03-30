namespace Features.Combat
{
    /// <summary>
    /// Контракт визуального отклика на изменение здоровья.
    /// </summary>
    public interface IHealthFeedback
    {
        void OnHealthChanged(float current, float max);
    }
}
