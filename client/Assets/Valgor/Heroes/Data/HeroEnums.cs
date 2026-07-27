using UnityEngine;

namespace Valgor.Heroes.Data
{
    public enum HeroRarity
    {
        Comum = 0,
        Rara = 1,
        Epica = 2,
        Lendaria = 3,
        Mitica = 4
    }

    public enum HeroFaction
    {
        RosaDeSangue = 0,
        AsasDoAmanhecer = 1,
        GuardaDaOrdem = 2
    }

    public static class HeroFactionIds
    {
        public const string RosaDeSangue = "ROSA_DE_SANGUE";
        public const string AsasDoAmanhecer = "ASAS_DO_AMANHECER";
        public const string GuardaDaOrdem = "GUARDA_DA_ORDEM";

        public static string ToId(HeroFaction faction) => faction switch
        {
            HeroFaction.RosaDeSangue => RosaDeSangue,
            HeroFaction.AsasDoAmanhecer => AsasDoAmanhecer,
            HeroFaction.GuardaDaOrdem => GuardaDaOrdem,
            _ => string.Empty
        };

        public static string ToDisplayName(HeroFaction faction) => faction switch
        {
            HeroFaction.RosaDeSangue => "Rosa de Sangue",
            HeroFaction.AsasDoAmanhecer => "Asas do Amanhecer",
            HeroFaction.GuardaDaOrdem => "Guarda da Ordem",
            _ => "—"
        };

        public static HeroFaction FromId(string id)
        {
            if (id == RosaDeSangue) return HeroFaction.RosaDeSangue;
            if (id == AsasDoAmanhecer) return HeroFaction.AsasDoAmanhecer;
            if (id == GuardaDaOrdem) return HeroFaction.GuardaDaOrdem;
            throw new System.ArgumentOutOfRangeException(nameof(id), id, "Facção desconhecida.");
        }
    }

    public enum CombatRole
    {
        Leadership = 0,
        Damage = 1,
        Control = 2,
        Support = 3,
        Protection = 4,
        Assassin = 5
    }

    public enum CombatPosition
    {
        Front = 0,
        Mid = 1,
        Back = 2,
        Flank = 3,
        MobileFront = 4
    }

    public enum SpecialPowerState
    {
        Ready = 0,
        Active = 1,
        Cooldown = 2
    }

    public enum TargetType
    {
        Self = 0,
        SelfOrAllies = 1,
        Enemies = 2,
        Area = 3
    }

    [System.Serializable]
    public sealed class BaseStats
    {
        public float Attack;
        public float Defense;
        public float Health;
        public float MagicAttack;
        public float MagicDefense;
        public float Mana;
        public float ManaRegen;
        public float ControlResistance;
        public float CooldownReduction;
        public float ElementalResistance;
    }
}
