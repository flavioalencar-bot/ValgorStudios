using UnityEngine;

namespace Valgor.Heroes.Preview360
{
    /// <summary>
    /// Builds a simple humanoid dummy from primitives (provisional preview mesh).
    /// </summary>
    public static class HumanoidDummyFactory
    {
        public const string LayerName = "HeroPreview";
        public const string PrefabPath = "Assets/Valgor/Heroes/Prefabs/HumanoidDummy.prefab";

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

            CreatePart(root.transform, "Hips", PrimitiveType.Cube, new Vector3(0f, 0.95f, 0f), new Vector3(0.42f, 0.22f, 0.28f), sharedMaterial);
            CreatePart(root.transform, "Torso", PrimitiveType.Capsule, new Vector3(0f, 1.35f, 0f), new Vector3(0.38f, 0.38f, 0.28f), sharedMaterial);
            CreatePart(root.transform, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.92f, 0f), new Vector3(0.28f, 0.28f, 0.28f), sharedMaterial);
            CreatePart(root.transform, "LeftArm", PrimitiveType.Capsule, new Vector3(-0.42f, 1.35f, 0f), new Vector3(0.14f, 0.32f, 0.14f), sharedMaterial);
            CreatePart(root.transform, "RightArm", PrimitiveType.Capsule, new Vector3(0.42f, 1.35f, 0f), new Vector3(0.14f, 0.32f, 0.14f), sharedMaterial);
            CreatePart(root.transform, "LeftLeg", PrimitiveType.Capsule, new Vector3(-0.14f, 0.45f, 0f), new Vector3(0.16f, 0.42f, 0.16f), sharedMaterial);
            CreatePart(root.transform, "RightLeg", PrimitiveType.Capsule, new Vector3(0.14f, 0.45f, 0f), new Vector3(0.16f, 0.42f, 0.16f), sharedMaterial);

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
