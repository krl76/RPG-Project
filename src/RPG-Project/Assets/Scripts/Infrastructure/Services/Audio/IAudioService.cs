using System;

namespace Infrastructure.Services.Audio
{
    public enum AudioChannel
    {
        Master = 0,
        Music = 1,
        Effects = 2
    }

    public interface IAudioService
    {
        event Action VolumesChanged;

        float MasterVolume { get; }
        float MusicVolume { get; }
        float EffectsVolume { get; }

        float GetEffectiveVolume(AudioChannel channel);
        void SetMasterVolume(float value);
        void SetMusicVolume(float value);
        void SetEffectsVolume(float value);
    }
}
