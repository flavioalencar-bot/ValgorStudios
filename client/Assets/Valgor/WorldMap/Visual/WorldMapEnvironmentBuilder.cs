using UnityEngine;
using Valgor.City.Visual;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Visual
{
    /// <summary>
    /// Terreno medieval provisório e atmosfera do World Map.
    /// </summary>
    public static class WorldMapEnvironmentBuilder
    {
        public static void Build(Transform parent)
        {
            var root = new GameObject("WorldEnvironment").transform;
            root.SetParent(parent, false);

            HideLegacyTerrain();
            BuildGround(root);
            SoftenLighting();
        }

        private static void HideLegacyTerrain()
        {
            // WorldTerrain + Ground da cena usam Default-Material Built-in → magenta no URP.
            foreach (var name in new[] { "WorldTerrain", "Ground" })
            {
                var legacy = GameObject.Find(name);
                if (legacy != null)
                {
                    legacy.SetActive(false);
                }
            }
        }

        private static void BuildGround(Transform root)
        {
            Part(root, "Ocean", Vector3.up * -0.25f, new Vector3(90f, 0.05f, 90f), new Color(0.12f, 0.2f, 0.28f));
            Part(root, "Mainland", Vector3.up * -0.08f, new Vector3(48f, 0.08f, 48f), new Color(0.28f, 0.42f, 0.26f));
            Part(root, "CoastRing", Vector3.up * -0.12f, new Vector3(56f, 0.06f, 56f), new Color(0.22f, 0.34f, 0.26f));
            Part(root, "RoadNS", new Vector3(0f, 0.02f, 0f), new Vector3(0.7f, 0.04f, 34f), new Color(0.42f, 0.36f, 0.28f));
            Part(root, "RoadEW", new Vector3(0f, 0.02f, 0f), new Vector3(34f, 0.04f, 0.7f), new Color(0.42f, 0.36f, 0.28f));
            // Colinas discretas (não “blocos gigantes”).
            Part(root, "HillsA", new Vector3(-11f, 0.12f, 9f), new Vector3(2.4f, 0.18f, 1.8f), new Color(0.3f, 0.4f, 0.28f));
            Part(root, "HillsB", new Vector3(12f, 0.1f, -7f), new Vector3(2.0f, 0.15f, 2.6f), new Color(0.32f, 0.42f, 0.3f));
            Part(root, "WoodsA", new Vector3(-8f, 0.35f, -10f), new Vector3(0.45f, 0.7f, 0.45f), new Color(0.18f, 0.32f, 0.16f));
            Part(root, "WoodsB", new Vector3(9f, 0.4f, 11f), new Vector3(0.5f, 0.85f, 0.5f), new Color(0.16f, 0.3f, 0.14f));
            Part(root, "WoodsC", new Vector3(14f, 0.32f, 3f), new Vector3(0.4f, 0.65f, 0.4f), new Color(0.2f, 0.34f, 0.18f));
        }

        private static void SoftenLighting()
        {
            var light = Object.FindFirstObjectByType<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                light.color = new Color(1f, 0.93f, 0.8f);
                light.intensity = 1.05f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(55f, -25f, 0f);
            }

            ApplyCameraAtmosphere(UnityEngine.Camera.main);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.3f, 0.38f, 0.45f);
            RenderSettings.ambientEquatorColor = new Color(0.36f, 0.34f, 0.28f);
            RenderSettings.ambientGroundColor = new Color(0.16f, 0.18f, 0.16f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.24f, 0.32f, 0.38f);
            RenderSettings.fogDensity = 0.014f;
        }

        public static void ApplyCameraAtmosphere(UnityEngine.Camera? camera)
        {
            if (camera == null)
            {
                return;
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.18f, 0.28f, 0.34f);
            camera.farClipPlane = 120f;
        }

        private static void Part(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            // Clona material da primitiva (incluído na build) — Shader.Find costuma falhar no player.
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
        }
    }

    public static class WorldNodeMeshFactory
    {
        private const float Scale = 0.42f;

        public static Bounds Build(WorldNodeKind kind, Transform parent, Color color)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);

            switch (kind)
            {
                case WorldNodeKind.City:
                    Part(visual.transform, PrimitiveType.Cube, new Vector3(0f, 0.55f, 0f), new Vector3(1.3f, 1.1f, 1.3f), color);
                    Part(visual.transform, PrimitiveType.Cube, new Vector3(0f, 1.25f, 0f), new Vector3(0.75f, 0.55f, 0.75f), Color.Lerp(color, Color.white, 0.12f));
                    Part(visual.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.7f, 0f), new Vector3(0.25f, 0.2f, 0.25f), new Color(0.75f, 0.58f, 0.28f));
                    break;
                case WorldNodeKind.Village:
                    Part(visual.transform, PrimitiveType.Cube, new Vector3(0f, 0.4f, 0f), new Vector3(0.9f, 0.75f, 0.9f), color);
                    Part(visual.transform, PrimitiveType.Cube, new Vector3(0.5f, 0.3f, 0.15f), new Vector3(0.55f, 0.55f, 0.55f), Color.Lerp(color, new Color(0.55f, 0.4f, 0.22f), 0.2f));
                    break;
                case WorldNodeKind.Resource:
                    Part(visual.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(0.9f, 0.35f, 0.9f), color);
                    Part(visual.transform, PrimitiveType.Cube, new Vector3(0f, 0.8f, 0f), new Vector3(0.45f, 0.5f, 0.45f), Color.Lerp(color, Color.white, 0.15f));
                    break;
                case WorldNodeKind.Creature:
                    Part(visual.transform, PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), Vector3.one * 1.0f, color);
                    Part(visual.transform, PrimitiveType.Cube, new Vector3(0f, 1.05f, 0.25f), new Vector3(0.22f, 0.28f, 0.4f), Color.Lerp(color, Color.black, 0.2f));
                    break;
                case WorldNodeKind.Dragon:
                    Part(visual.transform, PrimitiveType.Cube, new Vector3(0f, 0.55f, 0f), new Vector3(1.2f, 0.85f, 1.4f), color);
                    Part(visual.transform, PrimitiveType.Sphere, new Vector3(0f, 1.15f, -0.2f), Vector3.one * 0.6f, Color.Lerp(color, Color.red, 0.15f));
                    break;
                case WorldNodeKind.Landmark:
                    Part(visual.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.85f, 0f), new Vector3(0.55f, 0.85f, 0.55f), color);
                    Part(visual.transform, PrimitiveType.Sphere, new Vector3(0f, 1.75f, 0f), Vector3.one * 0.5f, new Color(0.8f, 0.68f, 0.35f));
                    break;
                default:
                    Part(visual.transform, PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(1f, 1f, 1f), color);
                    break;
            }

            visual.transform.localScale = Vector3.one * Scale;
            return Encapsulate(visual.transform);
        }

        public static Color ColorFor(WorldNodeKind kind, WorldNodeStatus status)
        {
            if (status == WorldNodeStatus.Locked) return new Color(0.3f, 0.32f, 0.34f);
            if (status == WorldNodeStatus.Depleted) return new Color(0.45f, 0.4f, 0.3f);
            return kind switch
            {
                WorldNodeKind.City => new Color(0.32f, 0.48f, 0.7f),
                WorldNodeKind.Village => new Color(0.42f, 0.58f, 0.36f),
                WorldNodeKind.Resource => new Color(0.78f, 0.62f, 0.28f),
                WorldNodeKind.Creature => new Color(0.78f, 0.35f, 0.28f),
                WorldNodeKind.Dragon => new Color(0.55f, 0.28f, 0.62f),
                WorldNodeKind.Landmark => new Color(0.5f, 0.55f, 0.6f),
                _ => Color.gray
            };
        }

        private static void Part(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
        }

        private static Bounds Encapsulate(Transform visual)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(Vector3.up, Vector3.one);
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return new Bounds(visual.InverseTransformPoint(bounds.center), bounds.size);
        }
    }
}
