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

        /// <summary>
        /// Yaw do Castelo: frente Tripo (escadaria/bandeiras no −Z do GLB) aponta ao Portão (−Z).
        /// Mesh Tier 6 já tem a entrada no −Z local — yaw 0, sem inverter.
        /// </summary>
        public const float CastleFacingGateYawDegrees = 0f;

        private static readonly Dictionary<string, CityBuildingLayoutEntry> Entries = BuildEntries();

        public static IReadOnlyDictionary<string, CityBuildingLayoutEntry> All =>
            CityCapitalLayoutRuntime.ActiveVersion == CityCapitalLayoutRuntime.Version2
                ? CityCapitalLayoutV2.Entries
                : Entries;

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

        public static bool TryGet(string buildingId, out CityBuildingLayoutEntry entry)
        {
            if (CityCapitalLayoutRuntime.ActiveVersion == CityCapitalLayoutRuntime.Version2)
            {
                return CityCapitalLayoutV2.TryGet(buildingId, out entry);
            }

            var canonical = BuildingIdMigration.ToCanonical(buildingId);
            return Entries.TryGetValue(canonical, out entry!);
        }

        public static CityPosition GetPosition(string buildingId) =>
            TryGet(buildingId, out var entry) ? entry.Position : new CityPosition(0f, 0f, 0f);

        public static CityDistrict GetDistrict(string buildingId) =>
            TryGet(buildingId, out var entry) ? entry.District : CityDistrict.Royal;

        /// <summary>Invariantes da versão ativa (V1 oficial ou V2 preview).</summary>
        public static IReadOnlyList<string> ValidateBlueprintInvariants()
        {
            if (CityCapitalLayoutRuntime.ActiveVersion == CityCapitalLayoutRuntime.Version2)
            {
                return CityCapitalLayoutV2.ValidateInvariants();
            }

            return ValidateV1Invariants();
        }

        private static IReadOnlyList<string> ValidateV1Invariants()
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

            if (!Entries.TryGetValue("castle", out var castle) ||
                Math.Abs(castle.Position.X) > 0.01f ||
                Math.Abs(castle.Position.Z) > 0.01f)
            {
                issues.Add("Castelo deve estar no centro (0,0).");
            }

            if (!Entries.TryGetValue("dragon-tower", out var tower) || tower.Position.Z < 14f)
            {
                issues.Add("Torre dos Dragões deve ficar ao norte elevado.");
            }

            if (!Entries.TryGetValue("wall", out var wall) || wall.Position.Z > -16f)
            {
                issues.Add("Portão/Muralha lógica deve ancorar o extremo sul.");
            }

            if (Entries.ContainsKey("wall") && Entries.ContainsKey("castle") && Entries.ContainsKey("dragon-tower"))
            {
                var gate = Entries["wall"].Position;
                var keep = Entries["castle"].Position;
                var north = Entries["dragon-tower"].Position;
                if (Math.Abs(gate.X) > 0.5f || Math.Abs(keep.X) > 0.5f || Math.Abs(north.X) > 1.5f)
                {
                    issues.Add("Eixo monumental Portão→Castelo→Torre deve permanecer alinhado em X≈0.");
                }

                if (!(gate.Z < keep.Z && keep.Z < north.Z))
                {
                    issues.Add("Ordem do eixo deve ser Portão (S) < Castelo < Torre (N).");
                }

                if (Math.Abs(gate.Z - MainGateZ) > 0.05f)
                {
                    issues.Add($"Portão deve coincidir com MainGateZ ({MainGateZ}).");
                }
            }

            if (Entries.TryGetValue("castle", out var castleFacing))
            {
                var yaw = castleFacing.YawDegrees % 360f;
                if (yaw < 0f)
                {
                    yaw += 360f;
                }

                if (Math.Abs(yaw - CastleFacingGateYawDegrees) > 0.5f &&
                    Math.Abs(yaw - (CastleFacingGateYawDegrees - 360f)) > 0.5f)
                {
                    issues.Add("Castelo deve encarar o Portão (yaw 0°; frente Tripo no −Z local).");
                }
            }

            if (Entries.TryGetValue("hero-hall", out var hall))
            {
                if (hall.Position.X >= -2f || hall.Position.Z < 2f)
                {
                    issues.Add("Salão dos Heróis deve ficar a oeste/noroeste do Castelo.");
                }
            }

            if (Entries.TryGetValue("training-ground", out var field) &&
                Entries.TryGetValue("academy", out var academy))
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
                    yawDegrees: CastleFacingGateYawDegrees, blueprintRole: "Castelo"),
                ["wall"] = new("wall", new CityPosition(0f, 0f, MainGateZ), CityDistrict.Access, 80,
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
