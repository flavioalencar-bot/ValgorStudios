namespace Valgor.Domain.Heroes;

public sealed class HeroSpecialPower
{
    private HeroSpecialPower()
    {
    }

    public string Id { get; private set; } = string.Empty;
    public string HeroId { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public float ActiveDurationSec { get; private set; }
    public float CooldownSec { get; private set; }
    public string TargetType { get; private set; } = "SelfOrAllies";
    public bool Interruptible { get; private set; } = true;
    public bool CanActivateWhileControlled { get; private set; }
    public string AnimationState { get; private set; } = "Special";
    public string VfxAddress { get; private set; } = string.Empty;
    public string SfxAddress { get; private set; } = string.Empty;

    public static HeroSpecialPower Create(
        string heroId,
        string displayName,
        float activeDurationSec,
        float cooldownSec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heroId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        if (activeDurationSec <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(activeDurationSec));
        }

        if (cooldownSec < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(cooldownSec));
        }

        var id = $"POWER_{heroId}";
        return new HeroSpecialPower
        {
            Id = id,
            HeroId = heroId.Trim(),
            DisplayName = displayName.Trim(),
            ActiveDurationSec = activeDurationSec,
            CooldownSec = cooldownSec,
            VfxAddress = $"vfx/special/{id}",
            SfxAddress = $"sfx/special/{id}"
        };
    }
}
