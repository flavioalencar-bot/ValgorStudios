using UnityEngine;

namespace Valgor.Heroes.Characters
{
    [CreateAssetMenu(menuName = "Valgor/Heroes/Model Import Profile", fileName = "HeroModelImportProfile")]
    public sealed class HeroModelImportProfile : ScriptableObject
    {
        public string HeroId;
        public float ExpectedHeightMeters = 2.05f;
        public float ImportScale = 1f;
        public ModelImporterAnimationTypeExpected AnimationType = ModelImporterAnimationTypeExpected.Humanoid;
        public int MaxBodyTextureSize = 2048;
        public int MaxWeaponTextureSize = 2048;
        public int Lod0MaxTris = 85000;
        public int Lod1MaxTris = 40000;
        public int Lod2MaxTris = 15000;
        public string[] RequiredSockets = HeroSocketIds.Required;
        public string[] RequiredAnimations = HeroAnimationIds.Required;
        public string PrefabOutputPath;
        public string AddressableKey;
    }

    public enum ModelImporterAnimationTypeExpected
    {
        None = 0,
        Legacy = 1,
        Generic = 2,
        Humanoid = 3
    }
}
