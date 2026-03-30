using System;

namespace Infrastructure.Services.Audio
{
    /// <summary>
    /// Контракт управления глобальными уровнями громкости.
    /// </summary>
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
