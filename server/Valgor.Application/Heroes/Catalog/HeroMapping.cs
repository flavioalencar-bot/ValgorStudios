using Valgor.Contracts.Heroes;
using Valgor.Domain.Heroes;

namespace Valgor.Application.Heroes.Catalog;

internal static class HeroMapping
{
    public static HeroDto ToDto(HeroDefinition hero)
    {
        SpecialPowerDto? power = null;
        if (hero.SpecialPower is not null)
        {
            var effects = hero.Effects
                .OrderBy(e => e.SortOrder)
                .Select(e => e.Description)
                .ToArray();

            power = new SpecialPowerDto(
                hero.SpecialPower.Id,
                hero.SpecialPower.DisplayName,
                hero.SpecialPower.ActiveDurationSec,
                hero.SpecialPower.CooldownSec,
                hero.SpecialPower.TargetType,
                hero.SpecialPower.Interruptible,
                hero.SpecialPower.CanActivateWhileControlled,
                hero.SpecialPower.AnimationState,
                hero.SpecialPower.VfxAddress,
                hero.SpecialPower.SfxAddress,
                effects);
        }

        return new HeroDto(
            hero.Id,
            hero.Name,
            hero.Title,
            hero.DisplayName,
            hero.Gender,
            hero.Rarity,
            hero.FactionId,
            hero.ClassName,
            hero.Role,
            hero.Position,
            hero.Weapon,
            hero.Element,
            hero.Status,
            hero.Notes,
            hero.DefaultSkinId,
            hero.PrefabAddress,
            hero.PortraitAddress,
            power);
    }
}
