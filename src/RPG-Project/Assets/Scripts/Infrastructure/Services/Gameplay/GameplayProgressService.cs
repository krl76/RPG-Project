using Core.Gameplay.Save.Data;
using Data.Configs;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Events;
using UnityEngine;

namespace Infrastructure.Services.Gameplay
{
    /// <summary>
    /// Считает очки, убийства и триггеры прогрессии матча.
    /// </summary>
    public sealed class GameplayProgressService : IGameplayProgressService
    {
        private readonly IConfigDataProvider _configDataProvider;
        private readonly IGameplayAudioService _gameplayAudioService;

        private bool _isVictoryTriggered;

        public GameplayProgressService(
            IConfigDataProvider configDataProvider,
            IGameplayAudioService gameplayAudioService)
        {
            _configDataProvider = configDataProvider;
            _gameplayAudioService = gameplayAudioService;
        }

        public int CurrentScore { get; private set; }
        public int RegularEnemiesKilled { get; private set; }
        public bool IsBossSpawnTriggered { get; private set; }

        public void RegisterEnemyKill(EnemyConfig enemyConfig)
        {
            if (enemyConfig == null)
            {
                return;
            }

            int scoreReward = Mathf.Max(0, enemyConfig.ScoreReward);
            if (scoreReward > 0)
            {
                CurrentScore += scoreReward;
                _gameplayAudioService.PlayScoreGained();
                PublishScoreChanged(animated: true);
            }

            if (enemyConfig.BehaviourType != EnemyBehaviourType.Regular)
            {
                return;
            }

            RegularEnemiesKilled++;
            TryTriggerBossSpawn();
            TryTriggerVictory();
        }

        public GameplayProgressSaveData CaptureSaveData() =>
            new GameplayProgressSaveData
            {
                CurrentScore = CurrentScore,
                RegularEnemiesKilled = RegularEnemiesKilled,
                BossSpawnTriggered = IsBossSpawnTriggered,
                VictoryTriggered = _isVictoryTriggered
            };

        public void RestoreProgress(GameplayProgressSaveData data)
        {
            CurrentScore = Mathf.Max(0, data?.CurrentScore ?? 0);
            RegularEnemiesKilled = Mathf.Max(0, data?.RegularEnemiesKilled ?? 0);
            IsBossSpawnTriggered = data?.BossSpawnTriggered ?? false;
            _isVictoryTriggered = data?.VictoryTriggered ?? false;

            PublishScoreChanged(animated: false);
        }

        public void ResetRuntimeData()
        {
            CurrentScore = 0;
            RegularEnemiesKilled = 0;
            IsBossSpawnTriggered = false;
            _isVictoryTriggered = false;
            _gameplayAudioService.StopVictoryMusic();
            PublishScoreChanged(animated: false);
        }

        private void TryTriggerBossSpawn()
        {
            int requiredKills = Mathf.Max(0, _configDataProvider.GetGameConfig()?.RegularEnemyKillsRequiredForBossSpawn ?? 0);
            if (IsBossSpawnTriggered || requiredKills <= 0 || RegularEnemiesKilled < requiredKills)
            {
                return;
            }

            IsBossSpawnTriggered = true;
            EventBus.RaiseEvent<IBossSpawnSubscriber>(sub => sub.OnBossSpawnRequested());
        }

        private void TryTriggerVictory()
        {
            int requiredKills = Mathf.Max(0, _configDataProvider.GetGameConfig()?.RegularEnemyKillsRequiredForVictory ?? 0);
            if (_isVictoryTriggered || requiredKills <= 0 || RegularEnemiesKilled < requiredKills)
            {
                return;
            }

            _isVictoryTriggered = true;
            _gameplayAudioService.PlayVictoryMusic();
        }

        private void PublishScoreChanged(bool animated)
        {
            EventBus.RaiseEvent<IScoreSubscriber>(sub => sub.OnScoreChanged(CurrentScore, animated));
        }
    }
}
