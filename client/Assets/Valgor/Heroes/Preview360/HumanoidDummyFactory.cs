using UnityEngine;
using Valgor.Core;

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

            const float h = TargetHeightMeters;
            var head = h / HeadUnits;
            var crotch = 4.05f * head;
            var knee = 2.05f * head;
            var shoulder = 6.55f * head;
            var chin = 7.45f * head;
            var headCenter = h - head * 0.5f;

            var thighLen = crotch - knee;
            var shinLen = knee - 0.02f;
            var torsoLen = chin - crotch - 0.08f;
            var armLen = 3.15f * head;

            CreatePart(root.transform, "Hips", PrimitiveType.Cube,
                new Vector3(0f, crotch - 0.02f, 0f),
                new Vector3(0.40f, 0.16f, 0.26f),
                sharedMaterial);

            CreatePart(root.transform, "Torso", PrimitiveType.Capsule,
                new Vector3(0f, crotch + torsoLen * 0.5f, 0.02f),
                new Vector3(0.34f, torsoLen * 0.5f, 0.24f),
                sharedMaterial);

            CreatePart(root.transform, "Head", PrimitiveType.Sphere,
                new Vector3(0f, headCenter, 0.02f),
                new Vector3(head * 0.92f, head * 0.92f, head * 0.92f),
                sharedMaterial);

            CreatePart(root.transform, "LeftArm", PrimitiveType.Capsule,
                new Vector3(-(0.22f + 0.12f), shoulder - armLen * 0.15f, 0f),
                new Vector3(0.11f, armLen * 0.5f, 0.11f),
                sharedMaterial);
            CreatePart(root.transform, "RightArm", PrimitiveType.Capsule,
                new Vector3(0.22f + 0.12f, shoulder - armLen * 0.15f, 0f),
                new Vector3(0.11f, armLen * 0.5f, 0.11f),
                sharedMaterial);

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
            RuntimeSafeMaterials.ApplyColor(material, color);
        }

        public static Material CreateUrpCompatibleMaterial(Color color)
        {
            return RuntimeSafeMaterials.Create(color, "HeroPreviewDummyMat");
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
