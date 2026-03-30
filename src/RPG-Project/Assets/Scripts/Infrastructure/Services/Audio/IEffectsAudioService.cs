using UnityEngine;

namespace Infrastructure.Services.Audio
{
    /// <summary>
    /// Контракт воспроизведения одноразовых эффектов и создания источников звука.
    /// </summary>
    public interface IEffectsAudioService
    {
        void PlayOneShot(AudioClip clip, float volume = 1f);
        AudioSource CreateConfiguredSource(Transform parent = null, string sourceName = null);
    }
}
