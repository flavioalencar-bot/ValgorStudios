using UnityEngine;

namespace Valgor.Heroes.Characters
{
    public sealed class HeroVfxController : MonoBehaviour
    {
        [SerializeField] private HeroSocketRegistry sockets;
        [SerializeField] private ParticleSystem specialAura;

        public void Bind(HeroSocketRegistry registry) => sockets = registry;

        public void PlaySpecialAura()
        {
            if (specialAura == null) return;
            specialAura.Play(true);
        }

        public void StopSpecialAura()
        {
            if (specialAura == null) return;
            specialAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
