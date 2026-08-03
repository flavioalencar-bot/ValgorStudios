using System;
using System.Collections.Generic;

namespace Valgor.City.Visual
{
    public enum CityTerrainAssetCategory
    {
        Mountain = 0,
        DistantMountain = 1,
        Cliff = 2,
        Plateau = 3,
        Valley = 4,
        River = 5,
        RiverBank = 6,
        Waterfall = 7,
        Lake = 8,
        Bridge = 9,
        Rock = 10,
        Slope = 11,
        Embankment = 12
    }

    public enum CityTerrainAssetStatus
    {
        Pending = 0,
        Generated = 1,
        Approved = 2,
        Rejected = 3,
        Fallback = 4
    }

    /// <summary>Entrada lógica do kit CapitalTerrainKit_01 (testável sem Unity).</summary>
    public sealed class CityTerrainAssetDefinition
    {
        public CityTerrainAssetDefinition(
            string assetKey,
            string officialFileName,
            CityTerrainAssetCategory category,
            float maxTris,
            string prompt,
            CityTerrainAssetStatus status = CityTerrainAssetStatus.Fallback)
        {
            AssetKey = assetKey;
            OfficialFileName = officialFileName;
            Category = category;
            MaxTris = maxTris;
            Prompt = prompt;
            Status = status;
            AssetId = $"VAL_Capital_Environment_{assetKey}";
        }

        public string AssetKey { get; }
        public string AssetId { get; }
        public string OfficialFileName { get; }
        public CityTerrainAssetCategory Category { get; }
        public float MaxTris { get; }
        public string Prompt { get; }
        public CityTerrainAssetStatus Status { get; set; }
        public string? TripoTaskId { get; set; }
        public string? SourcePath { get; set; }
        public string? RejectionReason { get; set; }
        public bool HasHighFidelity =>
            (Status is CityTerrainAssetStatus.Approved or CityTerrainAssetStatus.Generated) &&
            !string.IsNullOrWhiteSpace(SourcePath);
    }

    /// <summary>Catálogo lógico do kit de terreno — fonte única de IDs/prompts/orçamento.</summary>
    public static class CityTerrainAssetCatalog
    {
        public const string KitId = CityCapitalWorldConfig.KitId;

        private static readonly IReadOnlyList<CityTerrainAssetDefinition> EntriesInternal = BuildKit();

        public static IReadOnlyList<CityTerrainAssetDefinition> Entries => EntriesInternal;

        public static bool TryGet(string assetKey, out CityTerrainAssetDefinition entry)
        {
            for (var i = 0; i < EntriesInternal.Count; i++)
            {
                if (string.Equals(EntriesInternal[i].AssetKey, assetKey, StringComparison.Ordinal))
                {
                    entry = EntriesInternal[i];
                    return true;
                }
            }

            entry = null!;
            return false;
        }

        public static int Count => EntriesInternal.Count;

        public static IReadOnlyList<string> ValidateKitComplete()
        {
            var issues = new List<string>();
            var required = new[]
            {
                "Mountain_A", "Mountain_B", "Mountain_C", "Mountain_D",
                "DistantMountain_A", "DistantMountain_B",
                "Cliff_A", "Cliff_B", "Cliff_C", "Cliff_DragonTower",
                "Plateau_Central", "Plateau_DragonTower", "Plateau_Academy", "Plateau_Quarry", "Plateau_Mine",
                "Valley_A", "Valley_B",
                "River_Straight", "River_Curve_Left", "River_Curve_Right", "River_Wide", "Stream",
                "RiverBank_A", "RiverBank_B", "RiverBank_C",
                "Waterfall_Large", "Waterfall_Medium", "Waterfall_Small",
                "Lake_A",
                "StoneBridge_A", "WoodBridge_A",
                "Rock_Large_A", "Rock_Large_B", "Rock_Medium_A", "Rock_Medium_B", "Rock_Small_A", "Rock_Small_B",
                "Slope_A", "Slope_B", "Embankment_A", "Embankment_B"
            };

            if (EntriesInternal.Count < required.Length)
            {
                issues.Add($"Kit incompleto: {EntriesInternal.Count}/{required.Length}.");
            }

            foreach (var key in required)
            {
                if (!TryGet(key, out _))
                {
                    issues.Add($"Ausente no kit: {key}");
                }
            }

            return issues;
        }

        public static bool AutoReject(CityTerrainAssetDefinition def, float triangleCount, int materialCount, out string reason)
        {
            if (triangleCount > def.MaxTris * 1.15f)
            {
                reason = $"tris {triangleCount:0} > budget {def.MaxTris:0}";
                return true;
            }

            if (materialCount > 4)
            {
                reason = $"materiais {materialCount} > 4";
                return true;
            }

            reason = string.Empty;
            return false;
        }

        private static List<CityTerrainAssetDefinition> BuildKit()
        {
            var list = new List<CityTerrainAssetDefinition>();
            void Add(string key, CityTerrainAssetCategory cat, float maxTris, string prompt) =>
                list.Add(new CityTerrainAssetDefinition(
                    key,
                    $"VAL_Capital_Environment_{key}.glb",
                    cat,
                    maxTris,
                    prompt));

            Add("Mountain_A", CityTerrainAssetCategory.Mountain, CityCapitalWorldConfig.MaxTrisMountainNear,
                "Large modular medieval fantasy mountain for an isometric mobile strategy game. Broad rocky silhouette, layered granite cliffs, natural erosion, moderate height, wide stable base, realistic but game-optimized proportions. No buildings, no ruins, no people, no trees, no water, no snow, no background. Isolated complete 360-degree asset, flat base, central ground pivot, clean topology, PBR rock material, suitable for Unity, mobile and PC optimized.");
            Add("Mountain_B", CityTerrainAssetCategory.Mountain, CityCapitalWorldConfig.MaxTrisMountainNear,
                "Tall narrow fantasy mountain module for an isometric mobile strategy game. Steep layered rock faces, dramatic vertical silhouette, weathered granite, natural ledges and crevices. No buildings, no vegetation, no snow, no water, no characters, no background. Isolated 360-degree model, flat base, central pivot at ground level, clean topology, PBR materials, optimized for Unity mobile and PC.");
            Add("Mountain_C", CityTerrainAssetCategory.Mountain, CityCapitalWorldConfig.MaxTrisMountainDistant,
                "Wide low mountain ridge for a medieval fantasy mobile strategy game. Long horizontal silhouette, layered rocky slopes, natural erosion, several peaks, designed for distant horizon composition. No buildings, trees, water, characters or background. Lightweight isolated mesh, clean topology, PBR rock material, ground pivot, suitable for LOD and Unity mobile.");
            Add("Mountain_D", CityTerrainAssetCategory.Mountain, CityCapitalWorldConfig.MaxTrisMountainDistant,
                "Massive fantasy mountain backdrop with multiple peaks and deep rocky valleys, designed as distant scenery for an isometric medieval strategy game. Strong silhouette, low gameplay detail, broad base, atmospheric scale. No buildings, vegetation, water, snow, people or background. Optimized mesh, clean topology, PBR material, no collider, suitable for Unity horizon LOD.");
            Add("DistantMountain_A", CityTerrainAssetCategory.DistantMountain, CityCapitalWorldConfig.MaxTrisMountainDistant,
                "Distant low-detail fantasy mountain silhouette for isometric mobile strategy horizon. Broad soft peaks, atmospheric scale, minimal surface detail. No buildings, vegetation, water, snow, characters or background. Lightweight isolated mesh, PBR rock, ground pivot, Unity LOD billboard-friendly.");
            Add("DistantMountain_B", CityTerrainAssetCategory.DistantMountain, CityCapitalWorldConfig.MaxTrisMountainDistant,
                "Secondary distant mountain ridge for isometric medieval fantasy horizon. Soft multi-peak silhouette, low poly budget, no gameplay detail. No buildings, trees, water, characters or background. Clean mesh, PBR rock, ground pivot, Unity mobile optimized.");

            Add("Cliff_A", CityTerrainAssetCategory.Cliff, CityCapitalWorldConfig.MaxTrisCliff,
                "Modular vertical granite cliff for a medieval fantasy isometric strategy game. Tall rock face, layered stone, natural cracks, irregular top edge and stable flat back for terrain assembly. No vegetation, buildings, water, characters or background. Isolated 360-degree asset, clean topology, PBR rock material, ground pivot, optimized for Unity mobile.");
            Add("Cliff_B", CityTerrainAssetCategory.Cliff, CityCapitalWorldConfig.MaxTrisCliff,
                "Curved modular fantasy cliff segment for building an organic mountain perimeter in an isometric mobile strategy game. Weathered dark granite, broken ledges, natural erosion and irregular silhouette. No buildings, vegetation, water, characters or background. Clean isolated mesh, PBR material, central ground pivot, optimized for Unity.");
            Add("Cliff_C", CityTerrainAssetCategory.Cliff, CityCapitalWorldConfig.MaxTrisCliff,
                "Short modular fantasy cliff shelf for terrain transitions in an isometric mobile strategy game. Layered granite, irregular crest, stable base. No buildings, vegetation, water, characters or background. Clean mesh, PBR rock, ground pivot, Unity optimized.");
            Add("Cliff_DragonTower", CityTerrainAssetCategory.Cliff, CityCapitalWorldConfig.MaxTrisCliff,
                "Monumental rocky cliff and elevated plateau designed to support a Dragon Tower in a medieval fantasy isometric strategy game. Broad flat upper platform, dramatic south-facing cliff wall, natural access slope and carved stone stair path, dark weathered granite, strong silhouette. No tower, no buildings, no dragon, no vegetation, no water, no characters, no background. Isolated 360-degree asset, clean topology, PBR material, central ground pivot, optimized for Unity mobile and PC.");

            Add("Plateau_Central", CityTerrainAssetCategory.Plateau, CityCapitalWorldConfig.MaxTrisPlateau,
                "Large central terrain plateau for a medieval fantasy isometric mobile strategy game. Broad mostly flat playable top, subtle natural elevation, organic irregular edges, gentle slopes and stone foundation suitable for placing a capital city and multiple buildings. No buildings, roads, vegetation, water, characters or background. Clean low-cost terrain mesh, PBR earth and rock materials, ground pivot, Unity optimized.");
            Add("Plateau_DragonTower", CityTerrainAssetCategory.Plateau, CityCapitalWorldConfig.MaxTrisPlateau,
                "Elevated rocky plateau pad for Dragon Tower placement in an isometric fantasy strategy game. Flat construction top, rocky rim, natural approach. No tower, buildings, vegetation, water, characters or background. Clean mesh, PBR rock, ground pivot, Unity optimized.");
            Add("Plateau_Academy", CityTerrainAssetCategory.Plateau, CityCapitalWorldConfig.MaxTrisPlateau,
                "Medium elevated terrain plateau designed for a fantasy academy building in an isometric mobile strategy game. Flat construction area, gentle access ramp, rocky edges and natural terrain transitions. No building, vegetation, water, characters or background. Isolated clean mesh, PBR terrain material, ground pivot, optimized for Unity.");
            Add("Plateau_Quarry", CityTerrainAssetCategory.Plateau, CityCapitalWorldConfig.MaxTrisPlateau,
                "Rocky quarry plateau pad for isometric fantasy mobile strategy game. Flat work area, cut stone edges, debris-ready base. No buildings, vegetation, water, characters or background. Clean mesh, PBR rock and earth, ground pivot, Unity optimized.");
            Add("Plateau_Mine", CityTerrainAssetCategory.Plateau, CityCapitalWorldConfig.MaxTrisPlateau,
                "Mine entrance plateau pad for isometric fantasy mobile strategy game. Flat pad, rocky mound edges, tunnel mouth hint without interior. No buildings, vegetation, water, characters or background. Clean mesh, PBR rock, ground pivot, Unity optimized.");

            Add("Valley_A", CityTerrainAssetCategory.Valley, CityCapitalWorldConfig.MaxTrisPlateau,
                "Wide natural fantasy valley terrain module for an isometric mobile strategy game. Gentle slopes, broad grass-ready floor, rocky sides and clear space for rivers or roads. No buildings, vegetation, water, characters or background. Clean terrain mesh, PBR earth and rock material, ground pivot, optimized for Unity mobile.");
            Add("Valley_B", CityTerrainAssetCategory.Valley, CityCapitalWorldConfig.MaxTrisPlateau,
                "Secondary valley terrain module for isometric fantasy mobile strategy. Soft slopes, open floor, rocky flanks. No buildings, vegetation, water, characters or background. Clean mesh, PBR earth/rock, ground pivot, Unity optimized.");

            Add("River_Straight", CityTerrainAssetCategory.River, 8000f,
                "Modular straight river channel terrain asset for an isometric mobile fantasy strategy game. Natural curved banks, shallow central channel, stone and soil edges, designed to receive a separate Unity water material. No water surface, no vegetation, no bridge, no buildings, no characters, no background. Clean topology, flat modular ends, ground pivot, optimized for Unity mobile.");
            Add("River_Curve_Left", CityTerrainAssetCategory.River, 8000f,
                "Modular left-curving river channel for an isometric medieval fantasy mobile strategy game. Organic banks, shallow channel, rocky soil edges and seamless modular endpoints. No water surface, vegetation, bridge, buildings, people or background. Clean mesh, PBR earth and rock materials, ground pivot, Unity optimized.");
            Add("River_Curve_Right", CityTerrainAssetCategory.River, 8000f,
                "Modular right-curving river channel for an isometric medieval fantasy mobile strategy game. Natural shallow channel, irregular stone banks and compatible modular endpoints. No water surface, vegetation, bridge, buildings, characters or background. Clean topology, PBR terrain material, ground pivot, optimized for Unity.");
            Add("River_Wide", CityTerrainAssetCategory.River, 10000f,
                "Wide modular river mouth channel for isometric fantasy mobile strategy. Broad shallow bed, soft banks, no water surface mesh. No vegetation, bridges, buildings, characters or background. Clean mesh, PBR earth/rock, ground pivot, Unity optimized.");
            Add("Stream", CityTerrainAssetCategory.River, 5000f,
                "Narrow modular stream channel for isometric fantasy mobile strategy. Shallow bed, rocky edges, no water surface. No vegetation, buildings, characters or background. Lightweight mesh, PBR terrain, ground pivot, Unity optimized.");

            Add("RiverBank_A", CityTerrainAssetCategory.RiverBank, 5000f,
                "Modular rocky riverbank asset for a medieval fantasy isometric mobile strategy game. Natural stones, layered soil, irregular edge and gentle slope designed to border a separate water plane. No water, vegetation, buildings, characters or background. Isolated clean mesh, PBR material, ground pivot, optimized for Unity.");
            Add("RiverBank_B", CityTerrainAssetCategory.RiverBank, 5000f,
                "Secondary modular riverbank with mixed stone and soil for isometric fantasy mobile strategy. Irregular edge, gentle slope. No water, vegetation, buildings, characters or background. Clean mesh, PBR, ground pivot, Unity optimized.");
            Add("RiverBank_C", CityTerrainAssetCategory.RiverBank, 5000f,
                "Coastal-style modular bank for south lake/river mouth in isometric fantasy mobile strategy. Soft slope, pebble texture. No water, vegetation, buildings, characters or background. Clean mesh, PBR, ground pivot, Unity optimized.");

            Add("Waterfall_Large", CityTerrainAssetCategory.Waterfall, CityCapitalWorldConfig.MaxTrisCliff,
                "Large modular fantasy waterfall structure for an isometric mobile strategy game. Tall rocky drop, natural channel, upper and lower rock basins, designed to use separate Unity water and particle effects. No animated water mesh, no buildings, vegetation, characters or background. Isolated rock structure, clean topology, PBR material, ground pivot, optimized for mobile and PC.");
            Add("Waterfall_Medium", CityTerrainAssetCategory.Waterfall, 12000f,
                "Medium rocky waterfall drop for isometric fantasy mobile strategy. Mid-height cliff, basins for separate water FX. No water surface, vegetation, buildings, characters or background. Clean mesh, PBR rock, ground pivot, Unity optimized.");
            Add("Waterfall_Small", CityTerrainAssetCategory.Waterfall, 8000f,
                "Small modular rocky waterfall drop for a medieval fantasy isometric mobile strategy game. Low natural cliff, shallow channel and stone basin, prepared for separate water shader and foam particles. No water surface, vegetation, buildings, people or background. Clean isolated mesh, PBR rock material, Unity optimized.");
            Add("Lake_A", CityTerrainAssetCategory.Lake, 8000f,
                "Shallow lake basin terrain for isometric fantasy mobile strategy. Soft shores, rocky pockets, no water surface mesh. No vegetation, buildings, characters or background. Clean mesh, PBR earth/rock, ground pivot, Unity optimized.");

            Add("StoneBridge_A", CityTerrainAssetCategory.Bridge, CityCapitalWorldConfig.MaxTrisBridge,
                "Medieval fantasy stone bridge for an isometric mobile strategy game. Single broad arch, weathered light granite, low protective side walls, wide enough for troops and carts, monumental but game-optimized. No terrain, water, buildings, vegetation, characters or background. Isolated 360-degree model, flat connection ends, central ground pivot, PBR materials, Unity optimized.");
            Add("WoodBridge_A", CityTerrainAssetCategory.Bridge, CityCapitalWorldConfig.MaxTrisBridge,
                "Medieval wooden bridge for an isometric mobile fantasy strategy game. Heavy timber beams, plank deck, rope and iron reinforcement, slightly weathered but sturdy, suitable for a secondary river crossing. No terrain, water, buildings, vegetation, characters or background. Isolated clean mesh, flat connection ends, PBR wood and metal materials, ground pivot, Unity optimized.");

            Add("Rock_Large_A", CityTerrainAssetCategory.Rock, CityCapitalWorldConfig.MaxTrisRockLarge,
                "Large weathered fantasy granite rock formation for an isometric mobile strategy game. Strong natural silhouette, layered stone, cracks and irregular base. No vegetation, water, buildings, characters or background. Isolated 360-degree mesh, clean topology, PBR rock material, central ground pivot, mobile optimized.");
            Add("Rock_Large_B", CityTerrainAssetCategory.Rock, CityCapitalWorldConfig.MaxTrisRockLarge,
                "Alternate large granite rock formation for isometric fantasy mobile strategy. Irregular silhouette, weathered layers. No vegetation, water, buildings, characters or background. Clean mesh, PBR rock, ground pivot, Unity optimized.");
            Add("Rock_Medium_A", CityTerrainAssetCategory.Rock, CityCapitalWorldConfig.MaxTrisRockMedium,
                "Medium natural granite rock for a medieval fantasy isometric mobile strategy game. Irregular shape, weathered edges and stable base. No vegetation, water, buildings, characters or background. Clean isolated mesh, PBR rock material, ground pivot, optimized for Unity mobile.");
            Add("Rock_Medium_B", CityTerrainAssetCategory.Rock, CityCapitalWorldConfig.MaxTrisRockMedium,
                "Alternate medium granite rock for isometric fantasy mobile strategy. Stable base, weathered edges. No vegetation, water, buildings, characters or background. Clean mesh, PBR, ground pivot, Unity optimized.");
            Add("Rock_Small_A", CityTerrainAssetCategory.Rock, CityCapitalWorldConfig.MaxTrisRockSmall,
                "Small natural rock cluster for a medieval fantasy isometric mobile strategy game. Three to five irregular stones, coherent scale, weathered granite and stable base. No vegetation, water, buildings, characters or background. Lightweight isolated mesh, PBR rock material, Unity optimized.");
            Add("Rock_Small_B", CityTerrainAssetCategory.Rock, CityCapitalWorldConfig.MaxTrisRockSmall,
                "Alternate small rock cluster for isometric fantasy mobile strategy. Lightweight, stable base. No vegetation, water, buildings, characters or background. Clean mesh, PBR rock, Unity optimized.");

            Add("Slope_A", CityTerrainAssetCategory.Slope, 8000f,
                "Modular grass-ready terrain slope for isometric fantasy mobile strategy. Gentle incline, irregular edges. No buildings, vegetation, water, characters or background. Clean mesh, PBR earth, ground pivot, Unity optimized.");
            Add("Slope_B", CityTerrainAssetCategory.Slope, 8000f,
                "Steeper modular terrain slope for isometric fantasy mobile strategy. Rocky mix, irregular crest. No buildings, vegetation, water, characters or background. Clean mesh, PBR earth/rock, ground pivot, Unity optimized.");
            Add("Embankment_A", CityTerrainAssetCategory.Embankment, 6000f,
                "Modular earth embankment for roads and river edges in isometric fantasy mobile strategy. Compact soil face, flat top. No buildings, vegetation, water, characters or background. Clean mesh, PBR dirt, ground pivot, Unity optimized.");
            Add("Embankment_B", CityTerrainAssetCategory.Embankment, 6000f,
                "Stone-reinforced embankment module for isometric fantasy mobile strategy. Flat top, rocky face. No buildings, vegetation, water, characters or background. Clean mesh, PBR stone/dirt, ground pivot, Unity optimized.");

            return list;
        }
    }
}
