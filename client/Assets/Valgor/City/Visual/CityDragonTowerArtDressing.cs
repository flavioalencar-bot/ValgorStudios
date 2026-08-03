using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Vestimenta visual da Torre dos Dragões — platô, penhasco, escada, pedras, vegetação.
    /// Não altera BuildingId, Save, seleção, DragonNestView nem gameplay.
    /// </summary>
    public static class CityDragonTowerArtDressing
    {
        public const string RootName = "ArtDressing_DragonTower";

        /// <summary>Slot Y≈1.35 + local → alinha com platô distrito (top≈2.15).</summary>
        public const float PlateauTopY = 0.75f;

        public static void Attach(Transform visualRoot)
        {
            if (visualRoot == null)
            {
                return;
            }

            var existing = visualRoot.Find(RootName);
            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
            }

            var root = new GameObject(RootName).transform;
            root.SetParent(visualRoot, false);

            // Blend solo → rocha (anéis irregulares).
            Dec(root, PrimitiveType.Cylinder, "GroundBlend", new Vector3(0f, 0.05f, 0.6f),
                new Vector3(11f, 0.1f, 11f), CityArtMaterialCategory.Dirt);
            Dec(root, PrimitiveType.Cylinder, "MossRing", new Vector3(0.3f, 0.1f, 0.4f),
                new Vector3(9.2f, 0.12f, 9.0f), CityArtMaterialCategory.Grass);

            // Massa do platô (vários blocos — evita “disco” limpo).
            Dec(root, PrimitiveType.Cube, "PlateauMassA", new Vector3(0f, 0.7f, 0.2f),
                new Vector3(6.4f, 1.4f, 5.8f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "PlateauMassB", new Vector3(-1.2f, 0.55f, 1.4f),
                new Vector3(3.2f, 1.1f, 3.0f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "PlateauMassC", new Vector3(1.4f, 0.5f, -0.8f),
                new Vector3(2.8f, 1.0f, 2.6f), CityArtMaterialCategory.StoneLight);
            Dec(root, PrimitiveType.Cylinder, "PlateauCap", new Vector3(0f, PlateauTopY, 0.1f),
                new Vector3(4.2f, 0.22f, 4.0f), CityArtMaterialCategory.StoneLight);

            // Penhasco sul (face à Capital) — alto e irregular.
            Dec(root, PrimitiveType.Cube, "CliffMain", new Vector3(0f, 0.95f, -3.6f),
                new Vector3(7.2f, 2.2f, 1.6f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "CliffLeft", new Vector3(-2.8f, 0.7f, -4.2f),
                new Vector3(2.4f, 1.6f, 1.8f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "CliffRight", new Vector3(2.6f, 0.65f, -4.0f),
                new Vector3(2.2f, 1.5f, 1.7f), CityArtMaterialCategory.StoneLight);
            Dec(root, PrimitiveType.Cube, "CliffLedge", new Vector3(0f, 1.7f, -2.9f),
                new Vector3(5.0f, 0.35f, 1.0f), CityArtMaterialCategory.StoneLight);
            Dec(root, PrimitiveType.Cube, "CliffBase", new Vector3(0f, 0.2f, -4.8f),
                new Vector3(8.0f, 0.5f, 1.4f), CityArtMaterialCategory.Dirt);

            // Escadaria ceremonial (sul → platô).
            Dec(root, PrimitiveType.Cube, "StairBed", new Vector3(0f, 0.1f, -6.2f),
                new Vector3(3.6f, 0.14f, 4.2f), CityArtMaterialCategory.Dirt);
            Stair(root, 1, -6.4f, 0.28f, 3.3f);
            Stair(root, 2, -5.85f, 0.5f, 3.0f);
            Stair(root, 3, -5.35f, 0.72f, 2.7f);
            Stair(root, 4, -4.9f, 0.94f, 2.45f);
            Stair(root, 5, -4.5f, 1.16f, 2.2f);
            Stair(root, 6, -4.15f, 1.35f, 2.0f);
            Dec(root, PrimitiveType.Cube, "RailL", new Vector3(-1.75f, 1.0f, -5.2f),
                new Vector3(0.16f, 1.3f, 3.4f), CityArtMaterialCategory.StoneLight);
            Dec(root, PrimitiveType.Cube, "RailR", new Vector3(1.75f, 1.0f, -5.2f),
                new Vector3(0.16f, 1.3f, 3.4f), CityArtMaterialCategory.StoneLight);

            // Pedras soltas.
            Rock(root, new Vector3(-4.0f, 0.45f, 1.8f), new Vector3(1.6f, 0.9f, 1.3f));
            Rock(root, new Vector3(3.8f, 0.4f, 1.4f), new Vector3(1.4f, 0.75f, 1.5f));
            Rock(root, new Vector3(-3.2f, 0.55f, -1.6f), new Vector3(1.3f, 1.1f, 1.1f));
            Rock(root, new Vector3(3.4f, 0.4f, -2.2f), new Vector3(1.2f, 0.7f, 1.4f));
            Rock(root, new Vector3(-5.0f, 0.28f, -0.4f), new Vector3(1.0f, 0.55f, 0.9f));
            Rock(root, new Vector3(4.8f, 0.25f, 0.2f), new Vector3(0.95f, 0.5f, 1.0f));
            Rock(root, new Vector3(0.8f, 0.35f, 3.8f), new Vector3(1.2f, 0.65f, 1.1f));
            Rock(root, new Vector3(-1.6f, 0.3f, 4.0f), new Vector3(0.9f, 0.5f, 0.85f));
            Rock(root, new Vector3(-2.2f, 0.22f, -5.6f), new Vector3(0.8f, 0.45f, 0.7f));
            Rock(root, new Vector3(2.0f, 0.2f, -5.8f), new Vector3(0.75f, 0.4f, 0.8f));

            // Vegetação.
            Bush(root, new Vector3(-5.4f, 0.4f, 2.4f), 0.85f);
            Bush(root, new Vector3(5.2f, 0.38f, 2.0f), 0.8f);
            Bush(root, new Vector3(-4.6f, 0.35f, -3.6f), 0.7f);
            Bush(root, new Vector3(4.4f, 0.32f, -3.8f), 0.65f);
            Bush(root, new Vector3(-6.0f, 0.28f, 0.6f), 0.55f);
            Bush(root, new Vector3(5.8f, 0.28f, -0.8f), 0.58f);
            Bush(root, new Vector3(-2.2f, 0.4f, 4.8f), 0.5f);
            Bush(root, new Vector3(2.4f, 0.38f, 4.6f), 0.52f);
            Bush(root, new Vector3(-3.5f, 0.3f, -6.0f), 0.45f);
            Bush(root, new Vector3(3.2f, 0.28f, -6.2f), 0.48f);
        }

        public static void PlaceHighFidelityOnPlateau(Transform artHf)
        {
            if (artHf == null)
            {
                return;
            }

            artHf.localPosition = new Vector3(0f, PlateauTopY, 0.15f);
            artHf.localRotation = Quaternion.identity;
        }

        private static void Stair(Transform root, int index, float z, float y, float width)
        {
            var cat = index % 2 == 0 ? CityArtMaterialCategory.StoneLight : CityArtMaterialCategory.StoneDark;
            Dec(root, PrimitiveType.Cube, "Stair" + index, new Vector3(0f, y, z),
                new Vector3(width, 0.2f, 0.7f), cat);
        }

        private static void Rock(Transform root, Vector3 pos, Vector3 scale)
        {
            Dec(root, PrimitiveType.Cube, "Rock", pos, scale, CityArtMaterialCategory.StoneDark);
        }

        private static void Bush(Transform root, Vector3 pos, float size)
        {
            Dec(root, PrimitiveType.Sphere, "Bush", pos, Vector3.one * size, CityArtMaterialCategory.Grass);
            Dec(root, PrimitiveType.Cube, "Tuft", pos + new Vector3(0.12f, -0.05f, 0.08f),
                new Vector3(size * 0.75f, size * 0.4f, size * 0.75f), CityArtMaterialCategory.Grass);
        }

        private static GameObject Dec(
            Transform parent,
            PrimitiveType type,
            string name,
            Vector3 position,
            Vector3 scale,
            CityArtMaterialCategory category)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            var ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                go.layer = ignore;
            }

            CityArtMaterialLibrary.Apply(go.GetComponent<Renderer>(), category);
            return go;
        }
    }
}
