using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Props e identidade visual por distrito (sem gameplay / sem collider).
    /// </summary>
    public static class CityDistrictVisualBuilder
    {
        public static void BuildAll(Transform districts, Transform decorations, Transform ambient)
        {
            BuildRoyal(districts, decorations);
            BuildDragon(districts, decorations);
            BuildMilitary(districts, decorations);
            BuildEconomic(districts, decorations);
            BuildIndustrial(districts, decorations, ambient);
            BuildSacred(districts, decorations);
            BuildRural(districts, decorations);
        }

        private static void BuildRoyal(Transform districts, Transform decorations)
        {
            var root = Node(districts, "Royal");
            // Plataforma do Castelo + jardim nobre (espaÃ§o livre no centro para seleÃ§Ã£o).
            Dec(root, PrimitiveType.Cube, "CastlePlatform", new Vector3(0f, 0.06f, 0.4f),
                new Vector3(8.5f, 0.12f, 8.5f),
                Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.45f), SurfaceKind.Stone);
            Dec(root, PrimitiveType.Cube, "CastleSteps", new Vector3(0f, 0.18f, -2.1f),
                new Vector3(3.2f, 0.22f, 1.6f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            Dec(root, PrimitiveType.Cube, "NobleGardenL", new Vector3(-3.6f, 0.12f, 1.8f),
                new Vector3(2.2f, 0.08f, 2.8f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Gold, 0.08f), SurfaceKind.Vegetation);
            Dec(root, PrimitiveType.Cube, "NobleGardenR", new Vector3(3.6f, 0.12f, 1.8f),
                new Vector3(2.2f, 0.08f, 2.8f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Gold, 0.08f), SurfaceKind.Vegetation);

            if (CityAmbientQualitySettings.AllowFlags)
            {
                Banner(decorations, new Vector3(-2.8f, 0.3f, -1.4f));
                Banner(decorations, new Vector3(2.8f, 0.3f, -1.4f));
            }

            if (CityAmbientQualitySettings.AllowTorches)
            {
                Brazier(decorations, new Vector3(-3.2f, 0.55f, -0.6f));
                Brazier(decorations, new Vector3(3.2f, 0.55f, -0.6f));
            }

            // SalÃ£o dos HerÃ³is â€” identidade prÃ³pria (noroeste), sem invadir Castelo.
            Dec(root, PrimitiveType.Cube, "HeroHallPadTrim", new Vector3(-6.8f, 0.05f, 5.2f),
                new Vector3(4.2f, 0.06f, 3.8f),
                Color.Lerp(CityVisualMaterials.StoneLight, CityVisualMaterials.Gold, 0.12f), SurfaceKind.Stone);
            Dec(decorations, PrimitiveType.Cube, "HeroStatueL", new Vector3(-7.8f, 0.85f, 4.4f),
                new Vector3(0.45f, 1.5f, 0.45f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Dec(decorations, PrimitiveType.Cube, "HeroStatueR", new Vector3(-5.8f, 0.85f, 4.4f),
                new Vector3(0.45f, 1.5f, 0.45f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
        }

        private static void BuildDragon(Transform districts, Transform decorations)
        {
            var root = Node(districts, "Dragon");
            _ = root;
            var tz = CityCapitalLayoutConfig.GetPosition("dragon-tower").Z;

            // Platô principal: CityEnvironmentBuilder.BuildPlateaus (sincronizado com layout).
            Dec(decorations, PrimitiveType.Cube, "DragonCrest", new Vector3(0f, 2.4f, tz - 3.2f),
                new Vector3(1.5f, 0.22f, 0.4f), CityVisualMaterials.Gold, SurfaceKind.Metal);

            if (CityAmbientQualitySettings.AllowTorches)
            {
                Brazier(decorations, new Vector3(-3.0f, 2.35f, tz - 3.6f));
                Brazier(decorations, new Vector3(3.0f, 2.35f, tz - 3.6f));
            }
        }

        private static void BuildMilitary(Transform districts, Transform decorations)
        {
            var root = Node(districts, "Military");
            // Arena â€” ringe
            Dec(root, PrimitiveType.Cylinder, "ArenaRing", new Vector3(11.2f, 0.08f, 0.8f),
                new Vector3(4.5f, 0.1f, 4.5f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.3f), SurfaceKind.Dirt);
            Dec(decorations, PrimitiveType.Cube, "ArenaBanner", new Vector3(11.2f, 1.4f, 2.6f),
                new Vector3(0.12f, 1.6f, 0.8f), CityVisualMaterials.RoofRed, SurfaceKind.Plain);

            // Campo de Treinamento â€” aberto, alvos, cercas
            Dec(root, PrimitiveType.Cube, "TrainingYard", new Vector3(12.4f, 0.06f, -5.6f),
                new Vector3(5.5f, 0.08f, 4.8f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Wood, 0.2f), SurfaceKind.Dirt);
            Dec(decorations, PrimitiveType.Cube, "DummyA", new Vector3(11.2f, 0.7f, -4.6f),
                new Vector3(0.35f, 1.2f, 0.35f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "DummyB", new Vector3(13.4f, 0.7f, -6.2f),
                new Vector3(0.35f, 1.2f, 0.35f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "TargetBoard", new Vector3(13.8f, 0.75f, -4.4f),
                new Vector3(0.2f, 1.3f, 1.1f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "Tent", new Vector3(10.6f, 0.65f, -6.8f),
                new Vector3(1.6f, 1.1f, 1.2f), CityVisualMaterials.RoofRed, SurfaceKind.Plain);
            Fence(decorations, new Vector3(10.2f, 0.35f, -5.6f), new Vector3(0.12f, 0.7f, 4.2f));
            Fence(decorations, new Vector3(14.6f, 0.35f, -5.6f), new Vector3(0.12f, 0.7f, 4.2f));

            // Academia â€” mesa / mapas (fechado estratÃ©gico)
            Dec(root, PrimitiveType.Cube, "AcademyCourtyard", new Vector3(14.2f, 0.92f, 9.5f),
                new Vector3(4.2f, 0.08f, 3.6f),
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.RoofBlue, 0.15f), SurfaceKind.Stone);
            Dec(decorations, PrimitiveType.Cube, "StrategyTable", new Vector3(13.6f, 1.25f, 8.8f),
                new Vector3(1.4f, 0.35f, 0.9f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "MapBoard", new Vector3(15.2f, 1.55f, 9.8f),
                new Vector3(0.12f, 1.1f, 1.4f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            // Art Pass 2 â€” escudos, armas, catapulta decorativa, fogueira.
            Dec(decorations, PrimitiveType.Cube, "ShieldRack", new Vector3(10.4f, 0.85f, 1.6f),
                new Vector3(0.15f, 1.3f, 1.2f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "ShieldA", new Vector3(10.55f, 0.95f, 1.3f),
                new Vector3(0.12f, 0.7f, 0.55f), CityVisualMaterials.StoneLight, SurfaceKind.Metal);
            Dec(decorations, PrimitiveType.Cube, "WeaponRack", new Vector3(12.0f, 0.8f, -3.8f),
                new Vector3(1.1f, 1.2f, 0.2f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "CatapultDecor", new Vector3(9.6f, 0.55f, -0.4f),
                new Vector3(1.8f, 0.7f, 1.1f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cylinder, "CatapultArm", new Vector3(9.6f, 1.1f, -0.4f),
                new Vector3(0.15f, 0.9f, 0.15f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            if (CityAmbientQualitySettings.AllowTorches)
            {
                Brazier(decorations, new Vector3(10.8f, 0.55f, 0.2f));
            }
        }

        private static void BuildEconomic(Transform districts, Transform decorations)
        {
            var root = Node(districts, "Economic");
            Dec(decorations, PrimitiveType.Cube, "StallA", new Vector3(-8.2f, 0.55f, -7.4f),
                new Vector3(1.5f, 1.0f, 1.1f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "StallB", new Vector3(-9.4f, 0.5f, -5.8f),
                new Vector3(1.3f, 0.9f, 1.0f), CityVisualMaterials.RoofRed, SurfaceKind.Plain);
            Dec(decorations, PrimitiveType.Cube, "Crates", new Vector3(-7.0f, 0.35f, -5.4f),
                new Vector3(1.0f, 0.65f, 0.8f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cylinder, "BarrelStack", new Vector3(-6.0f, 0.4f, -6.8f),
                new Vector3(0.7f, 0.7f, 0.7f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "Cart", new Vector3(-4.8f, 0.4f, -7.8f),
                new Vector3(1.6f, 0.55f, 0.9f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "WarehouseCrates", new Vector3(5.2f, 0.4f, -9.8f),
                new Vector3(1.4f, 0.7f, 1.0f), CityVisualMaterials.Wood, SurfaceKind.Wood);

            // ResidÃªncia oficial â€” jardim prÃ³prio (distinto de casas decorativas).
            Dec(root, PrimitiveType.Cube, "ResidenceGarden", new Vector3(-5.8f, 0.05f, -8.8f),
                new Vector3(3.6f, 0.06f, 3.2f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Path, 0.2f), SurfaceKind.Vegetation);
            Dec(decorations, PrimitiveType.Cube, "ResidenceFence", new Vector3(-5.8f, 0.35f, -10.2f),
                new Vector3(3.4f, 0.55f, 0.12f), CityVisualMaterials.Wood, SurfaceKind.Wood);

            if (CityCapitalEnvironmentConfig.AllowHighExclusiveProps)
            {
                Dec(decorations, PrimitiveType.Cube, "AwningExtra", new Vector3(-8.8f, 0.95f, -6.6f),
                    new Vector3(2.2f, 0.08f, 1.4f), CityVisualMaterials.RoofBlue, SurfaceKind.Plain);
            }

            // Art Pass 2 â€” tecidos / mercadorias.
            Dec(decorations, PrimitiveType.Cube, "ClothPile", new Vector3(-7.6f, 0.35f, -6.2f),
                new Vector3(0.9f, 0.45f, 0.7f), CityVisualMaterials.RoofBlue, SurfaceKind.Plain);
            Dec(decorations, PrimitiveType.Cube, "GoodsSack", new Vector3(-6.4f, 0.3f, -7.6f),
                new Vector3(0.55f, 0.45f, 0.55f), CityVisualMaterials.Dirt, SurfaceKind.Dirt);
            Dec(decorations, PrimitiveType.Cylinder, "BarrelB", new Vector3(-5.4f, 0.4f, -6.2f),
                new Vector3(0.55f, 0.55f, 0.55f), CityVisualMaterials.Wood, SurfaceKind.Wood);
        }

        private static void BuildIndustrial(Transform districts, Transform decorations, Transform ambient)
        {
            var root = Node(districts, "Industrial");
            Dec(decorations, PrimitiveType.Cube, "LogPile", new Vector3(-13.8f, 0.45f, -12.6f),
                new Vector3(2.0f, 0.85f, 1.2f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "ToolRack", new Vector3(-12.4f, 0.7f, -13.8f),
                new Vector3(1.2f, 1.1f, 0.25f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "OreCart", new Vector3(-14.6f, 1.05f, 11.2f),
                new Vector3(1.3f, 0.6f, 0.9f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Dec(decorations, PrimitiveType.Cube, "StoneHeap", new Vector3(14.6f, 1.0f, 11.6f),
                new Vector3(1.5f, 0.7f, 1.2f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);

            // FundiÃ§Ã£o â€” fornalha visual (nÃ£o confundir com prop genÃ©rico).
            Dec(root, PrimitiveType.Cube, "FoundryYard", new Vector3(12.8f, 0.05f, -13f),
                new Vector3(4.8f, 0.08f, 4.2f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.35f), SurfaceKind.Dirt);
            Dec(decorations, PrimitiveType.Cube, "Furnace", new Vector3(13.6f, 0.95f, -12.4f),
                new Vector3(1.4f, 1.6f, 1.2f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Dec(decorations, PrimitiveType.Cylinder, "Chimney", new Vector3(13.6f, 2.1f, -12.4f),
                new Vector3(0.45f, 1.2f, 0.45f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            // Art Pass 2 â€” forja, minÃ©rio, ferramentas, carrinho.
            Dec(decorations, PrimitiveType.Cube, "Anvil", new Vector3(12.2f, 0.45f, -12.8f),
                new Vector3(0.7f, 0.45f, 0.45f), CityVisualMaterials.StoneDark, SurfaceKind.Metal);
            Dec(decorations, PrimitiveType.Cube, "OrePile", new Vector3(14.4f, 0.4f, -13.6f),
                new Vector3(1.1f, 0.55f, 0.9f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Dec(decorations, PrimitiveType.Cube, "ToolCart", new Vector3(11.6f, 0.4f, -13.8f),
                new Vector3(1.3f, 0.5f, 0.8f), CityVisualMaterials.Wood, SurfaceKind.Wood);

            var smokeScale = CityCapitalEnvironmentConfig.SmokeScale(CityAmbientQualitySettings.Current);
            if (smokeScale > 0f && CityAmbientQualitySettings.AllowSmoke)
            {
                var smoke = Dec(ambient, PrimitiveType.Sphere, "FoundrySmoke",
                    new Vector3(13.6f, 3.2f, -12.4f),
                    new Vector3(1.2f, 1.6f, 1.2f) * smokeScale,
                    new Color(0.5f, 0.5f, 0.5f, 0.3f), SurfaceKind.Plain);
                var millSmoke = Dec(ambient, PrimitiveType.Sphere, "MillSmoke",
                    new Vector3(-13.5f, 2.4f, -13.2f),
                    new Vector3(1.1f, 1.4f, 1.1f) * smokeScale,
                    new Color(0.55f, 0.55f, 0.55f, 0.28f), SurfaceKind.Plain);
                _ = smoke;
                _ = millSmoke;
            }
        }

        private static void BuildSacred(Transform districts, Transform decorations)
        {
            var root = Node(districts, "Sacred");
            Dec(root, PrimitiveType.Cube, "HospitalGarden", new Vector3(-9.2f, 0.05f, 4.2f),
                new Vector3(4.5f, 0.06f, 4.5f),
                Color.Lerp(CityVisualMaterials.Vegetation, Color.white, 0.08f), SurfaceKind.Vegetation);
            Dec(decorations, PrimitiveType.Cylinder, "HealingPool", new Vector3(-9.8f, 0.25f, 3.4f),
                new Vector3(1.2f, 0.15f, 1.2f), new Color(0.35f, 0.5f, 0.58f), SurfaceKind.Plain);
            Dec(decorations, PrimitiveType.Cube, "HospitalBench", new Vector3(-8.2f, 0.3f, 5.2f),
                new Vector3(1.2f, 0.35f, 0.4f), CityVisualMaterials.Wood, SurfaceKind.Wood);

            Dec(root, PrimitiveType.Cube, "TempleGarden", new Vector3(9.0f, 0.05f, 4.8f),
                new Vector3(4.5f, 0.06f, 4.5f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Gold, 0.1f), SurfaceKind.Vegetation);
            Dec(decorations, PrimitiveType.Cylinder, "TempleBasin", new Vector3(9.6f, 0.35f, 4.0f),
                new Vector3(1.0f, 0.25f, 1.0f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            Dec(decorations, PrimitiveType.Cube, "TemplePillar", new Vector3(8.2f, 1.0f, 5.6f),
                new Vector3(0.4f, 1.8f, 0.4f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            // Art Pass 2 â€” flores, velas, Ã¡rvores sagradas.
            Dec(decorations, PrimitiveType.Sphere, "SacredTree", new Vector3(10.4f, 1.3f, 5.8f),
                new Vector3(1.2f, 1.0f, 1.2f), CityVisualMaterials.Vegetation, SurfaceKind.Vegetation);
            Dec(decorations, PrimitiveType.Cylinder, "SacredTrunk", new Vector3(10.4f, 0.5f, 5.8f),
                new Vector3(0.2f, 0.5f, 0.2f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Sphere, "FlowerSacred", new Vector3(8.6f, 0.25f, 3.8f),
                Vector3.one * 0.3f, CityVisualMaterials.RoofRed, SurfaceKind.Plain);
            if (CityAmbientQualitySettings.AllowTorches)
            {
                Brazier(decorations, new Vector3(9.8f, 0.55f, 3.6f));
                Brazier(decorations, new Vector3(-8.4f, 0.55f, 3.2f));
            }
        }

        private static void BuildRural(Transform districts, Transform decorations)
        {
            var root = Node(districts, "Rural");
            Dec(root, PrimitiveType.Cube, "CropA", new Vector3(-14.8f, 0.12f, -6.5f),
                new Vector3(2.4f, 0.2f, 1.6f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Gold, 0.25f), SurfaceKind.Vegetation);
            Dec(root, PrimitiveType.Cube, "CropB", new Vector3(-13.2f, 0.12f, -8.2f),
                new Vector3(2.2f, 0.2f, 1.5f),
                Color.Lerp(CityVisualMaterials.Vegetation, new Color(0.55f, 0.45f, 0.2f), 0.3f), SurfaceKind.Vegetation);
            Fence(decorations, new Vector3(-15.6f, 0.35f, -7.5f), new Vector3(0.12f, 0.65f, 3.8f));
            Dec(decorations, PrimitiveType.Cube, "HayBale", new Vector3(-12.6f, 0.4f, -6.2f),
                new Vector3(0.9f, 0.7f, 0.7f),
                Color.Lerp(CityVisualMaterials.Gold, CityVisualMaterials.Wood, 0.2f), SurfaceKind.Plain);
            Dec(decorations, PrimitiveType.Cube, "FarmCart", new Vector3(-13.6f, 0.4f, -9.2f),
                new Vector3(1.5f, 0.5f, 0.85f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            // Art Pass 2 â€” espantalho, moinho, poÃ§o.
            Dec(decorations, PrimitiveType.Cylinder, "ScarecrowPole", new Vector3(-14.0f, 0.9f, -5.8f),
                new Vector3(0.12f, 0.9f, 0.12f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "Scarecrow", new Vector3(-14.0f, 1.5f, -5.8f),
                new Vector3(0.7f, 0.55f, 0.25f), CityVisualMaterials.RoofRed, SurfaceKind.Plain);
            Dec(decorations, PrimitiveType.Cylinder, "Well", new Vector3(-12.2f, 0.45f, -8.8f),
                new Vector3(1.0f, 0.55f, 1.0f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            Dec(decorations, PrimitiveType.Cylinder, "MillPost", new Vector3(-15.6f, 1.2f, -9.5f),
                new Vector3(0.25f, 1.2f, 0.25f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(decorations, PrimitiveType.Cube, "MillBlade", new Vector3(-15.6f, 2.1f, -9.5f),
                new Vector3(1.8f, 0.15f, 0.35f), CityVisualMaterials.Wood, SurfaceKind.Wood);
        }

        private static void Banner(Transform parent, Vector3 pos)
        {
            Dec(parent, PrimitiveType.Cylinder, "BannerPole", pos, new Vector3(0.08f, 1.1f, 0.08f),
                CityVisualMaterials.Wood, SurfaceKind.Wood);
            Dec(parent, PrimitiveType.Cube, "BannerCloth", pos + new Vector3(0.5f, 0.7f, 0f),
                new Vector3(0.95f, 0.6f, 0.05f), CityVisualMaterials.Gold, SurfaceKind.Plain);
        }

        private static void Brazier(Transform parent, Vector3 pos)
        {
            Dec(parent, PrimitiveType.Cylinder, "Brazier", pos, new Vector3(0.35f, 0.4f, 0.35f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            var flame = Dec(parent, PrimitiveType.Sphere, "BrazierFlame", pos + Vector3.up * 0.45f,
                Vector3.one * 0.32f, new Color(1f, 0.5f, 0.18f), SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(flame.GetComponent<Renderer>(), new Color(1f, 0.4f, 0.1f), 0.6f);
        }

        private static void Fence(Transform parent, Vector3 pos, Vector3 scale) =>
            Dec(parent, PrimitiveType.Cube, "Fence", pos, scale, CityVisualMaterials.Wood, SurfaceKind.Wood);

        private static Transform Node(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                return existing;
            }

            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }

        private static GameObject Dec(
            Transform parent,
            PrimitiveType type,
            string name,
            Vector3 position,
            Vector3 scale,
            Color color,
            SurfaceKind surface)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            var ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                go.layer = ignore;
            }

            CityVisualMaterials.ApplySurface(go.GetComponent<Renderer>(), color, surface);
            return go;
        }
    }
}
