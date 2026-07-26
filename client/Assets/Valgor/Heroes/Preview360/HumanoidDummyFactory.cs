using UnityEngine;

namespace Valgor.Heroes.Preview360
{
    /// <summary>
    /// Builds a provisional humanoid dummy with heroic adult proportions (~2.05 m, ~8.25 heads).
    /// Not final art — only a readable stand-in for preview framing.
    /// </summary>
    public static class HumanoidDummyFactory
    {
        public const string LayerName = "HeroPreview";
        public const string PrefabPath = "Assets/Valgor/Heroes/Prefabs/HumanoidDummy.prefab";

        /// <summary>Target standing height in meters.</summary>
        public const float TargetHeightMeters = 2.05f;

        /// <summary>Head units for heroic adult silhouette.</summary>
        public const float HeadUnits = 8.25f;

        public static int ResolveLayer()
        {
            var layer = LayerMask.NameToLayer(LayerName);
            return layer >= 0 ? layer : 0;
        }

        public static GameObject Create(Transform parent, Material sharedMaterial)
        {
            var root = new GameObject("HumanoidDummy");
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            var layer = ResolveLayer();
            SetLayerRecursive(root, layer);

            // Heroic landmarks from feet (8.25 heads, H=2.05):
            // head≈0.248, crotch≈4 heads, knee≈2 heads, chin≈7.5 heads
            const float h = TargetHeightMeters;
            var head = h / HeadUnits;                 // ~0.248
            var crotch = 4.05f * head;                // ~1.01 — longer legs
            var knee = 2.05f * head;                  // ~0.51
            var shoulder = 6.55f * head;              // ~1.63
            var chin = 7.45f * head;                  // ~1.85
            var headCenter = h - head * 0.5f;         // ~1.926

            // Unity capsule mesh height ≈ 2 * scale.y
            var thighLen = crotch - knee;
            var shinLen = knee - 0.02f;
            var torsoLen = chin - crotch - 0.08f;
            var armLen = 3.15f * head;

            // Hips / pelvis (wide, not tall)
            CreatePart(root.transform, "Hips", PrimitiveType.Cube,
                new Vector3(0f, crotch - 0.02f, 0f),
                new Vector3(0.40f, 0.16f, 0.26f),
                sharedMaterial);

            // Torso — strong but not compressed
            CreatePart(root.transform, "Torso", PrimitiveType.Capsule,
                new Vector3(0f, crotch + torsoLen * 0.5f, 0.02f),
                new Vector3(0.34f, torsoLen * 0.5f, 0.24f),
                sharedMaterial);

            // Head — slightly smaller relative to body (more head-units)
            CreatePart(root.transform, "Head", PrimitiveType.Sphere,
                new Vector3(0f, headCenter, 0.02f),
                new Vector3(head * 0.92f, head * 0.92f, head * 0.92f),
                sharedMaterial);

            // Shoulders / arms — longer, set at shoulder line
            CreatePart(root.transform, "LeftArm", PrimitiveType.Capsule,
                new Vector3(-(0.22f + 0.12f), shoulder - armLen * 0.15f, 0f),
                new Vector3(0.11f, armLen * 0.5f, 0.11f),
                sharedMaterial);
            CreatePart(root.transform, "RightArm", PrimitiveType.Capsule,
                new Vector3(0.22f + 0.12f, shoulder - armLen * 0.15f, 0f),
                new Vector3(0.11f, armLen * 0.5f, 0.11f),
                sharedMaterial);

            // Legs — longer thigh + shin for adult male read
            CreatePart(root.transform, "LeftThigh", PrimitiveType.Capsule,
                new Vector3(-0.12f, knee + thighLen * 0.5f, 0f),
                new Vector3(0.13f, thighLen * 0.5f, 0.13f),
                sharedMaterial);
            CreatePart(root.transform, "RightThigh", PrimitiveType.Capsule,
                new Vector3(0.12f, knee + thighLen * 0.5f, 0f),
                new Vector3(0.13f, thighLen * 0.5f, 0.13f),
                sharedMaterial);
            CreatePart(root.transform, "LeftShin", PrimitiveType.Capsule,
                new Vector3(-0.12f, 0.02f + shinLen * 0.5f, 0f),
                new Vector3(0.11f, shinLen * 0.5f, 0.11f),
                sharedMaterial);
            CreatePart(root.transform, "RightShin", PrimitiveType.Capsule,
                new Vector3(0.12f, 0.02f + shinLen * 0.5f, 0f),
                new Vector3(0.11f, shinLen * 0.5f, 0.11f),
                sharedMaterial);

            return root;
        }

        public static void ApplyMaterial(GameObject root, Material material)
        {
            if (root == null || material == null) return;
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.sharedMaterial = material;
            }
        }

        public static void ApplyColor(Material material, Color color)
        {
            if (material == null) return;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            material.color = color;
        }

        public static Material CreateUrpCompatibleMaterial(Color color)
        {
            var shader = Shader.Find("Valgor/Heroes/DummyUnlit")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogError("HumanoidDummyFactory: nenhum shader compatível encontrado.");
                return null;
            }

            var material = new Material(shader) { name = "HeroPreviewDummyMat" };
            ApplyColor(material, color);
            return material;
        }

        private static void CreatePart(
            Transform parent,
            string name,
            PrimitiveType type,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = localScale;

            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying) Object.Destroy(collider);
                else Object.DestroyImmediate(collider);
            }

            var renderer = part.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            SetLayerRecursive(part, parent.gameObject.layer);
        }

        public static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}
