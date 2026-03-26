using UnityEngine;

namespace Infrastructure.Services.Audio
{
    public interface IEffectsAudioService
    {
        void PlayOneShot(AudioClip clip, float volume = 1f);
    }
}
