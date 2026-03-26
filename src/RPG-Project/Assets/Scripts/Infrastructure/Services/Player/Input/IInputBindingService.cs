using System;

namespace Infrastructure.Services.Player.Input
{
    public enum InputBindingKey
    {
        MoveUp = 0,
        MoveDown = 1,
        MoveLeft = 2,
        MoveRight = 3,
        Jump = 4,
        Sprint = 5,
        SwordAttack = 6,
        MagicAttack = 7
    }

    public interface IInputBindingService
    {
        bool IsRebinding { get; }

        string GetBindingDisplay(InputBindingKey bindingKey);
        bool StartRebind(InputBindingKey bindingKey, Action onComplete = null, Action onCancel = null);
        void CancelRebind();
    }
}
