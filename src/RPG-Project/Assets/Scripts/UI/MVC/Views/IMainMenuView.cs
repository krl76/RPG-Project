using System;

namespace UI.MVC.Views
{
    public interface IMainMenuView
    {
        event Action PlayRequested;
        event Action SettingsRequested;
        event Action ExitRequested;
    }
}
