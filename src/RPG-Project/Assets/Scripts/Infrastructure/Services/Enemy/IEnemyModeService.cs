namespace Infrastructure.Services.Enemy
{
    public interface IEnemyModeService
    {
        bool IsPeacefulModeEnabled { get; }
        void SetPeacefulMode(bool isEnabled);
    }
}
