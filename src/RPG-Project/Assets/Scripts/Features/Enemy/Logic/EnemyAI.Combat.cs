using Data.Configs;
using Features.Combat;
using UnityEngine;

namespace Features.Enemy
{
    public partial class EnemyAI
    {
        public bool IsActionInProgress => _currentAction != EnemyActionType.None;

        private float _nextPrimaryAttackTime;
        private float _nextStrongAttackTime;
        private float _nextAirAttackTime;
        private float _actionTimeoutAt;
        private float _bossAttackUnlockAt;

        private bool _actionImpactConsumed;

        private EnemyActionType _currentAction = EnemyActionType.None;
        private GameObject _activeSustainedAttackEffect;

        public void StartAggressionAction()
        {
            StartAction(EnemyActionType.Aggression, Config.AggressionAnimationDuration);
            StopMovement();
            _enemyAnimation.PlayAggression();
            PlayAudioCue(Config.AggressionSound, 0, Config.BehaviourType == EnemyBehaviourType.Boss);
        }

        public void StartPrimaryAttack()
        {
            _nextPrimaryAttackTime = Time.time + Config.AttackCooldown;
            StartAction(EnemyActionType.Attack, Config.AttackAnimationDuration);
            StopMovement();
            _enemyAnimation.PlayAttack(IsUsingRangedAttack());
        }

        public void StartStrongAttack()
        {
            _nextStrongAttackTime = Time.time + Config.StrongAttackCooldown;
            StartAction(EnemyActionType.StrongAttack, Config.StrongAttackAnimationDuration);
            StopMovement();
            _enemyAnimation.PlayStrongAttack();
        }

        public void StartAirAttack()
        {
            _nextAirAttackTime = Time.time + Config.AirAttackCooldown;
            StartAction(EnemyActionType.AirAttack, Config.AirAttackAnimationDuration);
            StopMovement();
            _enemyAnimation.PlayAirAttack();
            PlayLoopingAudioCue(Config.AirAttackLoopSound, GetMagicAttackAudioVariationIndex());
        }

        public void StartEnrageAction()
        {
            _isEnraged = true;
            StartAction(EnemyActionType.Enrage, Config.EnrageAnimationDuration);
            StopMovement();
            _enemyAnimation.PlayEnrage();
        }

        private void StartAction(EnemyActionType actionType, float fallbackDuration)
        {
            _currentAction = actionType;
            _actionImpactConsumed = false;
            _actionTimeoutAt = Time.time + Mathf.Max(0.1f, fallbackDuration);
        }

        private void ResetActionState()
        {
            StopSustainedAttackEffect();
            StopStateAudio();
            StopLoopingStateAudio();
            _currentAction = EnemyActionType.None;
            _actionImpactConsumed = false;
            _actionTimeoutAt = 0f;
        }

        private void UpdateActionTimeout()
        {
            if (IsActionInProgress == false || Time.time < _actionTimeoutAt)
            {
                return;
            }

            if (IsCurrentActionAnimationLocked())
            {
                return;
            }

            CompleteAction(applyBossPostActionDelay: false);
        }

        private void OnActionCompleted()
        {
            if (_currentAction == EnemyActionType.AirAttack
                && _enemyAnimation.IsCurrentStateOrTransitioningTo("Land") == false)
            {
                return;
            }

            CompleteAction(applyBossPostActionDelay: true);
        }

        private void OnAttackEffectCompleted()
        {
            if (_currentAction != EnemyActionType.StrongAttack
                && _currentAction != EnemyActionType.AirAttack)
            {
                return;
            }

            StopSustainedAttackEffect();
            StopStateAudio();
        }

        private void OnAttackImpact()
        {
            if (_actionImpactConsumed)
            {
                return;
            }

            if (_currentAction == EnemyActionType.Attack)
            {
                ExecutePrimaryAttack();
                _actionImpactConsumed = true;
            }
        }

        private void OnStrongAttackImpact()
        {
            if (_actionImpactConsumed || _currentAction != EnemyActionType.StrongAttack)
            {
                return;
            }

            ExecuteStrongAttack();
            _actionImpactConsumed = true;
        }

        private void OnAirAttackImpact()
        {
            if (_actionImpactConsumed || _currentAction != EnemyActionType.AirAttack)
            {
                return;
            }

            ExecuteAirAttack();
            _actionImpactConsumed = true;
        }

        private void ExecutePrimaryAttack()
        {
            float damage = Config.Damage * GetCurrentDamageMultiplier();
            if (IsUsingRangedAttack())
            {
                ExecuteRangedAttack(
                    damage,
                    Config.ProjectileSpeed,
                    1,
                    0f,
                    GetActivePrimaryProjectilePrefab());
                return;
            }

            ExecuteMeleeAttack(damage, Config.HitRadius);
        }

        private void ExecuteStrongAttack()
        {
            float damage = Config.Damage * Config.StrongAttackDamageMultiplier * GetCurrentDamageMultiplier();
            EnemyAttackDeliveryType deliveryType = ResolveStrongAttackDeliveryType();
            GameObject strongAttackProjectilePrefab = GetStrongAttackProjectilePrefab();

            if (deliveryType == EnemyAttackDeliveryType.SustainedEffect)
            {
                StartSustainedAttackEffect(
                    damagePerSecond: damage,
                    radius: Config.StrongAttackEffectRadius,
                    emissionInterval: Config.StrongAttackDamageTickInterval,
                    projectileSpeed: Config.ProjectileSpeed * Config.StrongAttackProjectileSpeedMultiplier);
                return;
            }

            if (deliveryType == EnemyAttackDeliveryType.Ranged && strongAttackProjectilePrefab != null)
            {
                ExecuteRangedAttack(
                    damage,
                    Config.ProjectileSpeed * Config.StrongAttackProjectileSpeedMultiplier,
                    Mathf.Max(1, Config.StrongAttackProjectileCount),
                    Config.StrongAttackProjectileSpreadAngle,
                    strongAttackProjectilePrefab);
                return;
            }

            ExecuteMeleeAttack(damage, Config.HitRadius * Config.StrongAttackHitRadiusMultiplier);
        }

        private void ExecuteAirAttack()
        {
            float damage = Config.Damage * Config.AirAttackDamageMultiplier * GetCurrentDamageMultiplier();
            StartSustainedAttackEffect(
                damagePerSecond: damage,
                radius: Config.AirAttackEffectRadius,
                emissionInterval: Config.AirAttackDamageTickInterval,
                projectileSpeed: Config.ProjectileSpeed * Config.StrongAttackProjectileSpeedMultiplier);
        }

        private void ExecuteMeleeAttack(float damage, float radius)
        {
            bool shouldInterruptWithState = Config.BehaviourType == EnemyBehaviourType.Boss
                && _currentAction == EnemyActionType.Attack;

            if (Config.BehaviourType == EnemyBehaviourType.Boss)
            {
                PlayBossAttackAudioCue(Config.MeleeAttackSound, ref _lastBossMeleeAudioClipIndex, shouldInterruptWithState);
            }
            else
            {
                PlayAudioCue(Config.MeleeAttackSound, 0, shouldInterruptWithState);
            }

            if (TryGetDamageableInMeleeRadius(radius, out IDamageable victim))
            {
                victim.TakeDamage(damage);
            }
        }

        private void ExecuteRangedAttack(
            float damage,
            float projectileSpeed,
            int projectileCount,
            float spreadAngle,
            GameObject projectilePrefab)
        {
            PlayConfiguredMagicAttackSound();

            if (projectilePrefab == null)
            {
                return;
            }

            Transform origin = _shootPoint != null ? _shootPoint : transform;
            int safeProjectileCount = Mathf.Max(1, projectileCount);
            float startAngle = -spreadAngle * (safeProjectileCount - 1) * 0.5f;

            for (int i = 0; i < safeProjectileCount; i++)
            {
                float angleOffset = startAngle + spreadAngle * i;
                Vector3 moveDirection = GetRangedAttackDirection(origin, angleOffset);
                Quaternion rotation = moveDirection.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(moveDirection, Vector3.up)
                    : origin.rotation * Quaternion.Euler(0f, angleOffset, 0f);
                GameObject projectile = _gameObjectFactory.Instantiate(
                    projectilePrefab,
                    origin.position,
                    rotation);

                if (projectile.TryGetComponent(out MagicProjectile magicProjectile))
                {
                    magicProjectile.SetMoveDirection(moveDirection);
                    magicProjectile.Setup(damage, projectileSpeed);
                }
            }
        }

        private Vector3 GetRangedAttackDirection(Transform origin, float angleOffset)
        {
            Vector3 baseDirection;
            if (_playerTransform != null)
            {
                baseDirection = _playerTransform.position - origin.position;
                baseDirection.y = 0f;
            }
            else
            {
                baseDirection = origin.forward;
            }

            if (baseDirection.sqrMagnitude < 0.0001f)
            {
                baseDirection = origin.forward;
            }

            return Quaternion.Euler(0f, angleOffset, 0f) * baseDirection.normalized;
        }

        private float GetCurrentDamageMultiplier() =>
            _isEnraged ? Mathf.Max(1f, Config.EnragedDamageMultiplier) : 1f;

        private bool IsUsingRangedAttack()
        {
            return Config.Type == EnemyType.Ranged && GetActivePrimaryProjectilePrefab() != null;
        }

        private bool ShouldPlayHitAnimation()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return true;
            }

            return CurrentStateId == EnemyStateId.Chase && IsActionInProgress == false;
        }

        private bool IsBossActionAnimationLocked()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss || _enemyAnimation == null)
            {
                return false;
            }

            return _currentAction switch
            {
                EnemyActionType.Aggression => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Scream"),
                EnemyActionType.Attack => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Claw Attack"),
                EnemyActionType.StrongAttack => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Flame Attack"),
                EnemyActionType.AirAttack => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Take Off", "Fly Flame Attack", "Land"),
                EnemyActionType.Enrage => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Defend"),
                _ => false
            };
        }

        private bool IsCurrentActionAnimationLocked()
        {
            if (_enemyAnimation == null)
            {
                return false;
            }

            if (_currentAction == EnemyActionType.Attack)
            {
                return _enemyAnimation.IsAnyCurrentStateOrTransitioningTo(
                    "Attack",
                    "MagicAttack",
                    "Basic Attack",
                    "Claw Attack");
            }

            return IsBossActionAnimationLocked();
        }

        private float GetIncomingDamageMultiplier()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return 1f;
            }

            if (_isEnraged || GetHealthRatio() <= Config.EnrageHealthThreshold)
            {
                return Mathf.Clamp01(Config.DefendDamageTakenMultiplier);
            }

            return 1f;
        }

        private void ApplyBossPostActionDelay()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return;
            }

            switch (_currentAction)
            {
                case EnemyActionType.Aggression:
                    _bossAttackUnlockAt = Time.time + Mathf.Max(0f, Config.AggressionPostActionDelay);
                    break;
                case EnemyActionType.Enrage:
                    _bossAttackUnlockAt = Time.time + Mathf.Max(0f, Config.EnragePostActionDelay);
                    break;
            }
        }

        private bool CanBossStartOffensiveAction()
        {
            return Config.BehaviourType != EnemyBehaviourType.Boss || Time.time >= _bossAttackUnlockAt;
        }

        private EnemyAttackDeliveryType ResolveStrongAttackDeliveryType()
        {
            if (Config.StrongAttackDeliveryType != EnemyAttackDeliveryType.Auto)
            {
                return Config.StrongAttackDeliveryType;
            }

            if (GetActiveSustainedAttackEffectPrefab() != null
                || GetSustainedProjectilePrefab() != null)
            {
                return EnemyAttackDeliveryType.SustainedEffect;
            }

            return GetStrongAttackProjectilePrefab() != null
                ? EnemyAttackDeliveryType.Ranged
                : EnemyAttackDeliveryType.Melee;
        }

        private GameObject GetSustainedProjectilePrefab()
        {
            return _selectedBossElementVariation?.SustainedAttackProjectilePrefab;
        }

        private void CompleteAction(bool applyBossPostActionDelay)
        {
            EnemyActionType completedAction = _currentAction;
            if (completedAction == EnemyActionType.None)
            {
                return;
            }

            if (applyBossPostActionDelay)
            {
                ApplyBossPostActionDelay();
            }

            RotateAttackVariationAfterAction(completedAction);
            ResetActionState();
        }

        private void RotateAttackVariationAfterAction(EnemyActionType completedAction)
        {
            if (Config.BehaviourType == EnemyBehaviourType.Regular
                && completedAction == EnemyActionType.Attack)
            {
                AdvanceRegularAttackVariation();
                return;
            }

            if (Config.BehaviourType == EnemyBehaviourType.Boss
                && (completedAction == EnemyActionType.StrongAttack
                    || completedAction == EnemyActionType.AirAttack))
            {
                AdvanceBossAttackVariation();
            }
        }

        private void StartSustainedAttackEffect(float damagePerSecond, float radius, float emissionInterval, float projectileSpeed)
        {
            StopSustainedAttackEffect();

            Transform origin = _shootPoint != null ? _shootPoint : transform;
            if (origin == null)
            {
                return;
            }

            GameObject sustainedAttackEffectPrefab = GetActiveSustainedAttackEffectPrefab();
            if (sustainedAttackEffectPrefab != null)
            {
                _activeSustainedAttackEffect = _gameObjectFactory.Instantiate(
                    sustainedAttackEffectPrefab,
                    origin.position,
                    origin.rotation,
                    origin);
                SetWorldScale(_activeSustainedAttackEffect.transform, Vector3.one);
            }
            else
            {
                _activeSustainedAttackEffect = new GameObject("BossSustainedAttackEffect");
                _activeSustainedAttackEffect.transform.SetParent(origin, false);
                _activeSustainedAttackEffect.transform.localPosition = Vector3.zero;
                _activeSustainedAttackEffect.transform.localRotation = Quaternion.identity;
                SetWorldScale(_activeSustainedAttackEffect.transform, Vector3.one);
            }

            GameObject sustainedProjectilePrefab = GetSustainedProjectilePrefab();
            if (sustainedProjectilePrefab != null)
            {
                if (_activeSustainedAttackEffect.TryGetComponent(out EnemyPersistentDamageEffect damageEffect))
                {
                    Destroy(damageEffect);
                }

                if (_activeSustainedAttackEffect.TryGetComponent(out EnemyProjectileEmitter projectileEmitter) == false)
                {
                    projectileEmitter = _activeSustainedAttackEffect.AddComponent<EnemyProjectileEmitter>();
                }

                projectileEmitter.Setup(
                    _gameObjectFactory,
                    sustainedProjectilePrefab,
                    _playerTransform,
                    damagePerSecond,
                    projectileSpeed,
                    emissionInterval,
                    visualProjectileLifetime: Config.SustainedAttackProjectileLifetime,
                    visualRotationOffset: GetActiveSustainedProjectileRotationOffset());
            }
            else
            {
                if (_activeSustainedAttackEffect.TryGetComponent(out EnemyProjectileEmitter projectileEmitter))
                {
                    Destroy(projectileEmitter);
                }

                EnemyPersistentDamageEffect damageEffect;
                if (_activeSustainedAttackEffect.TryGetComponent(out damageEffect) == false)
                {
                    damageEffect = _activeSustainedAttackEffect.AddComponent<EnemyPersistentDamageEffect>();
                }

                damageEffect.Setup(damagePerSecond, emissionInterval, radius, _playerLayer);
            }

            PlayConfiguredMagicAttackSound();
        }

        private void StopSustainedAttackEffect()
        {
            if (_activeSustainedAttackEffect == null)
            {
                return;
            }

            Destroy(_activeSustainedAttackEffect);
            _activeSustainedAttackEffect = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (Config == null)
            {
                return;
            }

            DrawBossAttackPreviewGizmos(drawAllRanges: true);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false || Config == null || Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return;
            }

            if (_currentAction != EnemyActionType.Attack
                && _currentAction != EnemyActionType.StrongAttack
                && _currentAction != EnemyActionType.AirAttack)
            {
                return;
            }

            DrawBossAttackPreviewGizmos(drawAllRanges: false);
        }

        private void DrawBossAttackPreviewGizmos(bool drawAllRanges)
        {
            if (Config == null)
            {
                return;
            }

            if (drawAllRanges)
            {
                DrawRangeGizmo(transform.position, Config.AttackRange, new Color(1f, 0.92f, 0.16f, 0.8f));
                DrawRangeGizmo(transform.position, Config.StrongAttackRange, new Color(1f, 0.45f, 0.05f, 0.8f));

                if (Config.BehaviourType == EnemyBehaviourType.Boss)
                {
                    DrawRangeGizmo(transform.position, Config.AirAttackRange, new Color(0.35f, 0.85f, 1f, 0.8f));
                }
            }

            bool drawMeleeHit = drawAllRanges || _currentAction == EnemyActionType.Attack;
            if (drawMeleeHit)
            {
                Vector3 meleePoint = _meleeAttackPoint != null ? _meleeAttackPoint.position : transform.position;
                Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.85f);
                Gizmos.DrawWireSphere(meleePoint, Config.HitRadius);
            }

            if (Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return;
            }

            Vector3 shootPoint = _shootPoint != null ? _shootPoint.position : transform.position;

            if (drawAllRanges || _currentAction == EnemyActionType.StrongAttack)
            {
                Gizmos.color = new Color(1f, 0.45f, 0.05f, 0.85f);
                Gizmos.DrawWireSphere(shootPoint, Config.StrongAttackEffectRadius);
            }

            if (drawAllRanges || _currentAction == EnemyActionType.AirAttack)
            {
                Gizmos.color = new Color(0.35f, 0.85f, 1f, 0.85f);
                Gizmos.DrawWireSphere(shootPoint, Config.AirAttackEffectRadius);
            }
        }

        private static void DrawRangeGizmo(Vector3 origin, float radius, Color color)
        {
            if (radius <= 0f)
            {
                return;
            }

            Gizmos.color = color;
            Gizmos.DrawWireSphere(origin, radius);
        }

        private static void SetWorldScale(Transform target, Vector3 worldScale)
        {
            if (target == null)
            {
                return;
            }

            Transform parent = target.parent;
            if (parent == null)
            {
                target.localScale = worldScale;
                return;
            }

            Vector3 parentScale = parent.lossyScale;
            target.localScale = new Vector3(
                DivideScaleAxis(worldScale.x, parentScale.x),
                DivideScaleAxis(worldScale.y, parentScale.y),
                DivideScaleAxis(worldScale.z, parentScale.z));
        }

        private static float DivideScaleAxis(float scale, float parentScale)
        {
            return Mathf.Abs(parentScale) > 0.0001f
                ? scale / parentScale
                : scale;
        }

        private Transform GetMeleeAttackOrigin()
        {
            return _meleeAttackPoint != null ? _meleeAttackPoint : transform;
        }

        private bool TryGetDamageableInMeleeRadius(float radius, out IDamageable victim)
        {
            Transform attackOrigin = GetMeleeAttackOrigin();
            Collider[] hitColliders = Physics.OverlapSphere(
                attackOrigin.position,
                radius,
                ~0,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (TryResolveDamageable(hitColliders[i], out victim)
                    && IsAllowedTarget(victim))
                {
                    return true;
                }
            }

            victim = null;
            return false;
        }

        private static bool TryResolveDamageable(Collider hitCollider, out IDamageable victim)
        {
            victim = hitCollider.GetComponentInParent<IDamageable>();
            return victim != null && victim.IsAlive;
        }

        private bool IsAllowedTarget(IDamageable victim)
        {
            if (victim is not Component victimComponent)
            {
                return false;
            }

            int victimLayer = victimComponent.gameObject.layer;
            return ((_playerLayer.value >> victimLayer) & 1) != 0;
        }
    }
}
