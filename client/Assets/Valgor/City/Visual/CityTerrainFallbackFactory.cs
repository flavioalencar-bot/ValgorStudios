using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>Fallback procedural tipado por AssetKey do kit (enquanto Tripo/HF não chega).</summary>
    public static class CityTerrainFallbackFactory
    {
        public static GameObject Create(string assetKey, Transform parent)
        {
            var root = new GameObject(assetKey + "_Fallback");
            root.transform.SetParent(parent, false);

            if (!CityTerrainAssetCatalog.TryGet(assetKey, out var def))
            {
                MakeRock(root.transform, CityVisualMaterials.StoneDark, 1f);
                return root;
            }

            switch (def.Category)
            {
                case CityTerrainAssetCategory.DistantMountain:
                    MakeMountain(root.transform, soft: true);
                    break;
                case CityTerrainAssetCategory.Mountain:
                    MakeMountain(root.transform, soft: false);
                    break;
                case CityTerrainAssetCategory.Cliff:
                    MakeCliff(root.transform, assetKey.Contains("Dragon"));
                    break;
                case CityTerrainAssetCategory.Plateau:
                    MakePlateau(root.transform);
                    break;
                case CityTerrainAssetCategory.Valley:
                    MakeValley(root.transform);
                    break;
                case CityTerrainAssetCategory.River:
                case CityTerrainAssetCategory.Lake:
                    MakeWaterChannel(root.transform);
                    break;
                case CityTerrainAssetCategory.RiverBank:
                    MakeBank(root.transform);
                    break;
                case CityTerrainAssetCategory.Waterfall:
                    MakeWaterfall(root.transform);
                    break;
                case CityTerrainAssetCategory.Bridge:
                    MakeBridge(root.transform, assetKey.StartsWith("Wood"));
                    break;
                case CityTerrainAssetCategory.Rock:
                    MakeRock(root.transform, CityVisualMaterials.StoneDark, assetKey.Contains("Small") ? 0.7f : 1f);
                    break;
                case CityTerrainAssetCategory.Slope:
                case CityTerrainAssetCategory.Embankment:
                    MakeSlope(root.transform);
                    break;
                default:
                    MakeRock(root.transform, CityVisualMaterials.StoneDark, 1f);
                    break;
            }

            return root;
        }

        private static void MakeMountain(Transform root, bool soft)
        {
            // Silhueta em blocos (não esfera) — legível em câmera isométrica.
            var rock = soft
                ? Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Vegetation, 0.2f)
                : CityVisualMaterials.StoneDark;
            var mid = Color.Lerp(rock, CityVisualMaterials.StoneLight, 0.25f);
            Part(root, PrimitiveType.Cube, "Base", new Vector3(0f, 0.35f, 0f), new Vector3(1.2f, 0.7f, 1.0f), rock);
            Part(root, PrimitiveType.Cube, "Mid", new Vector3(0.05f, 0.85f, -0.05f), new Vector3(0.85f, 0.7f, 0.75f), mid);
            Part(root, PrimitiveType.Cube, "Peak", new Vector3(-0.05f, 1.35f, 0.05f), new Vector3(0.45f, 0.65f, 0.4f), rock);
            if (!soft)
            {
                Part(root, PrimitiveType.Cube, "SpurL", new Vector3(-0.45f, 0.55f, 0.15f), new Vector3(0.4f, 0.9f, 0.35f), mid);
                Part(root, PrimitiveType.Cube, "SpurR", new Vector3(0.5f, 0.45f, -0.2f), new Vector3(0.35f, 0.7f, 0.4f), rock);
            }
        }

        private static void MakeCliff(Transform root, bool dragon)
        {
            Part(root, PrimitiveType.Cube, "Face", new Vector3(0f, 0.5f, 0f), new Vector3(1.1f, 1f, 0.35f),
                CityVisualMaterials.StoneDark);
            Part(root, PrimitiveType.Cube, "Cap", new Vector3(0f, 1.05f, 0.15f), new Vector3(1.0f, 0.12f, 0.7f),
                CityVisualMaterials.StoneLight);
            if (dragon)
            {
                Part(root, PrimitiveType.Cube, "Pad", new Vector3(0f, 1.12f, 0.2f), new Vector3(0.7f, 0.08f, 0.55f),
                    Color.Lerp(CityVisualMaterials.StoneLight, CityVisualMaterials.Dirt, 0.2f));
                for (var i = 0; i < 5; i++)
                {
                    Part(root, PrimitiveType.Cube, $"Stair{i}",
                        new Vector3(0f, 0.18f + i * 0.16f, -0.55f - i * 0.08f),
                        new Vector3(0.45f - i * 0.04f, 0.1f, 0.18f),
                        i % 2 == 0 ? CityVisualMaterials.StoneDark : CityVisualMaterials.StoneLight);
                }
            }
        }

        private static void MakePlateau(Transform root)
        {
            Part(root, PrimitiveType.Cube, "Top", new Vector3(0f, 0.05f, 0f), new Vector3(1f, 0.1f, 1f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Vegetation, 0.35f));
            Part(root, PrimitiveType.Cube, "Rim", new Vector3(0f, 0.02f, 0f), new Vector3(1.05f, 0.06f, 1.05f),
                CityVisualMaterials.StoneDark);
        }

        private static void MakeValley(Transform root)
        {
            Part(root, PrimitiveType.Cube, "Floor", Vector3.zero, new Vector3(1f, 0.06f, 1f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.25f));
            Part(root, PrimitiveType.Cube, "SideL", new Vector3(-0.45f, 0.2f, 0f), new Vector3(0.15f, 0.4f, 1f),
                CityVisualMaterials.StoneDark);
            Part(root, PrimitiveType.Cube, "SideR", new Vector3(0.45f, 0.2f, 0f), new Vector3(0.15f, 0.4f, 1f),
                CityVisualMaterials.StoneDark);
        }

        private static void MakeWaterChannel(Transform root)
        {
            // Leito escavado + água fina + margens — sem bloco azul sólido.
            Part(root, PrimitiveType.Cube, "Bed", new Vector3(0f, -0.04f, 0f), new Vector3(1.05f, 0.08f, 1.05f),
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.35f));
            Part(root, PrimitiveType.Cube, "BankL", new Vector3(-0.48f, 0.06f, 0f), new Vector3(0.14f, 0.16f, 1.02f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Vegetation, 0.4f));
            Part(root, PrimitiveType.Cube, "BankR", new Vector3(0.48f, 0.06f, 0f), new Vector3(0.14f, 0.16f, 1.02f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Vegetation, 0.35f));

            var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.name = "Water";
            water.transform.SetParent(root, false);
            water.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            water.transform.localScale = new Vector3(0.55f, 0.04f, 0.88f);
            Object.Destroy(water.GetComponent<Collider>());
            // Força material seguro (evita ciano default URP Lit + luz).
            water.GetComponent<Renderer>().sharedMaterial =
                Valgor.Core.RuntimeSafeMaterials.Create(new Color(0.08f, 0.22f, 0.3f), "CapitalRiverWater");
        }

        private static void MakeBank(Transform root)
        {
            Part(root, PrimitiveType.Cube, "Bank", Vector3.zero, new Vector3(1f, 0.12f, 1f),
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.4f));
        }

        private static void MakeWaterfall(Transform root)
        {
            Part(root, PrimitiveType.Cube, "Rock", new Vector3(0f, 0.5f, 0f), new Vector3(0.9f, 1f, 0.5f),
                CityVisualMaterials.StoneDark);
            Part(root, PrimitiveType.Cube, "Sheet", new Vector3(0f, 0.45f, -0.28f), new Vector3(0.55f, 0.9f, 0.08f),
                Color.Lerp(new Color(0.22f, 0.38f, 0.48f), Color.white, 0.35f));
            Part(root, PrimitiveType.Cube, "Basin", new Vector3(0f, 0.05f, -0.45f), new Vector3(0.8f, 0.08f, 0.45f),
                CityVisualMaterials.StoneLight);
        }

        private static void MakeBridge(Transform root, bool wood)
        {
            var deck = wood ? CityVisualMaterials.Wood : CityVisualMaterials.StoneLight;
            var rail = wood ? Color.Lerp(CityVisualMaterials.Wood, Color.black, 0.28f) : CityVisualMaterials.StoneDark;
            Part(root, PrimitiveType.Cube, "Deck", new Vector3(0f, 0.2f, 0f), new Vector3(1f, 0.12f, 0.55f), deck);
            Part(root, PrimitiveType.Cube, "RailL", new Vector3(0f, 0.4f, 0.28f), new Vector3(1f, 0.2f, 0.06f), rail);
            Part(root, PrimitiveType.Cube, "RailR", new Vector3(0f, 0.4f, -0.28f), new Vector3(1f, 0.2f, 0.06f), rail);
            Part(root, PrimitiveType.Cube, "Arch", new Vector3(0f, 0.05f, 0f), new Vector3(0.35f, 0.25f, 0.5f), rail);
        }

        private static void MakeRock(Transform root, Color color, float s)
        {
            Part(root, PrimitiveType.Cube, "Rock", Vector3.up * 0.28f * s,
                new Vector3(0.9f * s, 0.55f * s, 0.75f * s), color);
            Part(root, PrimitiveType.Cube, "RockB", new Vector3(0.2f * s, 0.18f * s, -0.15f * s),
                new Vector3(0.55f * s, 0.4f * s, 0.5f * s),
                Color.Lerp(color, CityVisualMaterials.StoneLight, 0.2f));
        }

        private static void MakeSlope(Transform root)
        {
            Part(root, PrimitiveType.Cube, "Slope", new Vector3(0f, 0.15f, 0f), new Vector3(1f, 0.3f, 1f),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Vegetation, 0.3f));
        }

        private static void Part(
            Transform parent,
            PrimitiveType type,
            string name,
            Vector3 localPos,
            Vector3 scale,
            Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
        }
    }
}
