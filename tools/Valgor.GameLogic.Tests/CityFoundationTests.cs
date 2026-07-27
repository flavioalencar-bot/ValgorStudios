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
    public void Wall_RequiresOnlyCastleEqualToTargetLevel()
    {
        var req = BuildingRequirementCatalog.GetRequirement("wall", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Empty(req.RequiredBuildings);

        var atZero = BuildingRequirementCatalog.GetRequirement("wall", currentLevel: 0);
        Assert.Equal(1, atZero.MinimumCastleLevel);
    }

    [Fact]
    public void Wall_IsInBuildingCatalog()
    {
        Assert.True(BuildingCatalog.TryGet("wall", out var def));
        Assert.Equal("Muralha", def.DisplayName);
        Assert.True(def.MaxLevel >= 1);
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
    public void DragonTower_Level2_RequiresAcademy()
    {
        var req = BuildingRequirementCatalog.GetRequirement("dragon-tower", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "academy" && b.MinimumLevel == 1);
        Assert.Empty(req.RequiredUnlocks);
    }

    [Fact]
    public void Arena_RequiresCastleAndAcademy()
    {
        var req = BuildingRequirementCatalog.GetRequirement("arena", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "academy");
    }

    [Fact]
    public void Hospital_RequiresCastleAndFarm()
    {
        var req = BuildingRequirementCatalog.GetRequirement("hospital", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "farm");
    }

    [Fact]
    public void Temple_RequiresCastleAndHospital()
    {
        var req = BuildingRequirementCatalog.GetRequirement("temple", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "hospital");
    }

    [Fact]
    public void Market_RequiresCastleAndWarehouse()
    {
        var req = BuildingRequirementCatalog.GetRequirement("market", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "warehouse");
    }

    [Fact]
    public void Laboratory_RequiresCastleAcademyAndMine()
    {
        var req = BuildingRequirementCatalog.GetRequirement("laboratory", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "academy");
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "mine");
    }

    [Fact]
    public void Evaluator_BlocksDragonTowerMissingAcademy()
    {
        var tower = new BuildingInstance("dragon-tower", 1, BuildingState.Ready);
        var reason = BuildingRequirementEvaluator.GetFirstBlockReason(
            tower,
            castleLevel: 2,
            _ => 0,
            _ => true);

        Assert.NotNull(reason);
        Assert.Contains("Academia", reason);
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
    public void Evaluator_Laboratory_ReportsMultipleBlocks()
    {
        var lab = new BuildingInstance("laboratory", 1, BuildingState.Ready);
        var checks = BuildingRequirementEvaluator.Evaluate(lab, castleLevel: 1, _ => 0);
        Assert.Contains(checks, c => !c.Satisfied && c.Label == "Castelo");
        Assert.Contains(checks, c => !c.Satisfied && c.Label == "Academia" && c.JumpToDefinitionId == "academy");
        Assert.Contains(checks, c => !c.Satisfied && c.Label == "Mina" && c.JumpToDefinitionId == "mine");
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

    [Fact]
    public void Lumbermill_RequiresCastleAndFarm()
    {
        var req = BuildingRequirementCatalog.GetRequirement("lumbermill", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "farm" && b.MinimumLevel == 1);
    }

    [Fact]
    public void Quarry_RequiresCastleAndLumbermill()
    {
        var req = BuildingRequirementCatalog.GetRequirement("quarry", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "lumbermill" && b.MinimumLevel == 1);
    }

    [Fact]
    public void Mine_RequiresCastleAndQuarry()
    {
        var req = BuildingRequirementCatalog.GetRequirement("mine", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "quarry" && b.MinimumLevel == 1);
    }

    [Fact]
    public void Academy_RequiresCastleAndWarehouse()
    {
        var req = BuildingRequirementCatalog.GetRequirement("academy", currentLevel: 1);
        Assert.Equal(2, req.MinimumCastleLevel);
        Assert.Contains(req.RequiredBuildings, b => b.BuildingDefinitionId == "warehouse" && b.MinimumLevel == 1);
    }
}

public sealed class ProductionBuildingDetailsTests
{
    [Fact]
    public void FormatTimeToFill_ReportsFullAndMinutes()
    {
        Assert.Equal("Cheio", ProductionBuildingDetails.FormatTimeToFill(400, 400, 100));
        Assert.Equal("Sem produção", ProductionBuildingDetails.FormatTimeToFill(0, 400, 0));
        Assert.Equal("2h", ProductionBuildingDetails.FormatTimeToFill(0, 200, 100));
    }

    [Fact]
    public void DescribeUpgradeBenefit_UsesResourceLabel()
    {
        Assert.Contains("madeira", ProductionBuildingDetails.DescribeUpgradeBenefit("lumbermill"));
        Assert.Contains("pedra", ProductionBuildingDetails.DescribeUpgradeBenefit("quarry"));
        Assert.Contains("ferro", ProductionBuildingDetails.DescribeUpgradeBenefit("mine"));
    }
}

public sealed class SupportBuildingRulesTests
{
    [Fact]
    public void ArenaAndHospital_ScaleWithLevel()
    {
        Assert.True(SupportBuildingRules.GetArenaFormationCapacity(2) > SupportBuildingRules.GetArenaFormationCapacity(1));
        Assert.True(SupportBuildingRules.GetHospitalCapacity(2) > SupportBuildingRules.GetHospitalCapacity(1));
        Assert.Equal(0, SupportBuildingRules.GetHospitalUnitsInCare(5));
    }

    [Fact]
    public void DragonTowerBenefit_IsNonEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(SupportBuildingRules.DescribeUpgradeBenefit("dragon-tower", 1)));
        Assert.Contains("formação", SupportBuildingRules.BuildArenaDetails(1), System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Wall_StatsScaleWithLevel()
    {
        Assert.True(SupportBuildingRules.GetWallCityDefense(2) > SupportBuildingRules.GetWallCityDefense(1));
        Assert.True(SupportBuildingRules.GetWallHitPoints(2) > SupportBuildingRules.GetWallHitPoints(1));
        Assert.True(SupportBuildingRules.GetWallResistancePercent(2) > SupportBuildingRules.GetWallResistancePercent(1));
        Assert.Equal(0, SupportBuildingRules.GetWallCityDefense(0));
        Assert.Contains("Defesa", SupportBuildingRules.BuildWallDetails(1), System.StringComparison.OrdinalIgnoreCase);
        Assert.Contains("defesa", SupportBuildingRules.DescribeUpgradeBenefit("wall", 1), System.StringComparison.OrdinalIgnoreCase);
    }
}
