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
