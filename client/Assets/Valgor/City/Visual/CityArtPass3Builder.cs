using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Art Pass 3.0 — vestimenta completa da Capital (só visual).
    /// Não cria BuildingId, não altera Save/seleção/Fases 1–5.
    /// </summary>
    public static class CityArtPass3Builder
    {
        // Paleta vernacular (sem tetos “doce”).
        private static readonly Color RoofTerracotta = new(0.52f, 0.28f, 0.22f);
        private static readonly Color RoofSlate = new(0.32f, 0.34f, 0.38f);
        private static readonly Color RoofThatch = new(0.48f, 0.42f, 0.28f);
        private static readonly Color BodyStone = new(0.55f, 0.52f, 0.46f);
        private static readonly Color BodyTimber = new(0.42f, 0.32f, 0.22f);
        private static readonly Color WallBrown = new(0.48f, 0.42f, 0.36f);

        public static void Build(
            Transform terrain,
            Transform water,
            Transform roads,
            Transform districts,
            Transform decorations,
            Transform vegetation,
            Transform npcs)
        {
            if (!CityArtPass3Config.Enabled)
            {
                return;
            }

            var root = new GameObject("ArtPass3").transform;
            root.SetParent(decorations != null ? decorations : terrain, false);

            if (CityArtPass3Config.DistrictDressingEnabled)
            {
                if (!CityRoyalDistrictConfig.Enabled)
                {
                    BuildImperialDistrict(root, districts);
                }

                BuildDragonEnvirons(root);
                BuildEconomicDistrict(root);
                BuildMilitaryDistrict(root);
            }

            if (CityArtPass3Config.RoadCobbleEnabled && roads != null)
            {
                BuildCobbleAccents(roads);
            }

            if (CityArtPass3Config.WaterPolishEnabled && water != null)
            {
                BuildWaterPolish(water);
            }

            if (CityArtPass3Config.LayeredVegetationEnabled && vegetation != null)
            {
                BuildGardenBeds(vegetation);
            }

            if (CityArtPass3Config.AtmosphereMarkersEnabled && npcs != null)
            {
                BuildStaticNpcMarkers(npcs);
            }

            SoftenVisiblePads(districts);
        }

        /// <summary>Habitação vernacular multi-peça (substitui cubos coloridos).</summary>
        public static void BuildVernacularHousing(Transform decorations)
        {
            if (!CityArtPass3Config.Enabled || !CityArtPass3Config.VernacularHousingEnabled || decorations == null)
            {
                return;
            }

            var root = new GameObject("VernacularHousing").transform;
            root.SetParent(decorations, false);

            BuildHouseRing(root, "RingA", 8.6f, 14, BodyStone, RoofTerracotta);
            BuildHouseRing(root, "RingB", 11.0f, 18, BodyTimber, RoofSlate);
            BuildHouseRing(root, "RingC", 13.4f, 20, BodyStone, RoofThatch);
            BuildHouseRing(root, "RingD", 15.8f, 22, BodyTimber, RoofTerracotta);
            BuildHouseRing(root, "RingE", 17.9f, 18, BodyStone, RoofSlate);

            for (var i = 0; i < 8; i++)
            {
                var a = i * 0.7f;
                var pos = new Vector3(Mathf.Cos(a) * 5.0f, 0f, -3.4f + Mathf.Sin(a) * 2.0f);
                MarketStall(root, $"MarketStall_{i}", pos);
            }
        }

        public static Color FortStone => WallBrown;
        public static Color FortCap => Color.Lerp(WallBrown, CityVisualMaterials.StoneDark, 0.45f);

        private static void BuildHouseRing(
            Transform root,
            string prefix,
            float radius,
            int count,
            Color body,
            Color roof)
        {
            for (var i = 0; i < count; i++)
            {
                var a = (i / (float)count) * Mathf.PI * 2f + 0.09f;
                var pos = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                if (BlockedPlot(pos))
                {
                    continue;
                }

                VernacularHouse(root, $"{prefix}_{i}", pos, body, roof, i);
            }
        }

        private static bool BlockedPlot(Vector3 pos)
        {
            if (Mathf.Abs(pos.x) < 2.4f && pos.z < 2.5f)
            {
                return true;
            }

            if (CityRoyalDistrictConfig.Enabled)
            {
                var exclusion = CityRoyalDistrictConfig.VernacularExclusionRadius;
                if (new Vector2(pos.x, pos.z).magnitude < exclusion)
                {
                    return true;
                }
            }

            if (pos.z < -16f && Mathf.Abs(pos.x) < 4.2f)
            {
                return true;
            }

            var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
            if (Vector3.Distance(new Vector3(pos.x, 0f, pos.z), new Vector3(tower.X, 0f, tower.Z)) < 5.2f)
            {
                return true;
            }

            return false;
        }

        private static void VernacularHouse(Transform root, string name, Vector3 pos, Color body, Color roof, int seed)
        {
            var h = 0.95f + (seed % 3) * 0.22f;
            var w = 1.05f + (seed % 2) * 0.28f;
            var d = w * (0.85f + (seed % 3) * 0.08f);
            var yaw = (seed * 37f) % 360f;

            var host = new GameObject(name).transform;
            host.SetParent(root, false);
            host.localPosition = pos;
            host.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Prop(host, PrimitiveType.Cube, "Body", new Vector3(0f, h * 0.5f, 0f), new Vector3(w, h, d), body);
            Prop(host, PrimitiveType.Cube, "Plinth", new Vector3(0f, 0.08f, 0f), new Vector3(w * 1.05f, 0.16f, d * 1.05f),
                Color.Lerp(body, CityVisualMaterials.StoneDark, 0.35f));
            Prop(host, PrimitiveType.Cube, "Roof", new Vector3(0f, h + 0.22f, 0f), new Vector3(w * 1.22f, 0.38f, d * 1.15f), roof);
            Prop(host, PrimitiveType.Cube, "Ridge", new Vector3(0f, h + 0.42f, 0f), new Vector3(w * 0.2f, 0.12f, d * 1.05f),
                Color.Lerp(roof, CityVisualMaterials.StoneDark, 0.4f));
            if (seed % 3 != 0)
            {
                Prop(host, PrimitiveType.Cube, "Chimney", new Vector3(w * 0.28f, h + 0.55f, -d * 0.15f),
                    new Vector3(0.22f, 0.55f, 0.22f), CityVisualMaterials.StoneDark);
            }

            Prop(host, PrimitiveType.Cube, "Door", new Vector3(0f, 0.42f, d * 0.52f),
                new Vector3(0.28f, 0.7f, 0.06f), CityVisualMaterials.Wood);
            Prop(host, PrimitiveType.Cube, "Window", new Vector3(w * 0.28f, h * 0.55f, d * 0.52f),
                new Vector3(0.22f, 0.28f, 0.05f), new Color(0.35f, 0.42f, 0.48f));
        }

        private static void MarketStall(Transform root, string name, Vector3 pos)
        {
            Prop(root, PrimitiveType.Cube, name, pos + Vector3.up * 0.4f, new Vector3(0.95f, 0.7f, 0.75f), BodyTimber);
            Prop(root, PrimitiveType.Cube, name + "_Roof", pos + Vector3.up * 0.85f,
                new Vector3(1.15f, 0.12f, 0.9f), RoofTerracotta);
            Prop(root, PrimitiveType.Cube, name + "_Crate", pos + new Vector3(0.7f, 0.25f, 0.2f),
                new Vector3(0.35f, 0.35f, 0.35f), CityVisualMaterials.Wood);
        }

        private static void BuildImperialDistrict(Transform root, Transform districts)
        {
            var host = Node(root, "ImperialDressing");
            // Escadaria cerimonial extra + jardins nobres (Castelo permanece no slot).
            Prop(host, PrimitiveType.Cube, "GrandSteps1", new Vector3(0f, 0.14f, -3.4f),
                new Vector3(4.2f, 0.18f, 1.2f), CityVisualMaterials.StoneLight);
            Prop(host, PrimitiveType.Cube, "GrandSteps2", new Vector3(0f, 0.28f, -2.7f),
                new Vector3(3.6f, 0.18f, 1.0f), CityVisualMaterials.StoneDark);
            Prop(host, PrimitiveType.Cube, "GrandSteps3", new Vector3(0f, 0.42f, -2.15f),
                new Vector3(3.0f, 0.18f, 0.9f), CityVisualMaterials.StoneLight);

            Prop(host, PrimitiveType.Cube, "NobleHouseL", new Vector3(-4.8f, 0.7f, 3.2f),
                new Vector3(1.6f, 1.3f, 1.4f), BodyStone);
            Prop(host, PrimitiveType.Cube, "NobleHouseL_Roof", new Vector3(-4.8f, 1.5f, 3.2f),
                new Vector3(1.9f, 0.35f, 1.65f), RoofSlate);
            Prop(host, PrimitiveType.Cube, "NobleHouseR", new Vector3(4.8f, 0.7f, 3.2f),
                new Vector3(1.6f, 1.3f, 1.4f), BodyStone);
            Prop(host, PrimitiveType.Cube, "NobleHouseR_Roof", new Vector3(4.8f, 1.5f, 3.2f),
                new Vector3(1.9f, 0.35f, 1.65f), RoofSlate);

            Prop(host, PrimitiveType.Cube, "HedgeL", new Vector3(-3.2f, 0.35f, 0.2f),
                new Vector3(0.35f, 0.55f, 3.8f), CityVisualMaterials.Vegetation);
            Prop(host, PrimitiveType.Cube, "HedgeR", new Vector3(3.2f, 0.35f, 0.2f),
                new Vector3(0.35f, 0.55f, 3.8f), CityVisualMaterials.Vegetation);
            Prop(host, PrimitiveType.Cylinder, "CentralUrn", new Vector3(0f, 0.55f, 4.2f),
                new Vector3(0.55f, 0.55f, 0.55f), CityVisualMaterials.StoneDark);

            if (districts != null)
            {
                _ = districts;
            }
        }

        private static void BuildDragonEnvirons(Transform root)
        {
            var tz = CityCapitalLayoutConfig.GetPosition("dragon-tower").Z;
            var host = Node(root, "DragonDressing");
            for (var i = 0; i < 8; i++)
            {
                var a = i * 45f * Mathf.Deg2Rad;
                var r = 4.2f + (i % 3) * 0.6f;
                var pos = new Vector3(Mathf.Sin(a) * r, 0.35f + (i % 2) * 0.2f, tz + Mathf.Cos(a) * r * 0.55f - 1.2f);
                Prop(host, PrimitiveType.Sphere, $"DragonRock_{i}", pos,
                    new Vector3(0.9f + (i % 3) * 0.25f, 0.45f, 0.75f),
                    i % 2 == 0 ? CityVisualMaterials.StoneDark : CityVisualMaterials.StoneLight);
            }

            for (var i = 0; i < 6; i++)
            {
                var x = (i % 2 == 0 ? -1f : 1f) * (2.2f + i * 0.35f);
                Prop(host, PrimitiveType.Cylinder, $"DragonPine_{i}", new Vector3(x, 1.1f, tz - 6.8f - i * 0.15f),
                    new Vector3(0.55f, 1.1f, 0.55f), CityVisualMaterials.Vegetation);
                Prop(host, PrimitiveType.Cylinder, $"DragonPineTrunk_{i}", new Vector3(x, 0.4f, tz - 6.8f - i * 0.15f),
                    new Vector3(0.16f, 0.4f, 0.16f), CityVisualMaterials.Wood);
            }

            Prop(host, PrimitiveType.Cube, "ApproachPath", new Vector3(0f, 0.06f, tz - 10.5f),
                new Vector3(2.4f, 0.1f, 6.5f), Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneDark, 0.25f));
            Prop(host, PrimitiveType.Cube, "ApproachStepA", new Vector3(0f, 0.18f, tz - 8.2f),
                new Vector3(2.2f, 0.16f, 0.7f), CityVisualMaterials.StoneDark);
            Prop(host, PrimitiveType.Cube, "ApproachStepB", new Vector3(0f, 0.32f, tz - 7.6f),
                new Vector3(2.0f, 0.16f, 0.65f), CityVisualMaterials.StoneLight);
        }

        private static void BuildEconomicDistrict(Transform root)
        {
            var host = Node(root, "EconomicDressing");
            Vector3[] stalls =
            {
                new(-8.8f, 0f, -8.2f), new(-7.2f, 0f, -6.6f), new(-9.6f, 0f, -5.4f),
                new(-6.4f, 0f, -8.8f), new(-5.2f, 0f, -6.2f), new(-10.2f, 0f, -7.5f)
            };
            for (var i = 0; i < stalls.Length; i++)
            {
                MarketStall(host, $"EconStall_{i}", stalls[i]);
            }

            Prop(host, PrimitiveType.Cube, "WarehouseA", new Vector3(5.6f, 0.7f, -10.4f),
                new Vector3(2.2f, 1.3f, 1.6f), BodyTimber);
            Prop(host, PrimitiveType.Cube, "WarehouseA_Roof", new Vector3(5.6f, 1.5f, -10.4f),
                new Vector3(2.5f, 0.3f, 1.85f), RoofSlate);
            Prop(host, PrimitiveType.Cylinder, "BarrelA", new Vector3(-5.4f, 0.35f, -7.2f),
                new Vector3(0.55f, 0.55f, 0.55f), CityVisualMaterials.Wood);
            Prop(host, PrimitiveType.Cylinder, "BarrelB", new Vector3(-4.8f, 0.35f, -7.6f),
                new Vector3(0.5f, 0.5f, 0.5f), CityVisualMaterials.Wood);
            Prop(host, PrimitiveType.Cube, "CrateStack", new Vector3(-6.6f, 0.4f, -5.0f),
                new Vector3(1.1f, 0.7f, 0.9f), CityVisualMaterials.Wood);
            Prop(host, PrimitiveType.Cube, "CartBody", new Vector3(-4.2f, 0.4f, -8.4f),
                new Vector3(1.5f, 0.5f, 0.85f), BodyTimber);
            Prop(host, PrimitiveType.Cylinder, "CartWheelL", new Vector3(-4.7f, 0.28f, -8.9f),
                new Vector3(0.35f, 0.08f, 0.35f), CityVisualMaterials.StoneDark);
            Prop(host, PrimitiveType.Cylinder, "CartWheelR", new Vector3(-3.7f, 0.28f, -7.9f),
                new Vector3(0.35f, 0.08f, 0.35f), CityVisualMaterials.StoneDark);
            Prop(host, PrimitiveType.Cube, "MarketYard", new Vector3(-7.5f, 0.04f, -7.0f),
                new Vector3(6.5f, 0.06f, 5.2f), Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Path, 0.4f));
        }

        private static void BuildMilitaryDistrict(Transform root)
        {
            var host = Node(root, "MilitaryDressing");
            Prop(host, PrimitiveType.Cube, "DrillYard", new Vector3(12.2f, 0.05f, -2.2f),
                new Vector3(5.8f, 0.08f, 6.5f), Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.25f));
            for (var i = 0; i < 4; i++)
            {
                Prop(host, PrimitiveType.Cube, $"BannerPole_{i}", new Vector3(10.2f + i * 1.1f, 1.1f, 2.4f),
                    new Vector3(0.12f, 2.0f, 0.12f), CityVisualMaterials.Wood);
                Prop(host, PrimitiveType.Cube, $"Banner_{i}", new Vector3(10.55f + i * 1.1f, 1.6f, 2.4f),
                    new Vector3(0.7f, 0.9f, 0.05f), CityVisualMaterials.RoofRed);
            }

            Prop(host, PrimitiveType.Cube, "WeaponRackA", new Vector3(10.6f, 0.75f, -1.2f),
                new Vector3(1.4f, 1.2f, 0.25f), CityVisualMaterials.Wood);
            Prop(host, PrimitiveType.Cube, "WeaponRackB", new Vector3(13.6f, 0.75f, -3.8f),
                new Vector3(1.4f, 1.2f, 0.25f), CityVisualMaterials.Wood);
            Prop(host, PrimitiveType.Cube, "WatchHint", new Vector3(14.8f, 1.4f, 0.4f),
                new Vector3(1.1f, 2.6f, 1.1f), CityVisualMaterials.StoneDark);
            Prop(host, PrimitiveType.Cube, "WatchHintRoof", new Vector3(14.8f, 2.85f, 0.4f),
                new Vector3(1.35f, 0.35f, 1.35f), RoofSlate);
            Prop(host, PrimitiveType.Cube, "HospitalYard", new Vector3(-10.5f, 0.05f, 3.8f),
                new Vector3(3.5f, 0.06f, 3.2f), Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.Vegetation, 0.2f));
            Prop(host, PrimitiveType.Cube, "TentA", new Vector3(10.4f, 0.7f, -6.4f),
                new Vector3(1.5f, 1.15f, 1.3f), RoofTerracotta);
            Prop(host, PrimitiveType.Cube, "TentB", new Vector3(12.2f, 0.65f, -7.0f),
                new Vector3(1.3f, 1.05f, 1.15f), RoofSlate);
        }

        private static void BuildCobbleAccents(Transform roads)
        {
            var host = Node(roads, "CobbleAccents");
            // Cruzamentos e calçadas — overlays leves sobre as estradas existentes.
            Prop(host, PrimitiveType.Cube, "CrossNS", new Vector3(0f, 0.07f, -6f),
                new Vector3(3.2f, 0.05f, 3.2f), Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneDark, 0.2f));
            Prop(host, PrimitiveType.Cube, "CrossEW", new Vector3(0f, 0.07f, 4f),
                new Vector3(3.0f, 0.05f, 3.0f), Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.15f));
            Prop(host, PrimitiveType.Cube, "SidewalkW", new Vector3(-2.1f, 0.06f, -2f),
                new Vector3(0.55f, 0.05f, 14f), CityVisualMaterials.StoneDark);
            Prop(host, PrimitiveType.Cube, "SidewalkE", new Vector3(2.1f, 0.06f, -2f),
                new Vector3(0.55f, 0.05f, 14f), CityVisualMaterials.StoneDark);
            Prop(host, PrimitiveType.Cube, "GateApproach", new Vector3(0f, 0.08f, -16.5f),
                new Vector3(3.6f, 0.06f, 5.5f), Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.3f));
        }

        private static void BuildWaterPolish(Transform water)
        {
            var host = Node(water, "WaterPolish");
            var foam = new Color(0.55f, 0.68f, 0.72f);
            Prop(host, PrimitiveType.Cube, "FallsWest", new Vector3(-23.2f, 0.35f, 8f),
                new Vector3(1.2f, 1.4f, 0.6f), foam);
            Prop(host, PrimitiveType.Cube, "FallsEast", new Vector3(23.2f, 0.35f, -6f),
                new Vector3(1.2f, 1.4f, 0.6f), foam);
            for (var i = 0; i < 6; i++)
            {
                Prop(host, PrimitiveType.Sphere, $"ShoreRockW_{i}",
                    new Vector3(-22.0f + (i % 2) * 0.4f, 0.15f, -8f + i * 2.5f),
                    new Vector3(0.7f, 0.35f, 0.55f), CityVisualMaterials.StoneDark);
                Prop(host, PrimitiveType.Sphere, $"ShoreRockE_{i}",
                    new Vector3(22.0f - (i % 2) * 0.4f, 0.15f, -6f + i * 2.4f),
                    new Vector3(0.7f, 0.35f, 0.55f), CityVisualMaterials.StoneLight);
            }

            Prop(host, PrimitiveType.Cube, "ReedClusterW", new Vector3(-22.5f, 0.2f, 2f),
                new Vector3(1.2f, 0.45f, 3.5f), CityVisualMaterials.Vegetation);
            Prop(host, PrimitiveType.Cube, "ReedClusterE", new Vector3(22.5f, 0.2f, -2f),
                new Vector3(1.2f, 0.45f, 3.5f), CityVisualMaterials.Vegetation);
            Prop(host, PrimitiveType.Cube, "BridgeWestRail", new Vector3(-20.8f, 0.45f, 1.1f),
                new Vector3(3.0f, 0.35f, 0.12f), CityVisualMaterials.Wood);
            Prop(host, PrimitiveType.Cube, "BridgeEastRail", new Vector3(20.8f, 0.45f, -1.1f),
                new Vector3(3.0f, 0.35f, 0.12f), CityVisualMaterials.Wood);
        }

        private static void BuildGardenBeds(Transform vegetation)
        {
            var host = Node(vegetation, "GardenBeds");
            Vector3[] beds =
            {
                new(-4.2f, 0.1f, 2.4f), new(4.2f, 0.1f, 2.4f),
                new(-6.5f, 0.1f, -2.5f), new(6.5f, 0.1f, -2.5f),
                new(-3.5f, 0.1f, -10f), new(3.5f, 0.1f, -10f)
            };
            for (var i = 0; i < beds.Length; i++)
            {
                Prop(host, PrimitiveType.Cube, $"Bed_{i}", beds[i], new Vector3(1.6f, 0.12f, 1.1f),
                    CityVisualMaterials.Dirt);
                Prop(host, PrimitiveType.Cube, $"BedHedge_{i}", beds[i] + Vector3.up * 0.2f,
                    new Vector3(1.4f, 0.25f, 0.9f), CityVisualMaterials.Vegetation);
                Prop(host, PrimitiveType.Sphere, $"BedFlower_{i}", beds[i] + new Vector3(0.2f, 0.35f, 0f),
                    Vector3.one * 0.22f, new Color(0.7f, 0.35f, 0.45f));
            }
        }

        private static void BuildStaticNpcMarkers(Transform npcs)
        {
            var host = Node(npcs, "StaticMarkers");
            (string name, Vector3 pos, Color color)[] markers =
            {
                ("GuardPostL", new Vector3(-3.2f, 0.55f, -18.8f), new Color(0.32f, 0.35f, 0.42f)),
                ("GuardPostR", new Vector3(3.2f, 0.55f, -18.8f), new Color(0.32f, 0.35f, 0.42f)),
                ("MerchantIdle", new Vector3(-7.8f, 0.55f, -7.0f), new Color(0.5f, 0.32f, 0.2f)),
                ("CitizenA", new Vector3(-2.5f, 0.55f, -5.5f), new Color(0.45f, 0.4f, 0.35f)),
                ("CitizenB", new Vector3(2.8f, 0.55f, -4.2f), new Color(0.42f, 0.38f, 0.32f)),
                ("CitizenC", new Vector3(0.5f, 0.55f, 3.5f), new Color(0.48f, 0.42f, 0.36f)),
                ("SoldierA", new Vector3(11.5f, 0.55f, -2.0f), new Color(0.35f, 0.3f, 0.28f)),
                ("SoldierB", new Vector3(12.8f, 0.55f, -4.5f), new Color(0.35f, 0.3f, 0.28f)),
                ("PriestIdle", new Vector3(8.5f, 0.55f, 5.2f), new Color(0.72f, 0.7f, 0.66f)),
                ("HealerIdle", new Vector3(-9.0f, 0.55f, 4.5f), new Color(0.5f, 0.65f, 0.6f)),
                ("FarmerIdle", new Vector3(-15f, 0.55f, -8f), new Color(0.4f, 0.45f, 0.28f)),
                ("NobleIdle", new Vector3(-1.5f, 0.55f, 1.8f), new Color(0.55f, 0.45f, 0.35f))
            };

            var count = Mathf.Min(CityArtPass3Config.StaticNpcMarkerCount, markers.Length);
            for (var i = 0; i < count; i++)
            {
                var m = markers[i];
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = $"StaticNpc_{m.name}";
                go.transform.SetParent(host, false);
                go.transform.localPosition = m.pos;
                go.transform.localScale = new Vector3(0.32f, 0.42f, 0.32f);
                Object.Destroy(go.GetComponent<Collider>());
                var ignore = LayerMask.NameToLayer("Ignore Raycast");
                if (ignore >= 0)
                {
                    go.layer = ignore;
                }

                CityArtMaterialLibrary.ApplyVariant(go.GetComponent<Renderer>(), CityArtMaterialCategory.Wood, m.color, 0.8f);
            }
        }

        private static void SoftenVisiblePads(Transform districts)
        {
            if (!CityArtPass3Config.SoftDistrictPadsEnabled || districts == null)
            {
                return;
            }

            foreach (Transform child in districts)
            {
                if (child == null || !child.name.StartsWith("Pad"))
                {
                    continue;
                }

                var renderer = child.GetComponent<Renderer>();
                if (renderer == null)
                {
                    continue;
                }

                // Mistura com gramado — remove “tapete colorido” de protótipo.
                var soft = Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.25f);
                soft.a = 0.35f;
                CityVisualMaterials.Apply(renderer, soft);
            }
        }

        private static Transform Node(Transform parent, string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }

        private static void Prop(
            Transform parent,
            PrimitiveType type,
            string name,
            Vector3 pos,
            Vector3 scale,
            Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            var ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                go.layer = ignore;
            }

            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
        }
    }
}
