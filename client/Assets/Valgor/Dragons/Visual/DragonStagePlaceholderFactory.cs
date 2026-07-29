using UnityEngine;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Visual
{
    /// <summary>
    /// Placeholders 3D próprios e diferenciados por estágio (sem assets de terceiros).
    /// </summary>
    public static class DragonStagePlaceholderFactory
    {
        public static GameObject Create(Transform parent, DragonStageVisualConfig config)
        {
            var root = new GameObject($"Placeholder_{config.Stage}");
            root.transform.SetParent(parent, false);

            switch (config.Stage)
            {
                case DragonVisualStage.Egg:
                    BuildEgg(root.transform, config.PlaceholderTint);
                    break;
                case DragonVisualStage.Hatchling:
                    BuildHatchling(root.transform, config.PlaceholderTint);
                    break;
                case DragonVisualStage.Young:
                    BuildYoung(root.transform, config.PlaceholderTint);
                    break;
                case DragonVisualStage.Adolescent:
                    BuildAdolescent(root.transform, config.PlaceholderTint);
                    break;
                case DragonVisualStage.YoungAdult:
                    BuildYoungAdult(root.transform, config.PlaceholderTint);
                    break;
                case DragonVisualStage.Adult:
                    BuildAdult(root.transform, config.PlaceholderTint);
                    break;
                default:
                    BuildAncestral(root.transform, config.PlaceholderTint);
                    break;
            }

            TagPlaceholder(root, config);
            return root;
        }

        public static void SpawnSoftBurst(Transform parent, Color tint)
        {
            var burst = new GameObject("StageSoftBurst");
            burst.transform.SetParent(parent, false);
            burst.transform.localPosition = Vector3.up * 0.6f;
            var ps = burst.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.7f;
            main.startSize = 0.12f;
            main.startColor = tint;
            main.maxParticles = 24;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;
            Object.Destroy(burst, 1.4f);
        }

        private static void TagPlaceholder(GameObject root, DragonStageVisualConfig config)
        {
            var tag = new GameObject($"_PLACEHOLDER_{config.Stage}_{config.DisplayNamePt}");
            tag.transform.SetParent(root.transform, false);
        }

        private static void BuildEgg(Transform root, Color tint)
        {
            var body = Primitive(PrimitiveType.Sphere, root, "EggBody", tint, new Vector3(0f, 0f, 0f), new Vector3(1f, 1.25f, 1f));
            _ = body;
            Primitive(PrimitiveType.Sphere, root, "EggSpot", Color.Lerp(tint, Color.white, 0.25f),
                new Vector3(0.12f, 0.15f, 0.28f), new Vector3(0.28f, 0.22f, 0.18f));
        }

        private static void BuildHatchling(Transform root, Color tint)
        {
            Primitive(PrimitiveType.Capsule, root, "Body", tint, Vector3.zero, new Vector3(0.7f, 0.55f, 0.7f));
            Primitive(PrimitiveType.Sphere, root, "Head", Color.Lerp(tint, new Color(0.9f, 0.5f, 0.2f), 0.3f),
                new Vector3(0f, 0.55f, 0.25f), new Vector3(0.55f, 0.5f, 0.55f));
            Primitive(PrimitiveType.Cube, root, "Snout", Color.Lerp(tint, Color.black, 0.2f),
                new Vector3(0f, 0.45f, 0.55f), new Vector3(0.22f, 0.16f, 0.28f));
        }

        private static void BuildYoung(Transform root, Color tint)
        {
            Primitive(PrimitiveType.Capsule, root, "Body", tint, Vector3.zero, new Vector3(0.75f, 0.65f, 0.75f));
            Primitive(PrimitiveType.Sphere, root, "Head", Color.Lerp(tint, new Color(0.95f, 0.45f, 0.15f), 0.25f),
                new Vector3(0f, 0.7f, 0.3f), new Vector3(0.6f, 0.55f, 0.6f));
            Wing(root, "WingL", tint, new Vector3(-0.55f, 0.35f, 0f), new Vector3(0.08f, 0.35f, 0.55f), -25f);
            Wing(root, "WingR", tint, new Vector3(0.55f, 0.35f, 0f), new Vector3(0.08f, 0.35f, 0.55f), 25f);
        }

        private static void BuildAdolescent(Transform root, Color tint)
        {
            Primitive(PrimitiveType.Capsule, root, "Body", tint, Vector3.zero, new Vector3(0.8f, 0.75f, 0.8f));
            Primitive(PrimitiveType.Sphere, root, "Head", Color.Lerp(tint, new Color(0.85f, 0.4f, 0.18f), 0.2f),
                new Vector3(0f, 0.85f, 0.32f), new Vector3(0.65f, 0.58f, 0.65f));
            Primitive(PrimitiveType.Cube, root, "HornL", new Color(0.75f, 0.65f, 0.4f),
                new Vector3(-0.18f, 1.15f, 0.15f), new Vector3(0.1f, 0.35f, 0.1f));
            Primitive(PrimitiveType.Cube, root, "HornR", new Color(0.75f, 0.65f, 0.4f),
                new Vector3(0.18f, 1.15f, 0.15f), new Vector3(0.1f, 0.35f, 0.1f));
            Wing(root, "WingL", tint, new Vector3(-0.7f, 0.4f, 0f), new Vector3(0.1f, 0.45f, 0.7f), -30f);
            Wing(root, "WingR", tint, new Vector3(0.7f, 0.4f, 0f), new Vector3(0.1f, 0.45f, 0.7f), 30f);
        }

        private static void BuildYoungAdult(Transform root, Color tint)
        {
            Primitive(PrimitiveType.Capsule, root, "Body", tint, Vector3.zero, new Vector3(0.85f, 0.85f, 0.85f));
            Primitive(PrimitiveType.Sphere, root, "Head", Color.Lerp(tint, new Color(0.7f, 0.3f, 0.12f), 0.2f),
                new Vector3(0f, 0.95f, 0.35f), new Vector3(0.7f, 0.62f, 0.7f));
            for (var i = 0; i < 3; i++)
            {
                Primitive(PrimitiveType.Cube, root, $"Ridge{i}", Color.Lerp(tint, Color.black, 0.25f),
                    new Vector3(0f, 0.35f + i * 0.22f, -0.35f), new Vector3(0.12f, 0.18f, 0.18f));
            }

            Wing(root, "WingL", tint, new Vector3(-0.85f, 0.45f, 0f), new Vector3(0.12f, 0.55f, 0.85f), -32f);
            Wing(root, "WingR", tint, new Vector3(0.85f, 0.45f, 0f), new Vector3(0.12f, 0.55f, 0.85f), 32f);
        }

        private static void BuildAdult(Transform root, Color tint)
        {
            Primitive(PrimitiveType.Capsule, root, "Body", tint, Vector3.zero, new Vector3(0.95f, 0.95f, 0.95f));
            Primitive(PrimitiveType.Sphere, root, "Head", Color.Lerp(tint, new Color(0.55f, 0.22f, 0.1f), 0.25f),
                new Vector3(0f, 1.05f, 0.4f), new Vector3(0.78f, 0.68f, 0.78f));
            Primitive(PrimitiveType.Cube, root, "Jaw", Color.Lerp(tint, Color.black, 0.35f),
                new Vector3(0f, 0.85f, 0.75f), new Vector3(0.4f, 0.18f, 0.35f));
            Wing(root, "WingL", Color.Lerp(tint, Color.black, 0.15f),
                new Vector3(-1.0f, 0.5f, 0f), new Vector3(0.14f, 0.65f, 1.0f), -35f);
            Wing(root, "WingR", Color.Lerp(tint, Color.black, 0.15f),
                new Vector3(1.0f, 0.5f, 0f), new Vector3(0.14f, 0.65f, 1.0f), 35f);
            Primitive(PrimitiveType.Cube, root, "Tail", tint, new Vector3(0f, 0.15f, -0.85f), new Vector3(0.2f, 0.2f, 0.7f));
        }

        private static void BuildAncestral(Transform root, Color tint)
        {
            var bodyTint = Color.Lerp(tint, new Color(0.35f, 0.2f, 0.1f), 0.2f);
            Primitive(PrimitiveType.Capsule, root, "Body", bodyTint, Vector3.zero, new Vector3(1.05f, 1.05f, 1.05f));
            Primitive(PrimitiveType.Sphere, root, "Head", Color.Lerp(tint, new Color(0.9f, 0.7f, 0.25f), 0.35f),
                new Vector3(0f, 1.15f, 0.42f), new Vector3(0.85f, 0.72f, 0.85f));
            for (var i = 0; i < 5; i++)
            {
                var angle = (i - 2) * 18f;
                var horn = Primitive(PrimitiveType.Cube, root, $"Crown{i}", new Color(0.95f, 0.8f, 0.35f),
                    new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * 0.25f, 1.45f, 0.1f + Mathf.Cos(angle * Mathf.Deg2Rad) * 0.05f),
                    new Vector3(0.08f, 0.42f, 0.08f));
                horn.transform.localRotation = Quaternion.Euler(0f, 0f, angle * 0.4f);
            }

            Wing(root, "WingL", Color.Lerp(tint, new Color(0.9f, 0.65f, 0.2f), 0.2f),
                new Vector3(-1.15f, 0.55f, 0f), new Vector3(0.16f, 0.75f, 1.15f), -38f);
            Wing(root, "WingR", Color.Lerp(tint, new Color(0.9f, 0.65f, 0.2f), 0.2f),
                new Vector3(1.15f, 0.55f, 0f), new Vector3(0.16f, 0.75f, 1.15f), 38f);
            Primitive(PrimitiveType.Cube, root, "Tail", bodyTint, new Vector3(0f, 0.2f, -1.0f), new Vector3(0.22f, 0.22f, 0.9f));
        }

        private static void Wing(Transform root, string name, Color tint, Vector3 pos, Vector3 scale, float yaw)
        {
            var wing = Primitive(PrimitiveType.Cube, root, name, Color.Lerp(tint, Color.black, 0.1f), pos, scale);
            wing.transform.localRotation = Quaternion.Euler(10f, yaw, 0f);
        }

        private static GameObject Primitive(
            PrimitiveType type,
            Transform parent,
            string name,
            Color color,
            Vector3 localPos,
            Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            Object.Destroy(go.GetComponent<Collider>());
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                                       Shader.Find("Sprites/Default") ??
                                       Shader.Find("Standard"));
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", color);
                }

                if (mat.HasProperty("_Color"))
                {
                    mat.color = color;
                }

                renderer.sharedMaterial = mat;
            }

            return go;
        }
    }
}
