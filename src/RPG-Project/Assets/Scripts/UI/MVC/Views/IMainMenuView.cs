using System;

namespace UI.MVC.Views
{
    /// <summary>
    /// Контракт главного меню с действиями запуска, настроек и выхода.
    /// </summary>
    public interface IMainMenuView
    {
        event Action PlayRequested;
        event Action SettingsRequested;
        event Action ExitRequested;
    }
}
