using UnityEngine;

namespace prototype_Roma.Scripts
{
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