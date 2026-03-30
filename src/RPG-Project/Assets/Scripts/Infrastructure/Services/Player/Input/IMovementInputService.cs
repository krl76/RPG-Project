using UnityEngine;

namespace Infrastructure.Services.Player.Input
{
    /// <summary>
    /// Контракт обработки перемещения и прыжка игрока.
    /// </summary>
    public interface IMovementInputService
    {
        public bool CanMove { get; set; }
        public bool IsMoving {  get; set; }
        public Vector2 MoveVector { get; set; }
        public void InstallService();
        public void UninstallService();
        public void ContinueMoveAfterAction();
    }
}
