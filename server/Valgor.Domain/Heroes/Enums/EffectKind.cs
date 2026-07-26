namespace Valgor.Domain.Heroes.Enums;

/// <summary>
/// Abstraction for magic/special effects. Concrete payloads stay data-driven.
/// </summary>
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
