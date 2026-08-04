using System;
using System.Collections.Generic;
using Valgor.City.Buildings;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Configuração central da fundação ambiental da Capital (camadas 1–2).
    /// Coordenadas espacialmente alinhadas ao layout ativo (V1 oficial / V2 preview).
    /// </summary>
    public static class CityCapitalWorldConfig
    {
        public const string KitId = "CapitalTerrainKit_01";
        public const string WorldRootName = "CapitalWorldFoundation";
        public static bool Enabled => true;

        public const float WorldBoundsHalf = 56f;
        public const float PlayableRadius = 28f;
        public const float HorizonRadius = 46f;
        public const float DistantHorizonRadius = 58f;

        public const float ReservedBuildingRadius = 3.6f;
        public const float ReservedPlazaRadius = 5.5f;
        public const float ReservedRoadHalfWidth = 2.2f;
        public const float ReservedWallClearance = 2.8f;

        public const float MaxTrisMountainNear = 30000f;
        public const float MaxTrisMountainDistant = 12000f;
        public const float MaxTrisCliff = 20000f;
        public const float MaxTrisPlateau = 25000f;
        public const float MaxTrisBridge = 20000f;
        public const float MaxTrisRockLarge = 8000f;
        public const float MaxTrisRockMedium = 4000f;
        public const float MaxTrisRockSmall = 2000f;

        public static float VegetationDensity(CityAmbientQuality q) =>
            q switch
            {
                CityAmbientQuality.Low => 0.35f,
                CityAmbientQuality.High => 1.15f,
                _ => 0.75f
            };

        public static float RockDensity(CityAmbientQuality q) =>
            q switch
            {
                CityAmbientQuality.Low => 0.3f,
                CityAmbientQuality.High => 1.1f,
                _ => 0.7f
            };

        public static float WaterQualityFactor(CityAmbientQuality q) =>
            q switch
            {
                CityAmbientQuality.Low => 0.4f,
                CityAmbientQuality.High => 1.2f,
                _ => 0.85f
            };

        public static bool AllowDistantMountainShadows(CityAmbientQuality q) =>
            q >= CityAmbientQuality.High;

        public static bool AllowWaterParticles(CityAmbientQuality q) =>
            q >= CityAmbientQuality.Medium;

        public static bool AllowExtraFog(CityAmbientQuality q) =>
            q >= CityAmbientQuality.Medium;

        public static float LodNear = 22f;
        public static float LodMid = 40f;
        public static float LodFar = 70f;

        /// <summary>Slots oficiais — área reservada (sem props atravessando).</summary>
        public static IReadOnlyList<WorldReservedArea> ReservedBuildingAreas()
        {
            var list = new List<WorldReservedArea>(BuildingIdMigration.OfficialCount + 2);
            foreach (var id in BuildingIdMigration.CanonicalIds)
            {
                var p = CityCapitalLayoutConfig.GetPosition(id);
                var r = id switch
                {
                    "castle" => 6.5f,
                    "wall" => 4.5f,
                    "dragon-tower" => 5.0f,
                    "arena" => 4.8f,
                    "market" => 4.2f,
                    _ => ReservedBuildingRadius
                };
                list.Add(new WorldReservedArea(id, p.X, p.Z, r));
            }

            var castle = CityCapitalLayoutConfig.GetPosition("castle");
            var wall = CityCapitalLayoutConfig.GetPosition("wall");
            var plazaZ = (castle.Z + wall.Z) * 0.45f;
            list.Add(new WorldReservedArea("plaza", 0f, plazaZ, ReservedPlazaRadius));
            return list;
        }

        public static bool OverlapsReserved(float x, float z, float radius)
        {
            foreach (var area in ReservedBuildingAreas())
            {
                var dx = x - area.X;
                var dz = z - area.Z;
                var min = area.Radius + radius;
                if (dx * dx + dz * dz < min * min)
                {
                    return true;
                }
            }

            return false;
        }

        public static IReadOnlyList<WorldPropPlacement> HorizonMountains()
        {
            // Dentro do overview (~±32): silhuetas altas e escuras, sem discos.
            return new[]
            {
                new WorldPropPlacement("DistantMountain_A", 0f, 0.2f, 34f, 18f, 14f, 12f, 10f),
                new WorldPropPlacement("DistantMountain_B", 30f, 0.2f, 22f, -55f, 12f, 11f, 9f),
                new WorldPropPlacement("Mountain_C", -30f, 0.2f, 16f, 40f, 12f, 9f, 9f),
                new WorldPropPlacement("Mountain_D", 32f, 0.2f, -14f, 155f, 13f, 12f, 10f),
                new WorldPropPlacement("Mountain_A", -28f, 0.2f, -18f, -120f, 11f, 10f, 8f),
                new WorldPropPlacement("Mountain_B", 18f, 0.2f, 32f, 70f, 8f, 14f, 7f),
                new WorldPropPlacement("Mountain_A", -16f, 0.2f, 33f, 200f, 10f, 11f, 8f),
                new WorldPropPlacement("Mountain_C", 8f, 0.2f, -34f, -20f, 13f, 9f, 9f)
            };
        }

        public static IReadOnlyList<WorldPropPlacement> NearCliffsAndPlateaus()
        {
            var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
            var academy = CityCapitalLayoutConfig.GetPosition("academy");
            var quarry = CityCapitalLayoutConfig.GetPosition("quarry");
            var mine = CityCapitalLayoutConfig.GetPosition("mine");
            return new[]
            {
                new WorldPropPlacement("Cliff_DragonTower", tower.X, Math.Max(0.2f, tower.Y - 0.4f), tower.Z + 1.2f, 0f, 7.5f, 4.2f, 7.0f),
                new WorldPropPlacement("Plateau_DragonTower", tower.X, tower.Y * 0.5f, tower.Z, 12f, 5.2f, 2.2f, 5.0f),
                new WorldPropPlacement("Plateau_Central", 0.4f, -0.02f, 1.5f, 0f, 22f, 0.35f, 20f),
                new WorldPropPlacement("Plateau_Academy", academy.X, Math.Max(0.4f, academy.Y - 0.2f), academy.Z, -20f, 5.5f, 1.2f, 5.0f),
                new WorldPropPlacement("Plateau_Quarry", quarry.X, 0.55f, quarry.Z, 25f, 5.0f, 1.0f, 4.5f),
                new WorldPropPlacement("Plateau_Mine", mine.X, 0.55f, mine.Z, -30f, 5.0f, 1.0f, 4.5f),
                new WorldPropPlacement("Cliff_A", tower.X + 6.5f, 1.1f, tower.Z - 2f, 35f, 3.5f, 4.5f, 2.2f),
                new WorldPropPlacement("Cliff_B", tower.X - 5.5f, 1.0f, tower.Z - 1.5f, -40f, 3.2f, 4.2f, 2.0f),
                new WorldPropPlacement("Valley_A", -30f, -0.05f, 2f, 10f, 14f, 0.4f, 10f),
                new WorldPropPlacement("Valley_B", 30f, -0.05f, -4f, -8f, 14f, 0.4f, 10f)
            };
        }

        public static IReadOnlyList<WorldPropPlacement> RiverSegments()
        {
            // Contorna o jogável — azul vivo, dentro do overview.
            return new[]
            {
                new WorldPropPlacement("River_Straight", -24.5f, -0.08f, 0f, 0f, 3.8f, 0.28f, 40f),
                new WorldPropPlacement("River_Curve_Left", -25.2f, -0.07f, 15f, 25f, 3.4f, 0.26f, 11f),
                new WorldPropPlacement("River_Curve_Right", -24.8f, -0.07f, -15f, -25f, 3.2f, 0.26f, 11f),
                new WorldPropPlacement("River_Straight", 24.5f, -0.08f, 0f, 0f, 3.8f, 0.28f, 40f),
                new WorldPropPlacement("River_Curve_Right", 25.0f, -0.07f, 13f, -20f, 3.3f, 0.26f, 11f),
                new WorldPropPlacement("River_Curve_Left", 24.8f, -0.07f, -13f, 20f, 3.2f, 0.26f, 11f),
                new WorldPropPlacement("River_Wide", 0.5f, -0.1f, -30f, 90f, 14f, 0.24f, 4.2f),
                new WorldPropPlacement("Stream", 22f, -0.06f, 26f, 35f, 2.2f, 0.2f, 8f),
                new WorldPropPlacement("RiverBank_A", -22.6f, 0.01f, 0f, 0f, 1.2f, 0.16f, 38f),
                new WorldPropPlacement("RiverBank_B", 22.6f, 0.01f, 0f, 0f, 1.2f, 0.16f, 38f),
                new WorldPropPlacement("RiverBank_C", 0.5f, 0.01f, -27.5f, 90f, 12f, 0.14f, 2.0f),
                new WorldPropPlacement("Lake_A", -29f, -0.12f, 22f, 0f, 6f, 0.25f, 5f)
            };
        }

        public static IReadOnlyList<WorldPropPlacement> WaterfallsAndBridges()
        {
            return new[]
            {
                new WorldPropPlacement("Waterfall_Large", -26f, 0.5f, 16f, 15f, 3.8f, 5.0f, 2.4f),
                new WorldPropPlacement("Waterfall_Medium", 26.5f, 0.4f, -11f, -20f, 2.8f, 3.4f, 1.9f),
                new WorldPropPlacement("Waterfall_Small", -25f, 0.25f, -9f, 10f, 1.9f, 2.3f, 1.3f),
                new WorldPropPlacement("StoneBridge_A", -21.2f, 0.1f, 0f, 0f, 4.4f, 0.55f, 3.1f),
                new WorldPropPlacement("WoodBridge_A", 21.2f, 0.1f, 0f, 0f, 4.2f, 0.45f, 2.9f),
                new WorldPropPlacement("StoneBridge_A", 0.5f, 0.12f, -26.8f, 90f, 5.2f, 0.55f, 3.3f)
            };
        }

        public static IReadOnlyList<WorldPropPlacement> RockScatter(CityAmbientQuality quality)
        {
            var density = RockDensity(quality);
            var seeds = new (float x, float z, string id, float s)[]
            {
                (-23.2f, 6f, "Rock_Large_A", 1.4f),
                (-23.6f, -7f, "Rock_Large_B", 1.2f),
                (23.4f, -5f, "Rock_Large_A", 1.5f),
                (23.8f, 8f, "Rock_Medium_A", 1.1f),
                (-22.8f, 1f, "Rock_Medium_B", 0.9f),
                (19f, 20f, "Rock_Large_B", 1.6f),
                (-18f, 21f, "Rock_Medium_A", 1.2f),
                (16f, -24f, "Rock_Small_A", 0.7f),
                (-15f, -25f, "Rock_Small_B", 0.65f),
                (32f, 8f, "Rock_Large_A", 1.8f),
                (-33f, -6f, "Rock_Medium_B", 1.3f),
                (8f, 30f, "Rock_Small_A", 0.8f),
                (-8f, 31f, "Rock_Small_B", 0.75f),
                (34f, -18f, "Rock_Medium_A", 1.1f),
                (-34f, 14f, "Rock_Large_B", 1.7f)
            };

            var list = new List<WorldPropPlacement>();
            var budget = Math.Max(4, (int)(seeds.Length * density));
            for (var i = 0; i < budget && i < seeds.Length; i++)
            {
                var s = seeds[i];
                if (OverlapsReserved(s.x, s.z, s.s * 0.6f))
                {
                    continue;
                }

                list.Add(new WorldPropPlacement(s.id, s.x, 0.15f, s.z, i * 17f, s.s, s.s * 0.55f, s.s));
            }

            return list;
        }

        public static IReadOnlyList<string> ValidateInvariants()
        {
            var issues = new List<string>();
            if (!Enabled)
            {
                return issues;
            }

            var reserved = ReservedBuildingAreas();
            if (reserved.Count < BuildingIdMigration.OfficialCount)
            {
                issues.Add("Reserved areas incompletas para os 17 edifícios.");
            }

            foreach (var river in RiverSegments())
            {
                // Segmentos longos: usar metade da menor dimensão como raio efetivo (largura do canal).
                var channelRadius = Math.Min(river.ScaleX, river.ScaleZ) * 0.55f;
                if (OverlapsReserved(river.X, river.Z, channelRadius))
                {
                    issues.Add($"Rio atravessa área reservada perto de ({river.X:0.0},{river.Z:0.0}).");
                }
            }

            foreach (var rock in RockScatter(CityAmbientQuality.Medium))
            {
                if (OverlapsReserved(rock.X, rock.Z, rock.ScaleX * 0.5f))
                {
                    issues.Add($"Rocha atravessa slot em ({rock.X:0.0},{rock.Z:0.0}).");
                }
            }

            var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
            if (tower.X < 0f && CityCapitalLayoutRuntime.IsPreviewV2)
            {
                issues.Add("Preview V2: Torre deveria estar a leste (X>0).");
            }

            return issues;
        }
    }

    public readonly struct WorldReservedArea
    {
        public WorldReservedArea(string id, float x, float z, float radius)
        {
            Id = id;
            X = x;
            Z = z;
            Radius = radius;
        }

        public string Id { get; }
        public float X { get; }
        public float Z { get; }
        public float Radius { get; }
    }

    public readonly struct WorldPropPlacement
    {
        public WorldPropPlacement(
            string assetKey,
            float x,
            float y,
            float z,
            float yawDegrees,
            float scaleX,
            float scaleY,
            float scaleZ)
        {
            AssetKey = assetKey;
            X = x;
            Y = y;
            Z = z;
            YawDegrees = yawDegrees;
            ScaleX = scaleX;
            ScaleY = scaleY;
            ScaleZ = scaleZ;
        }

        public string AssetKey { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float YawDegrees { get; }
        public float ScaleX { get; }
        public float ScaleY { get; }
        public float ScaleZ { get; }
    }
}
