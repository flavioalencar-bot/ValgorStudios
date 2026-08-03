using System;
using System.Collections.Generic;
using Valgor.City.Buildings;
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
    /// Layout oficial — exatamente 17 edifícios canônicos.
    /// Eixo: Portão (S) → Praça → Castelo → Torre dos Dragões (N).
    /// </summary>
    public static class CityCapitalLayoutConfig
    {
        public const float WallRadius = 20.5f;
        public const float MainGateZ = -20.5f;
        public const float ImperialRoadHalfWidth = 1.55f;

        private static readonly Dictionary<string, CityBuildingLayoutEntry> Entries = BuildEntries();

        public static IReadOnlyDictionary<string, CityBuildingLayoutEntry> All => Entries;

        public static bool TryGet(string buildingId, out CityBuildingLayoutEntry entry)
        {
            var canonical = BuildingIdMigration.ToCanonical(buildingId);
            return Entries.TryGetValue(canonical, out entry!);
        }

        public static CityPosition GetPosition(string buildingId) =>
            TryGet(buildingId, out var entry) ? entry.Position : new CityPosition(0f, 0f, 0f);

        public static CityDistrict GetDistrict(string buildingId) =>
            TryGet(buildingId, out var entry) ? entry.District : CityDistrict.Royal;

        public static int ResolveEnvironmentTier(int castleLevel)
        {
            if (castleLevel <= 0) return 1;
            if (castleLevel <= 5) return 1;
            if (castleLevel <= 10) return 2;
            if (castleLevel <= 15) return 3;
            if (castleLevel <= 20) return 4;
            if (castleLevel <= 25) return 5;
            return 6;
        }

        public static IReadOnlyList<string> ValidateBlueprintInvariants()
        {
            var issues = new List<string>();
            if (Entries.Count != BuildingIdMigration.OfficialCount)
            {
                issues.Add($"Esperados {BuildingIdMigration.OfficialCount} edifícios, encontrados {Entries.Count}.");
            }

            foreach (var id in BuildingIdMigration.CanonicalIds)
            {
                if (!Entries.ContainsKey(id))
                {
                    issues.Add($"Layout ausente: {id}");
                }
            }

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

            if (TryGet("wall", out _) && TryGet("castle", out _) && TryGet("dragon-tower", out _))
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

            if (TryGet("hero-hall", out var hall) && TryGet("castle", out _))
            {
                if (hall.Position.X >= -2f || hall.Position.Z < 2f)
                {
                    issues.Add("Salão dos Heróis deve ficar a oeste/noroeste do Castelo.");
                }
            }

            if (TryGet("training-ground", out var field) && TryGet("academy", out var academy))
            {
                if (Math.Abs(field.Position.X - academy.Position.X) < 0.5f &&
                    Math.Abs(field.Position.Z - academy.Position.Z) < 0.5f)
                {
                    issues.Add("Campo de Treinamento e Academia não podem coincidir.");
                }
            }

            return issues;
        }

        private static Dictionary<string, CityBuildingLayoutEntry> BuildEntries()
        {
            return new Dictionary<string, CityBuildingLayoutEntry>(StringComparer.Ordinal)
            {
                ["castle"] = new("castle", new CityPosition(0f, 0f, 0f), CityDistrict.Royal, 100,
                    blueprintRole: "Castelo"),
                ["wall"] = new("wall", new CityPosition(0f, 0f, -19.2f), CityDistrict.Access, 80,
                    blueprintRole: "Muralha / Portão"),
                ["farm"] = new("farm", new CityPosition(-14.2f, 0f, -7.5f), CityDistrict.Rural, 25,
                    blueprintRole: "Fazenda"),
                ["lumbermill"] = new("lumbermill", new CityPosition(-13.5f, 0f, -13.2f), CityDistrict.Industrial, 25,
                    blueprintRole: "Serraria"),
                ["quarry"] = new("quarry", new CityPosition(15.2f, 0.85f, 12.2f), CityDistrict.Industrial, 25,
                    blueprintRole: "Pedreira"),
                ["mine"] = new("mine", new CityPosition(-15.0f, 0.85f, 11.8f), CityDistrict.Industrial, 25,
                    blueprintRole: "Mina"),
                ["warehouse"] = new("warehouse", new CityPosition(5.6f, 0f, -9.0f), CityDistrict.Economic, 30,
                    blueprintRole: "Armazém"),
                ["residence"] = new("residence", new CityPosition(-5.8f, 0f, -8.8f), CityDistrict.Economic, 35,
                    yawDegrees: 12f, blueprintRole: "Residência"),
                ["market"] = new("market", new CityPosition(-8.6f, 0f, -6.4f), CityDistrict.Economic, 50,
                    blueprintRole: "Mercado"),
                ["training-ground"] = new("training-ground", new CityPosition(12.4f, 0f, -5.6f), CityDistrict.Military, 30,
                    yawDegrees: -15f, blueprintRole: "Campo de Treinamento"),
                ["foundry"] = new("foundry", new CityPosition(12.8f, 0f, -13.0f), CityDistrict.Industrial, 30,
                    blueprintRole: "Fundição"),
                ["arena"] = new("arena", new CityPosition(11.2f, 0f, 0.8f), CityDistrict.Military, 45,
                    blueprintRole: "Arena"),
                ["academy"] = new("academy", new CityPosition(14.2f, 0.95f, 9.5f), CityDistrict.Military, 40,
                    blueprintRole: "Academia"),
                ["hospital"] = new("hospital", new CityPosition(-9.2f, 0f, 4.2f), CityDistrict.Sacred, 35,
                    blueprintRole: "Hospital"),
                ["temple"] = new("temple", new CityPosition(9.0f, 0f, 4.8f), CityDistrict.Sacred, 35,
                    blueprintRole: "Templo"),
                ["hero-hall"] = new("hero-hall", new CityPosition(-6.8f, 0f, 5.2f), CityDistrict.Royal, 40,
                    yawDegrees: 25f, blueprintRole: "Salão dos Heróis"),
                ["dragon-tower"] = new("dragon-tower", new CityPosition(0f, 1.35f, 25.5f), CityDistrict.Dragon, 90,
                    blueprintRole: "Torre dos Dragões")
            };
        }
    }
}
