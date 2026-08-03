using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Vestimenta Sprint 3 por BuildingId — arquitetura medieval + props temáticos.
    /// Não altera lógica; Ignore Raycast; materiais do pipeline.
    /// </summary>
    public static class CityOfficialBuildingDressing
    {
        public static void Attach(string buildingId, Transform visualRoot)
        {
            if (!CityArchitectureSprint3Config.Enabled || visualRoot == null)
            {
                return;
            }

            var id = Buildings.BuildingIdMigration.ToCanonical(buildingId);
            if (visualRoot.Find(CityArchitectureSprint3Config.DressingRootName) != null)
            {
                return;
            }

            var root = new GameObject(CityArchitectureSprint3Config.DressingRootName).transform;
            root.SetParent(visualRoot, false);

            if (CityArchitectureSprint3Config.SiteIntegrationEnabled)
            {
                SitePad(root, id);
            }

            if (!CityArchitectureSprint3Config.ThematicDressingEnabled)
            {
                return;
            }

            switch (id)
            {
                case "castle":
                case "wall":
                case "dragon-tower":
                    // Já possuem dressing dedicado / HF — só pad de integração.
                    break;
                case "market":
                    Market(root);
                    break;
                case "warehouse":
                    Warehouse(root);
                    break;
                case "foundry":
                    Foundry(root);
                    break;
                case "academy":
                    Academy(root);
                    break;
                case "hospital":
                    Hospital(root);
                    break;
                case "arena":
                    Arena(root);
                    break;
                case "training-ground":
                    Training(root);
                    break;
                case "hero-hall":
                    HeroHall(root);
                    break;
                case "temple":
                    Temple(root);
                    break;
                case "farm":
                    Farm(root);
                    break;
                case "lumbermill":
                    Lumber(root);
                    break;
                case "quarry":
                    Quarry(root);
                    break;
                case "mine":
                    Mine(root);
                    break;
                case "residence":
                    Residence(root);
                    break;
            }
        }

        private static void SitePad(Transform root, string id)
        {
            var wide = id is "arena" or "farm" or "training-ground" or "market";
            var fx = wide ? 5.6f : 4.4f;
            var fz = wide ? 5.2f : 4.0f;
            Prop(root, PrimitiveType.Cube, "SiteFoundation", new Vector3(0f, 0.04f, 0.1f),
                new Vector3(fx, 0.12f, fz), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "SiteCap", new Vector3(0f, 0.12f, 0.08f),
                new Vector3(fx * 0.92f, 0.08f, fz * 0.9f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cube, "ApproachPath", new Vector3(0f, 0.06f, 2.55f),
                new Vector3(1.8f, 0.08f, 1.6f), CityArtMaterialCategory.Dirt);
            Prop(root, PrimitiveType.Cube, "Step1", new Vector3(0f, 0.16f, 2.15f),
                new Vector3(1.6f, 0.12f, 0.55f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "Step2", new Vector3(0f, 0.28f, 1.75f),
                new Vector3(1.4f, 0.12f, 0.45f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cube, "LowWallL", new Vector3(-fx * 0.48f, 0.35f, 0.1f),
                new Vector3(0.18f, 0.55f, fz * 0.75f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "LowWallR", new Vector3(fx * 0.48f, 0.35f, 0.1f),
                new Vector3(0.18f, 0.55f, fz * 0.75f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "GardenL", new Vector3(-fx * 0.55f, 0.1f, -1.2f),
                new Vector3(0.9f, 0.14f, 1.4f), CityArtMaterialCategory.Grass);
            Prop(root, PrimitiveType.Cube, "GardenR", new Vector3(fx * 0.55f, 0.1f, -1.2f),
                new Vector3(0.9f, 0.14f, 1.4f), CityArtMaterialCategory.Grass);
        }

        private static void Market(Transform root)
        {
            Tent(root, "TentA", new Vector3(-1.8f, 0f, 1.6f), 1.4f, new Color(0.55f, 0.22f, 0.18f));
            Tent(root, "TentB", new Vector3(1.7f, 0f, 1.5f), 1.3f, new Color(0.28f, 0.38f, 0.55f));
            Tent(root, "TentC", new Vector3(-0.2f, 0f, -1.9f), 1.5f, new Color(0.48f, 0.36f, 0.2f));
            Awning(root, new Vector3(0f, 1.55f, 1.35f), 3.8f, 1.2f);
            Crate(root, new Vector3(-1.1f, 0.35f, 2.1f));
            Crate(root, new Vector3(-0.55f, 0.35f, 2.25f));
            Barrel(root, new Vector3(1.1f, 0.4f, 2.05f));
            Barrel(root, new Vector3(1.55f, 0.4f, 1.75f));
            Cart(root, new Vector3(2.35f, 0.35f, 0.2f));
            Prop(root, PrimitiveType.Cube, "StallTable", new Vector3(0f, 0.55f, 1.55f),
                new Vector3(2.2f, 0.12f, 0.7f), CityArtMaterialCategory.Wood);
        }

        private static void Warehouse(Transform root)
        {
            Prop(root, PrimitiveType.Cube, "DepotA", new Vector3(-2.2f, 0.7f, -0.8f),
                new Vector3(1.4f, 1.2f, 1.6f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "DepotRoof", new Vector3(-2.2f, 1.4f, -0.8f),
                new Vector3(1.6f, 0.18f, 1.8f), CityArtMaterialCategory.Roof);
            Crate(root, new Vector3(-1.4f, 0.35f, 2.0f));
            Crate(root, new Vector3(-0.9f, 0.35f, 2.15f));
            Crate(root, new Vector3(-1.15f, 0.75f, 2.05f));
            Barrel(root, new Vector3(1.6f, 0.4f, 2.0f));
            Barrel(root, new Vector3(2.05f, 0.4f, 1.7f));
            Prop(root, PrimitiveType.Cube, "Pallet", new Vector3(0.2f, 0.18f, 2.2f),
                new Vector3(1.4f, 0.12f, 0.9f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "SackPile", new Vector3(-0.2f, 0.4f, 2.15f),
                new Vector3(0.7f, 0.55f, 0.55f), CityArtMaterialCategory.Dirt);
            Prop(root, PrimitiveType.Cube, "LumberStack", new Vector3(2.3f, 0.45f, -0.4f),
                new Vector3(0.9f, 0.7f, 1.5f), CityArtMaterialCategory.Wood);
        }

        private static void Foundry(Transform root)
        {
            Prop(root, PrimitiveType.Cylinder, "ChimneyMain", new Vector3(1.35f, 3.2f, -0.5f),
                new Vector3(0.65f, 1.8f, 0.65f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cylinder, "ChimneySec", new Vector3(-1.15f, 2.9f, -0.65f),
                new Vector3(0.45f, 1.4f, 0.45f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "FurnaceBlock", new Vector3(0f, 0.9f, 1.7f),
                new Vector3(1.6f, 1.5f, 0.85f), CityArtMaterialCategory.StoneDark);
            var glow = Prop(root, PrimitiveType.Sphere, "FurnaceGlow", new Vector3(0f, 0.9f, 1.95f),
                new Vector3(0.75f, 0.55f, 0.4f), CityArtMaterialCategory.Metal);
            CityVisualMaterials.ApplyEmissiveHint(glow.GetComponent<Renderer>(), new Color(1f, 0.4f, 0.12f), 0.7f);
            Prop(root, PrimitiveType.Cube, "Anvil", new Vector3(-1.5f, 0.45f, 1.35f),
                new Vector3(0.7f, 0.55f, 0.45f), CityArtMaterialCategory.Metal);
            Prop(root, PrimitiveType.Cube, "CoalHeap", new Vector3(1.55f, 0.35f, 1.4f),
                new Vector3(0.9f, 0.45f, 0.7f), CityArtMaterialCategory.Dirt);
            Prop(root, PrimitiveType.Cube, "IngotStack", new Vector3(1.9f, 0.4f, 0.5f),
                new Vector3(0.55f, 0.45f, 0.9f), CityArtMaterialCategory.Metal);
            // Fumaça preparada (volumes soft — sem partículas).
            Prop(root, PrimitiveType.Sphere, "SmokeReadyA", new Vector3(1.35f, 5.1f, -0.5f),
                new Vector3(0.9f, 0.7f, 0.9f), CityArtMaterialCategory.Dirt, 0.35f);
            Prop(root, PrimitiveType.Sphere, "SmokeReadyB", new Vector3(1.5f, 5.6f, -0.35f),
                new Vector3(0.7f, 0.55f, 0.7f), CityArtMaterialCategory.Dirt, 0.25f);
        }

        private static void Academy(Transform root)
        {
            Prop(root, PrimitiveType.Cube, "LibraryWing", new Vector3(-1.8f, 1.2f, -0.3f),
                new Vector3(1.2f, 2.0f, 2.0f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cube, "LibraryRoof", new Vector3(-1.8f, 2.35f, -0.3f),
                new Vector3(1.4f, 0.22f, 2.2f), CityArtMaterialCategory.Roof);
            Prop(root, PrimitiveType.Cylinder, "ObsDome", new Vector3(1.7f, 5.35f, -0.2f),
                new Vector3(1.1f, 0.55f, 1.1f), CityArtMaterialCategory.Metal);
            Banner(root, new Vector3(-1.2f, 3.5f, 1.4f), new Color(0.22f, 0.34f, 0.7f));
            Banner(root, new Vector3(0.5f, 3.5f, 1.4f), new Color(0.22f, 0.34f, 0.7f));
            Prop(root, PrimitiveType.Cube, "BookCase", new Vector3(-1.85f, 1.1f, 0.9f),
                new Vector3(0.25f, 1.4f, 1.2f), CityArtMaterialCategory.Wood);
        }

        private static void Hospital(Transform root)
        {
            Prop(root, PrimitiveType.Cube, "WardWing", new Vector3(-1.7f, 0.95f, -0.2f),
                new Vector3(1.5f, 1.6f, 1.8f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cube, "WardRoof", new Vector3(-1.7f, 1.9f, -0.2f),
                new Vector3(1.7f, 0.2f, 2.0f), CityArtMaterialCategory.Roof);
            Tent(root, "MedTent", new Vector3(1.8f, 0f, 1.4f), 1.2f, new Color(0.85f, 0.82f, 0.75f));
            Prop(root, PrimitiveType.Cylinder, "Well", new Vector3(1.9f, 0.45f, -1.2f),
                new Vector3(0.9f, 0.55f, 0.9f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "GardenBed", new Vector3(-2.2f, 0.15f, 1.5f),
                new Vector3(1.4f, 0.2f, 1.6f), CityArtMaterialCategory.Grass);
            Prop(root, PrimitiveType.Cube, "CrossPathA", new Vector3(0f, 0.08f, 0.2f),
                new Vector3(3.2f, 0.06f, 0.55f), CityArtMaterialCategory.Dirt);
            Prop(root, PrimitiveType.Cube, "CrossPathB", new Vector3(0f, 0.08f, 0.2f),
                new Vector3(0.55f, 0.06f, 2.8f), CityArtMaterialCategory.Dirt);
            Prop(root, PrimitiveType.Cube, "CrossH", new Vector3(0f, 2.85f, 0.2f),
                new Vector3(1.1f, 0.18f, 0.18f), CityArtMaterialCategory.Metal);
            Prop(root, PrimitiveType.Cube, "CrossV", new Vector3(0f, 2.85f, 0.2f),
                new Vector3(0.18f, 0.18f, 1.1f), CityArtMaterialCategory.Metal);
        }

        private static void Arena(Transform root)
        {
            for (var i = 0; i < 8; i++)
            {
                var a = i * 45f * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Sin(a) * 2.35f, 1.55f, Mathf.Cos(a) * 2.35f);
                Prop(root, PrimitiveType.Cube, $"Stand_{i}", p,
                    new Vector3(1.1f, 0.7f, 0.55f), CityArtMaterialCategory.StoneLight);
                p.y = 2.35f;
                Banner(root, p, i % 2 == 0 ? new Color(0.55f, 0.16f, 0.14f) : new Color(0.72f, 0.58f, 0.28f));
            }

            Prop(root, PrimitiveType.Cube, "WeaponRack", new Vector3(0f, 0.7f, 2.35f),
                new Vector3(1.4f, 1.1f, 0.25f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "MilitaryPlaza", new Vector3(0f, 0.08f, 3.1f),
                new Vector3(3.2f, 0.08f, 1.4f), CityArtMaterialCategory.StoneDark);
        }

        private static void Training(Transform root)
        {
            Prop(root, PrimitiveType.Cube, "TargetA", new Vector3(-1.7f, 0.85f, 1.5f),
                new Vector3(0.2f, 1.5f, 1.2f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cylinder, "TargetFace", new Vector3(-1.7f, 1.55f, 1.5f),
                new Vector3(0.9f, 0.08f, 0.9f), CityArtMaterialCategory.Metal);
            Prop(root, PrimitiveType.Cube, "DummyPost", new Vector3(1.55f, 0.9f, -0.5f),
                new Vector3(0.28f, 1.6f, 0.28f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Sphere, "DummyHead", new Vector3(1.55f, 1.85f, -0.5f),
                Vector3.one * 0.5f, CityArtMaterialCategory.Dirt);
            Tent(root, "DrillTent", new Vector3(-0.4f, 0f, -1.7f), 1.6f, new Color(0.45f, 0.3f, 0.2f));
            Prop(root, PrimitiveType.Cube, "WeaponCrate", new Vector3(1.7f, 0.4f, 1.3f),
                new Vector3(1.0f, 0.55f, 0.6f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "Yard", new Vector3(0f, 0.06f, 0.2f),
                new Vector3(4.4f, 0.06f, 3.6f), CityArtMaterialCategory.Dirt);
        }

        private static void HeroHall(Transform root)
        {
            Prop(root, PrimitiveType.Cylinder, "ColL", new Vector3(-1.6f, 1.6f, 1.9f),
                new Vector3(0.4f, 1.5f, 0.4f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cylinder, "ColR", new Vector3(1.6f, 1.6f, 1.9f),
                new Vector3(0.4f, 1.5f, 0.4f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cube, "GrandStairs", new Vector3(0f, 0.45f, 2.4f),
                new Vector3(2.8f, 0.7f, 1.4f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "Crest", new Vector3(0f, 2.7f, 1.85f),
                new Vector3(1.2f, 0.9f, 0.15f), CityArtMaterialCategory.Metal);
            Banner(root, new Vector3(-1.1f, 3.9f, 0.2f), new Color(0.25f, 0.4f, 0.7f));
            Banner(root, new Vector3(1.1f, 3.9f, 0.2f), new Color(0.72f, 0.58f, 0.28f));
            Banner(root, new Vector3(0f, 4.2f, 0.2f), new Color(0.55f, 0.16f, 0.14f));
        }

        private static void Temple(Transform root)
        {
            Prop(root, PrimitiveType.Cube, "TempleStairs", new Vector3(0f, 0.5f, 2.2f),
                new Vector3(2.6f, 0.85f, 1.5f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cylinder, "PillarL", new Vector3(-1.35f, 1.5f, 1.45f),
                new Vector3(0.35f, 1.4f, 0.35f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cylinder, "PillarR", new Vector3(1.35f, 1.5f, 1.45f),
                new Vector3(0.35f, 1.4f, 0.35f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cube, "Altar", new Vector3(0f, 0.7f, -0.9f),
                new Vector3(1.2f, 1.0f, 0.7f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "GardenRing", new Vector3(0f, 0.12f, 0f),
                new Vector3(4.0f, 0.1f, 4.0f), CityArtMaterialCategory.Grass);
            var lamp = Prop(root, PrimitiveType.Sphere, "AltarLight", new Vector3(0f, 1.5f, -0.9f),
                Vector3.one * 0.35f, CityArtMaterialCategory.Metal);
            CityVisualMaterials.ApplyEmissiveHint(lamp.GetComponent<Renderer>(), new Color(0.95f, 0.85f, 0.45f), 0.55f);
        }

        private static void Farm(Transform root)
        {
            Prop(root, PrimitiveType.Cube, "CropA", new Vector3(0f, 0.35f, -1.7f),
                new Vector3(3.8f, 0.35f, 0.45f), CityArtMaterialCategory.Grass);
            Prop(root, PrimitiveType.Cube, "CropB", new Vector3(0f, 0.35f, -2.2f),
                new Vector3(3.8f, 0.35f, 0.45f), CityArtMaterialCategory.Grass);
            Prop(root, PrimitiveType.Cube, "FenceN", new Vector3(0f, 0.4f, -2.55f),
                new Vector3(4.2f, 0.55f, 0.1f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cylinder, "MillPost", new Vector3(2.2f, 1.4f, -0.2f),
                new Vector3(0.35f, 1.3f, 0.35f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "MillBlade", new Vector3(2.2f, 2.2f, -0.05f),
                new Vector3(1.6f, 0.12f, 0.2f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "BarnExtra", new Vector3(1.6f, 0.95f, 1.3f),
                new Vector3(1.5f, 1.5f, 1.4f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "BarnRoof", new Vector3(1.6f, 1.85f, 1.3f),
                new Vector3(1.7f, 0.2f, 1.6f), CityArtMaterialCategory.Roof);
            Cart(root, new Vector3(-2.3f, 0.35f, 1.4f));
        }

        private static void Lumber(Transform root)
        {
            Prop(root, PrimitiveType.Cylinder, "LogA", new Vector3(-1.6f, 0.35f, 1.5f),
                new Vector3(0.45f, 1.4f, 0.45f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cylinder, "LogB", new Vector3(-1.15f, 0.35f, 1.65f),
                new Vector3(0.4f, 1.3f, 0.4f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cylinder, "LogC", new Vector3(-1.4f, 0.75f, 1.55f),
                new Vector3(0.4f, 1.2f, 0.4f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cylinder, "SawBlade", new Vector3(-1.85f, 1.55f, 0.4f),
                new Vector3(1.2f, 0.08f, 1.2f), CityArtMaterialCategory.Metal);
            Prop(root, PrimitiveType.Cube, "WoodStack", new Vector3(1.7f, 0.55f, 1.2f),
                new Vector3(1.2f, 0.9f, 1.4f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "Shed", new Vector3(1.8f, 0.9f, -0.8f),
                new Vector3(1.6f, 1.5f, 1.5f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "ShedRoof", new Vector3(1.8f, 1.8f, -0.8f),
                new Vector3(1.8f, 0.2f, 1.7f), CityArtMaterialCategory.Roof);
        }

        private static void Quarry(Transform root)
        {
            Prop(root, PrimitiveType.Cube, "RockA", new Vector3(-1.5f, 0.7f, 1.2f),
                new Vector3(1.4f, 1.2f, 1.2f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "RockB", new Vector3(1.4f, 1.0f, 0.8f),
                new Vector3(1.5f, 1.8f, 1.3f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cube, "RockPile", new Vector3(0.2f, 0.45f, 1.8f),
                new Vector3(1.5f, 0.7f, 1.0f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "CranePost", new Vector3(2.0f, 1.6f, -0.4f),
                new Vector3(0.25f, 2.8f, 0.25f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "CraneArm", new Vector3(1.2f, 2.9f, -0.4f),
                new Vector3(1.8f, 0.18f, 0.18f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "CartBody", new Vector3(-0.3f, 0.4f, 2.2f),
                new Vector3(1.1f, 0.5f, 0.7f), CityArtMaterialCategory.Wood);
        }

        private static void Mine(Transform root)
        {
            Prop(root, PrimitiveType.Cube, "PortalFrame", new Vector3(0f, 1.1f, -1.5f),
                new Vector3(2.0f, 2.0f, 0.35f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "TunnelDark", new Vector3(0f, 0.9f, -1.9f),
                new Vector3(1.2f, 1.4f, 1.0f), CityArtMaterialCategory.Dirt);
            Prop(root, PrimitiveType.Cube, "BeamL", new Vector3(-0.85f, 1.5f, -1.3f),
                new Vector3(0.18f, 2.2f, 0.18f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "BeamR", new Vector3(0.85f, 1.5f, -1.3f),
                new Vector3(0.18f, 2.2f, 0.18f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "BeamTop", new Vector3(0f, 2.5f, -1.3f),
                new Vector3(2.0f, 0.18f, 0.18f), CityArtMaterialCategory.Wood);
            var lamp = Prop(root, PrimitiveType.Sphere, "Lantern", new Vector3(0.7f, 1.8f, -1.1f),
                Vector3.one * 0.28f, CityArtMaterialCategory.Metal);
            CityVisualMaterials.ApplyEmissiveHint(lamp.GetComponent<Renderer>(), new Color(1f, 0.75f, 0.35f), 0.6f);
            Prop(root, PrimitiveType.Cube, "OreCart", new Vector3(1.5f, 0.4f, 1.4f),
                new Vector3(1.0f, 0.55f, 0.7f), CityArtMaterialCategory.Wood);
        }

        private static void Residence(Transform root)
        {
            // Mansão nobre — pedra + madeira, sem telhado “doce”.
            Prop(root, PrimitiveType.Cube, "Wing", new Vector3(1.7f, 1.0f, 0.1f),
                new Vector3(1.5f, 1.7f, 2.0f), CityArtMaterialCategory.StoneLight);
            Prop(root, PrimitiveType.Cube, "WingRoof", new Vector3(1.7f, 2.0f, 0.1f),
                new Vector3(1.7f, 0.22f, 2.2f), CityArtMaterialCategory.Roof);
            Prop(root, PrimitiveType.Cube, "PorchColL", new Vector3(-0.7f, 0.9f, 1.55f),
                new Vector3(0.22f, 1.5f, 0.22f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "PorchColR", new Vector3(0.7f, 0.9f, 1.55f),
                new Vector3(0.22f, 1.5f, 0.22f), CityArtMaterialCategory.StoneDark);
            Prop(root, PrimitiveType.Cube, "GardenBed", new Vector3(-2.0f, 0.15f, 1.4f),
                new Vector3(1.2f, 0.2f, 1.0f), CityArtMaterialCategory.Grass);
            Banner(root, new Vector3(0f, 3.3f, 0.2f), new Color(0.45f, 0.2f, 0.18f));
        }

        private static void Tent(Transform root, string name, Vector3 pos, float width, Color canvas)
        {
            Prop(root, PrimitiveType.Cube, name + "PostL", pos + new Vector3(-width * 0.4f, 0.7f, 0f),
                new Vector3(0.1f, 1.2f, 0.1f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, name + "PostR", pos + new Vector3(width * 0.4f, 0.7f, 0f),
                new Vector3(0.1f, 1.2f, 0.1f), CityArtMaterialCategory.Wood);
            var roof = Prop(root, PrimitiveType.Cube, name + "Canvas", pos + new Vector3(0f, 1.35f, 0f),
                new Vector3(width, 0.12f, width * 0.85f), CityArtMaterialCategory.Roof);
            CityArtMaterialLibrary.ApplyVariant(roof.GetComponent<Renderer>(), CityArtMaterialCategory.Roof, canvas, 0.65f);
        }

        private static void Awning(Transform root, Vector3 pos, float w, float d)
        {
            Prop(root, PrimitiveType.Cube, "Awning", pos, new Vector3(w, 0.1f, d), CityArtMaterialCategory.Roof);
            Prop(root, PrimitiveType.Cube, "AwningPostL", pos + new Vector3(-w * 0.45f, -0.55f, d * 0.35f),
                new Vector3(0.1f, 1.0f, 0.1f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cube, "AwningPostR", pos + new Vector3(w * 0.45f, -0.55f, d * 0.35f),
                new Vector3(0.1f, 1.0f, 0.1f), CityArtMaterialCategory.Wood);
        }

        private static void Crate(Transform root, Vector3 pos) =>
            Prop(root, PrimitiveType.Cube, "Crate", pos, new Vector3(0.45f, 0.45f, 0.45f), CityArtMaterialCategory.Wood);

        private static void Barrel(Transform root, Vector3 pos) =>
            Prop(root, PrimitiveType.Cylinder, "Barrel", pos, new Vector3(0.5f, 0.38f, 0.5f), CityArtMaterialCategory.Wood);

        private static void Cart(Transform root, Vector3 pos)
        {
            Prop(root, PrimitiveType.Cube, "CartBed", pos, new Vector3(1.2f, 0.35f, 0.7f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cylinder, "WheelL", pos + new Vector3(-0.35f, -0.15f, 0.4f),
                new Vector3(0.45f, 0.08f, 0.45f), CityArtMaterialCategory.Wood);
            Prop(root, PrimitiveType.Cylinder, "WheelR", pos + new Vector3(0.35f, -0.15f, 0.4f),
                new Vector3(0.45f, 0.08f, 0.45f), CityArtMaterialCategory.Wood);
        }

        private static void Banner(Transform root, Vector3 tip, Color color)
        {
            Prop(root, PrimitiveType.Cylinder, "Pole", tip + Vector3.down * 0.45f,
                new Vector3(0.06f, 0.45f, 0.06f), CityArtMaterialCategory.Wood);
            var cloth = Prop(root, PrimitiveType.Cube, "Cloth", tip + new Vector3(0.22f, -0.1f, 0f),
                new Vector3(0.4f, 0.28f, 0.05f), CityArtMaterialCategory.Metal);
            CityArtMaterialLibrary.ApplyVariant(cloth.GetComponent<Renderer>(), CityArtMaterialCategory.Metal, color, 0.85f);
        }

        private static GameObject Prop(
            Transform parent,
            PrimitiveType type,
            string name,
            Vector3 localPos,
            Vector3 scale,
            CityArtMaterialCategory category,
            float alpha = 1f)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            go.layer = 2; // Ignore Raycast
            var color = CityArtMaterialLibrary.ColorOf(category);
            if (alpha < 0.99f)
            {
                color.a = alpha;
            }

            CityArtMaterialLibrary.ApplyVariant(go.GetComponent<Renderer>(), category, color, 0.15f);
            return go;
        }
    }
}
