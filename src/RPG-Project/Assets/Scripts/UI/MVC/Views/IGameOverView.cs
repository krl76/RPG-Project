using System;

namespace UI.MVC.Views
{
    public interface IGameOverView
    {
        event Action RestartRequested;
        event Action BackToMenuRequested;
    }
}
