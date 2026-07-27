using Valgor.City.Buildings;
using Valgor.City.Camera;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.Core;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityBoundsTests
{
    [Fact]
    public void ClampPosition_ConstrainsHorizontalCoordinates()
    {
        var bounds = new CityBounds(-5, 5, -3, 3);

        var result = bounds.ClampPosition(new CityPosition(10, 7, -10));

        Assert.Equal(5, result.X);
        Assert.Equal(7, result.Y);
        Assert.Equal(-3, result.Z);
    }
}

public sealed class BuildingSelectionServiceTests
{
    [Fact]
    public void Select_ThenDeselect_RaisesExpectedStates()
    {
        var service = new BuildingSelectionService();
        var building = new BuildingInstance("castle", 1, BuildingState.Ready);
        BuildingInstance? last = building;
        service.SelectionChanged += selected => last = selected;

        service.Select(building);
        Assert.Same(building, service.Selected);
        service.Deselect();

        Assert.Null(service.Selected);
        Assert.Null(last);
    }
}

public sealed class ResourceWalletTests
{
    [Fact]
    public void TrySpend_DoesNotMakeBalanceNegative()
    {
        var wallet = new ResourceWallet();
        wallet.Add(ResourceType.Gold, 10);

        var spent = wallet.TrySpend(ResourceType.Gold, 11);

        Assert.False(spent);
        Assert.Equal(10, wallet.Get(ResourceType.Gold));
    }

    [Fact]
    public void AddAndSpend_RaisesResourceChangedEvent()
    {
        var wallet = new ResourceWallet();
        ResourceChangedEvent? change = null;
        wallet.Changed += (_, args) => change = args;

        wallet.Add(ResourceType.Wood, 25);

        Assert.NotNull(change);
        Assert.Equal(ResourceType.Wood, change!.Resource);
        Assert.Equal(0, change.PreviousAmount);
        Assert.Equal(25, change.CurrentAmount);
    }
}

public sealed class CityNavigationConceptTests
{
    [Fact]
    public void CitySceneId_IsStable()
    {
        Assert.Equal("City", SceneIds.City);
    }

    [Fact]
    public void PlayerCity_WorldMap_PlayerCityTransition_IsValid()
    {
        var stateMachine = new GameStateMachine();
        stateMachine.TransitionTo(GameState.Bootstrapping);
        stateMachine.TransitionTo(GameState.Loading);
        stateMachine.TransitionTo(GameState.MainMenu);
        stateMachine.TransitionTo(GameState.PlayerCity);
        stateMachine.TransitionTo(GameState.WorldMap);
        stateMachine.TransitionTo(GameState.PlayerCity);

        Assert.Equal(GameState.PlayerCity, stateMachine.Current);
    }

    [Fact]
    public void ActiveSession_RemainsActiveAcrossConceptualNavigation()
    {
        var session = new GameSession();
        session.Begin();

        Assert.True(session.IsActive);
    }
}

public sealed class WarehouseRulesTests
{
    [Fact]
    public void CapacityAndProtection_ScaleWithLevel()
    {
        Assert.Equal(5_000, WarehouseRules.GetCapacity(0));
        Assert.Equal(7_500, WarehouseRules.GetCapacity(1));
        Assert.Equal(1_000, WarehouseRules.GetProtection(0));
        Assert.Equal(1_500, WarehouseRules.GetProtection(1));
        Assert.True(WarehouseRules.GetNextCapacity(1) > WarehouseRules.GetCapacity(1));
    }
}

public sealed class BuildingUpgradeRequirementsTests
{
    [Fact]
    public void Build_ListsSixResources_WithSatisfiedFlags()
    {
        var definition = BuildingCatalog.Get("castle");
        var building = new BuildingInstance("castle", 1, BuildingState.Ready);
        var wallet = new ResourceWallet();
        wallet.Add(ResourceType.Gold, 10_000);
        wallet.Add(ResourceType.Food, 10_000);
        wallet.Add(ResourceType.Wood, 10_000);
        wallet.Add(ResourceType.Stone, 10_000);
        wallet.Add(ResourceType.Iron, 10_000);
        wallet.Add(ResourceType.DragonEssence, 10_000);

        var reqs = BuildingUpgradeRequirements.Build(definition, building, wallet);

        Assert.Equal(6, reqs.Count);
        Assert.All(reqs, r => Assert.True(r.Satisfied));
        Assert.True(definition.GetUpgradeCost(ResourceType.Gold, 1) > 0);
        Assert.True(definition.GetUpgradeCost(ResourceType.DragonEssence, 1) > 0);
    }

    [Fact]
    public void InstantCompleteDiamondCost_ScalesWithRemainingTime()
    {
        Assert.Equal(0, BuildingUpgradeRequirements.InstantCompleteDiamondCost(TimeSpan.Zero));
        Assert.Equal(1, BuildingUpgradeRequirements.InstantCompleteDiamondCost(TimeSpan.FromSeconds(3)));
        Assert.Equal(2, BuildingUpgradeRequirements.InstantCompleteDiamondCost(TimeSpan.FromSeconds(6)));
    }
}
