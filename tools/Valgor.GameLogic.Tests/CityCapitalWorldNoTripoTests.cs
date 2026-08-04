using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityCapitalWorldNoTripoTests
{
    [Fact]
    public void TripoApiInvocationCount_StartsAtZero()
    {
        CityCapitalWorldNoTripo.ResetInvocationCountForTests();
        Assert.Equal(0, CityCapitalWorldNoTripo.TripoApiInvocationCount);
        CityCapitalWorldNoTripo.NeverCallTripoApi("unit-test");
        Assert.Equal(0, CityCapitalWorldNoTripo.TripoApiInvocationCount);
        CityCapitalWorldNoTripo.AssertZeroTripoUsage();
    }

    [Fact]
    public void IllegalInvocation_IsDetected()
    {
        CityCapitalWorldNoTripo.ResetInvocationCountForTests();
        CityCapitalWorldNoTripo.RecordIllegalTripoApiInvocation("test");
        Assert.Equal(1, CityCapitalWorldNoTripo.TripoApiInvocationCount);
        Assert.Throws<InvalidOperationException>(CityCapitalWorldNoTripo.AssertZeroTripoUsage);
        CityCapitalWorldNoTripo.ResetInvocationCountForTests();
    }

    [Fact]
    public void ModeFlagConstant_IsStable()
    {
        Assert.Equal("-capitalWorldNoTripoCorrection", CityCapitalWorldNoTripo.CliFlag);
        Assert.Equal("CapitalWorldNoTripoCorrection", CityCapitalWorldNoTripo.ModeName);
    }
}
