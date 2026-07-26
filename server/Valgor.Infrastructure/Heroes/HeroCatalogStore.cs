using Microsoft.EntityFrameworkCore;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Domain.Heroes;
using Valgor.Infrastructure.Persistence;
using Valgor.Infrastructure.Persistence.Seed;

namespace Valgor.Infrastructure.Heroes;

public sealed class HeroCatalogStore(ValgorDbContext dbContext) : IHeroCatalogStore
{
    public async Task<IReadOnlyList<HeroDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var heroes = await dbContext.HeroDefinitions
            .AsNoTracking()
            .Include(h => h.SpecialPower)
            .OrderBy(h => h.Id)
            .ToListAsync(cancellationToken);

        await AttachEffectsAndSkinsAsync(heroes, cancellationToken);
        return heroes;
    }

    public async Task<HeroDefinition?> GetByIdAsync(string heroId, CancellationToken cancellationToken = default)
    {
        var hero = await dbContext.HeroDefinitions
            .AsNoTracking()
            .Include(h => h.SpecialPower)
            .FirstOrDefaultAsync(h => h.Id == heroId, cancellationToken);

        if (hero is null)
        {
            return null;
        }

        await AttachEffectsAndSkinsAsync([hero], cancellationToken);
        return hero;
    }

    public async Task<IReadOnlyList<HeroFaction>> GetFactionsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.HeroFactions.AsNoTracking().OrderBy(f => f.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FactionTeamBonus>> GetTeamBonusesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.FactionTeamBonuses.AsNoTracking()
            .OrderBy(b => b.SameFactionCount)
            .ThenBy(b => b.OtherFactionCount)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetAdvantageDamageMultiplierAsync(CancellationToken cancellationToken = default)
    {
        var value = await dbContext.FactionAdvantages.AsNoTracking()
            .Select(a => (decimal?)a.DamageMultiplier)
            .FirstOrDefaultAsync(cancellationToken);
        return value ?? HeroesSeedLoader.DefaultAdvantageDamageMultiplier;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAdvantageMapAsync(CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.FactionAdvantages.AsNoTracking().ToListAsync(cancellationToken);
        return rows.ToDictionary(a => a.AttackerFactionId, a => a.DefenderFactionId, StringComparer.Ordinal);
    }

    private async Task AttachEffectsAndSkinsAsync(List<HeroDefinition> heroes, CancellationToken cancellationToken)
    {
        if (heroes.Count == 0)
        {
            return;
        }

        var heroIds = heroes.Select(h => h.Id).ToArray();
        var effects = await dbContext.HeroSpecialEffects.AsNoTracking()
            .Where(e => heroIds.Contains(e.HeroId))
            .OrderBy(e => e.SortOrder)
            .ToListAsync(cancellationToken);
        var skins = await dbContext.HeroSkins.AsNoTracking()
            .Where(s => heroIds.Contains(s.HeroId))
            .ToListAsync(cancellationToken);

        foreach (var hero in heroes)
        {
            foreach (var effect in effects.Where(e => e.HeroId == hero.Id))
            {
                hero.AddEffect(effect);
            }

            foreach (var skin in skins.Where(s => s.HeroId == hero.Id))
            {
                hero.AddSkin(skin);
            }
        }
    }
}
