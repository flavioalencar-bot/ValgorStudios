using UnityEngine;

namespace Valgor.Heroes.Characters
{
    public sealed class HeroAudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip specialClip;

        public void PlaySpecial()
        {
            if (source == null || specialClip == null) return;
            source.PlayOneShot(specialClip);
        }
    }
}
