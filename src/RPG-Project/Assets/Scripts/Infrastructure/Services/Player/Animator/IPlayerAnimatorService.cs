using Infrastructure.Services.Player.Input;
using UnityEngine;

namespace Infrastructure.Services.Player.Animator
{
    public interface IPlayerAnimatorService
    {
        public void InstallService();
        public void SetFightInputService(IFightInputService fightInputService);
        public void ResetTriggersByHit();
        public void SetMoveBool(bool state);
        public void SetFallBool(bool state);
        public void TriggerJump(bool isTrigger=true);
        public void TriggerLand();
        public void TriggerPhysicalAttack();
        public void TriggerMagicAttack();
        public void TriggerHit();
        public void TriggerDeath();
        public void ChangeMoveSpeed(float newSpeed);
        public void SetMoveVector(Vector2 vector);
    }
}