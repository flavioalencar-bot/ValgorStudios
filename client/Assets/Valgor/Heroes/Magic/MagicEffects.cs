namespace Valgor.Heroes.Magic
{
    public enum EffectKind
    {
        Damage = 0,
        Heal = 1,
        Shield = 2,
        Buff = 3,
        Debuff = 4,
        Control = 5,
        Mark = 6,
        Purify = 7,
        SummonPresence = 8,
        MobilityBlock = 9,
        Execute = 10
    }

    public enum MagicSchool
    {
        Elemental = 0,
        Luz = 1,
        Sombra = 2,
        Draconica = 3,
        Runica = 4,
        Abissal = 5,
        Eterea = 6
    }

    /// <summary>
    /// Extensible magic effect abstraction. Concrete combat resolution stays on the backend.
    /// </summary>
    public interface IMagicEffect
    {
        EffectKind Kind { get; }
        MagicSchool School { get; }
        string Description { get; }
    }

    public sealed class DescriptiveMagicEffect : IMagicEffect
    {
        public DescriptiveMagicEffect(EffectKind kind, MagicSchool school, string description)
        {
            Kind = kind;
            School = school;
            Description = description;
        }

        public EffectKind Kind { get; }
        public MagicSchool School { get; }
        public string Description { get; }
    }
}
