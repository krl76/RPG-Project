namespace Features.Combat
{
    public interface IHealthFeedback
    {
        void OnHealthChanged(float current, float max);
    }
}