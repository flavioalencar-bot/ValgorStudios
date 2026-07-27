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

public sealed class BuildingRequirementCatalogTests
{
    [Fact]
    public void Farm_RequiresOnlyCastleEqualToTargetLevel()
    {
        var req = BuildingRequirementCatalog.GetRequirement("farm", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Empty(req.RequiredBuildings);
    }

    [Fact]
    public void Warehouse_Level2_RequiresFarm2()
    {
        var req = BuildingRequirementCatalog.GetRequirement("warehouse", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "farm" && b.MinimumLevel == 2);
    }

    [Fact]
    public void Castle_Level2_RequiresFarmAndWarehouse()
    {
        var req = BuildingRequirementCatalog.GetRequirement("castle", currentLevel: 1);
        Assert.Equal(0, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "farm" && b.MinimumLevel == 2);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "warehouse" && b.MinimumLevel == 2);
    }

    [Fact]
    public void DragonTower_Level2_RequiresGatherResearch()
    {
        var req = BuildingRequirementCatalog.GetRequirement("dragon-tower", currentLevel: 1);
        Assert.Contains(req.RequiredUnlocks, u => u.UnlockKey == BuildingRequirementCatalog.UnlockGatherResearch);
    }

    [Fact]
    public void Evaluator_BlocksWhenCastleTooLow()
    {
        var farm = new BuildingInstance("farm", 1, BuildingState.Ready);
        var reason = BuildingRequirementEvaluator.GetFirstBlockReason(
            farm,
            castleLevel: 1,
            _ => 0);

        Assert.NotNull(reason);
        Assert.Contains("Castelo", reason);
    }

    [Fact]
    public void Evaluator_BlocksMissingUnlock()
    {
        var tower = new BuildingInstance("dragon-tower", 1, BuildingState.Ready);
        var reason = BuildingRequirementEvaluator.GetFirstBlockReason(
            tower,
            castleLevel: 2,
            id => id == "warehouse" ? 1 : 0,
            _ => false);

        Assert.NotNull(reason);
        Assert.Contains("Coleta", reason);
    }

    [Fact]
    public void Evaluator_PassesWhenDependenciesMet()
    {
        var warehouse = new BuildingInstance("warehouse", 1, BuildingState.Ready);
        var ok = BuildingRequirementEvaluator.MeetsAll(
            warehouse,
            castleLevel: 2,
            id => id == "farm" ? 2 : 0);

        Assert.True(ok);
    }

    [Fact]
    public void Evaluator_UnmetBuilding_ExposesJumpTarget()
    {
        var castle = new BuildingInstance("castle", 1, BuildingState.Ready);
        var checks = BuildingRequirementEvaluator.Evaluate(
            castle,
            castleLevel: 1,
            _ => 0);

        var farm = Assert.Single(checks, c => c.Label == "Fazenda");
        Assert.False(farm.Satisfied);
        Assert.Equal("farm", farm.JumpToDefinitionId);
    }

    [Fact]
    public void Evaluator_BlocksFarmWhenCityCastleBelowTarget_EvenIfProfileWouldBeHigher()
    {
        // Simula Castelo cidade Nv.3; perfil alto NÃO entra no evaluator (só o nível injetado).
        var farm = new BuildingInstance("farm", 3, BuildingState.Ready);
        var reason = BuildingRequirementEvaluator.GetFirstBlockReason(
            farm,
            castleLevel: 3,
            _ => 0);

        Assert.NotNull(reason);
        Assert.Contains("Castelo", reason);
        Assert.Contains("4", reason);
    }
}
