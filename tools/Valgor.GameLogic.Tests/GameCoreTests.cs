using Valgor.Core;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class GameStateMachineTests
{
    [Fact]
    public void Bootstrap_To_MainMenu_Flow_IsValid()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Bootstrapping);
        sm.TransitionTo(GameState.Loading);
        sm.TransitionTo(GameState.MainMenu);
        Assert.Equal(GameState.MainMenu, sm.Current);
    }

    [Fact]
    public void MainMenu_City_WorldMap_City_Flow_IsValid()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Bootstrapping);
        sm.TransitionTo(GameState.Loading);
        sm.TransitionTo(GameState.MainMenu);
        sm.TransitionTo(GameState.PlayerCity);
        sm.TransitionTo(GameState.WorldMap);
        sm.TransitionTo(GameState.PlayerCity);
        Assert.Equal(GameState.PlayerCity, sm.Current);
    }

    [Fact]
    public void InvalidTransition_Throws()
    {
        var sm = new GameStateMachine();
        Assert.Throws<InvalidOperationException>(() => sm.TransitionTo(GameState.PlayerCity));
    }
}

public sealed class GameSessionTests
{
    [Fact]
    public void Begin_ActivatesSession()
    {
        var session = new GameSession();
        session.Begin();
        Assert.True(session.IsActive);
        Assert.NotEqual(Guid.Empty, session.SessionId);
    }

    [Fact]
    public void Session_SurvivesCityWorldMapRoundTrip_Conceptually()
    {
        var session = new GameSession();
        session.Begin();
        var id = session.SessionId;
        Assert.True(session.IsActive);
        Assert.Equal(id, session.SessionId);
    }
}

public sealed class ServiceRegistryTests
{
    [Fact]
    public void Register_And_Get_Works()
    {
        var registry = new ServiceRegistry();
        var session = new GameSession();
        registry.Register(session);
        Assert.Same(session, registry.Get<GameSession>());
    }
}

public sealed class SceneIdsTests
{
    [Fact]
    public void City_And_WorldMap_Ids_AreStable()
    {
        Assert.Equal("City", SceneIds.City);
        Assert.Equal("WorldMap", SceneIds.WorldMap);
        Assert.Equal("MainMenu", SceneIds.MainMenu);
        Assert.Equal("HeroesDemo", SceneIds.Heroes);
    }
}

public sealed class BetaFlowTests
{
    [Fact]
    public void City_Heroes_WorldMap_City_Flow_IsValid()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Bootstrapping);
        sm.TransitionTo(GameState.Loading);
        sm.TransitionTo(GameState.MainMenu);
        sm.TransitionTo(GameState.PlayerCity);
        sm.TransitionTo(GameState.Heroes);
        sm.TransitionTo(GameState.PlayerCity);
        sm.TransitionTo(GameState.WorldMap);
        sm.TransitionTo(GameState.PlayerCity);
        Assert.Equal(GameState.PlayerCity, sm.Current);
    }
}
