using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Sprint 2 — Distrito Real / complexo imperial ao redor do Castelo.
    /// Só visual. Não move Castelo / Torre / Portão / Muralha. Sem BuildingId/Save.
    /// </summary>
    public static class CityRoyalDistrictBuilder
    {
        private static readonly Color StoneA = new(0.58f, 0.54f, 0.48f);
        private static readonly Color StoneB = new(0.42f, 0.39f, 0.35f);
        private static readonly Color Cobble = new(0.48f, 0.45f, 0.40f);
        private static readonly Color RoofNoble = new(0.28f, 0.30f, 0.34f);
        private static readonly Color BodyNoble = new(0.62f, 0.58f, 0.52f);
        private static readonly Color Water = new(0.25f, 0.42f, 0.52f);
        private static readonly Color Hedge = new(0.28f, 0.42f, 0.24f);
        private static readonly Color Flower = new(0.72f, 0.35f, 0.48f);
        private static readonly Color Banner = new(0.55f, 0.14f, 0.12f);
        private static readonly Color Gold = CityVisualMaterials.Gold;

        public static void Build(Transform roads, Transform decorations, Transform vegetation)
        {
            if (!CityRoyalDistrictConfig.Enabled)
            {
                return;
            }

            var parent = decorations != null ? decorations : roads;
            if (parent == null)
            {
                return;
            }

            var existing = parent.Find(CityRoyalDistrictConfig.RootName);
            if (existing != null)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
            }

            var root = new GameObject(CityRoyalDistrictConfig.RootName).transform;
            root.SetParent(parent, false);

            var plazaZ = CityCapitalEnvironmentConfig.PlazaCenterZ;
            BuildImperialPlaza(root, plazaZ);
            BuildFountain(root, plazaZ);
            BuildGrandStaircase(root);
            BuildFormalGardens(root, plazaZ);
            BuildMonument(root, plazaZ);
            BuildNobleResidences(root);
            BuildImperialRoads(root, plazaZ);
            BuildLighting(root, plazaZ);
            BuildProps(root, plazaZ);
            BuildFormalVegetation(root, plazaZ, vegetation);
        }

        private static void BuildImperialPlaza(Transform root, float plazaZ)
        {
            var host = Node(root, "ImperialPlaza");
            // Base monumental (não piso plano único).
            Prop(host, "PlazaBed", new Vector3(0f, 0.02f, plazaZ),
                new Vector3(CityRoyalDistrictConfig.PlazaRadius * 2.35f, 0.08f, CityRoyalDistrictConfig.PlazaRadius * 2.35f), StoneB);
            Prop(host, "PlazaCore", new Vector3(0f, 0.045f, plazaZ),
                new Vector3(4.6f, 0.1f, 4.6f), StoneA);

            // Anéis concêntricos.
            for (var r = 1; r <= CityRoyalDistrictConfig.PlazaRingCount; r++)
            {
                var rad = 2.2f + r * 1.15f;
                var col = r % 2 == 0 ? Cobble : StoneA;
                Prop(host, $"PlazaRing_{r}", new Vector3(0f, 0.05f + r * 0.004f, plazaZ),
                    new Vector3(rad * 2f, 0.06f, rad * 2f), col);
            }

            // Pavimentação radial (caminhos convergindo ao Castelo / fonte).
            for (var i = 0; i < CityRoyalDistrictConfig.RadialPathCount; i++)
            {
                var ang = i * (360f / CityRoyalDistrictConfig.RadialPathCount) * Mathf.Deg2Rad;
                var mid = new Vector3(Mathf.Sin(ang) * 3.4f, 0.07f, plazaZ + Mathf.Cos(ang) * 3.4f);
                var path = Prop(host, $"Radial_{i}", mid, new Vector3(1.15f, 0.07f, 5.2f), Cobble);
                path.transform.localRotation = Quaternion.Euler(0f, i * (360f / CityRoyalDistrictConfig.RadialPathCount), 0f);
            }

            // Ornamentos no perímetro da praça.
            for (var i = 0; i < 12; i++)
            {
                var a = i * 30f * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Sin(a) * 6.4f, 0.28f, plazaZ + Mathf.Cos(a) * 6.4f);
                Prop(host, $"PlazaUrn_{i}", p, new Vector3(0.35f, 0.45f, 0.35f), StoneB);
            }
        }

        private static void BuildFountain(Transform root, float plazaZ)
        {
            var host = Node(root, "MonumentalFountain");
            // Octógono aproximado (8 faces).
            for (var i = 0; i < CityRoyalDistrictConfig.FountainSides; i++)
            {
                var a = i * (360f / CityRoyalDistrictConfig.FountainSides) * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Sin(a) * 1.55f, 0.35f, plazaZ + Mathf.Cos(a) * 1.55f);
                var face = Prop(host, $"FountainRim_{i}", p, new Vector3(0.85f, 0.55f, 0.28f), StoneA);
                face.transform.localRotation = Quaternion.Euler(0f, i * (360f / CityRoyalDistrictConfig.FountainSides), 0f);
            }

            Prop(host, "FountainBasin", new Vector3(0f, 0.28f, plazaZ), new Vector3(3.2f, 0.35f, 3.2f), StoneB);
            Prop(host, "FountainWater", new Vector3(0f, 0.48f, plazaZ), new Vector3(2.7f, 0.16f, 2.7f), Water);
            Prop(host, "FountainStem", new Vector3(0f, 1.15f, plazaZ), new Vector3(0.55f, 0.95f, 0.55f), StoneA);
            Prop(host, "FountainBowl", new Vector3(0f, 1.7f, plazaZ), new Vector3(1.5f, 0.28f, 1.5f), StoneB);
            Prop(host, "FountainWaterTop", new Vector3(0f, 1.85f, plazaZ), new Vector3(1.15f, 0.1f, 1.15f), Water);
            Prop(host, "FountainCrest", new Vector3(0f, 2.25f, plazaZ), new Vector3(0.45f, 0.45f, 0.45f), Gold);

            // Espaço livre + anel ornamental.
            Prop(host, "FountainClearRing", new Vector3(0f, 0.06f, plazaZ), new Vector3(4.0f, 0.05f, 4.0f), Cobble);

            if (CityAmbientQualitySettings.AllowTorches)
            {
                for (var i = 0; i < 4; i++)
                {
                    var a = (i * 90f + 45f) * Mathf.Deg2Rad;
                    Brazier(host, new Vector3(Mathf.Sin(a) * 2.6f, 0.55f, plazaZ + Mathf.Cos(a) * 2.6f));
                }
            }
        }

        private static void BuildGrandStaircase(Transform root)
        {
            var host = Node(root, "RoyalStaircase");
            // Escadaria principal: praça (sul) → Castelo (centro).
            for (var i = 0; i < CityRoyalDistrictConfig.GrandStairSteps; i++)
            {
                var t = i / (float)(CityRoyalDistrictConfig.GrandStairSteps - 1);
                var z = -2.85f + i * 0.38f;
                var y = 0.12f + i * 0.14f;
                var w = 5.2f - t * 1.4f;
                Prop(host, $"GrandStep_{i}", new Vector3(0f, y, z), new Vector3(w, 0.16f, 0.48f),
                    i % 2 == 0 ? StoneA : StoneB);
            }

            Prop(host, "RailL", new Vector3(-2.55f, 0.85f, -1.2f), new Vector3(0.22f, 1.4f, 3.6f), StoneB);
            Prop(host, "RailR", new Vector3(2.55f, 0.85f, -1.2f), new Vector3(0.22f, 1.4f, 3.6f), StoneB);
            Prop(host, "PillarL", new Vector3(-2.7f, 1.1f, -2.9f), new Vector3(0.45f, 2.0f, 0.45f), StoneA);
            Prop(host, "PillarR", new Vector3(2.7f, 1.1f, -2.9f), new Vector3(0.45f, 2.0f, 0.45f), StoneA);
            Prop(host, "PillarCapL", new Vector3(-2.7f, 2.2f, -2.9f), new Vector3(0.6f, 0.22f, 0.6f), StoneB);
            Prop(host, "PillarCapR", new Vector3(2.7f, 2.2f, -2.9f), new Vector3(0.6f, 0.22f, 0.6f), StoneB);

            Flag(host, new Vector3(-2.7f, 2.4f, -2.9f));
            Flag(host, new Vector3(2.7f, 2.4f, -2.9f));
            if (CityAmbientQualitySettings.AllowTorches)
            {
                Torch(host, new Vector3(-2.7f, 1.7f, -2.5f));
                Torch(host, new Vector3(2.7f, 1.7f, -2.5f));
                Torch(host, new Vector3(-2.2f, 0.9f, -0.4f));
                Torch(host, new Vector3(2.2f, 0.9f, -0.4f));
            }
        }

        private static void BuildFormalGardens(Transform root, float plazaZ)
        {
            var host = Node(root, "ImperialGardens");
            // Quatro canteiros simétricos (NE NW SE SW) + laterais.
            Vector3[] beds =
            {
                new(-4.2f, 0.12f, plazaZ + 3.2f), new(4.2f, 0.12f, plazaZ + 3.2f),
                new(-4.2f, 0.12f, plazaZ - 3.2f), new(4.2f, 0.12f, plazaZ - 3.2f),
                new(-5.6f, 0.12f, plazaZ), new(5.6f, 0.12f, plazaZ),
                new(-3.4f, 0.12f, 1.8f), new(3.4f, 0.12f, 1.8f)
            };

            for (var i = 0; i < Mathf.Min(CityRoyalDistrictConfig.FormalGardenBeds, beds.Length); i++)
            {
                var p = beds[i];
                Prop(host, $"Bed_{i}", p, new Vector3(1.8f, 0.14f, 1.3f), CityVisualMaterials.Dirt);
                Prop(host, $"Hedge_{i}", p + Vector3.up * 0.22f, new Vector3(1.65f, 0.28f, 1.15f), Hedge);
                Prop(host, $"Flower_{i}", p + new Vector3(0.25f, 0.38f, 0f), Vector3.one * 0.28f, Flower);
                Prop(host, $"Fence_{i}", p + new Vector3(0f, 0.2f, -0.75f), new Vector3(1.7f, 0.25f, 0.08f),
                    CityVisualMaterials.Wood);
            }

            // Sebes formais no eixo.
            Prop(host, "HedgeAxisL", new Vector3(-2.4f, 0.32f, -0.2f), new Vector3(0.28f, 0.5f, 4.5f), Hedge);
            Prop(host, "HedgeAxisR", new Vector3(2.4f, 0.32f, -0.2f), new Vector3(0.28f, 0.5f, 4.5f), Hedge);
        }

        private static void BuildMonument(Transform root, float plazaZ)
        {
            var host = Node(root, "MonumentValgor");
            // Eixo Castelo ↔ praça: monumento ao norte da fonte (mais perto do Castelo).
            var z = plazaZ + 4.5f;
            Prop(host, "MonumentBase", new Vector3(0f, 0.25f, z), new Vector3(1.4f, 0.4f, 1.4f), StoneB);
            Prop(host, "MonumentPlinth", new Vector3(0f, 0.55f, z), new Vector3(1.0f, 0.25f, 1.0f), StoneA);
            Prop(host, "Obelisk", new Vector3(0f, 2.1f, z), new Vector3(0.45f, 2.8f, 0.45f), StoneA);
            Prop(host, "ObeliskTip", new Vector3(0f, 3.65f, z), new Vector3(0.35f, 0.45f, 0.35f), Gold);
            Prop(host, "CrestPlate", new Vector3(0f, 1.6f, z + 0.28f), new Vector3(0.55f, 0.7f, 0.1f), Gold);
            // Estátuas laterais menores.
            Prop(host, "StatueL", new Vector3(-1.8f, 1.0f, z), new Vector3(0.4f, 1.6f, 0.4f), StoneB);
            Prop(host, "StatueR", new Vector3(1.8f, 1.0f, z), new Vector3(0.4f, 1.6f, 0.4f), StoneB);
            Flag(host, new Vector3(-1.2f, 1.8f, z));
            Flag(host, new Vector3(1.2f, 1.8f, z));
        }

        private static void BuildNobleResidences(Transform root)
        {
            var host = Node(root, "NobleResidences");
            // Anel nobre — fora da praça, dentro da exclusão vernacular.
            // Canteiros próximos ao castelo (mais largos / legíveis).
            Vector3[] spots =
            {
                new(-6.8f, 0f, 3.2f), new(6.8f, 0f, 3.2f),
                new(-7.4f, 0f, 0.4f), new(7.4f, 0f, 0.4f),
                new(-6.6f, 0f, -2.8f), new(6.6f, 0f, -2.8f),
                new(-5.8f, 0f, 5.2f), new(5.8f, 0f, 5.2f)
            };

            var count = Mathf.Min(CityRoyalDistrictConfig.NobleResidenceCount, spots.Length);
            for (var i = 0; i < count; i++)
            {
                NobleHouse(host, $"Noble_{i}", spots[i], i);
            }
        }

        private static void NobleHouse(Transform host, string name, Vector3 pos, int seed)
        {
            var h = 1.85f + (seed % 3) * 0.25f;
            var w = 2.35f + (seed % 2) * 0.4f;
            var d = 1.95f + (seed % 2) * 0.3f;
            var yaw = seed % 2 == 0 ? 12f : -12f;
            var go = new GameObject(name).transform;
            go.SetParent(host, false);
            go.localPosition = pos;
            go.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Prop(go, "Body", new Vector3(0f, h * 0.5f, 0f), new Vector3(w, h, d), BodyNoble);
            Prop(go, "Plinth", new Vector3(0f, 0.1f, 0f), new Vector3(w * 1.08f, 0.2f, d * 1.08f), StoneB);
            Prop(go, "Roof", new Vector3(0f, h + 0.35f, 0f), new Vector3(w * 1.25f, 0.45f, d * 1.2f), RoofNoble);
            Prop(go, "Ridge", new Vector3(0f, h + 0.58f, 0f), new Vector3(w * 0.2f, 0.12f, d * 1.05f), StoneB);
            Prop(go, "Balcony", new Vector3(0f, h * 0.55f, d * 0.55f), new Vector3(w * 0.7f, 0.12f, 0.45f), StoneA);
            Prop(go, "Door", new Vector3(0f, 0.55f, d * 0.52f), new Vector3(0.35f, 0.9f, 0.08f), CityVisualMaterials.Wood);
            Prop(go, "WindowL", new Vector3(-w * 0.28f, h * 0.6f, d * 0.52f), new Vector3(0.28f, 0.35f, 0.06f),
                new Color(0.35f, 0.42f, 0.48f));
            Prop(go, "WindowR", new Vector3(w * 0.28f, h * 0.6f, d * 0.52f), new Vector3(0.28f, 0.35f, 0.06f),
                new Color(0.35f, 0.42f, 0.48f));
            Prop(go, "Garden", new Vector3(0f, 0.08f, -d * 0.7f), new Vector3(w * 0.9f, 0.1f, 0.7f), Hedge);
            Prop(go, "Fence", new Vector3(0f, 0.28f, -d * 0.95f), new Vector3(w * 0.95f, 0.35f, 0.08f), CityVisualMaterials.Wood);
        }

        private static void BuildImperialRoads(Transform root, float plazaZ)
        {
            var host = Node(root, "ImperialRoads");
            // Eixo monumental: portão → praça → castelo.
            Prop(host, "AvenueBed", new Vector3(0f, 0.03f, plazaZ - 4.5f), new Vector3(4.2f, 0.08f, 8.5f), StoneB);
            Prop(host, "Avenue", new Vector3(0f, 0.05f, plazaZ - 4.5f), new Vector3(3.4f, 0.08f, 8.0f), Cobble);
            Prop(host, "SidewalkL", new Vector3(-2.1f, 0.06f, plazaZ - 4.5f), new Vector3(0.55f, 0.07f, 8.0f), StoneA);
            Prop(host, "SidewalkR", new Vector3(2.1f, 0.06f, plazaZ - 4.5f), new Vector3(0.55f, 0.07f, 8.0f), StoneA);

            // Cruzamento praça.
            Prop(host, "CrossPlaza", new Vector3(0f, 0.055f, plazaZ), new Vector3(3.8f, 0.06f, 3.8f), StoneA);
            // Rampas laterais leves.
            Prop(host, "RampL", new Vector3(-3.6f, 0.12f, plazaZ - 1.5f), new Vector3(1.4f, 0.12f, 2.8f), Cobble);
            Prop(host, "RampR", new Vector3(3.6f, 0.12f, plazaZ - 1.5f), new Vector3(1.4f, 0.12f, 2.8f), Cobble);
            // Escadinhas laterais.
            for (var i = 0; i < 3; i++)
            {
                Prop(host, $"SideStepL_{i}", new Vector3(-3.2f, 0.14f + i * 0.12f, -0.8f + i * 0.35f),
                    new Vector3(1.2f, 0.12f, 0.4f), StoneB);
                Prop(host, $"SideStepR_{i}", new Vector3(3.2f, 0.14f + i * 0.12f, -0.8f + i * 0.35f),
                    new Vector3(1.2f, 0.12f, 0.4f), StoneB);
            }
        }

        private static void BuildLighting(Transform root, float plazaZ)
        {
            var host = Node(root, "ImperialLighting");
            for (var i = 0; i < CityRoyalDistrictConfig.LampPostCount; i++)
            {
                var a = i * (360f / CityRoyalDistrictConfig.LampPostCount) * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Sin(a) * 5.5f, 0f, plazaZ + Mathf.Cos(a) * 5.5f);
                Prop(host, $"Lamp_{i}", p + Vector3.up * 1.0f, new Vector3(0.14f, 1.9f, 0.14f), StoneB);
                Prop(host, $"LampGlow_{i}", p + Vector3.up * 2.05f, Vector3.one * 0.28f, new Color(1f, 0.75f, 0.4f));
            }

            Flag(host, new Vector3(-4.5f, 0.2f, plazaZ + 2f));
            Flag(host, new Vector3(4.5f, 0.2f, plazaZ + 2f));
            Flag(host, new Vector3(-4.5f, 0.2f, plazaZ - 2f));
            Flag(host, new Vector3(4.5f, 0.2f, plazaZ - 2f));

            if (CityAmbientQualitySettings.AllowTorches)
            {
                Brazier(host, new Vector3(-5.2f, 0.55f, plazaZ));
                Brazier(host, new Vector3(5.2f, 0.55f, plazaZ));
            }
        }

        private static void BuildProps(Transform root, float plazaZ)
        {
            var host = Node(root, "ImperialProps");
            Prop(host, "BenchN", new Vector3(-2.8f, 0.32f, plazaZ + 2.4f), new Vector3(1.4f, 0.35f, 0.4f), CityVisualMaterials.Wood);
            Prop(host, "BenchS", new Vector3(2.8f, 0.32f, plazaZ - 2.4f), new Vector3(1.4f, 0.35f, 0.4f), CityVisualMaterials.Wood);
            Prop(host, "BenchE", new Vector3(3.6f, 0.32f, plazaZ + 0.8f), new Vector3(0.4f, 0.35f, 1.4f), CityVisualMaterials.Wood);
            Prop(host, "BenchW", new Vector3(-3.6f, 0.32f, plazaZ + 0.8f), new Vector3(0.4f, 0.35f, 1.4f), CityVisualMaterials.Wood);
            Prop(host, "VaseA", new Vector3(-1.8f, 0.4f, plazaZ + 5.2f), new Vector3(0.4f, 0.55f, 0.4f), StoneA);
            Prop(host, "VaseB", new Vector3(1.8f, 0.4f, plazaZ + 5.2f), new Vector3(0.4f, 0.55f, 0.4f), StoneA);
            Prop(host, "CrateA", new Vector3(-5.5f, 0.3f, plazaZ - 4.8f), new Vector3(0.55f, 0.45f, 0.55f), CityVisualMaterials.Wood);
            Prop(host, "BarrelA", new Vector3(5.5f, 0.35f, plazaZ - 4.5f), new Vector3(0.5f, 0.55f, 0.5f), CityVisualMaterials.Wood);
            Prop(host, "Cart", new Vector3(-4.8f, 0.35f, plazaZ - 5.5f), new Vector3(1.4f, 0.45f, 0.75f), CityVisualMaterials.Wood);
            Prop(host, "SignPost", new Vector3(0f, 0.9f, plazaZ - 6.2f), new Vector3(0.12f, 1.6f, 0.12f), CityVisualMaterials.Wood);
            Prop(host, "SignBoard", new Vector3(0.35f, 1.35f, plazaZ - 6.2f), new Vector3(0.7f, 0.45f, 0.08f), StoneA);
        }

        private static void BuildFormalVegetation(Transform root, float plazaZ, Transform vegetation)
        {
            var host = Node(root, "FormalVegetation");
            _ = vegetation;
            for (var i = 0; i < CityRoyalDistrictConfig.OrnamentalTreePairs; i++)
            {
                var z = plazaZ - 5.5f + i * 1.5f;
                OrnamentalTree(host, $"OrnL_{i}", new Vector3(-4.8f, 0f, z));
                OrnamentalTree(host, $"OrnR_{i}", new Vector3(4.8f, 0f, z));
            }

            // Pedras decorativas simétricas.
            for (var i = 0; i < 6; i++)
            {
                var a = i * 60f * Mathf.Deg2Rad;
                Prop(host, $"DecorRock_{i}",
                    new Vector3(Mathf.Sin(a) * 6.8f, 0.18f, plazaZ + Mathf.Cos(a) * 6.8f),
                    new Vector3(0.55f, 0.3f, 0.45f), StoneB);
            }
        }

        private static void OrnamentalTree(Transform host, string name, Vector3 pos)
        {
            Prop(host, name + "_Trunk", pos + Vector3.up * 0.55f, new Vector3(0.22f, 0.55f, 0.22f), CityVisualMaterials.Wood);
            Prop(host, name + "_CanopyA", pos + Vector3.up * 1.35f, new Vector3(1.1f, 0.7f, 1.0f), Hedge);
            Prop(host, name + "_CanopyB", pos + Vector3.up * 1.85f, new Vector3(0.75f, 0.55f, 0.75f), Hedge);
        }

        private static void Flag(Transform host, Vector3 tip)
        {
            Prop(host, "FlagPole", tip + Vector3.up * 0.5f, new Vector3(0.08f, 1.0f, 0.08f), CityVisualMaterials.Wood);
            Prop(host, "FlagCloth", tip + new Vector3(0.4f, 1.1f, 0f), new Vector3(0.8f, 0.5f, 0.05f), Banner);
        }

        private static void Torch(Transform host, Vector3 pos)
        {
            Prop(host, "TorchPole", pos, new Vector3(0.1f, 0.35f, 0.1f), CityVisualMaterials.Wood);
            var flame = Prop(host, "TorchFlame", pos + Vector3.up * 0.42f, Vector3.one * 0.26f, new Color(1f, 0.5f, 0.15f));
            CityVisualMaterials.ApplyEmissiveHint(flame.GetComponent<Renderer>(), new Color(1f, 0.45f, 0.1f), 0.5f);
        }

        private static void Brazier(Transform host, Vector3 pos)
        {
            Prop(host, "BrazierBowl", pos, new Vector3(0.55f, 0.35f, 0.55f), StoneB);
            var flame = Prop(host, "BrazierFlame", pos + Vector3.up * 0.4f, Vector3.one * 0.4f, new Color(1f, 0.45f, 0.12f));
            CityVisualMaterials.ApplyEmissiveHint(flame.GetComponent<Renderer>(), new Color(1f, 0.4f, 0.08f), 0.55f);
        }

        private static Transform Node(Transform parent, string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }

        private static GameObject Prop(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            var ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                go.layer = ignore;
            }

            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
            return go;
        }
    }
}
