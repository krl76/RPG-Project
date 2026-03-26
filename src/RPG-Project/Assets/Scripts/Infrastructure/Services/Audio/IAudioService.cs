using System;

namespace Infrastructure.Services.Audio
{
    public interface IAudioService
    {
        event Action VolumesChanged;

        float MasterVolume { get; }
        float MusicVolume { get; }
        float EffectsVolume { get; }

        void SetMasterVolume(float value);
        void SetMusicVolume(float value);
        void SetEffectsVolume(float value);
    }
}
