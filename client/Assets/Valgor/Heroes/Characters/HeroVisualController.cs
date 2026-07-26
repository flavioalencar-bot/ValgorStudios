using UnityEngine;

namespace Valgor.Heroes.Characters
{
    /// <summary>
    /// Drives animator + special-power presentation for a hero visual prefab.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class HeroVisualController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private HeroSocketRegistry sockets;
        [SerializeField] private HeroVfxController vfx;
        [SerializeField] private HeroAudioController audioController;
        [SerializeField] private Transform weaponRoot;
        [SerializeField] private Transform previewAnchor;
        [SerializeField] private bool usingTechnicalFallback;
        [SerializeField] private string heroId;

        public bool UsingTechnicalFallback => usingTechnicalFallback;
        public string HeroId => heroId;
        public Transform PreviewAnchor => previewAnchor != null ? previewAnchor : transform;
        public HeroSocketRegistry Sockets => sockets;

        public void Configure(
            string id,
            Animator anim,
            HeroSocketRegistry socketRegistry,
            HeroVfxController vfxController,
            HeroAudioController audioCtrl,
            Transform weapon,
            Transform preview,
            bool fallback)
        {
            heroId = id;
            animator = anim;
            sockets = socketRegistry;
            vfx = vfxController;
            audioController = audioCtrl;
            weaponRoot = weapon;
            previewAnchor = preview;
            usingTechnicalFallback = fallback;
        }

        public void SetUsingFallback(bool value) => usingTechnicalFallback = value;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }

        public void PlayIdle()
        {
            if (animator == null || !animator.isActiveAndEnabled) return;
            if (HasState(HeroAnimationIds.Idle))
                animator.CrossFadeInFixedTime(HeroAnimationIds.Idle, 0.15f);
        }

        public void PlaySpecialPower()
        {
            if (animator != null && animator.isActiveAndEnabled && HasState(HeroAnimationIds.SpecialPower))
                animator.CrossFadeInFixedTime(HeroAnimationIds.SpecialPower, 0.1f);

            vfx?.PlaySpecialAura();
            audioController?.PlaySpecial();
        }

        public void AttachWeaponTo(string socketId)
        {
            if (weaponRoot == null || sockets == null) return;
            var socket = sockets.Get(socketId);
            if (socket == null) return;
            weaponRoot.SetParent(socket, false);
            weaponRoot.localPosition = Vector3.zero;
            weaponRoot.localRotation = Quaternion.identity;
        }

        private bool HasState(string stateName)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;
            return animator.HasState(0, Animator.StringToHash(stateName));
        }
    }
}
