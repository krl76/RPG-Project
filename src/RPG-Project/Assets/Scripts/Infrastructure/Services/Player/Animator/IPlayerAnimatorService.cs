using System;
using Infrastructure.Services.Player.Input;
using UnityEngine;

namespace Infrastructure.Services.Player.Animator
{
    /// <summary>
    /// Контракт управления анимациями и событиями аниматора игрока.
    /// </summary>
    public interface IPlayerAnimatorService
    {
        public bool IsInitilized { get; set; }
        public bool IsHitStateActive { get; }
        public void InstallService();
        public void UninstallService();
        public void SetFightInputService(IFightInputService fightInputService);
        public void ResetTriggersByHit();
        public void SetMoveBool(bool state);
        public void SetFallBool(bool state);
        public void TriggerJump(bool isTrigger=true);
        public void TriggerLand();
        public void TriggerPhysicalAttack();
        public void TriggerMagicAttack();
        public void TriggerGrabGun();
        public void TriggerHit();
        public void TriggerDeath();
        public void ChangeMoveSpeed(float newSpeed);
        void SetTurnValue(float value);
        public void SetMoveVector(Vector2 vector);
        void ProcessAnimationEvent(string eventId);
        
        event Action OnGrabGun;
        event Action OnGrabGunEnded;
        event Action OnAttackEnded;
        event Action OnShootEnded;
        event Action OnPhysicalAttack;
    }
}
