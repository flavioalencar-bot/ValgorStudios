using Valgor.Domain.Heroes;

namespace Valgor.Application.Common.Interfaces.Heroes;

public interface IHeroCatalogStore
{
    Task<IReadOnlyList<HeroDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<HeroDefinition?> GetByIdAsync(string heroId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HeroFaction>> GetFactionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FactionTeamBonus>> GetTeamBonusesAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetAdvantageDamageMultiplierAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, string>> GetAdvantageMapAsync(CancellationToken cancellationToken = default);
}
