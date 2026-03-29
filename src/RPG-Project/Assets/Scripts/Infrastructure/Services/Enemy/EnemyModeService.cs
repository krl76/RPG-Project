using Infrastructure.Providers.Configs;

namespace Infrastructure.Services.Enemy
{
    public sealed class EnemyModeService : IEnemyModeService
    {
        private readonly IConfigDataProvider _configDataProvider;
        private bool _isPeacefulModeEnabled;
        private bool _isInitialized;

        public EnemyModeService(IConfigDataProvider configDataProvider)
        {
            _configDataProvider = configDataProvider;
        }

        public bool IsPeacefulModeEnabled
        {
            get
            {
                EnsureInitialized();
                return _isPeacefulModeEnabled;
            }
            private set => _isPeacefulModeEnabled = value;
        }

        public void SetPeacefulMode(bool isEnabled)
        {
            EnsureInitialized();
            IsPeacefulModeEnabled = isEnabled;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            IsPeacefulModeEnabled = _configDataProvider.GetGameConfig()?.PeacefulModeEnabled ?? false;
        }
    }
}
