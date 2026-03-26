using System;
using Infrastructure.Services.Player.Input;

namespace UI.MVC.Views
{
    public interface ISettingsView
    {
        event Action CloseRequested;
        event Action<float> MasterVolumeChanged;
        event Action<float> MusicVolumeChanged;
        event Action<float> EffectsVolumeChanged;
        event Action<InputBindingKey> RebindRequested;

        void SetVolumes(float master, float music, float effects);
        void SetBindingDisplay(InputBindingKey bindingKey, string displayValue);
        void ShowRebindPrompt(InputBindingKey bindingKey);
        void SetRebindButtonsInteractable(bool isInteractable);
    }
}
