using System.Collections;
using UnityEngine;

namespace Valgor.Heroes.Characters
{
    public sealed class HeroVfxController : MonoBehaviour
    {
        public const float DefaultSpecialAuraSeconds = 10f;

        [SerializeField] private HeroSocketRegistry sockets;
        [SerializeField] private ParticleSystem specialAura;
        [SerializeField] private ParticleSystem runeRing;
        [SerializeField] private float specialAuraDuration = DefaultSpecialAuraSeconds;

        private Coroutine _specialRoutine;

        public void Bind(HeroSocketRegistry registry) => sockets = registry;

        public void Configure(ParticleSystem aura, ParticleSystem runes, float durationSeconds = DefaultSpecialAuraSeconds)
        {
            specialAura = aura;
            runeRing = runes;
            specialAuraDuration = durationSeconds > 0f ? durationSeconds : DefaultSpecialAuraSeconds;
        }

        public void PlaySpecialAura()
        {
            if (_specialRoutine != null)
                StopCoroutine(_specialRoutine);
            _specialRoutine = StartCoroutine(SpecialRoutine());
        }

        public void StopSpecialAura()
        {
            if (_specialRoutine != null)
            {
                StopCoroutine(_specialRoutine);
                _specialRoutine = null;
            }

            StopPs(specialAura);
            StopPs(runeRing);
        }

        private IEnumerator SpecialRoutine()
        {
            PlayPs(specialAura);
            PlayPs(runeRing);
            yield return new WaitForSeconds(specialAuraDuration);
            StopPs(specialAura);
            StopPs(runeRing);
            _specialRoutine = null;
        }

        private static void PlayPs(ParticleSystem ps)
        {
            if (ps == null) return;
            ps.Play(true);
        }

        private static void StopPs(ParticleSystem ps)
        {
            if (ps == null) return;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
