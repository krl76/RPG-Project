using System;

namespace UI.MVC.Views
{
    /// <summary>
    /// Контракт окна поражения с действиями рестарта и возврата в меню.
    /// </summary>
    public interface IGameOverView
    {
        event Action RestartRequested;
        event Action BackToMenuRequested;
    }
}
