using System;
using System.Collections.Generic;
using System.Linq;
using Core.Gameplay.Save.Data;
using Features.Enemy;
using Features.Player;
using Infrastructure.Repositories.Save;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Enemy;
using Infrastructure.Services.Player;
using Infrastructure.Services.Player.Input;
using UnityEngine;

namespace Core.Gameplay.Save
{
    public sealed class GameSaveInteractor : IGameSaveInteractor
    {
        private readonly IGameSaveRepository _gameSaveRepository;
        private readonly IPlayerService _playerService;
        private readonly IEnemyService _enemyService;
        private readonly ICameraService _cameraService;
        private readonly IFightInputService _fightInputService;

        private GameSaveData _pendingRestoreData;

        public GameSaveInteractor(
            IGameSaveRepository gameSaveRepository,
            IPlayerService playerService,
            IEnemyService enemyService,
            ICameraService cameraService,
            IFightInputService fightInputService)
        {
            _gameSaveRepository = gameSaveRepository;
            _playerService = playerService;
            _enemyService = enemyService;
            _cameraService = cameraService;
            _fightInputService = fightInputService;
        }

        public bool SaveGame()
        {
            if (_playerService.PlayerObject == null)
            {
                Debug.LogWarning("[GameSaveInteractor] Save skipped because player is not initialized.");
                return false;
            }

            var playerHealth = _playerService.PlayerObject.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogWarning("[GameSaveInteractor] Save skipped because PlayerHealth is missing.");
                return false;
            }

            Transform playerTransform = _playerService.PlayerTransform;
            var saveData = new GameSaveData
            {
                SavedAtUtc = DateTime.UtcNow.ToString("O"),
                Player = new PlayerSaveData
                {
                    Position = Vector3SaveData.FromVector3(playerTransform.position),
                    Rotation = Vector3SaveData.FromVector3(playerTransform.eulerAngles),
                    CameraAngles = Vector2SaveData.FromVector2(_cameraService.GetCameraAngle()),
                    CurrentHealth = playerHealth.CurrentHealth,
                    MaxHealth = playerHealth.MaxHealth,
                    MagicCooldownRemaining = _fightInputService.MagicCooldownRemaining,
                    MagicCooldownDuration = _fightInputService.MagicCooldownDuration
                },
                Enemies = _enemyService.CaptureSaveData().ToList()
            };

            _gameSaveRepository.Save(saveData);
            return true;
        }

        public bool PrepareLoadGame()
        {
            if (_gameSaveRepository.TryLoad(out var saveData) == false)
            {
                Debug.LogWarning("[GameSaveInteractor] Saved game was not found or could not be read.");
                return false;
            }

            _pendingRestoreData = saveData;
            return true;
        }

        public void ApplyPendingGameState()
        {
            if (_pendingRestoreData == null)
            {
                return;
            }

            ApplyPlayerState(_pendingRestoreData.Player);
            ApplyEnemyStates(_pendingRestoreData.Enemies);
            _pendingRestoreData = null;
        }

        public void ClearPendingRestore()
        {
            _pendingRestoreData = null;
        }

        public bool HasSave() => _gameSaveRepository.HasSave();

        private void ApplyPlayerState(PlayerSaveData playerSaveData)
        {
            if (playerSaveData == null || _playerService.PlayerObject == null)
            {
                return;
            }

            var playerObject = _playerService.PlayerObject;
            var playerMovement = playerObject.GetComponent<PlayerMovement>();
            var playerHealth = playerObject.GetComponent<PlayerHealth>();
            var rotation = Quaternion.Euler(playerSaveData.Rotation.ToVector3());

            if (playerMovement != null)
            {
                playerMovement.Warp(playerSaveData.Position.ToVector3(), rotation);
            }
            else
            {
                playerObject.transform.SetPositionAndRotation(playerSaveData.Position.ToVector3(), rotation);
            }

            playerHealth?.ApplySaveData(playerSaveData);
            _cameraService.SetCameraAngle(playerSaveData.CameraAngles.ToVector2());
            _fightInputService.RestoreMagicCooldown(
                playerSaveData.MagicCooldownRemaining,
                playerSaveData.MagicCooldownDuration);
        }

        private void ApplyEnemyStates(IEnumerable<EnemySaveData> enemySaveData)
        {
            if (enemySaveData == null)
            {
                return;
            }

            var dataById = enemySaveData
                .Where(data => data != null && string.IsNullOrWhiteSpace(data.Id) == false)
                .ToDictionary(data => data.Id, data => data);

            foreach (EnemyAI enemy in _enemyService.ActiveEnemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                if (dataById.TryGetValue(enemy.SaveId, out var saveData) == false)
                {
                    continue;
                }

                enemy.ApplySaveData(saveData);
            }
        }
    }
}
