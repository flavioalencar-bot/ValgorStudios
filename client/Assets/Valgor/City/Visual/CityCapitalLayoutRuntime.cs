using System;
using System.Collections.Generic;
using Valgor.City.Buildings;
using Valgor.City.Camera;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Runtime do layout espacial da Capital.
    /// V1 = oficial (default). V2 = preview QA até aprovação visual.
    /// </summary>
    public static class CityCapitalLayoutRuntime
    {
        public const int Version1 = 1;
        public const int Version2 = 2;

        /// <summary>Versão efetiva usada por <see cref="CityLayout"/> / bootstrap.</summary>
        public static int ActiveVersion { get; private set; } = Version1;

        /// <summary>True quando V2 está ativo só para preview (não oficial, não migrado).</summary>
        public static bool IsPreviewV2 { get; private set; }

        public static void UseOfficialV1()
        {
            ActiveVersion = Version1;
            IsPreviewV2 = false;
        }

        /// <summary>Ativa V2 apenas em memória para homologação visual.</summary>
        public static void UsePreviewV2()
        {
            ActiveVersion = Version2;
            IsPreviewV2 = true;
        }

        public static void ResetForTests()
        {
            UseOfficialV1();
        }
    }

    /// <summary>
    /// Coordenadas oficiais do layout V2 (Capital mobile orgânica).
    /// Não é o default até promoção pós-aprovação do preview.
    /// </summary>
    public static class CityCapitalLayoutV2
    {
        public const float MainGateZ = -20.5f;
        public const float PlazaCenterZ = -4.0f;
        public const float WallAnchorRadius = 21.5f;

        public static IReadOnlyDictionary<string, CityBuildingLayoutEntry> Entries { get; } =
            BuildEntries();

        public static bool TryGet(string buildingId, out CityBuildingLayoutEntry entry)
        {
            var canonical = BuildingIdMigration.ToCanonical(buildingId);
            return Entries.TryGetValue(canonical, out entry!);
        }

        public static CityPosition GetPosition(string buildingId) =>
            TryGet(buildingId, out var entry) ? entry.Position : new CityPosition(0f, 0f, 0f);

        public static IReadOnlyList<string> ValidateInvariants()
        {
            var issues = new List<string>();
            if (Entries.Count != BuildingIdMigration.OfficialCount)
            {
                issues.Add($"V2: esperados {BuildingIdMigration.OfficialCount}, há {Entries.Count}.");
            }

            foreach (var id in BuildingIdMigration.CanonicalIds)
            {
                if (!Entries.ContainsKey(id))
                {
                    issues.Add($"V2 ausente: {id}");
                }
            }

            if (!TryGet("castle", out var castle))
            {
                issues.Add("V2: castelo ausente.");
            }
            else
            {
                if (Math.Abs(castle.Position.X) > 3.5f)
                {
                    issues.Add("V2: Castelo deve permanecer perto do eixo cívico (|X|≤3.5).");
                }

                if (castle.Position.Z < 1.5f || castle.Position.Z > 8f)
                {
                    issues.Add("V2: Castelo deve estar recuado ao norte do mercado (Z∈[1.5,8]).");
                }
            }

            if (!TryGet("dragon-tower", out var tower) ||
                tower.Position.X < 8f ||
                tower.Position.Z < 12f ||
                tower.Position.Y < 1.5f)
            {
                issues.Add("V2: Torre deve ficar no quadrante NE elevado (X≥8, Z≥12, Y≥1.5).");
            }

            if (!TryGet("wall", out var wall) || wall.Position.Z > -16f)
            {
                issues.Add("V2: Portão/Muralha deve ancorar o sul.");
            }

            if (!TryGet("market", out var market) ||
                Math.Abs(market.Position.X) > 4f ||
                market.Position.Z > -6f)
            {
                issues.Add("V2: Mercado deve ficar sul/centro.");
            }

            if (!TryGet("arena", out var arena) || arena.Position.X < 8f)
            {
                issues.Add("V2: Arena deve ficar a leste.");
            }

            if (!TryGet("academy", out var academy) || academy.Position.X > -8f)
            {
                issues.Add("V2: Academia deve ficar a oeste/noroeste.");
            }

            if (!TryGet("training-ground", out var field) ||
                field.Position.X > -6f ||
                field.Position.Z > -4f)
            {
                issues.Add("V2: Campo deve ficar no setor militar sudoeste/oeste.");
            }

            // Overlap crítico: distância XZ mínima entre centros.
            const float minDist = 4.2f;
            var ids = new List<string>(Entries.Keys);
            for (var i = 0; i < ids.Count; i++)
            {
                var a = Entries[ids[i]].Position;
                for (var j = i + 1; j < ids.Count; j++)
                {
                    var b = Entries[ids[j]].Position;
                    var dx = a.X - b.X;
                    var dz = a.Z - b.Z;
                    var d = Math.Sqrt(dx * dx + dz * dz);
                    if (d < minDist)
                    {
                        issues.Add($"V2 overlap crítico: {ids[i]}↔{ids[j]} dist={d:0.00}<{minDist}.");
                    }
                }
            }

            return issues;
        }

        private static Dictionary<string, CityBuildingLayoutEntry> BuildEntries()
        {
            // Coordenadas derivadas da auditoria capital-reference-layout-audit (P0/P1).
            return new Dictionary<string, CityBuildingLayoutEntry>(StringComparer.Ordinal)
            {
                ["castle"] = new("castle", new CityPosition(0.6f, 0.12f, 3.8f), CityDistrict.Royal, 100,
                    yawDegrees: 0f, blueprintRole: "Castelo (foco, recuado)"),
                ["wall"] = new("wall", new CityPosition(0.4f, 0f, MainGateZ), CityDistrict.Access, 80,
                    blueprintRole: "Muralha / Portão sul"),
                ["dragon-tower"] = new("dragon-tower", new CityPosition(14.8f, 2.45f, 17.8f), CityDistrict.Dragon, 90,
                    blueprintRole: "Torre NE penhasco"),
                ["arena"] = new("arena", new CityPosition(13.2f, 0f, 1.6f), CityDistrict.Military, 45,
                    blueprintRole: "Arena leste"),
                ["academy"] = new("academy", new CityPosition(-13.8f, 1.05f, 5.8f), CityDistrict.Military, 40,
                    blueprintRole: "Academia oeste elevada"),
                ["training-ground"] = new("training-ground", new CityPosition(-12.2f, 0f, -8.8f), CityDistrict.Military, 30,
                    yawDegrees: 18f, blueprintRole: "Campo / Quartel sudoeste"),
                ["temple"] = new("temple", new CityPosition(10.2f, 0.15f, 6.4f), CityDistrict.Sacred, 35,
                    blueprintRole: "Templo leste"),
                ["hospital"] = new("hospital", new CityPosition(-10.4f, 0f, -2.8f), CityDistrict.Sacred, 35,
                    blueprintRole: "Hospital oeste/sul"),
                ["market"] = new("market", new CityPosition(-1.2f, 0f, -9.2f), CityDistrict.Economic, 50,
                    blueprintRole: "Mercado sul/centro"),
                ["foundry"] = new("foundry", new CityPosition(13.4f, 0f, -11.8f), CityDistrict.Industrial, 30,
                    blueprintRole: "Fundição sudeste"),
                ["hero-hall"] = new("hero-hall", new CityPosition(-7.2f, 0.25f, 6.8f), CityDistrict.Royal, 40,
                    yawDegrees: 20f, blueprintRole: "Salão NO do Castelo"),
                ["warehouse"] = new("warehouse", new CityPosition(4.2f, 0f, -10.2f), CityDistrict.Economic, 30,
                    blueprintRole: "Armazém entre mercado e leste"),
                ["residence"] = new("residence", new CityPosition(-4.8f, 0f, -6.6f), CityDistrict.Economic, 35,
                    yawDegrees: 10f, blueprintRole: "Residência entre mercado e castelo"),
                ["farm"] = new("farm", new CityPosition(-16.8f, 0f, -3.2f), CityDistrict.Rural, 25,
                    blueprintRole: "Fazenda periferia oeste"),
                ["lumbermill"] = new("lumbermill", new CityPosition(-15.2f, 0f, -14.2f), CityDistrict.Industrial, 25,
                    blueprintRole: "Serraria periferia SO"),
                ["quarry"] = new("quarry", new CityPosition(16.6f, 0.95f, 9.2f), CityDistrict.Industrial, 25,
                    blueprintRole: "Pedreira periferia leste"),
                ["mine"] = new("mine", new CityPosition(-16.2f, 0.95f, 9.8f), CityDistrict.Industrial, 25,
                    blueprintRole: "Mina periferia oeste")
            };
        }
    }
}
