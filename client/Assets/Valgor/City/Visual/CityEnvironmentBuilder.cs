using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Terreno amplo, caminhos de pedra, horizonte e iluminação com profundidade.
    /// Não aumenta “lixo” decorativo — poucos elementos legíveis.
    /// </summary>
    public static class CityEnvironmentBuilder
    {
        public static void Build(Transform parent)
        {
            var root = new GameObject("CityEnvironment").transform;
            root.SetParent(parent, false);

            HideSceneGround();
            BuildTerrain(root);
            BuildHorizon(root);
            BuildDistrictPads(root);
            BuildPlaza(root);
            BuildRoads(root);
            BuildWallRing(root);
            BuildMainGate(root);
            BuildTrees(root);
            SoftenLighting();
        }

        private static void BuildTerrain(Transform root)
        {
            // Terreno maior que o enquadramento — nunca deixa ver “fora”.
            Flat(root, "GrassFar", Vector3.zero, new Vector3(90f, 0.06f, 90f),
                    CityVisualMaterials.Vegetation, SurfaceKind.Vegetation)
                .transform.localPosition = new Vector3(0f, -0.06f, 0f);

            Flat(root, "GrassMid", Vector3.zero, new Vector3(52f, 0.07f, 52f),
                    Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.12f),
                    SurfaceKind.Vegetation)
                .transform.localPosition = new Vector3(0f, -0.03f, 0f);

            Flat(root, "InnerSoil", Vector3.zero, new Vector3(34f, 0.05f, 34f),
                    Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.28f),
                    SurfaceKind.Dirt)
                .transform.localPosition = new Vector3(0f, -0.01f, 0f);
        }

        private static void BuildHorizon(Transform root)
        {
            // Colinas / floresta baixa nas bordas (silhueta, não debug cubes).
            var hill = Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.StoneDark, 0.35f);
            var mist = Color.Lerp(CityVisualMaterials.Vegetation, new Color(0.45f, 0.5f, 0.48f), 0.4f);
            const float radius = 34f;
            const int hills = 10;
            for (var i = 0; i < hills; i++)
            {
                var angle = i * (360f / hills) * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Sin(angle) * radius, 1.1f, Mathf.Cos(angle) * radius);
                var w = 7.5f + (i % 3) * 1.2f;
                Part(PrimitiveType.Sphere, root, $"Hill_{i}", pos, new Vector3(w, 3.2f + i % 2, w * 0.85f), hill,
                    SurfaceKind.Vegetation);
            }

            // Anel de névoa/terreno distante (cor de gramado, não azul).
            Flat(root, "MistRing", Vector3.zero, new Vector3(78f, 0.04f, 78f), mist, SurfaceKind.Vegetation)
                .transform.localPosition = new Vector3(0f, -0.08f, 0f);
        }

        private static void BuildDistrictPads(Transform root)
        {
            Flat(root, "PadEconomy", new Vector3(-9.5f, 0.005f, 2.5f), new Vector3(9f, 0.04f, 14f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.2f), SurfaceKind.Dirt);
            Flat(root, "PadMilitary", new Vector3(9.5f, 0.005f, 0f), new Vector3(7.5f, 0.04f, 11f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.15f), SurfaceKind.Dirt);
            Flat(root, "PadCommerce", new Vector3(0f, 0.005f, -9.5f), new Vector3(13f, 0.04f, 5.5f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Path, 0.2f), SurfaceKind.Dirt);
            Flat(root, "PadMystic", new Vector3(6.5f, 0.005f, 8.5f), new Vector3(9f, 0.04f, 9f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.RoofBlue, 0.12f), SurfaceKind.Dirt);
        }

        private static void BuildPlaza(Transform root)
        {
            Flat(root, "PlazaRim", Vector3.zero, new Vector3(11f, 0.08f, 11f),
                    Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneDark, 0.2f), SurfaceKind.Path)
                .transform.localPosition = new Vector3(0f, 0.01f, 0f);
            Flat(root, "Plaza", Vector3.zero, new Vector3(8.5f, 0.1f, 8.5f),
                    CityVisualMaterials.Path, SurfaceKind.Path)
                .transform.localPosition = new Vector3(0f, 0.025f, 0f);
            Flat(root, "CastleForecourt", new Vector3(0f, 0.04f, 4.4f), new Vector3(5.8f, 0.08f, 3.0f),
                Color.Lerp(CityVisualMaterials.Path, CityVisualMaterials.StoneLight, 0.15f), SurfaceKind.Path);
        }

        private static void BuildRoads(Transform root)
        {
            // Pedra / terra compactada — bordas suaves via faixa de terra sob o caminho.
            Road(root, "RoadNS", Vector3.zero, new Vector3(1.9f, 0.07f, 20f), 0f);
            Road(root, "RoadEW", Vector3.zero, new Vector3(20f, 0.07f, 1.9f), 0f);
            Road(root, "RoadToTower", new Vector3(3.0f, 0.03f, 3.0f), new Vector3(1.5f, 0.07f, 8f), 45f);
            Road(root, "RoadToFarm", new Vector3(-4.8f, 0.03f, -0.8f), new Vector3(1.45f, 0.07f, 7.5f), -18f);
            Road(root, "RoadToAcademy", new Vector3(4.8f, 0.03f, -1.6f), new Vector3(1.45f, 0.07f, 7.5f), 22f);
            Road(root, "RoadToWarehouse", new Vector3(2.5f, 0.03f, -5.5f), new Vector3(1.4f, 0.07f, 5.5f), 35f);
        }

        private static void Road(Transform root, string name, Vector3 pos, Vector3 scale, float yaw)
        {
            var bedScale = new Vector3(scale.x + 0.55f, scale.y * 0.7f, scale.z + 0.55f);
            var bed = Flat(root, name + "_Bed", pos, bedScale, CityVisualMaterials.Dirt, SurfaceKind.Dirt);
            bed.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            bed.transform.localPosition = pos + Vector3.down * 0.01f;

            var stone = Flat(root, name, pos, scale, CityVisualMaterials.Path, SurfaceKind.Path);
            stone.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            stone.transform.localPosition = pos;
        }

        private static void BuildWallRing(Transform root)
        {
            var wall = CityVisualMaterials.StoneLight;
            var cap = CityVisualMaterials.StoneDark;
            const float radius = 16.5f;
            const float height = 2.1f;
            const float thickness = 0.65f;
            const int segments = 16;
            for (var i = 0; i < segments; i++)
            {
                // Aberturas: sul (portão principal), eixos N/E/W menores.
                if (i is 0 or 4 or 8 or 12)
                {
                    continue;
                }

                var angle = i * (360f / segments) * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Sin(angle) * radius, height * 0.5f, Mathf.Cos(angle) * radius);
                var piece = Part(PrimitiveType.Cube, root, $"Wall_{i}", pos,
                    new Vector3(3.2f, height, thickness), wall, SurfaceKind.Stone);
                piece.transform.LookAt(new Vector3(0f, height * 0.5f, 0f));
                var capGo = Part(PrimitiveType.Cube, root, $"WallCap_{i}", pos + Vector3.up * (height * 0.52f),
                    new Vector3(3.2f, 0.28f, thickness + 0.12f), cap, SurfaceKind.Stone);
                capGo.transform.rotation = piece.transform.rotation;
            }
        }

        private static void BuildMainGate(Transform root)
        {
            // Um portão principal coerente (sul) — sem 4 portões “de debug”.
            var stone = CityVisualMaterials.StoneLight;
            var dark = CityVisualMaterials.StoneDark;
            var z = 16.5f;
            Part(PrimitiveType.Cube, root, "GateL", new Vector3(-2.1f, 1.5f, z), new Vector3(1.4f, 3.0f, 1.4f), dark,
                SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, "GateR", new Vector3(2.1f, 1.5f, z), new Vector3(1.4f, 3.0f, 1.4f), dark,
                SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, "GateArch", new Vector3(0f, 3.2f, z), new Vector3(5.0f, 0.55f, 1.5f), stone,
                SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, "GateGold", new Vector3(0f, 3.55f, z), new Vector3(1.4f, 0.18f, 0.45f),
                CityVisualMaterials.Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, "GateDoor", new Vector3(0f, 1.35f, z + 0.15f), new Vector3(2.4f, 2.5f, 0.25f),
                CityVisualMaterials.Wood, SurfaceKind.Wood);

            // Tochas do portão (2 — legíveis).
            Torch(root, new Vector3(-2.1f, 2.6f, z - 0.75f));
            Torch(root, new Vector3(2.1f, 2.6f, z - 0.75f));

            // Portões laterais menores (entrada simples, sem ouro espalhado).
            SideGate(root, new Vector3(0f, 0f, -16.5f), false);
            SideGate(root, new Vector3(16.5f, 0f, 0f), true);
            SideGate(root, new Vector3(-16.5f, 0f, 0f), true);
        }

        private static void SideGate(Transform root, Vector3 g, bool alongX)
        {
            var stone = CityVisualMaterials.StoneDark;
            var left = alongX ? new Vector3(g.x, 1.0f, g.z - 1.35f) : new Vector3(g.x - 1.35f, 1.0f, g.z);
            var right = alongX ? new Vector3(g.x, 1.0f, g.z + 1.35f) : new Vector3(g.x + 1.35f, 1.0f, g.z);
            Part(PrimitiveType.Cube, root, "SideGateL", left, new Vector3(1.0f, 2.0f, 1.0f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, "SideGateR", right, new Vector3(1.0f, 2.0f, 1.0f), stone, SurfaceKind.Stone);
            var arch = new Vector3(g.x, 2.15f, g.z);
            Part(PrimitiveType.Cube, root, "SideGateArch", arch,
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

        private static void BuildTrees(Transform root)
        {
            var trunk = Color.Lerp(CityVisualMaterials.Wood, CityVisualMaterials.StoneDark, 0.15f);
            var leaf = CityVisualMaterials.Vegetation;
            // Poucas árvores bem proporcionadas nas bordas dos distritos — sem “lixo”.
            Vector3[] spots =
            {
                new(-11.5f, 0f, -5.5f), new(-10.5f, 0f, 5.5f),
                new(11.2f, 0f, -4.5f), new(10.5f, 0f, 5.2f),
                new(-5.5f, 0f, 11f), new(5.2f, 0f, -10.5f),
                new(-13.5f, 0f, 0.5f), new(13.2f, 0f, 1.2f)
            };

            for (var i = 0; i < spots.Length; i++)
            {
                var spot = spots[i];
                Part(PrimitiveType.Cylinder, root, $"Trunk_{i}", spot + Vector3.up * 0.7f,
                    new Vector3(0.28f, 0.7f, 0.28f), trunk, SurfaceKind.Wood);
                Part(PrimitiveType.Sphere, root, $"Canopy_{i}", spot + Vector3.up * 1.65f,
                    new Vector3(1.35f, 1.15f, 1.35f), leaf, SurfaceKind.Vegetation);
            }
        }

        private static void SoftenLighting() => ApplyDayLighting();

        public static void ApplyDayLighting()
        {
            var light = Object.FindFirstObjectByType<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                light.color = new Color(1f, 0.9f, 0.72f);
                light.intensity = 1.35f;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.72f;
                light.transform.rotation = Quaternion.Euler(48f, -40f, 0f);
            }

            // Fundo / fog em tom de névoa-grama — nunca azul puro nas bordas.
            var haze = new Color(0.52f, 0.56f, 0.5f);
            ApplySky(haze,
                new Color(0.55f, 0.62f, 0.72f),
                new Color(0.5f, 0.46f, 0.38f),
                new Color(0.26f, 0.24f, 0.2f),
                haze,
                0.022f);
        }

        public static void ApplyNightLighting()
        {
            var light = Object.FindFirstObjectByType<Light>();
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
                0.032f);
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
                camera.farClipPlane = 120f;
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
            SurfaceKind surface = SurfaceKind.Stone)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.ApplySurface(go.GetComponent<Renderer>(), color, surface);
            return go;
        }
    }
}
