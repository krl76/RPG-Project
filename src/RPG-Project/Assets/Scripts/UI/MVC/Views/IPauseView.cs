using System;

namespace UI.MVC.Views
{
    public interface IPauseView
    {
        event Action ResumeRequested;
        event Action SettingsRequested;
        event Action ExitToMainMenuRequested;
    }
}
