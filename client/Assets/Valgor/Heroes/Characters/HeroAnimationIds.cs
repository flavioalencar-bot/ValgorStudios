namespace Valgor.Heroes.Characters
{
    public static class HeroAnimationIds
    {
        public const string Idle = "Idle";
        public const string IdleCombat = "Idle_Combat";
        public const string Walk = "Walk";
        public const string Run = "Run";
        public const string TurnLeft = "Turn_Left";
        public const string TurnRight = "Turn_Right";
        public const string Attack01 = "Attack_01";
        public const string Attack02 = "Attack_02";
        public const string HeavyAttack = "Heavy_Attack";
        public const string SpecialPower = "Special_Power";
        public const string HitFront = "Hit_Front";
        public const string HitBack = "Hit_Back";
        public const string Stun = "Stun";
        public const string Victory = "Victory";
        public const string Defeat = "Defeat";
        public const string Death = "Death";

        public static readonly string[] Required =
        {
            Idle, IdleCombat, Walk, Run, TurnLeft, TurnRight,
            Attack01, Attack02, HeavyAttack, SpecialPower,
            HitFront, HitBack, Stun, Victory, Defeat, Death
        };
    }
}
