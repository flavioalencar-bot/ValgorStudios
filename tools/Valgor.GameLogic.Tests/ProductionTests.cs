using Valgor.City.Data;
using Valgor.City.Production;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class FakeClock : IGameClock
{
    public FakeClock(DateTime utcNow) => UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
    public DateTime UtcNow { get; set; }
}

public sealed class OfflineProductionCalculatorTests
{
    [Fact]
    public void Production_IsProportionalToElapsedTime()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var produced = OfflineProductionCalculator.CalculateProduced(
            ratePerHour: 100,
            currentAccumulated: 0,
            capacity: 1000,
            lastUpdatedUtc: start,
            nowUtc: start.AddHours(2),
            maxOfflineDuration: TimeSpan.FromHours(12));

        Assert.Equal(200, produced);
    }

    [Fact]
    public void Production_RespectsCapacity()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var produced = OfflineProductionCalculator.CalculateProduced(
            100, currentAccumulated: 950, capacity: 1000, start, start.AddHours(5), TimeSpan.FromHours(12));

        Assert.Equal(50, produced);
    }

    [Fact]
    public void Production_StopsWhenFull()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var produced = OfflineProductionCalculator.CalculateProduced(
            100, 1000, 1000, start, start.AddHours(3), TimeSpan.FromHours(12));

        Assert.Equal(0, produced);
    }

    [Fact]
    public void Offline_IsCappedAt12Hours()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var produced = OfflineProductionCalculator.CalculateProduced(
            10, 0, 10_000, start, start.AddHours(48), TimeSpan.FromHours(12));

        Assert.Equal(120, produced);
    }
}

public sealed class ResourceProductionServiceTests
{
    [Fact]
    public void Collect_AddsToWallet_AndZerosOnlyAccumulated()
    {
        var clock = new FakeClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var production = new ResourceProductionService(clock, ProductionCatalog.Settings);
        var wallet = new ResourceWallet();
        wallet.Add(ResourceType.Food, 10);
        var collection = new ResourceCollectionService(production, wallet);
        var farm = new BuildingInstance("farm", 1, BuildingState.Ready);
        production.RegisterBuilding(farm);

        clock.UtcNow = clock.UtcNow.AddHours(1);
        production.ApplyUntil(clock.UtcNow);

        var collected = collection.Collect(farm);

        Assert.True(collected > 0);
        Assert.Equal(10 + collected, wallet.Get(ResourceType.Food));
        Assert.Equal(0, production.GetState("farm").Accumulated);
    }

    [Fact]
    public void ReapplySameTimestamp_DoesNotDuplicateProduction()
    {
        var clock = new FakeClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var production = new ResourceProductionService(clock, ProductionCatalog.Settings);
        var farm = new BuildingInstance("farm", 1, BuildingState.Ready);
        production.RegisterBuilding(farm);

        clock.UtcNow = clock.UtcNow.AddHours(1);
        production.ApplyUntil(clock.UtcNow);
        var first = production.GetState("farm").Accumulated;
        production.ApplyUntil(clock.UtcNow);

        Assert.Equal(first, production.GetState("farm").Accumulated);
    }

    [Fact]
    public void Upgrade_IncreasesRateAndCapacity()
    {
        var clock = new FakeClock(DateTime.UtcNow);
        var production = new ResourceProductionService(clock, ProductionCatalog.Settings);
        var farm = new BuildingInstance("farm", 1, BuildingState.Ready);
        production.RegisterBuilding(farm);

        var rate1 = production.GetRatePerHour(farm);
        var cap1 = production.GetCapacity(farm);
        farm.CompleteUpgrade();
        production.OnBuildingUpgraded(farm);

        Assert.True(production.GetRatePerHour(farm) > rate1);
        Assert.True(production.GetCapacity(farm) > cap1);
    }

    [Fact]
    public void Diamonds_HaveNoPassiveProductionDefinition()
    {
        Assert.False(ProductionCatalog.TryGet("warehouse", out _));
        Assert.Throws<ArgumentException>(() =>
            new ResourceProductionDefinition("x", ResourceType.Diamonds, 1, 1));
    }

    [Fact]
    public void Wallet_NeverGoesNegative_OnSpend()
    {
        var wallet = new ResourceWallet();
        wallet.Add(ResourceType.Gold, 5);
        Assert.False(wallet.TrySpend(ResourceType.Gold, 6));
        Assert.Equal(5, wallet.Get(ResourceType.Gold));
    }

    [Fact]
    public void CityWorldMap_KeepsEconomyInMemoryRepository()
    {
        var clock = new FakeClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var repo = new LocalProductionRepository("test.prod");
        var production = new ResourceProductionService(clock, ProductionCatalog.Settings);
        var wallet = new ResourceWallet();
        var farm = new BuildingInstance("farm", 1, BuildingState.Ready);
        production.RegisterBuilding(farm);
        clock.UtcNow = clock.UtcNow.AddHours(2);
        production.ApplyUntil(clock.UtcNow);

        var snapshot = new ProductionSnapshot { SavedAtUtc = clock.UtcNow };
        snapshot.Buildings["farm"] = production.GetState("farm");
        snapshot.Wallet[ResourceType.Food] = 42;
        repo.Save(snapshot);

        var loaded = repo.Load();
        Assert.NotNull(loaded);
        Assert.Equal(production.GetState("farm").Accumulated, loaded!.Buildings["farm"].Accumulated);
        Assert.Equal(42, loaded.Wallet[ResourceType.Food]);
    }
}
