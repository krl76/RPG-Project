using System;

namespace UI.MVC.Views
{
    public interface IPauseView
    {
        event Action ResumeRequested;
        event Action SaveRequested;
        event Action LoadRequested;
        event Action SettingsRequested;
        event Action ExitToMainMenuRequested;
    }
}
