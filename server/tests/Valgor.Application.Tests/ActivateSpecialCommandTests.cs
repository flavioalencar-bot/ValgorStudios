using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Valgor.Application.Common.Interfaces;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Application.Common.Results;
using Valgor.Application.Heroes.ActivateSpecial;
using Valgor.Domain.Heroes;
using Valgor.Infrastructure.Persistence.Seed;

namespace Valgor.Application.Tests;

public sealed class ActivateSpecialCommandTests
{
    [Fact]
    public async Task Rejects_Unknown_Hero()
    {
        var handler = CreateHandler(out var playerId);
        var result = await handler.Handle(
            new ActivateSpecialCommand("b1", "HERO_UNKNOWN", playerId, "k1"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("not_found", result.Error!.Code);
    }

    [Fact]
    public async Task Rejects_Player_Without_Hero()
    {
        var handler = CreateHandler(out _, grantHero: false);
        var otherPlayer = Guid.NewGuid();
        var result = await handler.Handle(
            new ActivateSpecialCommand("b1", "HERO_VORTEX_000", otherPlayer, "k1"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("unauthorized", result.Error!.Code);
    }

    [Fact]
    public async Task Rejects_When_On_Cooldown()
    {
        var handler = CreateHandler(out var playerId);
        var first = await handler.Handle(
            new ActivateSpecialCommand("b-cd", "HERO_ELYRA_001", playerId, "idem-a"),
            CancellationToken.None);
        Assert.True(first.IsSuccess);

        var second = await handler.Handle(
            new ActivateSpecialCommand("b-cd", "HERO_ELYRA_001", playerId, "idem-b"),
            CancellationToken.None);
        Assert.True(second.IsFailure);
        Assert.Equal("conflict", second.Error!.Code);
    }

    [Fact]
    public async Task Duplicate_Idempotency_Replays()
    {
        var handler = CreateHandler(out var playerId);
        var first = await handler.Handle(
            new ActivateSpecialCommand("b-idem", "HERO_AKEMI_005", playerId, "same-key"),
            CancellationToken.None);
        var second = await handler.Handle(
            new ActivateSpecialCommand("b-idem", "HERO_AKEMI_005", playerId, "same-key"),
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.True(second.Value.IdempotentReplay);
        Assert.Equal(first.Value.ActiveUntilUtc, second.Value.ActiveUntilUtc);
    }

    private static ActivateSpecialCommandHandler CreateHandler(out Guid playerId, bool grantHero = true)
    {
        playerId = Guid.NewGuid();
        var bundle = HeroesSeedLoader.LoadFromEmbeddedOrFile();
        var catalog = new FakeCatalogStore(bundle);
        var players = new FakePlayerHeroStore(playerId, grantHero ? bundle.Heroes : []);
        var battles = new FakeBattleSpecialStore();
        var clock = new FakeClock(DateTime.UtcNow);
        return new ActivateSpecialCommandHandler(catalog, players, battles, clock);
    }

    private sealed class FakeClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class FakeCatalogStore(HeroesSeedBundle bundle) : IHeroCatalogStore
    {
        public Task<IReadOnlyList<HeroDefinition>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(bundle.Heroes);

        public Task<HeroDefinition?> GetByIdAsync(string heroId, CancellationToken cancellationToken = default) =>
            Task.FromResult(bundle.Heroes.FirstOrDefault(h => h.Id == heroId));

        public Task<IReadOnlyList<HeroFaction>> GetFactionsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(bundle.Factions);

        public Task<IReadOnlyList<FactionTeamBonus>> GetTeamBonusesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(bundle.TeamBonuses);

        public Task<decimal> GetAdvantageDamageMultiplierAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(HeroesSeedLoader.DefaultAdvantageDamageMultiplier);

        public Task<IReadOnlyDictionary<string, string>> GetAdvantageMapAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, string>>(
                bundle.Advantages.ToDictionary(a => a.AttackerFactionId, a => a.DefenderFactionId));
    }

    private sealed class FakePlayerHeroStore(Guid ownerId, IReadOnlyList<HeroDefinition> heroes) : IPlayerHeroStore
    {
        public Task<IReadOnlyList<PlayerHero>> GetByPlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            if (playerId != ownerId)
            {
                return Task.FromResult<IReadOnlyList<PlayerHero>>([]);
            }

            var roster = heroes.Select(h => PlayerHero.Create(playerId, h.Id, h.DefaultSkinId)).ToArray();
            return Task.FromResult<IReadOnlyList<PlayerHero>>(roster);
        }

        public Task EnsureRosterAsync(Guid playerId, IReadOnlyList<HeroDefinition> catalog, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeBattleSpecialStore : IBattleSpecialStore
    {
        private readonly Dictionary<string, BattleHeroSpecialState> _states = new(StringComparer.Ordinal);

        private static string Key(string battleId, Guid playerId, string heroId) => $"{battleId}|{playerId}|{heroId}";

        public Task<BattleHeroSpecialState?> GetAsync(string battleId, Guid playerId, string heroId, CancellationToken cancellationToken = default)
        {
            _states.TryGetValue(Key(battleId, playerId, heroId), out var state);
            return Task.FromResult(state);
        }

        public Task<BattleHeroSpecialState> GetOrCreateAsync(string battleId, Guid playerId, string heroId, CancellationToken cancellationToken = default)
        {
            var key = Key(battleId, playerId, heroId);
            if (!_states.TryGetValue(key, out var state))
            {
                state = BattleHeroSpecialState.Create(battleId, playerId, heroId);
                _states[key] = state;
            }

            return Task.FromResult(state);
        }

        public Task SaveAsync(BattleHeroSpecialState state, CancellationToken cancellationToken = default)
        {
            _states[Key(state.BattleId, state.PlayerId, state.HeroId)] = state;
            return Task.CompletedTask;
        }
    }
}
