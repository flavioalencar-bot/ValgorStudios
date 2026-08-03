using System;
using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Ambiente da Capital: terreno, eixo monumental, muralha, rios, distritos e tiers.
    /// Hierarquia alinhada ao blueprint oficial (sem lógica de gameplay).
    /// </summary>
    public static class CityEnvironmentBuilder
    {
        private static Renderer[] _fortRenderers = Array.Empty<Renderer>();
        private static Color[] _fortBaseColors = Array.Empty<Color>();

        public static void Build(Transform parent)
        {
            var root = new GameObject("CityRoot").transform;
            root.SetParent(parent, false);

            HideSceneGround();

            var terrain = Node(root, "Terrain");
            var water = Node(root, "Water");
            var roads = Node(root, "Roads");
            var districts = Node(root, "Districts");
            var decorations = Node(root, "Decorations");
            var vegetation = Node(root, "Vegetation");
            var ambient = Node(root, "AmbientFX");
            var tierGroups = Node(root, "TierGroups");
            Node(root, "Walls"); // preenchido em ApplyWallLevel
            Node(root, "Buildings"); // slots ficam em BuildingSlots no bootstrap
            Node(root, "NPCs");
            Node(root, "Navigation");

            BuildTerrain(terrain);
            BuildHorizon(terrain);
            CityReliefBuilder.Build(terrain);
            BuildRivers(water);
            BuildDistrictPads(districts);
            if (CityRoyalDistrictConfig.Enabled)
            {
                // Sprint 2 assume praça / complexo imperial (BuildPlaza legado omitido).
            }
            else
            {
                BuildPlaza(roads);
            }

            BuildRoads(roads);
            BuildDistrictRoads(roads);
            BuildPlateaus(terrain);
            CityDistrictVisualBuilder.BuildAll(districts, decorations, ambient);
            if (!(CityArtPass3Config.Enabled && CityArtPass3Config.LayeredVegetationEnabled))
            {
                BuildTrees(vegetation);
            }

            CityNatureBuilder.Build(vegetation);
            BuildTierDecoration(tierGroups);
            BuildAmbientProps(decorations, ambient);
            CityUrbanDensityBuilder.Build(decorations);
            CityScenarioCompositionBuilder.Build(terrain, water, decorations, vegetation);
            var npcs = root.Find("NPCs") ?? Node(root, "NPCs");
            CityArtPass3Builder.Build(terrain, water, roads, districts, decorations, vegetation, npcs);
            if (CityRoyalDistrictConfig.Enabled)
            {
                CityRoyalDistrictBuilder.Build(roads, decorations, vegetation);
            }

            CityFaunaBuilder.Build(ambient);
            CityNpcAmbientBuilder.Build(npcs);
            CityDistrictAmbienceAudio.Build(root);
            SoftenLighting();
            CityArtMaterialLibrary.WarmPalette();
            CityAtmosphereFx.Attach(root);
            // Muralha sempre presente no boot (nível visual mínimo 1) — reforça leitura da Capital.
            ApplyWallLevel(parent, 1);
            TryStaticBatch(root);
            CityArtPass2PerfProbe.Ensure();
            TryAttachLodHints(root);
        }

        public static void ApplyWallLevel(Transform cityHost, int wallLevel)
        {
            if (cityHost == null)
            {
                return;
            }

            var existing = cityHost.Find("CityFortifications");
            if (existing != null)
            {
                existing.name = "CityFortifications_PendingDestroy";
                UnityEngine.Object.Destroy(existing.gameObject);
            }

            // Preferir Walls sob CityRoot; fallback no host.
            var cityRoot = cityHost.Find("CityRoot");
            var wallsParent = cityRoot != null ? cityRoot.Find("Walls") : null;
            var parent = wallsParent != null ? wallsParent : cityHost;

            var root = new GameObject("CityFortifications").transform;
            root.SetParent(parent, false);
            var level = Math.Max(0, wallLevel);
            if (CityFortressWallConfig.Enabled && CityFortressWallPlacer.TryBuild(root, level))
            {
                CacheFortificationRenderers(root);
                return;
            }

            // Fallback legado (Art Pass 3 / procedural) se o kit Sprint 1 falhar.
            BuildWallRing(root, level);
            BuildMainGate(root, level);
            CacheFortificationRenderers(root);
        }

        public static Transform? FindFortifications(Transform cityRoot)
        {
            if (cityRoot == null)
            {
                return null;
            }

            var direct = cityRoot.Find("CityFortifications");
            if (direct != null)
            {
                return direct;
            }

            var nested = cityRoot.Find("CityRoot/Walls/CityFortifications");
            return nested;
        }

        public static void SetFortificationsHighlighted(bool selected)
        {
            if (_fortRenderers.Length == 0 || _fortBaseColors.Length != _fortRenderers.Length)
            {
                return;
            }

            var gold = new Color(0.86f, 0.72f, 0.38f);
            for (var i = 0; i < _fortRenderers.Length; i++)
            {
                var renderer = _fortRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                var color = selected
                    ? Color.Lerp(_fortBaseColors[i], gold, 0.42f)
                    : _fortBaseColors[i];
                CityVisualMaterials.Apply(renderer, color);
            }
        }

        private static void CacheFortificationRenderers(Transform root)
        {
            _fortRenderers = root.GetComponentsInChildren<Renderer>();
            _fortBaseColors = new Color[_fortRenderers.Length];
            for (var i = 0; i < _fortRenderers.Length; i++)
            {
                _fortBaseColors[i] = ReadRendererColor(_fortRenderers[i]);
            }
        }

        private static Color ReadRendererColor(Renderer renderer)
        {
            var material = renderer.sharedMaterial;
            if (material == null)
            {
                return Color.gray;
            }

            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }

            return material.color;
        }

        private static Transform Node(Transform parent, string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }

        private static void BuildTerrain(Transform root)
        {
            Flat(root, "GrassFar", Vector3.zero, new Vector3(110f, 0.06f, 110f),
                    CityVisualMaterials.Vegetation, SurfaceKind.Vegetation)
                .transform.localPosition = new Vector3(0f, -0.06f, 0f);

            Flat(root, "GrassMid", Vector3.zero, new Vector3(64f, 0.07f, 64f),
                    Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.12f),
                    SurfaceKind.Vegetation)
                .transform.localPosition = new Vector3(0f, -0.03f, 0f);

            Flat(root, "InnerSoil", Vector3.zero, new Vector3(42f, 0.05f, 42f),
                    Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.28f),
                    SurfaceKind.Dirt)
                .transform.localPosition = new Vector3(0f, -0.01f, 0f);

            // Manchas de gramado / terra para quebrar o “tapete” plano.
            var meadow = Color.Lerp(CityVisualMaterials.Vegetation, new Color(0.4f, 0.52f, 0.28f), 0.35f);
            var dry = Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Vegetation, 0.45f);
            Flat(root, "MeadowPatchA", new Vector3(-10f, 0.008f, 8f), new Vector3(8f, 0.04f, 6f), meadow, SurfaceKind.Vegetation);
            Flat(root, "MeadowPatchB", new Vector3(11f, 0.008f, -9f), new Vector3(7f, 0.04f, 7f), meadow, SurfaceKind.Vegetation);
            Flat(root, "DryPatchA", new Vector3(-8f, 0.008f, -12f), new Vector3(6f, 0.04f, 5f), dry, SurfaceKind.Dirt);
            Flat(root, "DryPatchB", new Vector3(9f, 0.008f, 10f), new Vector3(5.5f, 0.04f, 5f), dry, SurfaceKind.Dirt);
        }

        private static void BuildHorizon(Transform root)
        {
            var hill = Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.StoneDark, 0.35f);
            var mist = Color.Lerp(CityVisualMaterials.Vegetation, new Color(0.45f, 0.5f, 0.48f), 0.4f);
            const float radius = 42f;
            const int hills = CityCapitalFinalConfig.HorizonMountainCount;
            for (var i = 0; i < hills; i++)
            {
                var angle = i * (360f / hills) * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Sin(angle) * radius, 1.2f, Mathf.Cos(angle) * radius);
                var w = 8f + (i % 3) * 1.3f;
                var h = 3.4f + (i % 4) * 0.55f;
                Part(PrimitiveType.Sphere, root, $"Hill_{i}", pos, new Vector3(w, h, w * 0.85f), hill,
                    SurfaceKind.Vegetation);
                if (i % 3 == 0)
                {
                    Part(PrimitiveType.Sphere, root, $"Peak_{i}", pos + Vector3.up * (h * 0.35f),
                        new Vector3(w * 0.45f, h * 0.55f, w * 0.4f), mist, SurfaceKind.Vegetation);
                }
            }

            Flat(root, "MistRing", Vector3.zero, new Vector3(96f, 0.04f, 96f), mist, SurfaceKind.Vegetation)
                .transform.localPosition = new Vector3(0f, -0.08f, 0f);
        }

        private static void BuildRivers(Transform root)
        {
            var water = new Color(0.22f, 0.38f, 0.48f);
            var bank = Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.35f);
            var reed = Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.15f);

            // Rios laterais com leve curva (segmentos), fora do anel, sem atravessar o eixo.
            Flat(root, "RiverWest", new Vector3(-24.5f, -0.12f, 0f), new Vector3(4.2f, 0.2f, 48f), water, SurfaceKind.Plain);
            Flat(root, "RiverWestBendN", new Vector3(-25.2f, -0.11f, 14f), new Vector3(3.4f, 0.18f, 10f), water, SurfaceKind.Plain);
            Flat(root, "RiverWestBendS", new Vector3(-25.0f, -0.11f, -14f), new Vector3(3.2f, 0.18f, 10f), water, SurfaceKind.Plain);
            Flat(root, "RiverEast", new Vector3(24.5f, -0.12f, 0f), new Vector3(4.2f, 0.2f, 48f), water, SurfaceKind.Plain);
            Flat(root, "RiverEastBendN", new Vector3(25.3f, -0.11f, 12f), new Vector3(3.3f, 0.18f, 10f), water, SurfaceKind.Plain);
            Flat(root, "RiverEastBendS", new Vector3(25.1f, -0.11f, -12f), new Vector3(3.2f, 0.18f, 10f), water, SurfaceKind.Plain);
            Flat(root, "BankWest", new Vector3(-22.2f, -0.02f, 0f), new Vector3(1.2f, 0.12f, 46f), bank, SurfaceKind.Dirt);
            Flat(root, "BankEast", new Vector3(22.2f, -0.02f, 0f), new Vector3(1.2f, 0.12f, 46f), bank, SurfaceKind.Dirt);
            Flat(root, "ReedWest", new Vector3(-22.8f, 0.05f, 4f), new Vector3(0.8f, 0.25f, 8f), reed, SurfaceKind.Vegetation);
            Flat(root, "ReedEast", new Vector3(22.8f, 0.05f, -3f), new Vector3(0.8f, 0.25f, 8f), reed, SurfaceKind.Vegetation);

            // Pontes com guardrails leves.
            Flat(root, "BridgeWest", new Vector3(-20.8f, 0.08f, 0f), new Vector3(3.2f, 0.18f, 2.4f),
                CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            Flat(root, "BridgeEast", new Vector3(20.8f, 0.08f, 0f), new Vector3(3.2f, 0.18f, 2.4f),
                CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, "BridgeWestRailN", new Vector3(-20.8f, 0.35f, 1.0f),
                new Vector3(3.0f, 0.25f, 0.12f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, "BridgeWestRailS", new Vector3(-20.8f, 0.35f, -1.0f),
                new Vector3(3.0f, 0.25f, 0.12f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, "BridgeEastRailN", new Vector3(20.8f, 0.35f, 1.0f),
                new Vector3(3.0f, 0.25f, 0.12f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, "BridgeEastRailS", new Vector3(20.8f, 0.35f, -1.0f),
                new Vector3(3.0f, 0.25f, 0.12f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);

            // Margens úmidas + vegetação ribeirinha.
            FlatShared(root, "WetBankW", new Vector3(-22.6f, 0.01f, 0f), new Vector3(0.7f, 0.08f, 40f),
                CityArtMaterialCategory.Dirt);
            FlatShared(root, "WetBankE", new Vector3(22.6f, 0.01f, 0f), new Vector3(0.7f, 0.08f, 40f),
                CityArtMaterialCategory.Dirt);
            FlatShared(root, "ReedClumpW1", new Vector3(-22.9f, 0.12f, 8f), new Vector3(0.9f, 0.35f, 2.2f),
                CityArtMaterialCategory.Grass);
            FlatShared(root, "ReedClumpW2", new Vector3(-23.1f, 0.12f, -6f), new Vector3(0.9f, 0.35f, 2.0f),
                CityArtMaterialCategory.Grass);
            FlatShared(root, "ReedClumpE1", new Vector3(23.0f, 0.12f, 5f), new Vector3(0.9f, 0.35f, 2.2f),
                CityArtMaterialCategory.Grass);

            if (CityAmbientQualitySettings.AllowWaterFx)
            {
                Part(PrimitiveType.Cube, root, "FallWest", new Vector3(-24.5f, 0.35f, 10f),
                    new Vector3(1.2f, 1.1f, 0.35f), Color.Lerp(water, Color.white, 0.25f), SurfaceKind.Plain);
                Part(PrimitiveType.Cube, root, "FallEast", new Vector3(24.5f, 0.35f, -8f),
                    new Vector3(1.2f, 1.1f, 0.35f), Color.Lerp(water, Color.white, 0.25f), SurfaceKind.Plain);
                Part(PrimitiveType.Cube, root, "FallWest2", new Vector3(-25.0f, 0.3f, -9f),
                    new Vector3(1.0f, 0.9f, 0.3f), Color.Lerp(water, Color.white, 0.2f), SurfaceKind.Plain);
                // Espuma / reflexo discreto.
                PartShared(PrimitiveType.Cube, root, "FoamWest", new Vector3(-24.5f, 0.02f, 9.2f),
                    new Vector3(1.6f, 0.05f, 1.2f), CityArtMaterialCategory.Water);
                PartShared(PrimitiveType.Cube, root, "FoamEast", new Vector3(24.5f, 0.02f, -7.2f),
                    new Vector3(1.6f, 0.05f, 1.2f), CityArtMaterialCategory.Water);
                PartShared(PrimitiveType.Cube, root, "ReflectHintW", new Vector3(-24.5f, -0.08f, 0f),
                    new Vector3(3.6f, 0.04f, 20f), CityArtMaterialCategory.Water);
            }

            Part(PrimitiveType.Sphere, root, "RockW1", new Vector3(-23.2f, 0.2f, 6f), new Vector3(1.4f, 0.7f, 1.1f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Sphere, root, "RockW2", new Vector3(-23.6f, 0.18f, -7f), new Vector3(1.2f, 0.6f, 1.0f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Sphere, root, "RockE1", new Vector3(23.4f, 0.2f, -5f), new Vector3(1.5f, 0.75f, 1.2f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Sphere, root, "RockE2", new Vector3(23.8f, 0.18f, 8f), new Vector3(1.3f, 0.65f, 1.1f),
                CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            Part(PrimitiveType.Sphere, root, "RockW3", new Vector3(-22.8f, 0.15f, 1f), new Vector3(0.9f, 0.45f, 0.8f),
                CityVisualMaterials.StoneLight, SurfaceKind.Stone);
        }

        private static void BuildDistrictPads(Transform root)
        {
            var soft = CityArtPass3Config.Enabled && CityArtPass3Config.SoftDistrictPadsEnabled;
            var soil = soft
                ? Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.22f)
                : Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.2f);
            Flat(root, "PadRoyal", new Vector3(0f, 0.004f, 1.5f), new Vector3(14f, 0.04f, 12f),
                soft || CityRoyalDistrictConfig.Enabled
                    ? Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.18f)
                    : Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.2f),
                SurfaceKind.Path);
            var dragonZ = CityCapitalLayoutConfig.GetPosition("dragon-tower").Z;
            Flat(root, "PadDragon", new Vector3(0f, 0.08f, dragonZ), new Vector3(14f, 0.12f, 14f),
                soft
                    ? Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.2f)
                    : Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Vegetation, 0.35f), SurfaceKind.Dirt);
            Flat(root, "PadSacredNW", new Vector3(-9f, 0.005f, 4.5f), new Vector3(7f, 0.04f, 7f),
                soil, SurfaceKind.Dirt);
            Flat(root, "PadSacredNE", new Vector3(9f, 0.005f, 4.8f), new Vector3(7f, 0.04f, 7f),
                soil, SurfaceKind.Dirt);
            Flat(root, "PadMilitary", new Vector3(12f, 0.005f, -1f), new Vector3(9f, 0.04f, 14f),
                soft
                    ? Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.12f)
                    : Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.18f), SurfaceKind.Dirt);
            Flat(root, "PadEconomic", new Vector3(-4f, 0.005f, -7.5f), new Vector3(16f, 0.04f, 7f),
                soft
                    ? Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Path, 0.15f)
                    : Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Path, 0.25f), SurfaceKind.Dirt);
            Flat(root, "PadIndustrialSW", new Vector3(-13.5f, 0.005f, -13f), new Vector3(8f, 0.04f, 6f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Wood, soft ? 0.08f : 0.15f), SurfaceKind.Dirt);
            Flat(root, "PadFoundry", new Vector3(12.8f, 0.005f, -13f), new Vector3(6f, 0.04f, 5f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, soft ? 0.15f : 0.25f), SurfaceKind.Dirt);
            Flat(root, "PadResidence", new Vector3(-5.8f, 0.005f, -8.8f), new Vector3(5f, 0.04f, 4.5f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Path, soft ? 0.1f : 0.15f), SurfaceKind.Dirt);
            Flat(root, "PadRural", new Vector3(-14f, 0.005f, -7.2f), new Vector3(7f, 0.04f, 6f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.2f), SurfaceKind.Vegetation);
            Flat(root, "PadMine", new Vector3(-15f, 0.7f, 11.5f), new Vector3(6f, 0.2f, 5f),
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.3f), SurfaceKind.Stone);
            Flat(root, "PadQuarry", new Vector3(15f, 0.7f, 12f), new Vector3(6f, 0.2f, 5f),
                Color.Lerp(CityVisualMaterials.StoneLight, CityVisualMaterials.Dirt, 0.25f), SurfaceKind.Stone);
            Flat(root, "PadAcademy", new Vector3(14f, 0.8f, 9.4f), new Vector3(6f, 0.2f, 5f),
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.RoofBlue, soft ? 0.05f : 0.12f), SurfaceKind.Stone);
        }

        private static void BuildPlateaus(Transform root)
        {
            var dragonZ = CityCapitalLayoutConfig.GetPosition("dragon-tower").Z;
            const float topY = 2.05f;

            // Platô rochoso da Torre (norte) — penhasco, escada, pedras, vegetação.
            FlatShared(root, "PlateauDragonMass", new Vector3(0f, topY * 0.5f, dragonZ),
                new Vector3(8.2f, topY, 7.4f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonMassL", new Vector3(-2.8f, topY * 0.42f, dragonZ + 1.8f),
                new Vector3(3.8f, topY * 0.85f, 3.6f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonMassR", new Vector3(2.6f, topY * 0.4f, dragonZ - 1.2f),
                new Vector3(3.4f, topY * 0.8f, 3.2f), CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlateauDragonCap", new Vector3(0f, topY + 0.1f, dragonZ),
                new Vector3(5.6f, 0.24f, 5.2f), CityArtMaterialCategory.StoneLight);

            // Penhasco sul (face à Capital).
            FlatShared(root, "PlateauDragonCliff", new Vector3(0f, 1.25f, dragonZ - 4.3f),
                new Vector3(9.2f, 2.7f, 2.3f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonCliffL", new Vector3(-3.5f, 1.0f, dragonZ - 5.1f),
                new Vector3(3.1f, 2.2f, 2.4f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonCliffR", new Vector3(3.3f, 0.95f, dragonZ - 4.9f),
                new Vector3(2.9f, 2.1f, 2.3f), CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlateauDragonCliffBase", new Vector3(0f, 0.28f, dragonZ - 5.9f),
                new Vector3(10.2f, 0.6f, 2.0f), CityArtMaterialCategory.Dirt);

            // Escadaria ceremonial.
            FlatShared(root, "PlateauDragonStairBed", new Vector3(0f, 0.12f, dragonZ - 7.5f),
                new Vector3(4.3f, 0.16f, 5.0f), CityArtMaterialCategory.Dirt);
            FlatShared(root, "PlateauDragonStair1", new Vector3(0f, 0.3f, dragonZ - 7.7f),
                new Vector3(3.9f, 0.22f, 0.8f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonStair2", new Vector3(0f, 0.58f, dragonZ - 7.1f),
                new Vector3(3.55f, 0.22f, 0.75f), CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlateauDragonStair3", new Vector3(0f, 0.88f, dragonZ - 6.55f),
                new Vector3(3.25f, 0.22f, 0.75f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonStair4", new Vector3(0f, 1.18f, dragonZ - 6.05f),
                new Vector3(2.95f, 0.22f, 0.7f), CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlateauDragonStair5", new Vector3(0f, 1.48f, dragonZ - 5.6f),
                new Vector3(2.65f, 0.22f, 0.65f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonStair6", new Vector3(0f, 1.78f, dragonZ - 5.2f),
                new Vector3(2.4f, 0.22f, 0.6f), CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlateauDragonStair7", new Vector3(0f, 2.02f, dragonZ - 4.8f),
                new Vector3(2.2f, 0.2f, 0.55f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonRailL", new Vector3(-2.05f, 1.2f, dragonZ - 6.3f),
                new Vector3(0.22f, 1.85f, 4.1f), CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlateauDragonRailR", new Vector3(2.05f, 1.2f, dragonZ - 6.3f),
                new Vector3(0.22f, 1.85f, 4.1f), CityArtMaterialCategory.StoneLight);

            // Pedras + vegetação.
            FlatShared(root, "PlateauDragonRockA", new Vector3(-5.3f, 0.75f, dragonZ + 2.3f),
                new Vector3(2.1f, 1.5f, 1.8f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonRockB", new Vector3(5.1f, 0.7f, dragonZ + 1.7f),
                new Vector3(1.9f, 1.4f, 2.0f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonRockC", new Vector3(-4.5f, 0.85f, dragonZ - 2.5f),
                new Vector3(1.8f, 1.7f, 1.6f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonRockD", new Vector3(4.3f, 0.65f, dragonZ - 2.9f),
                new Vector3(1.7f, 1.3f, 1.9f), CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlateauDragonRockE", new Vector3(-2.7f, 0.5f, dragonZ - 7.0f),
                new Vector3(1.3f, 1.0f, 1.2f), CityArtMaterialCategory.StoneDark);
            FlatShared(root, "PlateauDragonRockF", new Vector3(2.5f, 0.45f, dragonZ - 7.2f),
                new Vector3(1.2f, 0.9f, 1.3f), CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlateauDragonBushA", new Vector3(-6.3f, 0.55f, dragonZ + 3.1f),
                Vector3.one * 1.15f, CityArtMaterialCategory.Grass);
            FlatShared(root, "PlateauDragonBushB", new Vector3(6.1f, 0.52f, dragonZ + 2.7f),
                Vector3.one * 1.1f, CityArtMaterialCategory.Grass);
            FlatShared(root, "PlateauDragonBushC", new Vector3(-5.5f, 0.48f, dragonZ - 4.7f),
                Vector3.one * 0.9f, CityArtMaterialCategory.Grass);
            FlatShared(root, "PlateauDragonBushD", new Vector3(5.3f, 0.45f, dragonZ - 4.9f),
                Vector3.one * 0.88f, CityArtMaterialCategory.Grass);
            FlatShared(root, "PlateauDragonBushE", new Vector3(-2.9f, 0.42f, dragonZ - 8.2f),
                Vector3.one * 0.75f, CityArtMaterialCategory.Grass);
            FlatShared(root, "PlateauDragonBushF", new Vector3(2.7f, 0.4f, dragonZ - 8.4f),
                Vector3.one * 0.75f, CityArtMaterialCategory.Grass);

            // Via de aproximação Capital → Torre.
            FlatShared(root, "PlateauDragonApproach", new Vector3(0f, 0.08f, (dragonZ + 2f) * 0.5f),
                new Vector3(2.6f, 0.12f, Mathf.Max(6f, dragonZ - 10f)), CityArtMaterialCategory.StoneDark);

            Flat(root, "PlateauAcademy", new Vector3(14.2f, 0.55f, 9.5f), new Vector3(5.5f, 0.55f, 4.5f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Flat(root, "PlateauMine", new Vector3(-15f, 0.5f, 11.8f), new Vector3(5f, 0.5f, 4.2f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Flat(root, "PlateauQuarry", new Vector3(15.2f, 0.5f, 12.2f), new Vector3(5f, 0.5f, 4.2f),
                CityVisualMaterials.StoneLight, SurfaceKind.Stone);
        }

        private static void BuildPlaza(Transform root)
        {
            var z = CityCapitalEnvironmentConfig.PlazaCenterZ;
            // Praça nobre — piso em pedra, fora do eixo de seleção do Castelo.
            FlatShared(root, "PlazaRim", new Vector3(0f, 0.02f, z), new Vector3(13.2f, 0.08f, 13.2f),
                CityArtMaterialCategory.StoneDark);
            FlatShared(root, "Plaza", new Vector3(0f, 0.035f, z), new Vector3(10.2f, 0.1f, 10.2f),
                CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlazaCore", new Vector3(0f, 0.05f, z), new Vector3(4.2f, 0.12f, 4.2f),
                CityArtMaterialCategory.StoneLight);
            FlatShared(root, "PlazaRing", new Vector3(0f, 0.055f, z), new Vector3(7.8f, 0.05f, 7.8f),
                CityArtMaterialCategory.Metal);

            // Fonte refinada (Ignore Raycast — não bloqueia Castelo).
            PartShared(PrimitiveType.Cylinder, root, "FountainBase", new Vector3(0f, 0.38f, z),
                new Vector3(2.0f, 0.38f, 2.0f), CityArtMaterialCategory.StoneLight);
            PartShared(PrimitiveType.Cylinder, root, "FountainBowl", new Vector3(0f, 0.72f, z),
                new Vector3(1.55f, 0.18f, 1.55f), CityArtMaterialCategory.StoneDark);
            PartShared(PrimitiveType.Cylinder, root, "FountainStem", new Vector3(0f, 1.05f, z),
                new Vector3(0.4f, 0.45f, 0.4f), CityArtMaterialCategory.StoneLight);
            PartShared(PrimitiveType.Cylinder, root, "FountainTop", new Vector3(0f, 1.45f, z),
                new Vector3(0.85f, 0.16f, 0.85f), CityArtMaterialCategory.StoneDark);
            PartShared(PrimitiveType.Cylinder, root, "FountainWater", new Vector3(0f, 0.68f, z),
                new Vector3(1.35f, 0.1f, 1.35f), CityArtMaterialCategory.Water);

            // Brasão de Valgor no piso (anel ao redor da fonte — não cobre o Castelo).
            FlatShared(root, "ValgorCrest", new Vector3(0f, 0.07f, z - 2.4f), new Vector3(1.6f, 0.06f, 1.6f),
                CityArtMaterialCategory.Metal);
            PartShared(PrimitiveType.Cube, root, "CrestGem", new Vector3(0f, 0.18f, z - 2.4f),
                new Vector3(0.55f, 0.12f, 0.55f), CityArtMaterialCategory.Metal);

            // Bancos, canteiros e estátuas pequenas nos acessos laterais.
            PartShared(PrimitiveType.Cube, root, "BenchNE", new Vector3(-3.2f, 0.3f, z + 3.0f),
                new Vector3(1.6f, 0.3f, 0.42f), CityArtMaterialCategory.Wood);
            PartShared(PrimitiveType.Cube, root, "BenchNW", new Vector3(3.2f, 0.3f, z + 3.0f),
                new Vector3(1.6f, 0.3f, 0.42f), CityArtMaterialCategory.Wood);
            PartShared(PrimitiveType.Cube, root, "BenchSE", new Vector3(-3.2f, 0.3f, z - 3.0f),
                new Vector3(1.6f, 0.3f, 0.42f), CityArtMaterialCategory.Wood);
            PartShared(PrimitiveType.Cube, root, "BenchSW", new Vector3(3.2f, 0.3f, z - 3.0f),
                new Vector3(1.6f, 0.3f, 0.42f), CityArtMaterialCategory.Wood);
            PartShared(PrimitiveType.Cube, root, "PlanterE", new Vector3(4.6f, 0.24f, z),
                new Vector3(1.0f, 0.35f, 1.0f), CityArtMaterialCategory.Dirt);
            PartShared(PrimitiveType.Cube, root, "PlanterEHedge", new Vector3(4.6f, 0.5f, z),
                new Vector3(0.85f, 0.3f, 0.85f), CityArtMaterialCategory.Grass);
            PartShared(PrimitiveType.Cube, root, "PlanterW", new Vector3(-4.6f, 0.24f, z),
                new Vector3(1.0f, 0.35f, 1.0f), CityArtMaterialCategory.Dirt);
            PartShared(PrimitiveType.Cube, root, "PlanterWHedge", new Vector3(-4.6f, 0.5f, z),
                new Vector3(0.85f, 0.3f, 0.85f), CityArtMaterialCategory.Grass);
            // Estátuas + monumento (fora do eixo de seleção do Castelo).
            PartShared(PrimitiveType.Cylinder, root, "StatueBaseE", new Vector3(5.5f, 0.25f, z + 2.2f),
                new Vector3(0.55f, 0.25f, 0.55f), CityArtMaterialCategory.StoneDark);
            PartShared(PrimitiveType.Cube, root, "StatueE", new Vector3(5.5f, 0.85f, z + 2.2f),
                new Vector3(0.35f, 1.0f, 0.35f), CityArtMaterialCategory.StoneLight);
            PartShared(PrimitiveType.Cylinder, root, "StatueBaseW", new Vector3(-5.5f, 0.25f, z + 2.2f),
                new Vector3(0.55f, 0.25f, 0.55f), CityArtMaterialCategory.StoneDark);
            PartShared(PrimitiveType.Cube, root, "StatueW", new Vector3(-5.5f, 0.85f, z + 2.2f),
                new Vector3(0.35f, 1.0f, 0.35f), CityArtMaterialCategory.StoneLight);
            PartShared(PrimitiveType.Cylinder, root, "MonumentBase", new Vector3(0f, 0.2f, z + 3.6f),
                new Vector3(1.1f, 0.25f, 1.1f), CityArtMaterialCategory.StoneDark);
            PartShared(PrimitiveType.Cube, root, "MonumentObelisk", new Vector3(0f, 1.35f, z + 3.6f),
                new Vector3(0.45f, 2.1f, 0.45f), CityArtMaterialCategory.StoneLight);
            PartShared(PrimitiveType.Cube, root, "MonumentCrest", new Vector3(0f, 2.55f, z + 3.6f),
                new Vector3(0.7f, 0.2f, 0.2f), CityArtMaterialCategory.Metal);
            // Jardins ornamentais + árvores da praça (laterais).
            PartShared(PrimitiveType.Sphere, root, "OrnTreeL", new Vector3(-5.8f, 1.4f, z + 1.2f),
                new Vector3(1.3f, 1.1f, 1.3f), CityArtMaterialCategory.Grass);
            PartShared(PrimitiveType.Cylinder, root, "OrnTrunkL", new Vector3(-5.8f, 0.55f, z + 1.2f),
                new Vector3(0.22f, 0.55f, 0.22f), CityArtMaterialCategory.Wood);
            PartShared(PrimitiveType.Sphere, root, "OrnTreeR", new Vector3(5.8f, 1.4f, z + 1.2f),
                new Vector3(1.3f, 1.1f, 1.3f), CityArtMaterialCategory.Grass);
            PartShared(PrimitiveType.Cylinder, root, "OrnTrunkR", new Vector3(5.8f, 0.55f, z + 1.2f),
                new Vector3(0.22f, 0.55f, 0.22f), CityArtMaterialCategory.Wood);
            PartShared(PrimitiveType.Cube, root, "FlowerBedN", new Vector3(0f, 0.12f, z + 4.6f),
                new Vector3(3.2f, 0.16f, 0.9f), CityArtMaterialCategory.Grass);

            if (CityAmbientQualitySettings.AllowFlags)
            {
                Flag(root, new Vector3(-4.0f, 0.15f, z + 3.6f));
                Flag(root, new Vector3(4.0f, 0.15f, z + 3.6f));
            }

            if (CityAmbientQualitySettings.AllowTorches)
            {
                Torch(root, new Vector3(-5.0f, 0.75f, z - 1.4f));
                Torch(root, new Vector3(5.0f, 0.75f, z - 1.4f));
                Torch(root, new Vector3(-5.0f, 0.75f, z + 1.4f));
                Torch(root, new Vector3(5.0f, 0.75f, z + 1.4f));
            }
        }

        private static void BuildRoads(Transform root)
        {
            var imperial = Node(root, "ImperialRoad");
            var main = Node(root, "MainRoads");
            var secondary = Node(root, "SecondaryRoads");

            // Portão (S) → Praça → Castelo — Estrada Imperial pavimentada.
            var imperialW = CityCapitalEnvironmentConfig.ImperialRoadWidth;
            Road(imperial, "ImperialSouth", new Vector3(0f, 0.04f, -11.5f), new Vector3(imperialW, 0.1f, 17f), 0f, imperialGrade: true);
            Road(imperial, "ImperialNorth", new Vector3(0f, 0.04f, 9f), new Vector3(imperialW * 0.85f, 0.1f, 16f), 0f, imperialGrade: true);
            // Bordas, grama e drenagem lateral.
            FlatShared(imperial, "ImperialEdgeL", new Vector3(-imperialW * 0.55f, 0.055f, -8f),
                new Vector3(0.28f, 0.08f, 22f), CityArtMaterialCategory.StoneDark);
            FlatShared(imperial, "ImperialEdgeR", new Vector3(imperialW * 0.55f, 0.055f, -8f),
                new Vector3(0.28f, 0.08f, 22f), CityArtMaterialCategory.StoneDark);
            FlatShared(imperial, "ImperialGrassL", new Vector3(-imperialW * 0.78f, 0.03f, -8f),
                new Vector3(0.55f, 0.06f, 22f), CityArtMaterialCategory.Grass);
            FlatShared(imperial, "ImperialGrassR", new Vector3(imperialW * 0.78f, 0.03f, -8f),
                new Vector3(0.55f, 0.06f, 22f), CityArtMaterialCategory.Grass);
            FlatShared(imperial, "DrainL", new Vector3(-imperialW * 0.62f, 0.02f, -8f),
                new Vector3(0.18f, 0.05f, 22f), CityArtMaterialCategory.Dirt);
            FlatShared(imperial, "DrainR", new Vector3(imperialW * 0.62f, 0.02f, -8f),
                new Vector3(0.18f, 0.05f, 22f), CityArtMaterialCategory.Dirt);
            // Pedras irregulares / desgaste (marcas no eixo).
            FlatShared(imperial, "WearA", new Vector3(-0.55f, 0.07f, -14f), new Vector3(0.7f, 0.04f, 1.2f),
                CityArtMaterialCategory.StoneDark);
            FlatShared(imperial, "WearB", new Vector3(0.7f, 0.07f, -10f), new Vector3(0.9f, 0.04f, 0.9f),
                CityArtMaterialCategory.StoneDark);
            FlatShared(imperial, "WearC", new Vector3(-0.35f, 0.07f, -6.5f), new Vector3(0.8f, 0.04f, 1.1f),
                CityArtMaterialCategory.StoneLight);
            FlatShared(imperial, "WearD", new Vector3(0.4f, 0.07f, -3.5f), new Vector3(0.75f, 0.04f, 0.85f),
                CityArtMaterialCategory.StoneDark);
            if (CityAmbientQualitySettings.AllowFlags)
            {
                Flag(imperial, new Vector3(-2.4f, 0.2f, -8f));
                Flag(imperial, new Vector3(2.4f, 0.2f, -8f));
                Flag(imperial, new Vector3(-2.1f, 0.2f, 6f));
                Flag(imperial, new Vector3(2.1f, 0.2f, 6f));
            }

            Road(main, "MainToArena", new Vector3(5.5f, 0.03f, 0.4f), new Vector3(1.7f, 0.07f, 11f), 90f);
            Road(main, "MainToMarket", new Vector3(-4.2f, 0.03f, -3.2f), new Vector3(1.7f, 0.07f, 9f), -55f);
            Road(main, "MainToTemple", new Vector3(4.5f, 0.03f, 2.4f), new Vector3(1.55f, 0.07f, 9f), 40f);
            Road(main, "MainToHospital", new Vector3(-4.5f, 0.03f, 2.2f), new Vector3(1.55f, 0.07f, 9f), -40f);

            Road(secondary, "SecToFarm", new Vector3(-11f, 0.03f, -6.5f), new Vector3(1.35f, 0.07f, 8f), -25f);
            Road(secondary, "SecToMill", new Vector3(-10f, 0.03f, -11f), new Vector3(1.35f, 0.07f, 8f), -35f);
            Road(secondary, "SecToWarehouse", new Vector3(3f, 0.03f, -7f), new Vector3(1.35f, 0.07f, 6f), 25f);
            Road(secondary, "SecToTraining", new Vector3(10f, 0.03f, -3.5f), new Vector3(1.35f, 0.07f, 7f), 15f);
            Road(secondary, "SecToAcademy", new Vector3(10f, 0.03f, 5.5f), new Vector3(1.35f, 0.07f, 8f), 35f);
            Road(secondary, "SecToMine", new Vector3(-10f, 0.03f, 8f), new Vector3(1.35f, 0.07f, 9f), -30f);
            Road(secondary, "SecToQuarry", new Vector3(10f, 0.03f, 8.5f), new Vector3(1.35f, 0.07f, 9f), 30f);
            Road(secondary, "SecToHeroHall", new Vector3(-3.4f, 0.03f, 2.6f), new Vector3(1.3f, 0.07f, 6f), -20f);
            Road(secondary, "SecToFoundry", new Vector3(8.5f, 0.03f, -10f), new Vector3(1.3f, 0.07f, 7f), 40f);
            Road(secondary, "SecToResidence", new Vector3(-5.0f, 0.03f, -7.2f), new Vector3(1.25f, 0.07f, 5f), -10f);
        }

        /// <summary>Identidade de pavimento por distrito (Art Pass 2.0).</summary>
        private static void BuildDistrictRoads(Transform root)
        {
            var styles = Node(root, "DistrictRoads");
            // Militar — pedra desgastada.
            FlatShared(styles, "MilRoad", new Vector3(11.5f, 0.035f, -2f), new Vector3(2.2f, 0.08f, 10f),
                CityArtMaterialCategory.StoneDark);
            // Industrial — terra compactada.
            FlatShared(styles, "IndRoad", new Vector3(12f, 0.03f, -10f), new Vector3(2.0f, 0.08f, 8f),
                CityArtMaterialCategory.Dirt);
            FlatShared(styles, "IndRoadMine", new Vector3(-12f, 0.85f, 10f), new Vector3(2.0f, 0.08f, 7f),
                CityArtMaterialCategory.Dirt);
            // Rural — barro.
            FlatShared(styles, "RuralRoad", new Vector3(-12.5f, 0.03f, -8f), new Vector3(2.0f, 0.08f, 8f),
                CityArtMaterialCategory.Dirt);
            // Sagrado — pedra clara.
            FlatShared(styles, "SacredRoadE", new Vector3(7.5f, 0.04f, 3.5f), new Vector3(1.9f, 0.08f, 6f),
                CityArtMaterialCategory.StoneLight);
            FlatShared(styles, "SacredRoadW", new Vector3(-7.5f, 0.04f, 3.2f), new Vector3(1.9f, 0.08f, 6f),
                CityArtMaterialCategory.StoneLight);
            // Mercado — mistura pedra/terra.
            FlatShared(styles, "MarketBed", new Vector3(-7.5f, 0.028f, -6.5f), new Vector3(3.2f, 0.07f, 5.5f),
                CityArtMaterialCategory.Dirt);
            FlatShared(styles, "MarketStone", new Vector3(-7.5f, 0.04f, -6.5f), new Vector3(2.4f, 0.06f, 4.5f),
                CityArtMaterialCategory.StoneLight);
        }

        private static void Road(
            Transform root,
            string name,
            Vector3 pos,
            Vector3 scale,
            float yaw,
            bool imperialGrade = false)
        {
            var bedScale = new Vector3(scale.x + (imperialGrade ? 0.85f : 0.55f), scale.y * 0.7f, scale.z + 0.55f);
            var bed = Flat(root, name + "_Bed", pos, bedScale, CityVisualMaterials.Dirt, SurfaceKind.Dirt);
            bed.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            bed.transform.localPosition = pos + Vector3.down * 0.01f;

            var stoneColor = imperialGrade
                ? Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.35f)
                : CityVisualMaterials.Path;
            var stone = Flat(root, name, pos, scale, stoneColor, SurfaceKind.Path);
            stone.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            stone.transform.localPosition = pos;
        }

        private static void BuildWallRing(Transform root, int wallLevel)
        {
            var art3 = CityArtPass3Config.Enabled && CityArtPass3Config.ClosedWallEnabled;
            var wall = art3
                ? CityArtPass3Builder.FortStone
                : Color.Lerp(CityVisualMaterials.StoneLight, CityVisualMaterials.StoneDark, 0.35f);
            var cap = art3 ? CityArtPass3Builder.FortCap : CityVisualMaterials.StoneDark;
            var height = wallLevel <= 0 ? 3.4f : 3.6f + Math.Min(wallLevel, 6) * 0.34f;
            var thickness = wallLevel <= 0 ? 1.45f : 1.55f + Math.Min(wallLevel, 5) * 0.09f;
            if (art3)
            {
                height += 0.35f;
                thickness += 0.15f;
            }

            var radius = CityCapitalLayoutConfig.WallRadius;
            var segments = art3 ? CityArtPass3Config.WallSegments : 20;
            // Abertura sul reservada ao Portão HF (BuildingId=wall). Anel fecha o restante.
            for (var i = 0; i < segments; i++)
            {
                var angle = i * (360f / segments) * Mathf.Deg2Rad + Mathf.PI;
                // Sul = Cos≈-1 após +PI; abrir ~±18° para o WallGate.
                var southDot = Mathf.Cos(angle); // após +PI, sul tem cos negativo grande
                var isSouthOpening = southDot < -0.92f;
                if (isSouthOpening)
                {
                    continue;
                }

                var pos = new Vector3(Mathf.Sin(angle) * radius, height * 0.5f, Mathf.Cos(angle) * radius);
                var segW = art3 ? (2f * Mathf.PI * radius / segments) * 0.95f : 3.6f;
                var piece = FortPart(PrimitiveType.Cube, root, $"Wall_{i}", pos,
                    new Vector3(segW, height, thickness), wall, SurfaceKind.Stone);
                piece.transform.LookAt(new Vector3(0f, height * 0.5f, 0f));
                // Passarela superior.
                var walk = FortPart(PrimitiveType.Cube, root, $"WallWalk_{i}", pos + Vector3.up * (height * 0.42f),
                    new Vector3(segW * 0.92f, 0.16f, thickness + 0.35f), CityVisualMaterials.Path, SurfaceKind.Path);
                walk.transform.rotation = piece.transform.rotation;
                // Ameias (merlões).
                for (var m = -1; m <= 1; m++)
                {
                    var merlon = FortPart(PrimitiveType.Cube, root, $"WallMerlon_{i}_{m}",
                        pos + piece.transform.right * (m * (segW * 0.28f)) + Vector3.up * (height * 0.58f),
                        new Vector3(Mathf.Max(0.45f, segW * 0.18f), 0.55f, thickness + 0.2f), cap, SurfaceKind.Stone);
                    merlon.transform.rotation = piece.transform.rotation;
                }

                var capGo = FortPart(PrimitiveType.Cube, root, $"WallCap_{i}", pos + Vector3.up * (height * 0.52f),
                    new Vector3(segW, 0.22f, thickness + 0.12f), cap, SurfaceKind.Stone);
                capGo.transform.rotation = piece.transform.rotation;

                var towerEvery = art3 ? 5 : 4;
                if (i % towerEvery == 2)
                {
                    var towerH = height + 1.15f;
                    var tower = FortPart(art3 ? PrimitiveType.Cylinder : PrimitiveType.Cube, root, $"WallTower_{i}",
                        pos + Vector3.up * (height * 0.28f),
                        new Vector3(1.35f, towerH, 1.35f), cap, SurfaceKind.Stone);
                    tower.transform.rotation = piece.transform.rotation;
                    FortPart(PrimitiveType.Cube, root, $"WallTowerRoof_{i}",
                        pos + Vector3.up * (height * 0.28f + towerH * 0.55f),
                        new Vector3(1.55f, 0.4f, 1.55f),
                        art3 ? CityArtPass3Builder.FortCap : CityVisualMaterials.RoofBlue, SurfaceKind.Roof);
                    if (CityAmbientQualitySettings.AllowFlags)
                    {
                        Flag(root, pos + Vector3.up * (height * 0.28f + towerH * 0.75f) + piece.transform.forward * -0.2f);
                    }
                }
            }

            if (art3)
            {
                BuildCornerTowers(root, wallLevel, height, radius, cap);
            }
        }

        private static void BuildCornerTowers(Transform root, int wallLevel, float wallHeight, float radius, Color cap)
        {
            _ = wallLevel;
            float[] corners = { 45f, 135f, 225f, 315f };
            for (var i = 0; i < corners.Length && i < CityArtPass3Config.CornerTowerCount; i++)
            {
                var angle = corners[i] * Mathf.Deg2Rad + Mathf.PI;
                var pos = new Vector3(Mathf.Sin(angle) * radius, wallHeight * 0.55f, Mathf.Cos(angle) * radius);
                var h = wallHeight + 1.8f;
                FortPart(PrimitiveType.Cylinder, root, $"CornerTower_{i}", pos,
                    new Vector3(1.7f, h, 1.7f), cap, SurfaceKind.Stone);
                FortPart(PrimitiveType.Cylinder, root, $"CornerTowerCap_{i}", pos + Vector3.up * (h * 0.55f),
                    new Vector3(1.95f, 0.35f, 1.95f), CityArtPass3Builder.FortStone, SurfaceKind.Stone);
                // Merlões do canto.
                for (var m = 0; m < 4; m++)
                {
                    var ma = m * 90f * Mathf.Deg2Rad;
                    FortPart(PrimitiveType.Cube, root, $"CornerMerlon_{i}_{m}",
                        pos + new Vector3(Mathf.Sin(ma) * 0.75f, h * 0.58f, Mathf.Cos(ma) * 0.75f),
                        new Vector3(0.45f, 0.5f, 0.45f), cap, SurfaceKind.Stone);
                }

                if (CityAmbientQualitySettings.AllowFlags)
                {
                    Flag(root, pos + Vector3.up * (h * 0.7f));
                }
            }
        }

        private static void BuildMainGate(Transform root, int wallLevel)
        {
            var art3 = CityArtPass3Config.Enabled && CityArtPass3Config.ClosedWallEnabled;
            var stone = art3 ? CityArtPass3Builder.FortStone : CityVisualMaterials.StoneLight;
            var dark = art3 ? CityArtPass3Builder.FortCap : CityVisualMaterials.StoneDark;
            var z = CityCapitalLayoutConfig.MainGateZ;
            var towerH = wallLevel <= 0 ? 2.6f : 3.0f + Math.Min(wallLevel, 5) * 0.28f;

            // Art Pass 3: Portão HF (BuildingId=wall) é o volume principal —
            // ambiente só conecta asas + ponte (sem segundo portão cinza).
            if (!art3)
            {
                FortPart(PrimitiveType.Cube, root, "GateL", new Vector3(-2.35f, towerH * 0.5f, z),
                    new Vector3(1.65f, towerH, 1.7f), dark, SurfaceKind.Stone);
                FortPart(PrimitiveType.Cube, root, "GateR", new Vector3(2.35f, towerH * 0.5f, z),
                    new Vector3(1.65f, towerH, 1.7f), dark, SurfaceKind.Stone);
                FortPart(PrimitiveType.Cube, root, "GateArch", new Vector3(0f, towerH + 0.25f, z),
                    new Vector3(5.6f, 0.65f, 1.8f), stone, SurfaceKind.Stone);
                FortPart(PrimitiveType.Cube, root, "GateArchInner", new Vector3(0f, towerH - 0.15f, z),
                    new Vector3(3.4f, 0.45f, 1.5f), dark, SurfaceKind.Stone);
                FortPart(PrimitiveType.Cube, root, "GateGold", new Vector3(0f, towerH + 0.65f, z),
                    new Vector3(1.7f, 0.2f, 0.5f), CityVisualMaterials.Gold, SurfaceKind.Metal);

                var doorH = Math.Min(3.0f, towerH * 0.9f);
                FortPart(PrimitiveType.Cube, root, "GateDoorL", new Vector3(-0.65f, doorH * 0.5f, z - 0.12f),
                    new Vector3(1.2f, doorH, 0.28f), CityVisualMaterials.Wood, SurfaceKind.Wood);
                FortPart(PrimitiveType.Cube, root, "GateDoorR", new Vector3(0.65f, doorH * 0.5f, z - 0.12f),
                    new Vector3(1.2f, doorH, 0.28f), CityVisualMaterials.Wood, SurfaceKind.Wood);
                FortPart(PrimitiveType.Cube, root, "GateHinge", new Vector3(0f, doorH * 0.55f, z - 0.28f),
                    new Vector3(0.14f, doorH * 0.7f, 0.1f), CityVisualMaterials.Gold, SurfaceKind.Metal);
                FortPart(PrimitiveType.Cube, root, "GateBar", new Vector3(0f, doorH * 0.35f, z - 0.28f),
                    new Vector3(2.0f, 0.12f, 0.1f), CityVisualMaterials.Gold, SurfaceKind.Metal);
            }
            else
            {
                // Asas de conexão anel → WallGate HF.
                FortPart(PrimitiveType.Cube, root, "GateWingL", new Vector3(-4.6f, towerH * 0.55f, z + 0.4f),
                    new Vector3(2.4f, towerH * 1.05f, 1.5f), dark, SurfaceKind.Stone);
                FortPart(PrimitiveType.Cube, root, "GateWingR", new Vector3(4.6f, towerH * 0.55f, z + 0.4f),
                    new Vector3(2.4f, towerH * 1.05f, 1.5f), dark, SurfaceKind.Stone);
                FortPart(PrimitiveType.Cube, root, "GateWingWalkL", new Vector3(-4.6f, towerH * 1.05f, z + 0.4f),
                    new Vector3(2.3f, 0.16f, 1.7f), CityVisualMaterials.Path, SurfaceKind.Path);
                FortPart(PrimitiveType.Cube, root, "GateWingWalkR", new Vector3(4.6f, towerH * 1.05f, z + 0.4f),
                    new Vector3(2.3f, 0.16f, 1.7f), CityVisualMaterials.Path, SurfaceKind.Path);
            }

            // Ponte de acesso.
            Flat(root, "GateBridge", new Vector3(0f, 0.1f, z - 2.2f), new Vector3(3.4f, 0.14f, 3.2f),
                CityVisualMaterials.Path, SurfaceKind.Path);
            FortPart(PrimitiveType.Cube, root, "BridgeRailL", new Vector3(-1.7f, 0.45f, z - 2.2f),
                new Vector3(0.18f, 0.65f, 3.0f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            FortPart(PrimitiveType.Cube, root, "BridgeRailR", new Vector3(1.7f, 0.45f, z - 2.2f),
                new Vector3(0.18f, 0.65f, 3.0f), CityVisualMaterials.Wood, SurfaceKind.Wood);

            if (CityAmbientQualitySettings.AllowTorches)
            {
                Torch(root, new Vector3(-4.2f, towerH * 0.85f, z + 0.85f));
                Torch(root, new Vector3(4.2f, towerH * 0.85f, z + 0.85f));
                Torch(root, new Vector3(-1.4f, 0.9f, z - 3.2f));
                Torch(root, new Vector3(1.4f, 0.9f, z - 3.2f));
            }

            if (CityAmbientQualitySettings.AllowFlags)
            {
                Flag(root, new Vector3(-5.2f, towerH * 0.75f, z + 0.45f));
                Flag(root, new Vector3(5.2f, towerH * 0.75f, z + 0.45f));
            }

            // Portões laterais nas pontes E/W/N.
            SideGate(root, new Vector3(CityCapitalLayoutConfig.WallRadius, 0f, 0f), true, wallLevel);
            SideGate(root, new Vector3(-CityCapitalLayoutConfig.WallRadius, 0f, 0f), true, wallLevel);
            SideGate(root, new Vector3(0f, 0f, CityCapitalLayoutConfig.WallRadius), false, wallLevel);
        }

        private static void SideGate(Transform root, Vector3 g, bool alongX, int wallLevel)
        {
            var stone = CityVisualMaterials.StoneDark;
            var h = wallLevel <= 0 ? 1.6f : 1.9f + Math.Min(wallLevel, 4) * 0.15f;
            var left = alongX ? new Vector3(g.x, h * 0.5f, g.z - 1.35f) : new Vector3(g.x - 1.35f, h * 0.5f, g.z);
            var right = alongX ? new Vector3(g.x, h * 0.5f, g.z + 1.35f) : new Vector3(g.x + 1.35f, h * 0.5f, g.z);
            FortPart(PrimitiveType.Cube, root, "SideGateL", left, new Vector3(1.0f, h, 1.0f), stone, SurfaceKind.Stone);
            FortPart(PrimitiveType.Cube, root, "SideGateR", right, new Vector3(1.0f, h, 1.0f), stone, SurfaceKind.Stone);
            var arch = new Vector3(g.x, h + 0.15f, g.z);
            FortPart(PrimitiveType.Cube, root, "SideGateArch", arch,
                alongX ? new Vector3(1.0f, 0.35f, 2.9f) : new Vector3(2.9f, 0.35f, 1.0f), stone, SurfaceKind.Stone);
        }

        private static void Torch(Transform root, Vector3 pos)
        {
            Part(PrimitiveType.Cylinder, root, "TorchPole", pos, new Vector3(0.1f, 0.35f, 0.1f),
                CityVisualMaterials.Wood, SurfaceKind.Wood);
            var flame = Part(PrimitiveType.Sphere, root, "TorchFlame", pos + Vector3.up * 0.45f,
                Vector3.one * 0.28f, new Color(1f, 0.55f, 0.2f), SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(flame.GetComponent<Renderer>(), new Color(1f, 0.45f, 0.1f), 0.55f);
        }

        private static void Flag(Transform root, Vector3 pos)
        {
            Part(PrimitiveType.Cylinder, root, "FlagPole", pos, new Vector3(0.08f, 0.9f, 0.08f),
                CityVisualMaterials.Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, "FlagCloth", pos + new Vector3(0.45f, 0.55f, 0f),
                new Vector3(0.9f, 0.55f, 0.05f), CityVisualMaterials.RoofRed, SurfaceKind.Plain);
        }

        private static void BuildTrees(Transform root)
        {
            var trunk = Color.Lerp(CityVisualMaterials.Wood, CityVisualMaterials.StoneDark, 0.15f);
            var leaf = CityVisualMaterials.Vegetation;
            var bush = Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.2f);
            Vector3[] spots =
            {
                new(-11.5f, 0f, -4.5f), new(-16f, 0f, -2f), new(-17f, 0f, 4f),
                new(11.5f, 0f, -4f), new(16.5f, 0f, 2f), new(17f, 0f, -6f),
                new(-6f, 0f, 11f), new(6.5f, 0f, 11.5f),
                new(-12f, 0f, -15f), new(4f, 0f, -14f),
                new(-18f, 0f, -10f), new(18f, 0f, 8f),
                new(-8.5f, 0f, 8.5f), new(8.2f, 0f, 8.8f),
                new(-15.5f, 0f, -5.5f), new(15.8f, 0f, -11f),
                new(-3.5f, 0f, 12.5f), new(3.2f, 0f, -15.5f),
                new(-19f, 0f, 2f), new(19f, 0f, -2f),
                new(-10f, 0f, 14f), new(10.5f, 0f, 14.5f)
            };

            var count = CityCapitalEnvironmentConfig.VegetationCount(CityAmbientQualitySettings.Current, 1);
            count = Math.Min(count, spots.Length);
            for (var i = 0; i < count; i++)
            {
                var spot = spots[i];
                // Evitar eixo monumental e praça.
                if (Mathf.Abs(spot.x) < 2.8f && spot.z > -16f && spot.z < 16f)
                {
                    continue;
                }

                var h = 0.45f + (i % 4) * 0.12f;
                var trunkScale = 0.22f + (i % 3) * 0.04f;
                Part(PrimitiveType.Cylinder, root, $"Trunk_{i}", spot + Vector3.up * h,
                    new Vector3(trunkScale, h, trunkScale), trunk, SurfaceKind.Wood);
                if (i % 5 == 0)
                {
                    Part(PrimitiveType.Sphere, root, $"Bush_{i}", spot + Vector3.up * 0.35f,
                        new Vector3(0.9f, 0.55f, 0.9f), bush, SurfaceKind.Vegetation);
                }
                else
                {
                    Part(PrimitiveType.Sphere, root, $"Canopy_{i}", spot + Vector3.up * (h * 2.1f),
                        new Vector3(1.05f + (i % 3) * 0.18f, 0.95f, 1.05f), leaf, SurfaceKind.Vegetation);
                }
            }

            if (CityCapitalEnvironmentConfig.AllowHighExclusiveProps)
            {
                for (var i = 0; i < 6; i++)
                {
                    var a = i * 55f * Mathf.Deg2Rad;
                    var p = new Vector3(Mathf.Sin(a) * 19f, 0.15f, Mathf.Cos(a) * 19f);
                    Part(PrimitiveType.Sphere, root, $"GrassClump_{i}", p,
                        new Vector3(0.6f, 0.25f, 0.6f), bush, SurfaceKind.Vegetation);
                }
            }
        }

        private static void BuildTierDecoration(Transform root)
        {
            for (var t = 1; t <= 6; t++)
            {
                var tier = Node(root, $"Tier{t}");
                var density = CityCapitalEnvironmentConfig.TierVisualDensity(t);

                // Tier I: vila — quase sem ornamentação.
                if (t == 1)
                {
                    Flat(tier, "DirtTrim", new Vector3(0f, 0.05f, -3.2f), new Vector3(8f, 0.03f, 8f),
                        CityVisualMaterials.Dirt, SurfaceKind.Dirt);
                }

                if (t >= 2)
                {
                    Flat(tier, "ExtraPathTrim", new Vector3(0f, 0.06f, -3.2f),
                        new Vector3(10f * density, 0.04f, 10f * density),
                        Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.08f * t), SurfaceKind.Path);
                    Part(PrimitiveType.Sphere, tier, "YoungTree", new Vector3(-5.5f, 0.9f, -6f),
                        new Vector3(0.9f, 0.9f, 0.9f), CityVisualMaterials.Vegetation, SurfaceKind.Vegetation);
                }

                if (t >= 3)
                {
                    Part(PrimitiveType.Cylinder, tier, "GardenRing", new Vector3(0f, 0.2f, -3.2f),
                        new Vector3(4.8f, 0.08f, 4.8f),
                        Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Gold, 0.08f), SurfaceKind.Vegetation);
                    Flat(tier, "MarketExpansion", new Vector3(-8f, 0.04f, -7f), new Vector3(3.5f, 0.04f, 2.5f),
                        Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Path, 0.3f), SurfaceKind.Dirt);
                }

                if (t >= 4)
                {
                    Flag(tier, new Vector3(-2.2f, 0.2f, -1.2f));
                    Flag(tier, new Vector3(2.2f, 0.2f, -1.2f));
                    Flag(tier, new Vector3(0f, 0.2f, 4.5f));
                    Torch(tier, new Vector3(-5f, 0.8f, 1f));
                    Torch(tier, new Vector3(5f, 0.8f, 1f));
                }

                if (t >= 5)
                {
                    Part(PrimitiveType.Cube, tier, "Monument", new Vector3(0f, 1.1f, 2.8f),
                        new Vector3(0.7f, 2.0f, 0.7f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);
                    Part(PrimitiveType.Cylinder, tier, "CapitalFountain", new Vector3(-6.5f, 0.5f, -1f),
                        new Vector3(1.2f, 0.4f, 1.2f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);
                    Flat(tier, "NoblePave", new Vector3(0f, 0.07f, 0.5f), new Vector3(6f, 0.04f, 6f),
                        Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.Gold, 0.1f), SurfaceKind.Path);
                }

                if (t >= 6)
                {
                    Part(PrimitiveType.Cube, tier, "ImperialStatue", new Vector3(-4.5f, 1.4f, -3.2f),
                        new Vector3(1.1f, 2.6f, 1.1f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
                    Part(PrimitiveType.Cube, tier, "ImperialStatueR", new Vector3(4.5f, 1.4f, -3.2f),
                        new Vector3(1.1f, 2.6f, 1.1f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
                    Flag(tier, new Vector3(-3.5f, 0.2f, -4.5f));
                    Flag(tier, new Vector3(3.5f, 0.2f, -4.5f));
                    Part(PrimitiveType.Cube, tier, "ImperialBannerArch", new Vector3(0f, 2.2f, -3.2f),
                        new Vector3(5.5f, 0.25f, 0.35f), CityVisualMaterials.Gold, SurfaceKind.Metal);
                }

                tier.gameObject.SetActive(t == 1);
            }
        }

        private static void BuildAmbientProps(Transform decorations, Transform ambient)
        {
            // Casas decorativas (≠ Residência oficial) — afastadas do slot residence.
            Part(PrimitiveType.Cube, decorations, "CottageA", new Vector3(-10.5f, 0.55f, -10.8f),
                new Vector3(1.6f, 1.0f, 1.4f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, decorations, "CottageARoof", new Vector3(-10.5f, 1.2f, -10.8f),
                new Vector3(1.8f, 0.35f, 1.6f), CityVisualMaterials.RoofRed, SurfaceKind.Roof);
            Part(PrimitiveType.Cube, decorations, "CottageB", new Vector3(2.2f, 0.5f, -12.5f),
                new Vector3(1.4f, 0.9f, 1.2f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, decorations, "CottageBRoof", new Vector3(2.2f, 1.1f, -12.5f),
                new Vector3(1.6f, 0.3f, 1.4f), CityVisualMaterials.RoofBlue, SurfaceKind.Roof);

            if (CityAmbientQualitySettings.AllowSmoke)
            {
                var s = CityCapitalEnvironmentConfig.SmokeScale(CityAmbientQualitySettings.Current);
                Part(PrimitiveType.Sphere, ambient, "SmokeHint", new Vector3(-13.2f, 2.2f, -13f),
                    new Vector3(1.4f, 1.8f, 1.4f) * s, new Color(0.55f, 0.55f, 0.55f, 0.35f), SurfaceKind.Plain);
            }

            if (CityAmbientQualitySettings.AllowBirds)
            {
                Part(PrimitiveType.Sphere, ambient, "BirdHintA", new Vector3(-8f, 6f, 4f),
                    Vector3.one * 0.25f, new Color(0.2f, 0.2f, 0.22f), SurfaceKind.Plain);
                Part(PrimitiveType.Sphere, ambient, "BirdHintB", new Vector3(6f, 7f, -2f),
                    Vector3.one * 0.22f, new Color(0.22f, 0.2f, 0.2f), SurfaceKind.Plain);
            }

            if (CityCapitalEnvironmentConfig.AllowExtraParticles)
            {
                Part(PrimitiveType.Sphere, ambient, "DustHint", new Vector3(0f, 0.4f, -10f),
                    new Vector3(2.5f, 0.3f, 1.2f), new Color(0.55f, 0.5f, 0.4f, 0.2f), SurfaceKind.Plain);
            }
        }

        private static void SoftenLighting() => ApplyDayLighting();

        public static void ApplyDayLighting()
        {
            var light = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                // Manhã/tarde cinematográfica — leitura preservada em Medium.
                light.color = CityCapitalFinalConfig.CinematicLightingEnabled
                    ? new Color(1f, 0.86f, 0.62f)
                    : new Color(1f, 0.92f, 0.78f);
                light.intensity = CityCapitalFinalConfig.CinematicLightingEnabled ? 1.55f : 1.42f;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.68f;
                light.transform.rotation = Quaternion.Euler(42f, -48f, 0f);
            }

            // Contraste legível em 1080×640 — ambient um pouco mais frio no céu.
            var haze = new Color(0.5f, 0.54f, 0.48f);
            ApplySky(haze,
                new Color(0.52f, 0.6f, 0.72f),
                new Color(0.5f, 0.45f, 0.36f),
                new Color(0.22f, 0.2f, 0.18f),
                haze,
                CityAmbientQualitySettings.AllowLightFog ? 0.012f : 0.009f);
        }

        public static void ApplyNightLighting()
        {
            var light = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                light.color = new Color(0.5f, 0.58f, 0.85f);
                light.intensity = 0.5f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(30f, -48f, 0f);
            }

            var nightHaze = new Color(0.12f, 0.14f, 0.18f);
            ApplySky(nightHaze,
                new Color(0.16f, 0.2f, 0.32f),
                new Color(0.24f, 0.2f, 0.16f),
                new Color(0.08f, 0.08f, 0.1f),
                nightHaze,
                0.028f);
        }

        private static void ApplySky(
            Color background,
            Color sky,
            Color equator,
            Color ground,
            Color fog,
            float fogDensity)
        {
            var camera = UnityEngine.Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = background;
                camera.farClipPlane = 160f;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = sky;
            RenderSettings.ambientEquatorColor = equator;
            RenderSettings.ambientGroundColor = ground;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fog;
            RenderSettings.fogDensity = fogDensity;
        }

        private static void HideSceneGround()
        {
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                ground.SetActive(false);
            }
        }

        private static GameObject Flat(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Color color,
            SurfaceKind surface = SurfaceKind.Dirt) =>
            Part(PrimitiveType.Cube, parent, name, position, scale, color, surface);

        private static GameObject FlatShared(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            CityArtMaterialCategory category) =>
            PartShared(PrimitiveType.Cube, parent, name, position, scale, category);

        private static GameObject PartShared(
            PrimitiveType type,
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            CityArtMaterialCategory category,
            bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            if (!keepCollider)
            {
                UnityEngine.Object.Destroy(go.GetComponent<Collider>());
                var ignore = LayerMask.NameToLayer("Ignore Raycast");
                if (ignore >= 0)
                {
                    go.layer = ignore;
                }
            }

            CityArtMaterialLibrary.Apply(go.GetComponent<Renderer>(), category);
            return go;
        }

        private static GameObject Part(
            PrimitiveType type,
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Color color,
            SurfaceKind surface = SurfaceKind.Stone,
            bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            if (!keepCollider)
            {
                UnityEngine.Object.Destroy(go.GetComponent<Collider>());
                var ignore = LayerMask.NameToLayer("Ignore Raycast");
                if (ignore >= 0)
                {
                    go.layer = ignore;
                }
            }

            CityVisualMaterials.ApplySurface(go.GetComponent<Renderer>(), color, surface);
            return go;
        }

        private static GameObject FortPart(
            PrimitiveType type,
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Color color,
            SurfaceKind surface = SurfaceKind.Stone) =>
            Part(type, parent, name, position, scale, color, surface, keepCollider: true);

        private static void TryStaticBatch(Transform cityRoot)
        {
            if (cityRoot == null)
            {
                return;
            }

            // Batch estático das peças ambientais (Terrain/Roads/Decor) — Medium estável.
            var batchRoots = new[]
            {
                cityRoot.Find("Terrain"),
                cityRoot.Find("Roads"),
                cityRoot.Find("Decorations"),
                cityRoot.Find("Vegetation"),
                cityRoot.Find("Districts")
            };
            for (var i = 0; i < batchRoots.Length; i++)
            {
                var node = batchRoots[i];
                if (node == null || node.childCount == 0)
                {
                    continue;
                }

                try
                {
                    StaticBatchingUtility.Combine(node.gameObject);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Valgor.City] StaticBatch skip {node.name}: {ex.Message}");
                }
            }
        }

        private static void TryAttachLodHints(Transform cityRoot)
        {
            if (cityRoot == null || CityAmbientQualitySettings.Current == CityAmbientQuality.Low)
            {
                return;
            }

            // LOD simples: horizonte / relevo longe — desliga sob Low já; sob Medium/High usa cull distance via Camera.
            var cam = UnityEngine.Camera.main;
            if (cam == null)
            {
                return;
            }

            cam.layerCullSpherical = true;
            // Frustum culling nativo do Unity permanece ativo; occlusion só se volume existir na cena.
        }
    }
}
