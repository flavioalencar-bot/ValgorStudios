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
            // Fundo cinematográfico — fora do jogável, sem collider (raio visível no overview mobile).
            return new[]
            {
                new WorldPropPlacement("DistantMountain_A", 0f, 3.2f, 44f, 18f, 16f, 11f, 12f),
                new WorldPropPlacement("DistantMountain_B", 36f, 2.8f, 28f, -55f, 14f, 10f, 11f),
                new WorldPropPlacement("Mountain_C", -38f, 2.6f, 18f, 40f, 13f, 8f, 10f),
                new WorldPropPlacement("Mountain_D", 40f, 3.5f, -16f, 155f, 15f, 12f, 12f),
                new WorldPropPlacement("Mountain_A", -34f, 2.4f, -22f, -120f, 11f, 7.5f, 9f),
                new WorldPropPlacement("Mountain_B", 22f, 3.0f, 38f, 70f, 8f, 12f, 7f),
                new WorldPropPlacement("Mountain_A", -18f, 2.2f, 42f, 200f, 10f, 7f, 8f),
                new WorldPropPlacement("Mountain_C", 10f, 2.4f, -42f, -20f, 14f, 7.5f, 10f)
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
            // Contorna o jogável — fora dos 17 slots (raio canal estreito).
            return new[]
            {
                new WorldPropPlacement("River_Straight", -27.5f, -0.12f, 0f, 0f, 3.6f, 0.22f, 42f),
                new WorldPropPlacement("River_Curve_Left", -28.2f, -0.11f, 17f, 25f, 3.2f, 0.2f, 12f),
                new WorldPropPlacement("River_Curve_Right", -27.8f, -0.11f, -17f, -25f, 3.0f, 0.2f, 12f),
                new WorldPropPlacement("River_Straight", 27.5f, -0.12f, 0f, 0f, 3.6f, 0.22f, 42f),
                new WorldPropPlacement("River_Curve_Right", 28.0f, -0.11f, 15f, -20f, 3.1f, 0.2f, 12f),
                new WorldPropPlacement("River_Curve_Left", 27.8f, -0.11f, -15f, 20f, 3.0f, 0.2f, 12f),
                new WorldPropPlacement("River_Wide", 0.5f, -0.14f, -33f, 90f, 16f, 0.18f, 4.5f),
                new WorldPropPlacement("Stream", 20f, -0.1f, 24f, 35f, 2.0f, 0.15f, 9f),
                new WorldPropPlacement("RiverBank_A", -25.4f, -0.02f, 0f, 0f, 1.1f, 0.14f, 40f),
                new WorldPropPlacement("RiverBank_B", 25.4f, -0.02f, 0f, 0f, 1.1f, 0.14f, 40f),
                new WorldPropPlacement("RiverBank_C", 0.5f, -0.02f, -30.5f, 90f, 14f, 0.12f, 2.0f),
                new WorldPropPlacement("Lake_A", -32f, -0.16f, 24f, 0f, 6.5f, 0.2f, 5.5f)
            };
        }

        public static IReadOnlyList<WorldPropPlacement> WaterfallsAndBridges()
        {
            return new[]
            {
                new WorldPropPlacement("Waterfall_Large", -28f, 0.6f, 18f, 15f, 3.5f, 4.5f, 2.2f),
                new WorldPropPlacement("Waterfall_Medium", 28.5f, 0.45f, -12f, -20f, 2.6f, 3.2f, 1.8f),
                new WorldPropPlacement("Waterfall_Small", -27f, 0.3f, -10f, 10f, 1.8f, 2.2f, 1.2f),
                new WorldPropPlacement("StoneBridge_A", -22.5f, 0.08f, 0f, 0f, 4.2f, 0.5f, 3.0f),
                new WorldPropPlacement("WoodBridge_A", 22.5f, 0.08f, 0f, 0f, 4.0f, 0.4f, 2.8f),
                new WorldPropPlacement("StoneBridge_A", 0.5f, 0.1f, -28.5f, 90f, 5.0f, 0.5f, 3.2f)
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
