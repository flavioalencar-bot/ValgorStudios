namespace Valgor.Heroes.Characters
{
    /// <summary>Canonical socket names for hero prefabs (Vortex and future heroes).</summary>
    public static class HeroSocketIds
    {
        public const string RightHand = "Socket_RightHand";
        public const string LeftHand = "Socket_LeftHand";
        public const string BackWeapon = "Socket_BackWeapon";
        public const string HipWeapon = "Socket_HipWeapon";
        public const string HeadVfx = "Socket_HeadVFX";
        public const string ChestVfx = "Socket_ChestVFX";
        public const string FootLeftVfx = "Socket_FootLeftVFX";
        public const string FootRightVfx = "Socket_FootRightVFX";
        public const string DragonLink = "Socket_DragonLink";

        public static readonly string[] Required =
        {
            RightHand, LeftHand, BackWeapon, HipWeapon,
            HeadVfx, ChestVfx, FootLeftVfx, FootRightVfx, DragonLink
        };
    }
}
