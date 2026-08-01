using System;
using System.Collections.Generic;
using Valgor.City.Camera;

namespace Valgor.City.Visual
{
    /// <summary>Distritos oficiais da Capital de Valgor (blueprint visual).</summary>
    public enum CityDistrict
    {
        Royal = 0,
        Dragon = 1,
        Sacred = 2,
        Military = 3,
        Economic = 4,
        Industrial = 5,
        Rural = 6,
        Access = 7
    }

    /// <summary>
    /// Entrada de layout visual. Não altera estado lógico/save do edifício.
    /// Catálogo oficial permanece com 15 IDs; Portão = visual da Muralha ao sul.
    /// </summary>
    [Serializable]
    public sealed class CityBuildingLayoutEntry
    {
        public CityBuildingLayoutEntry(
            string buildingId,
            CityPosition position,
            CityDistrict district,
            int visualPriority = 0,
            float yawDegrees = 0f,
            float scale = 1f,
            int minimumEnvironmentTier = 1,
            string blueprintRole = "")
        {
            BuildingId = buildingId ?? throw new ArgumentNullException(nameof(buildingId));
            Position = position;
            District = district;
            VisualPriority = visualPriority;
            YawDegrees = yawDegrees;
            Scale = scale <= 0f ? 1f : scale;
            MinimumEnvironmentTier = Math.Clamp(minimumEnvironmentTier, 1, 6);
            BlueprintRole = blueprintRole ?? string.Empty;
        }

        public string BuildingId { get; }
        public CityPosition Position { get; }
        public CityDistrict District { get; }
        public int VisualPriority { get; }
        public float YawDegrees { get; }
        public float Scale { get; }
        public int MinimumEnvironmentTier { get; }
        public string BlueprintRole { get; }
    }

    /// <summary>
    /// Fonte única de coordenadas do blueprint "VALGOR — CAPITAL DO REINO".
    /// Eixo: Portão (S) → Praça → Castelo → Torre dos Dragões (N).
    /// </summary>
    public static class CityCapitalLayoutConfig
    {
        public const float WallRadius = 20.5f;
        public const float MainGateZ = -20.5f;
        public const float ImperialRoadHalfWidth = 1.55f;

        private static readonly Dictionary<string, CityBuildingLayoutEntry> Entries = BuildEntries();

        public static IReadOnlyDictionary<string, CityBuildingLayoutEntry> All => Entries;

        public static bool TryGet(string buildingId, out CityBuildingLayoutEntry entry) =>
            Entries.TryGetValue(buildingId, out entry!);

        public static CityPosition GetPosition(string buildingId) =>
            TryGet(buildingId, out var entry) ? entry.Position : new CityPosition(0f, 0f, 0f);

        public static CityDistrict GetDistrict(string buildingId) =>
            TryGet(buildingId, out var entry) ? entry.District : CityDistrict.Royal;

        public static int ResolveEnvironmentTier(int castleLevel)
        {
            if (castleLevel <= 0)
            {
                return 1;
            }

            if (castleLevel <= 5) return 1;
            if (castleLevel <= 10) return 2;
            if (castleLevel <= 15) return 3;
            if (castleLevel <= 20) return 4;
            if (castleLevel <= 25) return 5;
            return 6;
        }

        /// <summary>
        /// Validações estruturais do blueprint (sem Unity).
        /// </summary>
        public static IReadOnlyList<string> ValidateBlueprintInvariants()
        {
            var issues = new List<string>();
            if (!TryGet("castle", out var castle) ||
                Math.Abs(castle.Position.X) > 0.01f ||
                Math.Abs(castle.Position.Z) > 0.01f)
            {
                issues.Add("Castelo deve estar no centro (0,0).");
            }

            if (!TryGet("dragon-tower", out var tower) || tower.Position.Z < 14f)
            {
                issues.Add("Torre dos Dragões deve ficar ao norte elevado.");
            }

            if (!TryGet("wall", out var wall) || wall.Position.Z > -16f)
            {
                issues.Add("Portão/Muralha lógica deve ancorar o extremo sul.");
            }

            if (TryGet("castle", out _) && TryGet("wall", out _) && TryGet("dragon-tower", out _))
            {
                var gate = GetPosition("wall");
                var keep = GetPosition("castle");
                var north = GetPosition("dragon-tower");
                if (Math.Abs(gate.X) > 0.5f || Math.Abs(keep.X) > 0.5f || Math.Abs(north.X) > 1.5f)
                {
                    issues.Add("Eixo monumental Portão→Castelo→Torre deve permanecer alinhado em X≈0.");
                }

                if (!(gate.Z < keep.Z && keep.Z < north.Z))
                {
                    issues.Add("Ordem do eixo deve ser Portão (S) < Castelo < Torre (N).");
                }
            }

            if (Entries.Count != 15)
            {
                issues.Add($"Esperados 15 edifícios lógicos, encontrados {Entries.Count}.");
            }

            return issues;
        }

        private static Dictionary<string, CityBuildingLayoutEntry> BuildEntries()
        {
            // Y > 0 = platô / relevo visual (só apresentação).
            return new Dictionary<string, CityBuildingLayoutEntry>(StringComparer.Ordinal)
            {
                ["castle"] = new(
                    "castle",
                    new CityPosition(0f, 0f, 0f),
                    CityDistrict.Royal,
                    visualPriority: 100,
                    blueprintRole: "Castelo — núcleo absoluto"),

                // Instituto ocupa o slot visual do Salão dos Heróis (NW) sem novo ID de gameplay.
                ["institute"] = new(
                    "institute",
                    new CityPosition(-6.8f, 0f, 5.2f),
                    CityDistrict.Royal,
                    visualPriority: 40,
                    yawDegrees: 25f,
                    blueprintRole: "Salão dos Heróis (mapa visual)"),

                ["dragon-tower"] = new(
                    "dragon-tower",
                    new CityPosition(0f, 1.35f, 17.6f),
                    CityDistrict.Dragon,
                    visualPriority: 90,
                    blueprintRole: "Torre dos Dragões — platô norte"),

                ["hospital"] = new(
                    "hospital",
                    new CityPosition(-9.2f, 0f, 4.2f),
                    CityDistrict.Sacred,
                    visualPriority: 35,
                    blueprintRole: "Hospital — distrito sagrado NW"),

                ["temple"] = new(
                    "temple",
                    new CityPosition(9.0f, 0f, 4.8f),
                    CityDistrict.Sacred,
                    visualPriority: 35,
                    blueprintRole: "Templo — distrito sagrado NE"),

                ["arena"] = new(
                    "arena",
                    new CityPosition(11.2f, 0f, 0.8f),
                    CityDistrict.Military,
                    visualPriority: 45,
                    blueprintRole: "Arena — leste do Castelo"),

                // Laboratório ocupa o slot visual do Campo de Treinamento.
                ["laboratory"] = new(
                    "laboratory",
                    new CityPosition(12.4f, 0f, -5.6f),
                    CityDistrict.Military,
                    visualPriority: 30,
                    yawDegrees: -15f,
                    blueprintRole: "Campo de Treinamento (mapa visual)"),

                ["academy"] = new(
                    "academy",
                    new CityPosition(14.2f, 0.95f, 9.5f),
                    CityDistrict.Military,
                    visualPriority: 40,
                    blueprintRole: "Academia — platô NE"),

                ["market"] = new(
                    "market",
                    new CityPosition(-8.6f, 0f, -6.4f),
                    CityDistrict.Economic,
                    visualPriority: 50,
                    blueprintRole: "Mercado — SW interno"),

                ["warehouse"] = new(
                    "warehouse",
                    new CityPosition(5.6f, 0f, -9.0f),
                    CityDistrict.Economic,
                    visualPriority: 30,
                    blueprintRole: "Armazém — SSE interno"),

                ["lumbermill"] = new(
                    "lumbermill",
                    new CityPosition(-13.5f, 0f, -13.2f),
                    CityDistrict.Industrial,
                    visualPriority: 25,
                    blueprintRole: "Serraria — sul externo oeste"),

                ["quarry"] = new(
                    "quarry",
                    new CityPosition(15.2f, 0.85f, 12.2f),
                    CityDistrict.Industrial,
                    visualPriority: 25,
                    blueprintRole: "Pedreira — NE montanhoso"),

                ["mine"] = new(
                    "mine",
                    new CityPosition(-15.0f, 0.85f, 11.8f),
                    CityDistrict.Industrial,
                    visualPriority: 25,
                    blueprintRole: "Mina — NW montanhoso"),

                ["farm"] = new(
                    "farm",
                    new CityPosition(-14.2f, 0f, -7.5f),
                    CityDistrict.Rural,
                    visualPriority: 25,
                    blueprintRole: "Fazenda — SW externo"),

                // Único edifício lógico "wall"; âncora visual do Portão principal ao sul.
                ["wall"] = new(
                    "wall",
                    new CityPosition(0f, 0f, -19.2f),
                    CityDistrict.Access,
                    visualPriority: 80,
                    blueprintRole: "Portão Principal / Muralha")
            };
        }
    }
}
