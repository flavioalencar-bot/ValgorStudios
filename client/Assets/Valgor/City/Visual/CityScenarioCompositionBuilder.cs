using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Passo de cenário: aproxima a Capital da composição de referência
    /// (cidade circular, muralha legível, densidade, horizonte) sem assets externos.
    /// </summary>
    public static class CityScenarioCompositionBuilder
    {
        public static void Build(Transform terrain, Transform water, Transform decorations, Transform vegetation)
        {
            if (!CityCapitalFinalConfig.ScenarioCompositionEnabled)
            {
                return;
            }

            BuildMoat(water);
            BuildOuterMountains(terrain);
            BuildForestBelts(vegetation);
            if (CityArtPass3Config.Enabled && CityArtPass3Config.VernacularHousingEnabled)
            {
                CityArtPass3Builder.BuildVernacularHousing(decorations);
            }
            else
            {
                BuildDenseHousing(decorations);
            }

            BuildRuralOuter(decorations);
        }

        private static void BuildMoat(Transform water)
        {
            var waterColor = new Color(0.18f, 0.36f, 0.48f);
            var foam = Color.Lerp(waterColor, Color.white, 0.35f);
            var radius = CityCapitalLayoutConfig.WallRadius + 2.4f;
            const int segs = 24;
            for (var i = 0; i < segs; i++)
            {
                if (i is 0 or 1 or segs - 1)
                {
                    continue;
                }

                var a = i * (360f / segs) * Mathf.Deg2Rad + Mathf.PI;
                var pos = new Vector3(Mathf.Sin(a) * radius, -0.08f, Mathf.Cos(a) * radius);
                var piece = Prop(water, PrimitiveType.Cube, $"Moat_{i}", pos, new Vector3(3.4f, 0.2f, 2.2f), waterColor);
                piece.rotation = Quaternion.LookRotation(new Vector3(-pos.x, 0f, -pos.z));
            }

            Prop(water, PrimitiveType.Cube, "MoatBridgeBed",
                new Vector3(0f, -0.02f, CityCapitalLayoutConfig.MainGateZ - 1.2f),
                new Vector3(4.2f, 0.16f, 3.5f), CityVisualMaterials.StoneDark);
            Prop(water, PrimitiveType.Cube, "MoatFoamS",
                new Vector3(0f, 0.02f, CityCapitalLayoutConfig.MainGateZ - 3.2f),
                new Vector3(3.5f, 0.08f, 1.2f), foam);
        }

        private static void BuildOuterMountains(Transform terrain)
        {
            var rock = Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Vegetation, 0.25f);
            var snow = new Color(0.78f, 0.8f, 0.82f);
            var forest = Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.StoneDark, 0.15f);
            const float radius = 28f;
            const int count = 22;
            for (var i = 0; i < count; i++)
            {
                var a = i * (360f / count) * Mathf.Deg2Rad;
                var r = radius + (i % 3) * 1.8f;
                var pos = new Vector3(Mathf.Sin(a) * r, 2.2f, Mathf.Cos(a) * r);
                var w = 9f + (i % 4) * 1.6f;
                var h = 5.5f + (i % 5) * 1.1f;
                Prop(terrain, PrimitiveType.Sphere, $"ScenarioPeak_{i}", pos, new Vector3(w, h, w * 0.85f), rock);
                if (i % 2 == 0)
                {
                    Prop(terrain, PrimitiveType.Sphere, $"ScenarioSnow_{i}", pos + Vector3.up * (h * 0.45f),
                        new Vector3(w * 0.4f, h * 0.35f, w * 0.35f), snow);
                }

                Prop(terrain, PrimitiveType.Sphere, $"ScenarioForest_{i}", pos + new Vector3(0f, -h * 0.15f, 0f),
                    new Vector3(w * 0.7f, h * 0.35f, w * 0.7f), forest);
            }

            Prop(terrain, PrimitiveType.Cube, "DragonCliffMass", new Vector3(0f, 2.4f, 22.5f),
                new Vector3(14f, 5.2f, 8f), rock);
            Prop(terrain, PrimitiveType.Cube, "DragonCliffFace", new Vector3(0f, 1.8f, 19.8f),
                new Vector3(10f, 3.6f, 3.5f), Color.Lerp(rock, CityVisualMaterials.StoneDark, 0.4f));
        }

        private static void BuildForestBelts(Transform vegetation)
        {
            var trunk = CityVisualMaterials.Wood;
            var canopy = CityVisualMaterials.Vegetation;
            var darkCanopy = Color.Lerp(canopy, CityVisualMaterials.StoneDark, 0.25f);
            PlaceTreeRing(vegetation, "InnerTrees", 18.2f, 28, trunk, canopy);
            PlaceTreeRing(vegetation, "OuterTrees", 26f, 36, trunk, darkCanopy);
        }

        private static void PlaceTreeRing(
            Transform root,
            string prefix,
            float radius,
            int count,
            Color trunk,
            Color canopy)
        {
            for (var i = 0; i < count; i++)
            {
                var a = (i / (float)count) * Mathf.PI * 2f + 0.05f * i;
                var pos = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                if (pos.z < -18f && Mathf.Abs(pos.x) < 4f)
                {
                    continue;
                }

                Prop(root, PrimitiveType.Cylinder, $"{prefix}_Trunk_{i}", pos + Vector3.up * 0.7f,
                    new Vector3(0.22f, 1.4f, 0.22f), trunk);
                if (CityArtPass3Config.Enabled && CityArtPass3Config.LayeredVegetationEnabled)
                {
                    var dark = Color.Lerp(canopy, CityVisualMaterials.StoneDark, 0.25f);
                    // Pinheiro em camadas (sem bola única).
                    var isPine = i % 3 == 0;
                    if (isPine)
                    {
                        Prop(root, PrimitiveType.Cylinder, $"{prefix}_ConeA_{i}", pos + Vector3.up * 1.55f,
                            new Vector3(1.0f, 0.7f, 1.0f), canopy);
                        Prop(root, PrimitiveType.Cylinder, $"{prefix}_ConeB_{i}", pos + Vector3.up * 2.05f,
                            new Vector3(0.7f, 0.55f, 0.7f), dark);
                        Prop(root, PrimitiveType.Cylinder, $"{prefix}_ConeC_{i}", pos + Vector3.up * 2.45f,
                            new Vector3(0.4f, 0.4f, 0.4f), canopy);
                    }
                    else
                    {
                        Prop(root, PrimitiveType.Sphere, $"{prefix}_CanopyA_{i}", pos + Vector3.up * 1.55f,
                            new Vector3(1.15f, 0.7f, 1.0f), canopy);
                        Prop(root, PrimitiveType.Sphere, $"{prefix}_CanopyB_{i}", pos + Vector3.up * 2.05f,
                            new Vector3(0.85f, 0.65f, 0.85f), dark);
                    }
                }
                else
                {
                    Prop(root, PrimitiveType.Sphere, $"{prefix}_Canopy_{i}", pos + Vector3.up * 1.7f,
                        Vector3.one * (1.1f + (i % 3) * 0.15f), canopy);
                }
            }
        }

        private static void BuildDenseHousing(Transform decorations)
        {
            var stone = CityVisualMaterials.StoneLight;
            var wood = CityVisualMaterials.Wood;
            var roofRed = CityVisualMaterials.RoofRed;
            var roofBlue = CityVisualMaterials.RoofBlue;
            var roofOrange = Color.Lerp(roofRed, CityVisualMaterials.Gold, 0.25f);

            BuildHouseRing(decorations, "HouseA", 8.5f, 16, stone, roofRed);
            BuildHouseRing(decorations, "HouseB", 10.8f, 20, wood, roofBlue);
            BuildHouseRing(decorations, "HouseC", 13.2f, 22, stone, roofOrange);
            BuildHouseRing(decorations, "HouseD", 15.6f, 24, wood, roofRed);
            BuildHouseRing(decorations, "HouseE", 17.8f, 20, stone, roofBlue);

            for (var i = 0; i < 10; i++)
            {
                var a = i * 0.55f;
                var pos = new Vector3(Mathf.Cos(a) * 4.8f, 0.35f, -3.2f + Mathf.Sin(a) * 2.2f);
                Prop(decorations, PrimitiveType.Cube, $"Stall_{i}", pos, new Vector3(0.9f, 0.7f, 0.7f), wood);
                Prop(decorations, PrimitiveType.Cube, $"StallRoof_{i}", pos + Vector3.up * 0.45f,
                    new Vector3(1.1f, 0.12f, 0.85f), roofOrange);
            }
        }

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
                var a = (i / (float)count) * Mathf.PI * 2f + 0.11f;
                var pos = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                if (Mathf.Abs(pos.x) < 2.2f && pos.z < 2f)
                {
                    continue;
                }

                if (CityRoyalDistrictConfig.Enabled &&
                    new Vector2(pos.x, pos.z).magnitude < CityRoyalDistrictConfig.VernacularExclusionRadius)
                {
                    continue;
                }

                if (pos.z < -16f && Mathf.Abs(pos.x) < 4f)
                {
                    continue;
                }

                if (Vector3.Distance(new Vector3(pos.x, 0f, pos.z), new Vector3(0f, 0f, 17.5f)) < 4.5f)
                {
                    continue;
                }

                var h = 0.85f + (i % 3) * 0.25f;
                var w = 1.0f + (i % 2) * 0.25f;
                Prop(root, PrimitiveType.Cube, $"{prefix}_{i}", pos + Vector3.up * (h * 0.5f),
                    new Vector3(w, h, w * 0.9f), body);
                Prop(root, PrimitiveType.Cube, $"{prefix}_Roof_{i}", pos + Vector3.up * (h + 0.2f),
                    new Vector3(w * 1.15f, 0.35f, w * 1.05f), roof);
            }
        }

        private static void BuildRuralOuter(Transform decorations)
        {
            var field = Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Gold, 0.2f);
            Prop(decorations, PrimitiveType.Cube, "FieldSW", new Vector3(-16f, 0.04f, -24f),
                new Vector3(8f, 0.08f, 5f), field);
            Prop(decorations, PrimitiveType.Cube, "FieldSE", new Vector3(16f, 0.04f, -24f),
                new Vector3(8f, 0.08f, 5f), field);
            Prop(decorations, PrimitiveType.Cylinder, "WindmillPole", new Vector3(-18f, 1.6f, -22f),
                new Vector3(0.35f, 3.2f, 0.35f), CityVisualMaterials.StoneLight);
            Prop(decorations, PrimitiveType.Cube, "WindmillBlade", new Vector3(-18f, 3.0f, -22f),
                new Vector3(3.2f, 0.2f, 0.35f), CityVisualMaterials.Wood);
        }

        private static Transform Prop(
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
            go.layer = 2;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
            return go.transform;
        }
    }
}
