using UnityEngine;
using Valgor.Heroes.Data;

namespace Valgor.Heroes.Skins
{
    [CreateAssetMenu(menuName = "Valgor/Heroes/Skin Definition", fileName = "HeroSkin")]
    public sealed class HeroSkinDefinitionSO : ScriptableObject
    {
        public string SkinId;
        public string HeroId;
        public string Name;
        public string Rarity;
        public string ModelAddress;
        public string MaterialSetAddress;
        public string PortraitAddress;
        public string VfxOverrides;
        public string SfxOverrides;
        public string AnimationOverrides;
        public bool CompetitiveNormalization = true;
        public BaseStats StatModifiers = new();
    }

    public sealed class HeroSkinController : MonoBehaviour
    {
        [SerializeField] private HeroSkinDefinitionSO activeSkin;
        [SerializeField] private Transform modelRoot;

        public HeroSkinDefinitionSO ActiveSkin => activeSkin;

        public void ApplySkin(HeroSkinDefinitionSO skin)
        {
            activeSkin = skin;
            // Dummy visual swap: final Addressables models replace placeholders later.
            if (modelRoot != null && skin != null)
            {
                modelRoot.name = $"Skin_{skin.SkinId}";
            }
        }
    }
}
