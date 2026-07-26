using Valgor.Domain.Heroes;
using Valgor.Domain.Heroes.Services;

namespace Valgor.Domain.Tests;

public sealed class FactionAdvantageResolverTests
{
    private static FactionAdvantageResolver Create() =>
        new(
            new Dictionary<string, string>
            {
                [FactionIds.RosaDeSangue] = FactionIds.GuardaDaOrdem,
                [FactionIds.GuardaDaOrdem] = FactionIds.AsasDoAmanhecer,
                [FactionIds.AsasDoAmanhecer] = FactionIds.RosaDeSangue
            },
            1.15m);

    [Fact]
    public void Rosa_Beats_Guarda()
    {
        var resolver = Create();
        Assert.True(resolver.HasAdvantage(FactionIds.RosaDeSangue, FactionIds.GuardaDaOrdem));
        Assert.Equal(1.15m, resolver.ResolveDamageMultiplier(FactionIds.RosaDeSangue, FactionIds.GuardaDaOrdem));
    }

    [Fact]
    public void Guarda_Beats_Asas()
    {
        var resolver = Create();
        Assert.True(resolver.HasAdvantage(FactionIds.GuardaDaOrdem, FactionIds.AsasDoAmanhecer));
        Assert.Equal(1.15m, resolver.ResolveDamageMultiplier(FactionIds.GuardaDaOrdem, FactionIds.AsasDoAmanhecer));
    }

    [Fact]
    public void Asas_Beats_Rosa()
    {
        var resolver = Create();
        Assert.True(resolver.HasAdvantage(FactionIds.AsasDoAmanhecer, FactionIds.RosaDeSangue));
        Assert.Equal(1.15m, resolver.ResolveDamageMultiplier(FactionIds.AsasDoAmanhecer, FactionIds.RosaDeSangue));
    }

    [Fact]
    public void Reverse_Match_Has_No_Advantage()
    {
        var resolver = Create();
        Assert.False(resolver.HasAdvantage(FactionIds.GuardaDaOrdem, FactionIds.RosaDeSangue));
        Assert.Equal(1.0m, resolver.ResolveDamageMultiplier(FactionIds.GuardaDaOrdem, FactionIds.RosaDeSangue));
    }
}
