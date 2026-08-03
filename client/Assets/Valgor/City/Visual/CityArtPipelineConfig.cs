using System;
using System.Collections.Generic;

namespace Valgor.City.Visual
{
    /// <summary>Constantes da pipeline de arte de alta fidelidade (testável sem Unity).</summary>
    public static class CityArtPipelineConfig
    {
        public const bool Enabled = true;
        public const string PackCoreId = "CapitalArtPack_Core_v1";
        public const string NamingPrefix = "VAL_Capital_";
        public const string HighFidelityChildName = "ArtHF";
        public const string VisualRootName = "Visual";
        /// <summary>Root visual exclusivo da muralha (filho de Visual) — sem lógica.</summary>
        public const string WallVisualRootName = "WallVisualRoot";
        public const string CatalogResourcesKey = "Valgor/CityArtAssetCatalog";
        public const int LodLevels = 4;

        public static readonly string[] ForbiddenScriptTypeNames =
        {
            "BuildingView",
            "BuildingSlot",
            "CityController",
            "DragonNestView",
            "CityHudController",
            "BuildingSelectionPresenter"
        };

        public static readonly string[] CorePackAssetIds =
        {
            "architecture.castle",
            "architecture.dragon-tower",
            "architecture.wall-segment-a",
            "architecture.wall-segment-b",
            "architecture.wall-segment-c",
            "architecture.wall-segment-d",
            "architecture.gate",
            "architecture.bridge",
            "architecture.fountain",
            "environment.rock-a",
            "environment.rock-b",
            "environment.rock-c",
            "environment.rock-d",
            "environment.rock-e",
            "environment.tree-a",
            "environment.tree-b",
            "environment.tree-c",
            "environment.tree-d",
            "environment.tree-e",
            "environment.bush-a",
            "environment.bush-b",
            "environment.bush-c",
            "props.house-decor-a",
            "props.house-decor-b",
            "props.house-decor-c",
            "materials.water",
            "environment.mountain-distant"
        };

        public static readonly string[] OfficialBuildingAssetIds =
        {
            "building.castle",
            "building.wall",
            "building.farm",
            "building.lumbermill",
            "building.quarry",
            "building.mine",
            "building.warehouse",
            "building.residence",
            "building.market",
            "building.training-ground",
            "building.foundry",
            "building.arena",
            "building.academy",
            "building.hospital",
            "building.temple",
            "building.hero-hall",
            "building.dragon-tower"
        };

        public static string AssetIdForBuilding(string buildingId) =>
            "building." + (buildingId ?? string.Empty).Trim().ToLowerInvariant();

        public static string PrefabName(string category, string name, string variant) =>
            $"{NamingPrefix}{category}_{name}_{variant}";

        public static bool MatchesNamingConvention(string prefabName) =>
            !string.IsNullOrWhiteSpace(prefabName) &&
            prefabName.StartsWith(NamingPrefix, StringComparison.Ordinal);

        /// <summary>
        /// Impede cruzamento Castle ↔ DragonTower no catálogo/runtime.
        /// Prefab null/empty = sem HF (ok).
        /// </summary>
        public static bool PrefabMatchesBuilding(string buildingId, string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return true;
            }

            var id = (buildingId ?? string.Empty).Trim().ToLowerInvariant();
            var name = prefabName.Trim();
            var isDragonTowerPrefab =
                name.IndexOf("DragonTower", StringComparison.OrdinalIgnoreCase) >= 0;
            var isCastlePrefab =
                name.IndexOf("Castle", StringComparison.OrdinalIgnoreCase) >= 0 &&
                !isDragonTowerPrefab;
            var isWallGatePrefab =
                name.IndexOf("WallGate", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (name.IndexOf("Wall", StringComparison.OrdinalIgnoreCase) >= 0 &&
                 !isCastlePrefab &&
                 !isDragonTowerPrefab);

            if (id == "castle")
            {
                return isCastlePrefab && !isDragonTowerPrefab && !isWallGatePrefab;
            }

            if (id == "dragon-tower")
            {
                return isDragonTowerPrefab;
            }

            if (id == "wall")
            {
                return isWallGatePrefab;
            }

            return true;
        }

        public static bool IsForbiddenScript(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return false;
            }

            for (var i = 0; i < ForbiddenScriptTypeNames.Length; i++)
            {
                if (string.Equals(ForbiddenScriptTypeNames[i], typeName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<string> ValidateDescriptorRules(CityArtAssetDescriptor d)
        {
            var issues = new List<string>();
            if (d == null)
            {
                issues.Add("descriptor null");
                return issues;
            }

            if (string.IsNullOrWhiteSpace(d.AssetId))
            {
                issues.Add("AssetId vazio");
            }

            if (!d.SlotReady)
            {
                issues.Add("slot não pronto");
            }

            return issues;
        }
    }

    /// <summary>Descritor lógico (paths Resources) — sem referências UnityEngine.Object.</summary>
    public sealed class CityArtAssetDescriptor
    {
        public string AssetId { get; set; } = string.Empty;
        public string BuildingId { get; set; } = string.Empty;
        public string ResourcesKeyHigh { get; set; } = string.Empty;
        public string ResourcesKeyMedium { get; set; } = string.Empty;
        public string ResourcesKeyLow { get; set; } = string.Empty;
        public bool UsesExistingCastleLoader { get; set; }
        public bool SlotReady { get; set; } = true;
        public bool ContentAvailable { get; set; }
    }

    public enum CityArtResolveMode
    {
        HighFidelity = 0,
        Medium = 1,
        Low = 2,
        ProceduralFallback = 3,
        ExistingCastleLoader = 4
    }

    public readonly struct CityArtResolveResult
    {
        public CityArtResolveResult(
            string assetId,
            string buildingId,
            CityArtResolveMode mode,
            string resourcesKey,
            bool usedFallback)
        {
            AssetId = assetId;
            BuildingId = buildingId;
            Mode = mode;
            ResourcesKey = resourcesKey;
            UsedFallback = usedFallback;
        }

        public string AssetId { get; }
        public string BuildingId { get; }
        public CityArtResolveMode Mode { get; }
        public string ResourcesKey { get; }
        public bool UsedFallback { get; }
    }

    /// <summary>Catálogo lógico padrão — slots prontos, conteúdo ausente até o pack chegar.</summary>
    public static class CityArtAssetCatalogLogic
    {
        public static List<CityArtAssetDescriptor> CreateDefaultBuildingSlots()
        {
            var list = new List<CityArtAssetDescriptor>(CityArtPipelineConfig.OfficialBuildingAssetIds.Length);
            foreach (var buildingId in BuildingIdList())
            {
                var assetId = CityArtPipelineConfig.AssetIdForBuilding(buildingId);
                list.Add(new CityArtAssetDescriptor
                {
                    AssetId = assetId,
                    BuildingId = buildingId,
                    ResourcesKeyHigh = $"Valgor/Art/Capital/Buildings/{buildingId}_High",
                    ResourcesKeyMedium = $"Valgor/Art/Capital/Buildings/{buildingId}_Medium",
                    ResourcesKeyLow = $"Valgor/Art/Capital/Buildings/{buildingId}_Low",
                    UsesExistingCastleLoader = buildingId == "castle",
                    SlotReady = true,
                    ContentAvailable = false
                });
            }

            return list;
        }

        public static List<CityArtAssetDescriptor> CreateCorePackSlots()
        {
            var list = new List<CityArtAssetDescriptor>(CityArtPipelineConfig.CorePackAssetIds.Length);
            foreach (var id in CityArtPipelineConfig.CorePackAssetIds)
            {
                list.Add(new CityArtAssetDescriptor
                {
                    AssetId = id,
                    BuildingId = string.Empty,
                    ResourcesKeyHigh = $"Valgor/Art/Capital/Core/{id.Replace('.', '/')}_High",
                    ResourcesKeyMedium = $"Valgor/Art/Capital/Core/{id.Replace('.', '/')}_Medium",
                    ResourcesKeyLow = $"Valgor/Art/Capital/Core/{id.Replace('.', '/')}_Low",
                    SlotReady = true,
                    ContentAvailable = false
                });
            }

            return list;
        }

        public static CityArtAssetDescriptor? FindByBuildingId(
            IReadOnlyList<CityArtAssetDescriptor> entries,
            string buildingId)
        {
            if (entries == null || string.IsNullOrWhiteSpace(buildingId))
            {
                return null;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                if (string.Equals(entries[i].BuildingId, buildingId, StringComparison.Ordinal))
                {
                    return entries[i];
                }
            }

            return null;
        }

        public static CityArtResolveResult Resolve(
            CityArtAssetDescriptor? entry,
            string buildingId,
            bool highLoaded,
            bool mediumLoaded,
            bool lowLoaded,
            int qualityRank)
        {
            if (entry == null || !entry.SlotReady)
            {
                return new CityArtResolveResult(
                    CityArtPipelineConfig.AssetIdForBuilding(buildingId),
                    buildingId,
                    CityArtResolveMode.ProceduralFallback,
                    string.Empty,
                    usedFallback: true);
            }

            // qualityRank: 2=High, 1=Medium, 0=Low — HF do catálogo tem prioridade sobre loader legado.
            if (qualityRank >= 2 && highLoaded)
            {
                return new CityArtResolveResult(
                    entry.AssetId, buildingId, CityArtResolveMode.HighFidelity, entry.ResourcesKeyHigh, false);
            }

            if (qualityRank >= 1 && mediumLoaded)
            {
                return new CityArtResolveResult(
                    entry.AssetId, buildingId, CityArtResolveMode.Medium, entry.ResourcesKeyMedium, false);
            }

            if (lowLoaded)
            {
                return new CityArtResolveResult(
                    entry.AssetId, buildingId, CityArtResolveMode.Low, entry.ResourcesKeyLow, false);
            }

            if (mediumLoaded)
            {
                return new CityArtResolveResult(
                    entry.AssetId, buildingId, CityArtResolveMode.Medium, entry.ResourcesKeyMedium, false);
            }

            if (highLoaded)
            {
                return new CityArtResolveResult(
                    entry.AssetId, buildingId, CityArtResolveMode.HighFidelity, entry.ResourcesKeyHigh, false);
            }

            // Sem pack HF: Castelo continua no loader Resources homologado.
            if (entry.UsesExistingCastleLoader)
            {
                return new CityArtResolveResult(
                    entry.AssetId,
                    buildingId,
                    CityArtResolveMode.ExistingCastleLoader,
                    string.Empty,
                    usedFallback: false);
            }

            return new CityArtResolveResult(
                entry.AssetId,
                buildingId,
                CityArtResolveMode.ProceduralFallback,
                string.Empty,
                usedFallback: true);
        }

        private static IEnumerable<string> BuildingIdList()
        {
            yield return "castle";
            yield return "wall";
            yield return "farm";
            yield return "lumbermill";
            yield return "quarry";
            yield return "mine";
            yield return "warehouse";
            yield return "residence";
            yield return "market";
            yield return "training-ground";
            yield return "foundry";
            yield return "arena";
            yield return "academy";
            yield return "hospital";
            yield return "temple";
            yield return "hero-hall";
            yield return "dragon-tower";
        }
    }
}
