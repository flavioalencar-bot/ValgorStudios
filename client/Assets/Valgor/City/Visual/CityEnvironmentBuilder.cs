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
            BuildRivers(water);
            BuildDistrictPads(districts);
            BuildPlaza(roads);
            BuildRoads(roads);
            BuildPlateaus(terrain);
            BuildTrees(vegetation);
            BuildTierDecoration(tierGroups);
            BuildAmbientProps(decorations, ambient);
            SoftenLighting();
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
        }

        private static void BuildHorizon(Transform root)
        {
            var hill = Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.StoneDark, 0.35f);
            var mist = Color.Lerp(CityVisualMaterials.Vegetation, new Color(0.45f, 0.5f, 0.48f), 0.4f);
            const float radius = 42f;
            const int hills = 12;
            for (var i = 0; i < hills; i++)
            {
                var angle = i * (360f / hills) * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Sin(angle) * radius, 1.2f, Mathf.Cos(angle) * radius);
                var w = 8f + (i % 3) * 1.3f;
                Part(PrimitiveType.Sphere, root, $"Hill_{i}", pos, new Vector3(w, 3.4f + i % 2, w * 0.85f), hill,
                    SurfaceKind.Vegetation);
            }

            Flat(root, "MistRing", Vector3.zero, new Vector3(96f, 0.04f, 96f), mist, SurfaceKind.Vegetation)
                .transform.localPosition = new Vector3(0f, -0.08f, 0f);
        }

        private static void BuildRivers(Transform root)
        {
            var water = new Color(0.22f, 0.38f, 0.48f);
            var bank = Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.35f);

            // Rios laterais — fora do anel de muralha, sem atravessar o eixo.
            Flat(root, "RiverWest", new Vector3(-24.5f, -0.12f, 0f), new Vector3(4.2f, 0.2f, 48f), water, SurfaceKind.Plain);
            Flat(root, "RiverEast", new Vector3(24.5f, -0.12f, 0f), new Vector3(4.2f, 0.2f, 48f), water, SurfaceKind.Plain);
            Flat(root, "BankWest", new Vector3(-22.2f, -0.02f, 0f), new Vector3(1.2f, 0.12f, 46f), bank, SurfaceKind.Dirt);
            Flat(root, "BankEast", new Vector3(22.2f, -0.02f, 0f), new Vector3(1.2f, 0.12f, 46f), bank, SurfaceKind.Dirt);

            // Pontes principais (E/W) sobre o fosso visual.
            Flat(root, "BridgeWest", new Vector3(-20.8f, 0.08f, 0f), new Vector3(3.2f, 0.18f, 2.4f),
                CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            Flat(root, "BridgeEast", new Vector3(20.8f, 0.08f, 0f), new Vector3(3.2f, 0.18f, 2.4f),
                CityVisualMaterials.StoneLight, SurfaceKind.Stone);

            if (CityAmbientQualitySettings.AllowWaterFx)
            {
                Part(PrimitiveType.Cube, root, "FallWest", new Vector3(-24.5f, 0.35f, 10f),
                    new Vector3(1.2f, 1.1f, 0.35f), Color.Lerp(water, Color.white, 0.25f), SurfaceKind.Plain);
                Part(PrimitiveType.Cube, root, "FallEast", new Vector3(24.5f, 0.35f, -8f),
                    new Vector3(1.2f, 1.1f, 0.35f), Color.Lerp(water, Color.white, 0.25f), SurfaceKind.Plain);
            }

            // Rochas de margem (sem collider).
            Part(PrimitiveType.Sphere, root, "RockW1", new Vector3(-23.2f, 0.2f, 6f), new Vector3(1.4f, 0.7f, 1.1f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Sphere, root, "RockE1", new Vector3(23.4f, 0.2f, -5f), new Vector3(1.5f, 0.75f, 1.2f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
        }

        private static void BuildDistrictPads(Transform root)
        {
            Flat(root, "PadRoyal", new Vector3(0f, 0.004f, 1.5f), new Vector3(14f, 0.04f, 12f),
                Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.2f), SurfaceKind.Path);
            Flat(root, "PadDragon", new Vector3(0f, 1.2f, 17.2f), new Vector3(10f, 0.25f, 8f),
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.RoofBlue, 0.2f), SurfaceKind.Stone);
            Flat(root, "PadSacredNW", new Vector3(-9f, 0.005f, 4.5f), new Vector3(7f, 0.04f, 7f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Path, 0.35f), SurfaceKind.Dirt);
            Flat(root, "PadSacredNE", new Vector3(9f, 0.005f, 4.8f), new Vector3(7f, 0.04f, 7f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Path, 0.35f), SurfaceKind.Dirt);
            Flat(root, "PadMilitary", new Vector3(12f, 0.005f, -1f), new Vector3(9f, 0.04f, 14f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.18f), SurfaceKind.Dirt);
            Flat(root, "PadEconomic", new Vector3(-4f, 0.005f, -7.5f), new Vector3(16f, 0.04f, 7f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Path, 0.25f), SurfaceKind.Dirt);
            Flat(root, "PadIndustrialSW", new Vector3(-13.5f, 0.005f, -13f), new Vector3(8f, 0.04f, 6f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Wood, 0.15f), SurfaceKind.Dirt);
            Flat(root, "PadFoundry", new Vector3(12.8f, 0.005f, -13f), new Vector3(6f, 0.04f, 5f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.25f), SurfaceKind.Dirt);
            Flat(root, "PadResidence", new Vector3(-5.8f, 0.005f, -8.8f), new Vector3(5f, 0.04f, 4.5f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Path, 0.15f), SurfaceKind.Dirt);
            Flat(root, "PadRural", new Vector3(-14f, 0.005f, -7.2f), new Vector3(7f, 0.04f, 6f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.2f), SurfaceKind.Vegetation);
            Flat(root, "PadMine", new Vector3(-15f, 0.7f, 11.5f), new Vector3(6f, 0.2f, 5f),
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.3f), SurfaceKind.Stone);
            Flat(root, "PadQuarry", new Vector3(15f, 0.7f, 12f), new Vector3(6f, 0.2f, 5f),
                Color.Lerp(CityVisualMaterials.StoneLight, CityVisualMaterials.Dirt, 0.25f), SurfaceKind.Stone);
            Flat(root, "PadAcademy", new Vector3(14f, 0.8f, 9.4f), new Vector3(6f, 0.2f, 5f),
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.RoofBlue, 0.12f), SurfaceKind.Stone);
        }

        private static void BuildPlateaus(Transform root)
        {
            // Bases elevadas sob Torre / Academia / Mina / Pedreira.
            Flat(root, "PlateauDragonRim", new Vector3(0f, 0.9f, 17.5f), new Vector3(11.5f, 0.9f, 9.5f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Flat(root, "PlateauAcademy", new Vector3(14.2f, 0.55f, 9.5f), new Vector3(5.5f, 0.55f, 4.5f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Flat(root, "PlateauMine", new Vector3(-15f, 0.5f, 11.8f), new Vector3(5f, 0.5f, 4.2f),
                CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Flat(root, "PlateauQuarry", new Vector3(15.2f, 0.5f, 12.2f), new Vector3(5f, 0.5f, 4.2f),
                CityVisualMaterials.StoneLight, SurfaceKind.Stone);
        }

        private static void BuildPlaza(Transform root)
        {
            // Praça circular aproximada (anel + núcleo) ao sul do Castelo / diante da fachada.
            Flat(root, "PlazaRim", new Vector3(0f, 0.02f, -3.2f), new Vector3(12f, 0.08f, 12f),
                Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneDark, 0.25f), SurfaceKind.Path);
            Flat(root, "Plaza", new Vector3(0f, 0.035f, -3.2f), new Vector3(9.2f, 0.1f, 9.2f),
                CityVisualMaterials.Path, SurfaceKind.Path);
            Flat(root, "PlazaCore", new Vector3(0f, 0.05f, -3.2f), new Vector3(3.2f, 0.12f, 3.2f),
                Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.35f), SurfaceKind.Stone);

            // Fonte central (sem collider).
            Part(PrimitiveType.Cylinder, root, "FountainBase", new Vector3(0f, 0.35f, -3.2f),
                new Vector3(1.6f, 0.35f, 1.6f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, "FountainWater", new Vector3(0f, 0.55f, -3.2f),
                new Vector3(1.1f, 0.12f, 1.1f), new Color(0.3f, 0.45f, 0.55f), SurfaceKind.Plain);

            // Quatro acessos / brasão simples.
            Flat(root, "Crest", new Vector3(0f, 0.08f, -3.2f), new Vector3(1.1f, 0.05f, 1.1f),
                CityVisualMaterials.Gold, SurfaceKind.Metal);
        }

        private static void BuildRoads(Transform root)
        {
            var imperial = Node(root, "ImperialRoad");
            var main = Node(root, "MainRoads");
            var secondary = Node(root, "SecondaryRoads");

            // Portão (S) → Praça → Castelo
            Road(imperial, "ImperialSouth", new Vector3(0f, 0.04f, -11.5f), new Vector3(3.1f, 0.09f, 17f), 0f, imperialGrade: true);
            // Castelo → Torre (N)
            Road(imperial, "ImperialNorth", new Vector3(0f, 0.04f, 9f), new Vector3(2.5f, 0.09f, 16f), 0f, imperialGrade: true);

            Road(main, "MainToArena", new Vector3(5.5f, 0.03f, 0.4f), new Vector3(1.7f, 0.07f, 11f), 90f);
            Road(main, "MainToMarket", new Vector3(-4.2f, 0.03f, -3.2f), new Vector3(1.7f, 0.07f, 9f), -55f);
            Road(main, "MainToTemple", new Vector3(4.5f, 0.03f, 2.4f), new Vector3(1.55f, 0.07f, 9f), 40f);
            Road(main, "MainToHospital", new Vector3(-4.5f, 0.03f, 2.2f), new Vector3(1.55f, 0.07f, 9f), -40f);

            Road(secondary, "SecToFarm", new Vector3(-11f, 0.03f, -6.5f), new Vector3(1.35f, 0.07f, 8f), -25f);
            Road(secondary, "SecToMill", new Vector3(-10f, 0.03f, -11f), new Vector3(1.35f, 0.07f, 8f), -35f);
            Road(secondary, "SecToWarehouse", new Vector3(3f, 0.03f, -7f), new Vector3(1.35f, 0.07f, 6f), 25f);
            Road(secondary, "SecToLab", new Vector3(10f, 0.03f, -3.5f), new Vector3(1.35f, 0.07f, 7f), 15f);
            Road(secondary, "SecToAcademy", new Vector3(10f, 0.03f, 5.5f), new Vector3(1.35f, 0.07f, 8f), 35f);
            Road(secondary, "SecToMine", new Vector3(-10f, 0.03f, 8f), new Vector3(1.35f, 0.07f, 9f), -30f);
            Road(secondary, "SecToQuarry", new Vector3(10f, 0.03f, 8.5f), new Vector3(1.35f, 0.07f, 9f), 30f);
            Road(secondary, "SecToInstitute", new Vector3(-3.4f, 0.03f, 2.6f), new Vector3(1.3f, 0.07f, 6f), -20f);
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
            var wall = CityVisualMaterials.StoneLight;
            var cap = CityVisualMaterials.StoneDark;
            var height = wallLevel <= 0 ? 1.0f : 1.55f + Math.Min(wallLevel, 6) * 0.28f;
            var thickness = wallLevel <= 0 ? 0.45f : 0.55f + Math.Min(wallLevel, 5) * 0.06f;
            var radius = CityCapitalLayoutConfig.WallRadius;
            const int segments = 20;
            // Segmento do sul (i≈0) fica aberto para o Portão Principal.
            for (var i = 0; i < segments; i++)
            {
                if (i is 0 or 1 or segments - 1)
                {
                    continue;
                }

                var angle = i * (360f / segments) * Mathf.Deg2Rad;
                // 0° = +Z no layout antigo; queremos Portão no -Z → rotacionar 180°.
                angle += Mathf.PI;
                var pos = new Vector3(Mathf.Sin(angle) * radius, height * 0.5f, Mathf.Cos(angle) * radius);
                var piece = FortPart(PrimitiveType.Cube, root, $"Wall_{i}", pos,
                    new Vector3(3.6f, height, thickness), wall, SurfaceKind.Stone);
                piece.transform.LookAt(new Vector3(0f, height * 0.5f, 0f));
                var capGo = FortPart(PrimitiveType.Cube, root, $"WallCap_{i}", pos + Vector3.up * (height * 0.52f),
                    new Vector3(3.6f, 0.28f, thickness + 0.12f), cap, SurfaceKind.Stone);
                capGo.transform.rotation = piece.transform.rotation;

                if (wallLevel >= 3 && i % 4 == 2)
                {
                    var towerH = height + 0.85f;
                    var tower = FortPart(PrimitiveType.Cube, root, $"WallTower_{i}",
                        pos + Vector3.up * (height * 0.25f),
                        new Vector3(1.15f, towerH, 1.15f), cap, SurfaceKind.Stone);
                    tower.transform.rotation = piece.transform.rotation;
                    FortPart(PrimitiveType.Cube, root, $"WallTowerRoof_{i}",
                        pos + Vector3.up * (height * 0.25f + towerH * 0.55f),
                        new Vector3(1.3f, 0.35f, 1.3f), CityVisualMaterials.RoofBlue, SurfaceKind.Roof);
                }
            }
        }

        private static void BuildMainGate(Transform root, int wallLevel)
        {
            var stone = CityVisualMaterials.StoneLight;
            var dark = CityVisualMaterials.StoneDark;
            var z = CityCapitalLayoutConfig.MainGateZ;
            var towerH = wallLevel <= 0 ? 2.2f : 2.6f + Math.Min(wallLevel, 5) * 0.25f;
            FortPart(PrimitiveType.Cube, root, "GateL", new Vector3(-2.2f, towerH * 0.5f, z),
                new Vector3(1.5f, towerH, 1.5f), dark, SurfaceKind.Stone);
            FortPart(PrimitiveType.Cube, root, "GateR", new Vector3(2.2f, towerH * 0.5f, z),
                new Vector3(1.5f, towerH, 1.5f), dark, SurfaceKind.Stone);
            FortPart(PrimitiveType.Cube, root, "GateArch", new Vector3(0f, towerH + 0.2f, z),
                new Vector3(5.2f, 0.55f, 1.6f), stone, SurfaceKind.Stone);
            if (wallLevel >= 2)
            {
                FortPart(PrimitiveType.Cube, root, "GateGold", new Vector3(0f, towerH + 0.55f, z),
                    new Vector3(1.5f, 0.18f, 0.45f), CityVisualMaterials.Gold, SurfaceKind.Metal);
            }

            FortPart(PrimitiveType.Cube, root, "GateDoor", new Vector3(0f, 1.35f, z - 0.15f),
                new Vector3(2.5f, Math.Min(2.8f, towerH * 0.85f), 0.25f),
                CityVisualMaterials.Wood, SurfaceKind.Wood);

            if (wallLevel >= 1 && CityAmbientQualitySettings.AllowTorches)
            {
                Torch(root, new Vector3(-2.2f, towerH * 0.85f, z + 0.75f));
                Torch(root, new Vector3(2.2f, towerH * 0.85f, z + 0.75f));
            }

            if (CityAmbientQualitySettings.AllowFlags)
            {
                Flag(root, new Vector3(-3.4f, towerH * 0.7f, z + 0.4f));
                Flag(root, new Vector3(3.4f, towerH * 0.7f, z + 0.4f));
            }

            // Portões laterais nas pontes E/W.
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
            Vector3[] spots =
            {
                new(-11.5f, 0f, -4.5f), new(-16f, 0f, -2f), new(-17f, 0f, 4f),
                new(11.5f, 0f, -4f), new(16.5f, 0f, 2f), new(17f, 0f, -6f),
                new(-6f, 0f, 11f), new(6.5f, 0f, 11.5f),
                new(-12f, 0f, -15f), new(4f, 0f, -14f),
                new(-18f, 0f, -10f), new(18f, 0f, 8f)
            };

            for (var i = 0; i < spots.Length; i++)
            {
                var spot = spots[i];
                var h = 0.55f + (i % 3) * 0.12f;
                Part(PrimitiveType.Cylinder, root, $"Trunk_{i}", spot + Vector3.up * h,
                    new Vector3(0.26f + (i % 2) * 0.04f, h, 0.26f), trunk, SurfaceKind.Wood);
                Part(PrimitiveType.Sphere, root, $"Canopy_{i}", spot + Vector3.up * (h * 2.1f),
                    new Vector3(1.15f + (i % 3) * 0.15f, 1.0f, 1.15f), leaf, SurfaceKind.Vegetation);
            }
        }

        private static void BuildTierDecoration(Transform root)
        {
            for (var t = 1; t <= 6; t++)
            {
                var tier = Node(root, $"Tier{t}");
                // Densidade progressiva — props leves sem gameplay.
                if (t >= 2)
                {
                    Flat(tier, "ExtraPathTrim", new Vector3(0f, 0.06f, -3.2f), new Vector3(10f, 0.04f, 10f),
                        Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.1f * t), SurfaceKind.Path);
                }

                if (t >= 3)
                {
                    Part(PrimitiveType.Cylinder, tier, "GardenRing", new Vector3(0f, 0.2f, -3.2f),
                        new Vector3(4.5f, 0.08f, 4.5f),
                        Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Gold, 0.08f), SurfaceKind.Vegetation);
                }

                if (t >= 4)
                {
                    Flag(tier, new Vector3(-2.2f, 0.2f, -1.2f));
                    Flag(tier, new Vector3(2.2f, 0.2f, -1.2f));
                }

                if (t >= 5)
                {
                    Part(PrimitiveType.Cube, tier, "Monument", new Vector3(0f, 1.1f, 2.8f),
                        new Vector3(0.7f, 2.0f, 0.7f), CityVisualMaterials.StoneLight, SurfaceKind.Stone);
                }

                if (t >= 6)
                {
                    Part(PrimitiveType.Cube, tier, "ImperialStatue", new Vector3(-4.5f, 1.4f, -3.2f),
                        new Vector3(1.1f, 2.6f, 1.1f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
                    Part(PrimitiveType.Cube, tier, "ImperialStatueR", new Vector3(4.5f, 1.4f, -3.2f),
                        new Vector3(1.1f, 2.6f, 1.1f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
                }

                tier.gameObject.SetActive(t == 1);
            }
        }

        private static void BuildAmbientProps(Transform decorations, Transform ambient)
        {
            // Mercado / industrial — props sem collider.
            Part(PrimitiveType.Cube, decorations, "MarketStall", new Vector3(-7.2f, 0.55f, -7.2f),
                new Vector3(1.8f, 1.1f, 1.2f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, decorations, "Barrels", new Vector3(-6.2f, 0.35f, -5.8f),
                new Vector3(0.8f, 0.7f, 0.8f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, decorations, "WoodPile", new Vector3(-12.5f, 0.4f, -12.5f),
                new Vector3(1.6f, 0.8f, 1.0f), CityVisualMaterials.Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, decorations, "OrePile", new Vector3(-14.2f, 0.95f, 10.8f),
                new Vector3(1.4f, 0.7f, 1.1f), CityVisualMaterials.StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, decorations, "Targets", new Vector3(12.8f, 0.6f, -4.8f),
                new Vector3(0.3f, 1.2f, 1.4f), CityVisualMaterials.Wood, SurfaceKind.Wood);

            if (CityAmbientQualitySettings.AllowSmoke)
            {
                Part(PrimitiveType.Sphere, ambient, "SmokeHint", new Vector3(-13.2f, 2.2f, -13f),
                    new Vector3(1.4f, 1.8f, 1.4f), new Color(0.55f, 0.55f, 0.55f, 0.35f), SurfaceKind.Plain);
            }
        }

        private static void SoftenLighting() => ApplyDayLighting();

        public static void ApplyDayLighting()
        {
            var light = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                light.color = new Color(1f, 0.9f, 0.72f);
                light.intensity = 1.35f;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.72f;
                light.transform.rotation = Quaternion.Euler(48f, -40f, 0f);
            }

            var haze = new Color(0.52f, 0.56f, 0.5f);
            ApplySky(haze,
                new Color(0.55f, 0.62f, 0.72f),
                new Color(0.5f, 0.46f, 0.38f),
                new Color(0.26f, 0.24f, 0.2f),
                haze,
                0.018f);
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
    }
}
