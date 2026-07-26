namespace Valgor.Heroes.Characters.Vortex
{
    /// <summary>
    /// Canonical paths and keys for Vortex real-hero pipeline.
    /// Source of truth: docs/game-design/heroes/VALGOR_SPRINT_HERO_REAL_VORTEX.md
    /// </summary>
    public static class VortexAssetPaths
    {
        public const string HeroId = "HERO_VORTEX_000";
        public const string AddressablePrefabKey = "heroes/HERO_VORTEX_000/prefab";
        public const string AddressablePortraitKey = "heroes/HERO_VORTEX_000/portrait";

        public const string Root = "Assets/Valgor/Heroes/Characters/Vortex";
        public const string Models = Root + "/Models";
        public const string Textures = Root + "/Textures";
        public const string Materials = Root + "/Materials";
        public const string Animations = Root + "/Animations";
        public const string Prefabs = Root + "/Prefabs";
        public const string Portraits = Root + "/Portraits";
        public const string Vfx = Root + "/VFX";
        public const string Audio = Root + "/Audio";
        public const string Data = Root + "/Data";

        public const string Lod0 = Models + "/Vortex_LOD0.fbx";
        public const string Lod1 = Models + "/Vortex_LOD1.fbx";
        public const string Lod2 = Models + "/Vortex_LOD2.fbx";
        public const string DragonSword = Models + "/Vortex_DragonSword.fbx";

        public const string HeroPrefab = Prefabs + "/Vortex_Hero.prefab";
        public const string AnimatorController = Animations + "/Vortex_Animator.controller";
        public const string ImportProfile = Data + "/Vortex_ImportProfile.asset";
        public const string PipelineStatus = Data + "/Vortex_PipelineStatus.asset";

        public static readonly string[] RequiredTextures =
        {
            Textures + "/Vortex_Body_BaseColor.png",
            Textures + "/Vortex_Body_Normal.png",
            Textures + "/Vortex_Body_Mask.png",
            Textures + "/Vortex_Armor_BaseColor.png",
            Textures + "/Vortex_Armor_Normal.png",
            Textures + "/Vortex_Armor_Mask.png",
            Textures + "/Vortex_Weapon_BaseColor.png",
            Textures + "/Vortex_Weapon_Normal.png"
        };

        public static readonly string[] RequiredMaterials =
        {
            Materials + "/MAT_Vortex_Skin.mat",
            Materials + "/MAT_Vortex_Hair.mat",
            Materials + "/MAT_Vortex_ArmorBlack.mat",
            Materials + "/MAT_Vortex_ArmorGold.mat",
            Materials + "/MAT_Vortex_Cloth.mat",
            Materials + "/MAT_Vortex_Eyes.mat",
            Materials + "/MAT_Vortex_Sword.mat"
        };

        public static readonly string[] RequiredModelCandidates =
        {
            Lod0,
            Models + "/Vortex.fbx",
            Models + "/Vortex.glb",
            Models + "/Vortex.gltf"
        };

        public const float TargetHeightMeters = 2.05f;
        public const int MaxBodyTextureSize = 2048;
        public const int MaxWeaponTextureSize = 2048;
        public const int Lod0TrisMax = 85000;
        public const int Lod1TrisMax = 40000;
        public const int Lod2TrisMax = 15000;
    }
}
