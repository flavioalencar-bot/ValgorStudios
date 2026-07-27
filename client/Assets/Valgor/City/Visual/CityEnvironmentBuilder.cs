using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Chão, praça, caminhos, céu e iluminação quente da cidade provisional.
    /// </summary>
    public static class CityEnvironmentBuilder
    {
        public static void Build(Transform parent)
        {
            var root = new GameObject("CityEnvironment").transform;
            root.SetParent(parent, false);

            HideSceneGround();
            BuildTerrain(root);
            BuildDistrictPads(root);
            BuildPlaza(root);
            BuildRoads(root);
            BuildWallRing(root);
            BuildGates(root);
            BuildTrees(root);
            SoftenLighting();
        }

        private static void BuildDistrictPads(Transform root)
        {
            // Economia (oeste)
            Flat(root, "PadEconomy", new Vector3(-10f, 0.005f, 3f), new Vector3(10f, 0.05f, 16f),
                new Color(0.3f, 0.38f, 0.26f));
            // Militar (leste)
            Flat(root, "PadMilitary", new Vector3(10f, 0.005f, 0f), new Vector3(8f, 0.05f, 12f),
                new Color(0.36f, 0.32f, 0.28f));
            // Comércio (sul)
            Flat(root, "PadCommerce", new Vector3(0f, 0.005f, -10.5f), new Vector3(14f, 0.05f, 6f),
                new Color(0.34f, 0.3f, 0.24f));
            // Místico (nordeste)
            Flat(root, "PadMystic", new Vector3(7f, 0.005f, 9f), new Vector3(10f, 0.05f, 10f),
                new Color(0.28f, 0.3f, 0.36f));
        }

        private static void BuildGates(Transform root)
        {
            var stone = new Color(0.4f, 0.38f, 0.35f);
            var gold = new Color(0.72f, 0.58f, 0.28f);
            // Portões nos eixos (aberturas da muralha).
            Vector3[] gates =
            {
                new(0f, 0f, 15.5f), new(0f, 0f, -15.5f),
                new(15.5f, 0f, 0f), new(-15.5f, 0f, 0f)
            };
            for (var i = 0; i < gates.Length; i++)
            {
                var g = gates[i];
                var alongX = Mathf.Abs(g.x) > Mathf.Abs(g.z);
                var left = alongX ? new Vector3(g.x, 1.1f, g.z - 1.6f) : new Vector3(g.x - 1.6f, 1.1f, g.z);
                var right = alongX ? new Vector3(g.x, 1.1f, g.z + 1.6f) : new Vector3(g.x + 1.6f, 1.1f, g.z);
                Part(PrimitiveType.Cube, root, $"GateL_{i}", left, new Vector3(1.1f, 2.2f, 1.1f), stone);
                Part(PrimitiveType.Cube, root, $"GateR_{i}", right, new Vector3(1.1f, 2.2f, 1.1f), stone);
                var arch = alongX
                    ? new Vector3(g.x, 2.35f, g.z)
                    : new Vector3(g.x, 2.35f, g.z);
                Part(PrimitiveType.Cube, root, $"GateArch_{i}", arch,
                    alongX ? new Vector3(1.1f, 0.4f, 3.4f) : new Vector3(3.4f, 0.4f, 1.1f), stone);
                Part(PrimitiveType.Cube, root, $"GateGold_{i}", arch + Vector3.up * 0.35f,
                    alongX ? new Vector3(0.35f, 0.2f, 1.2f) : new Vector3(1.2f, 0.2f, 0.35f), gold);
            }
        }

        private static void HideSceneGround()
        {
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                ground.SetActive(false);
            }
        }

        private static void BuildTerrain(Transform root)
        {
            Flat(root, "Grass", Vector3.zero, new Vector3(38f, 0.08f, 38f), new Color(0.28f, 0.4f, 0.24f))
                .transform.localPosition = new Vector3(0f, -0.04f, 0f);

            Flat(root, "OuterRing", Vector3.zero, new Vector3(30f, 0.06f, 30f), new Color(0.32f, 0.36f, 0.28f))
                .transform.localPosition = new Vector3(0f, -0.01f, 0f);
        }

        private static void BuildPlaza(Transform root)
        {
            Flat(root, "Plaza", Vector3.zero, new Vector3(9f, 0.12f, 9f), new Color(0.48f, 0.44f, 0.38f))
                .transform.localPosition = new Vector3(0f, 0.02f, 0f);

            Flat(root, "PlazaRim", Vector3.zero, new Vector3(10.5f, 0.1f, 10.5f), new Color(0.4f, 0.36f, 0.32f))
                .transform.localPosition = new Vector3(0f, 0.01f, 0f);
        }

        private static void BuildRoads(Transform root)
        {
            var road = new Color(0.4f, 0.36f, 0.3f);
            Flat(root, "RoadNS", Vector3.zero, new Vector3(2.1f, 0.09f, 24f), road)
                .transform.localPosition = new Vector3(0f, 0.03f, 0f);
            Flat(root, "RoadEW", Vector3.zero, new Vector3(24f, 0.09f, 2.1f), road)
                .transform.localPosition = new Vector3(0f, 0.03f, 0f);
            Flat(root, "RoadNE", new Vector3(4.5f, 0.03f, 4.5f), new Vector3(1.8f, 0.09f, 11f), road)
                .transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            Flat(root, "RoadToTower", new Vector3(3.5f, 0.03f, 3.5f), new Vector3(1.6f, 0.09f, 9f), road)
                .transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        }

        private static void BuildWallRing(Transform root)
        {
            var wall = new Color(0.42f, 0.4f, 0.37f);
            const float radius = 15.5f;
            const float height = 1.5f;
            const float thickness = 0.55f;
            const int segments = 12;
            for (var i = 0; i < segments; i++)
            {
                if (i is 0 or 3 or 6 or 9)
                {
                    continue;
                }

                var angle = i * (360f / segments) * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Sin(angle) * radius, height * 0.5f, Mathf.Cos(angle) * radius);
                var piece = Part(PrimitiveType.Cube, root, $"Wall_{i}", pos, new Vector3(4.0f, height, thickness), wall);
                piece.transform.LookAt(new Vector3(0f, height * 0.5f, 0f));
                Part(PrimitiveType.Cube, root, $"WallCap_{i}", pos + Vector3.up * (height * 0.55f),
                    new Vector3(4.0f, 0.25f, thickness + 0.15f), new Color(0.38f, 0.36f, 0.34f));
            }
        }

        private static void BuildTrees(Transform root)
        {
            var trunk = new Color(0.32f, 0.2f, 0.1f);
            var leaf = new Color(0.24f, 0.46f, 0.24f);
            Vector3[] spots =
            {
                new(-12f, 0f, -6f), new(-11f, 0f, 6f), new(12f, 0f, -5f),
                new(11f, 0f, 5f), new(-6f, 0f, 12f), new(6f, 0f, -12f),
                new(-13f, 0f, 0f), new(13f, 0f, 1f),
                new(-5f, 0f, -8f), new(5f, 0f, 8f), new(-9f, 0f, -11f), new(8f, 0f, 11f)
            };

            for (var i = 0; i < spots.Length; i++)
            {
                var spot = spots[i];
                Part(PrimitiveType.Cylinder, root, $"Trunk_{i}", spot + Vector3.up * 0.55f, new Vector3(0.32f, 0.55f, 0.32f), trunk);
                Part(PrimitiveType.Sphere, root, $"Canopy_{i}", spot + Vector3.up * 1.4f, Vector3.one * 1.25f, leaf);
            }
        }

        private static void SoftenLighting()
        {
            var light = Object.FindFirstObjectByType<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                light.color = new Color(1f, 0.92f, 0.78f);
                light.intensity = 1.2f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
            }

            var camera = UnityEngine.Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.48f, 0.62f, 0.78f);
                camera.farClipPlane = 80f;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.72f, 0.88f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.5f, 0.42f);
            RenderSettings.ambientGroundColor = new Color(0.28f, 0.26f, 0.22f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.58f, 0.68f, 0.8f);
            RenderSettings.fogDensity = 0.018f;
        }

        private static GameObject Flat(Transform parent, string name, Vector3 position, Vector3 scale, Color color) =>
            Part(PrimitiveType.Cube, parent, name, position, scale, color);

        private static GameObject Part(
            PrimitiveType type,
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
            return go;
        }
    }
}
