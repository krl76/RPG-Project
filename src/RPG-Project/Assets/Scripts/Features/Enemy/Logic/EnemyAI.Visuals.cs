using System.Collections.Generic;
using Core.Gameplay.Save.Data;
using Data.Configs;
using UnityEngine;

namespace Features.Enemy
{
    public partial class EnemyAI
    {
        private static readonly Dictionary<string, int> LastVariationIndexByKey = new();

        private int _selectedRegularVariationIndex = -1;
        private int _selectedBossElementIndex = -1;

        private GameObject _activeWeaponEffect;
        private BossElementVariation _selectedBossElementVariation;

        private void InitializeVisualVariation()
        {
            if (Config == null)
            {
                return;
            }

            if (Config.BehaviourType == EnemyBehaviourType.Boss)
            {
                ApplyBossElementVariation(
                    ChooseNonRepeatingVariationIndex(GetBossVariationKey(), Config.BossElementVariations.Count),
                    rememberChoice: true);
                return;
            }

            ApplyRegularVariation(
                ChooseNonRepeatingVariationIndex(GetRegularVariationKey(), GetRegularVariationCount()),
                rememberChoice: true);
        }

        private void AdvanceRegularAttackVariation()
        {
            int variationCount = GetRegularVariationCount();
            if (variationCount <= 1)
            {
                return;
            }

            ApplyRegularVariation(
                ChooseNextNonRepeatingIndex(_selectedRegularVariationIndex, variationCount),
                rememberChoice: false);
        }

        private void AdvanceBossAttackVariation()
        {
            int variationCount = Config.BossElementVariations.Count;
            if (variationCount <= 1)
            {
                return;
            }

            ApplyBossElementVariation(
                ChooseNextNonRepeatingIndex(_selectedBossElementIndex, variationCount),
                rememberChoice: false);
        }

        private void RestoreVisualVariation(EnemySaveData data)
        {
            if (data == null)
            {
                return;
            }

            if (Config.BehaviourType == EnemyBehaviourType.Boss)
            {
                if (data.HasSelectedBossElement)
                {
                    ApplyBossElementVariation(data.SelectedBossElementIndex, rememberChoice: false);
                }

                return;
            }

            if (data.HasSelectedRegularVariation)
            {
                ApplyRegularVariation(data.SelectedRegularVariationIndex, rememberChoice: false);
            }
        }

        private void ApplyRegularVariation(int index, bool rememberChoice)
        {
            _selectedRegularVariationIndex = NormalizeVariationIndex(index, GetRegularVariationCount());
            if (rememberChoice && _selectedRegularVariationIndex >= 0)
            {
                LastVariationIndexByKey[GetRegularVariationKey()] = _selectedRegularVariationIndex;
            }

            RebuildWeaponEffectVisual();
        }

        private void ApplyBossElementVariation(int index, bool rememberChoice)
        {
            _selectedBossElementIndex = NormalizeVariationIndex(index, Config.BossElementVariations.Count);
            _selectedBossElementVariation = _selectedBossElementIndex >= 0
                ? Config.BossElementVariations[_selectedBossElementIndex]
                : null;

            if (rememberChoice && _selectedBossElementIndex >= 0)
            {
                LastVariationIndexByKey[GetBossVariationKey()] = _selectedBossElementIndex;
            }

            RebuildWeaponEffectVisual();
        }

        private void RebuildWeaponEffectVisual()
        {
            DestroyActiveWeaponEffect();

            GameObject weaponEffectPrefab = GetActiveWeaponEffectPrefab();
            Transform attachPoint = _meleeAttackPoint != null ? _meleeAttackPoint : transform;
            if (weaponEffectPrefab == null || attachPoint == null)
            {
                return;
            }

            _activeWeaponEffect = _gameObjectFactory != null
                ? _gameObjectFactory.Instantiate(
                    weaponEffectPrefab,
                    attachPoint.position,
                    attachPoint.rotation,
                    attachPoint)
                : Instantiate(weaponEffectPrefab, attachPoint.position, attachPoint.rotation, attachPoint);

            _activeWeaponEffect.transform.localPosition = Vector3.zero;
            _activeWeaponEffect.transform.localRotation = Quaternion.identity;
        }

        private void DestroyActiveWeaponEffect()
        {
            if (_activeWeaponEffect == null)
            {
                return;
            }

            if (_gameObjectFactory != null)
            {
                _gameObjectFactory.Destroy(_activeWeaponEffect);
            }
            else
            {
                Destroy(_activeWeaponEffect);
            }

            _activeWeaponEffect = null;
        }

        private int GetRegularVariationCount()
        {
            return Config.Type == EnemyType.Melee
                ? Config.MeleeWeaponEffectPrefabs.Count
                : GetRegularRangedVariationCount();
        }

        private string GetRegularVariationKey() =>
            $"regular:{Config.Id}:{Config.Type}";

        private string GetBossVariationKey() =>
            $"boss:{Config.Id}:element";

        private static int ChooseNonRepeatingVariationIndex(string key, int count)
        {
            if (count <= 0)
            {
                return -1;
            }

            if (count == 1)
            {
                LastVariationIndexByKey[key] = 0;
                return 0;
            }

            if (LastVariationIndexByKey.TryGetValue(key, out int lastIndex) == false
                || lastIndex < 0
                || lastIndex >= count)
            {
                int firstIndex = Random.Range(0, count);
                LastVariationIndexByKey[key] = firstIndex;
                return firstIndex;
            }

            int selectedIndex = Random.Range(0, count - 1);
            if (selectedIndex >= lastIndex)
            {
                selectedIndex++;
            }

            LastVariationIndexByKey[key] = selectedIndex;
            return selectedIndex;
        }

        private static int NormalizeVariationIndex(int index, int count)
        {
            return index >= 0 && index < count ? index : -1;
        }

        private static int ChooseNextNonRepeatingIndex(int previousIndex, int count)
        {
            if (count <= 0)
            {
                return -1;
            }

            if (count == 1)
            {
                return 0;
            }

            int normalizedPreviousIndex = NormalizeVariationIndex(previousIndex, count);
            if (normalizedPreviousIndex < 0)
            {
                return Random.Range(0, count);
            }

            int selectedIndex = Random.Range(0, count - 1);
            if (selectedIndex >= normalizedPreviousIndex)
            {
                selectedIndex++;
            }

            return selectedIndex;
        }

        private GameObject GetSelectedRegularProjectilePrefab()
        {
            if (Config.Type != EnemyType.Ranged
                || _selectedRegularVariationIndex < 0
                || _selectedRegularVariationIndex >= Config.RangedProjectileVisualPrefabs.Count)
            {
                return null;
            }

            return Config.RangedProjectileVisualPrefabs[_selectedRegularVariationIndex];
        }

        private GameObject GetActivePrimaryProjectilePrefab()
        {
            if (Config.Type == EnemyType.Ranged)
            {
                return GetSelectedRegularProjectilePrefab();
            }

            return null;
        }

        private GameObject GetStrongAttackProjectilePrefab()
        {
            GameObject sustainedProjectilePrefab = GetSustainedProjectilePrefab();
            if (sustainedProjectilePrefab != null)
            {
                return sustainedProjectilePrefab;
            }

            return GetActivePrimaryProjectilePrefab();
        }

        private GameObject GetActiveWeaponEffectPrefab()
        {
            if (Config.BehaviourType == EnemyBehaviourType.Boss)
            {
                return _selectedBossElementVariation?.MeleeWeaponEffectPrefab;
            }

            if (Config.Type != EnemyType.Melee || _selectedRegularVariationIndex < 0)
            {
                return null;
            }

            return _selectedRegularVariationIndex < Config.MeleeWeaponEffectPrefabs.Count
                ? Config.MeleeWeaponEffectPrefabs[_selectedRegularVariationIndex]
                : null;
        }

        private GameObject GetActiveSustainedAttackEffectPrefab()
        {
            return _selectedBossElementVariation?.SustainedAttackEffectPrefab;
        }

        private Vector3 GetActiveSustainedProjectileRotationOffset()
        {
            return _selectedBossElementVariation != null
                ? _selectedBossElementVariation.SustainedAttackProjectileRotationOffset
                : Vector3.zero;
        }

        private int GetRegularRangedVariationCount()
        {
            return Config.RangedProjectileVisualPrefabs.Count;
        }
    }
}
