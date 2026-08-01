using System.Linq;
using Valgor.City.Buildings;
using Valgor.City.Data;
using Valgor.City.Production;
using Valgor.City.Visual;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CityOfficialBuildingsTests
{
    [Fact]
    public void Catalog_HasExactly17CanonicalBuildings()
    {
        Assert.Equal(BuildingIdMigration.OfficialCount, BuildingCatalog.All.Count);
        Assert.Equal(BuildingIdMigration.OfficialCount, BuildingIdMigration.CanonicalIds.Length);
        foreach (var id in BuildingIdMigration.CanonicalIds)
        {
            Assert.True(BuildingCatalog.TryGet(id, out var def));
            Assert.Equal(id, def.Id);
            Assert.False(string.IsNullOrWhiteSpace(def.DisplayName));
        }
    }

    [Fact]
    public void Layout_HasExactly17CanonicalEntries()
    {
        Assert.Equal(BuildingIdMigration.OfficialCount, CityCapitalLayoutConfig.All.Count);
        Assert.Empty(CityCapitalLayoutConfig.ValidateBlueprintInvariants());
    }

    [Fact]
    public void Aliases_ResolveToCanonical()
    {
        Assert.Equal("hero-hall", BuildingIdMigration.ToCanonical("institute"));
        Assert.Equal("training-ground", BuildingIdMigration.ToCanonical("laboratory"));
        Assert.Equal("lumbermill", BuildingIdMigration.ToCanonical("sawmill"));
        Assert.True(BuildingCatalog.TryGet("institute", out var hall));
        Assert.Equal("hero-hall", hall.Id);
        Assert.Equal("Salão dos Heróis", hall.DisplayName);
        Assert.True(BuildingCatalog.TryGet("laboratory", out var field));
        Assert.Equal("training-ground", field.Id);
    }

    [Fact]
    public void Distinct_Roles_AreNotShared()
    {
        Assert.NotEqual(
            CityCapitalLayoutConfig.GetPosition("training-ground").Z,
            CityCapitalLayoutConfig.GetPosition("academy").Z);
        Assert.True(CityCapitalLayoutConfig.GetPosition("hero-hall").X < -2f);
        Assert.Equal(CityDistrict.Economic, CityCapitalLayoutConfig.GetDistrict("residence"));
        Assert.Equal(CityDistrict.Industrial, CityCapitalLayoutConfig.GetDistrict("foundry"));
    }

    [Fact]
    public void OfficialDisplayNames_MatchProductList()
    {
        Assert.Equal("Castelo", BuildingCatalog.Get("castle").DisplayName);
        Assert.Equal("Muralha", BuildingCatalog.Get("wall").DisplayName);
        Assert.Equal("Residência", BuildingCatalog.Get("residence").DisplayName);
        Assert.Equal("Campo de Treinamento", BuildingCatalog.Get("training-ground").DisplayName);
        Assert.Equal("Fundição", BuildingCatalog.Get("foundry").DisplayName);
        Assert.Equal("Salão dos Heróis", BuildingCatalog.Get("hero-hall").DisplayName);
        Assert.Equal("Torre dos Dragões", BuildingCatalog.Get("dragon-tower").DisplayName);
    }
}

public sealed class CityBuildingMigrationTests
{
    [Fact]
    public void Migrate_Legacy15_To17_PreservesLevelsAndAddsNew()
    {
        var legacy = new ProductionSnapshot();
        legacy.BuildingProgress["castle"] = new BuildingProgressRecord
        {
            DefinitionId = "castle",
            Level = 8,
            State = BuildingState.Ready
        };
        legacy.BuildingProgress["institute"] = new BuildingProgressRecord
        {
            DefinitionId = "institute",
            Level = 2,
            State = BuildingState.Ready
        };
        legacy.BuildingProgress["laboratory"] = new BuildingProgressRecord
        {
            DefinitionId = "laboratory",
            Level = 3,
            State = BuildingState.Available
        };
        legacy.Wallet[ResourceType.Gold] = 12345;

        var migrated = BuildingIdMigration.MigrateSnapshot(legacy);
        Assert.Equal(8, migrated.BuildingProgress["castle"].Level);
        Assert.Equal(2, migrated.BuildingProgress["hero-hall"].Level);
        Assert.Equal(BuildingState.Ready, migrated.BuildingProgress["hero-hall"].State);
        Assert.Equal(3, migrated.BuildingProgress["training-ground"].Level);
        Assert.False(migrated.BuildingProgress.ContainsKey("institute"));
        Assert.False(migrated.BuildingProgress.ContainsKey("laboratory"));
        Assert.Equal(0, migrated.BuildingProgress["residence"].Level);
        Assert.Equal(BuildingState.Available, migrated.BuildingProgress["residence"].State);
        Assert.Equal(0, migrated.BuildingProgress["foundry"].Level);
        Assert.Equal(12345, migrated.Wallet[ResourceType.Gold]);
    }

    [Fact]
    public void Migrate_IsIdempotent()
    {
        var first = BuildingIdMigration.MigrateSnapshot(new ProductionSnapshot
        {
            BuildingProgress =
            {
                ["laboratory"] = new BuildingProgressRecord
                {
                    DefinitionId = "laboratory",
                    Level = 1,
                    State = BuildingState.Ready
                }
            }
        });
        var second = BuildingIdMigration.MigrateSnapshot(first);
        Assert.Equal(first.BuildingProgress["training-ground"].Level, second.BuildingProgress["training-ground"].Level);
        Assert.Equal(BuildingIdMigration.OfficialCount, BuildingCatalog.All.Count);
    }
}

public sealed class CityCapitalLayoutTests
{
    [Fact]
    public void Blueprint_HasExactly17LogicalBuildings()
    {
        Assert.Equal(17, CityCapitalLayoutConfig.All.Count);
        Assert.Empty(CityCapitalLayoutConfig.ValidateBlueprintInvariants());
    }

    [Fact]
    public void Blueprint_Axis_GateCastleTowerAligned()
    {
        var gate = CityCapitalLayoutConfig.GetPosition("wall");
        var castle = CityCapitalLayoutConfig.GetPosition("castle");
        var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");

        Assert.Equal(0f, castle.X, 2);
        Assert.Equal(0f, castle.Z, 2);
        Assert.True(gate.Z < castle.Z);
        Assert.True(castle.Z < tower.Z);
        Assert.True(tower.Y > 0.5f);
        Assert.True(System.Math.Abs(gate.X) < 0.5f);
        Assert.True(System.Math.Abs(tower.X) < 1.5f);
    }

    [Fact]
    public void Blueprint_Districts_MatchOfficialRoles()
    {
        Assert.Equal(CityDistrict.Royal, CityCapitalLayoutConfig.GetDistrict("castle"));
        Assert.Equal(CityDistrict.Dragon, CityCapitalLayoutConfig.GetDistrict("dragon-tower"));
        Assert.Equal(CityDistrict.Sacred, CityCapitalLayoutConfig.GetDistrict("hospital"));
        Assert.Equal(CityDistrict.Sacred, CityCapitalLayoutConfig.GetDistrict("temple"));
        Assert.Equal(CityDistrict.Military, CityCapitalLayoutConfig.GetDistrict("arena"));
        Assert.Equal(CityDistrict.Economic, CityCapitalLayoutConfig.GetDistrict("market"));
        Assert.Equal(CityDistrict.Industrial, CityCapitalLayoutConfig.GetDistrict("mine"));
        Assert.Equal(CityDistrict.Rural, CityCapitalLayoutConfig.GetDistrict("farm"));
        Assert.Equal(CityDistrict.Access, CityCapitalLayoutConfig.GetDistrict("wall"));
        Assert.Equal(CityDistrict.Royal, CityCapitalLayoutConfig.GetDistrict("hero-hall"));
        Assert.Equal(CityDistrict.Military, CityCapitalLayoutConfig.GetDistrict("training-ground"));
    }

    [Fact]
    public void EnvironmentTier_MapsCastleLevels()
    {
        Assert.Equal(1, CityCapitalLayoutConfig.ResolveEnvironmentTier(1));
        Assert.Equal(1, CityCapitalLayoutConfig.ResolveEnvironmentTier(5));
        Assert.Equal(2, CityCapitalLayoutConfig.ResolveEnvironmentTier(6));
        Assert.Equal(3, CityCapitalLayoutConfig.ResolveEnvironmentTier(15));
        Assert.Equal(4, CityCapitalLayoutConfig.ResolveEnvironmentTier(16));
        Assert.Equal(5, CityCapitalLayoutConfig.ResolveEnvironmentTier(25));
        Assert.Equal(6, CityCapitalLayoutConfig.ResolveEnvironmentTier(30));
    }

    [Fact]
    public void Blueprint_NoDuplicateBuildingIds()
    {
        var ids = CityCapitalLayoutConfig.All.Keys.ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void MainGate_IsSouthOfWallRadius()
    {
        Assert.True(CityCapitalLayoutConfig.MainGateZ < 0f);
        Assert.True(System.Math.Abs(CityCapitalLayoutConfig.MainGateZ) >= CityCapitalLayoutConfig.WallRadius - 0.01f);
    }
}
