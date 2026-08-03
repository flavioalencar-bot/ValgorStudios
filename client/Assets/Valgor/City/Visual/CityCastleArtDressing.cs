using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Vestimenta visual do Castelo (Art Pass 1.0) — não altera o prefab lógico.
    /// Fundação, escadaria, contrafortes, jardins e tochas em Ignore Raycast.
    /// </summary>
    public static class CityCastleArtDressing
    {
        public const string RootName = "CastleArtDressing";

        public static void Attach(Transform visualRoot)
        {
            if (visualRoot == null)
            {
                return;
            }

            if (visualRoot.Find(RootName) != null)
            {
                return;
            }

            var root = new GameObject(RootName).transform;
            root.SetParent(visualRoot, false);

            // Fundação ampla de pedra.
            Dec(root, PrimitiveType.Cube, "Foundation", new Vector3(0f, 0.06f, 0.2f),
                new Vector3(8.6f, 0.18f, 7.8f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "FoundationCap", new Vector3(0f, 0.18f, 0.15f),
                new Vector3(7.8f, 0.12f, 7.0f), CityArtMaterialCategory.StoneLight);

            // Escadaria principal (sul / frente iso).
            Dec(root, PrimitiveType.Cube, "StairBed", new Vector3(0f, 0.08f, 4.55f),
                new Vector3(4.6f, 0.1f, 2.8f), CityArtMaterialCategory.Dirt);
            Dec(root, PrimitiveType.Cube, "Stair1", new Vector3(0f, 0.22f, 4.35f),
                new Vector3(4.0f, 0.16f, 0.85f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "Stair2", new Vector3(0f, 0.4f, 3.85f),
                new Vector3(3.5f, 0.16f, 0.75f), CityArtMaterialCategory.StoneLight);
            Dec(root, PrimitiveType.Cube, "Stair3", new Vector3(0f, 0.58f, 3.4f),
                new Vector3(3.0f, 0.16f, 0.65f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "Stair4", new Vector3(0f, 0.76f, 3.0f),
                new Vector3(2.55f, 0.14f, 0.55f), CityArtMaterialCategory.StoneLight);

            // Contrafortes laterais.
            Buttress(root, -3.55f);
            Buttress(root, 3.55f);

            // Torres marcantes (silhueta extra — complementa prefab real).
            CornerAccent(root, new Vector3(-3.35f, 0f, -2.9f));
            CornerAccent(root, new Vector3(3.35f, 0f, -2.9f));
            CornerAccent(root, new Vector3(-3.35f, 0f, 2.7f), shorter: true);
            CornerAccent(root, new Vector3(3.35f, 0f, 2.7f), shorter: true);

            // Brasões / bandeiras.
            Banner(root, new Vector3(-2.15f, 2.7f, 3.25f));
            Banner(root, new Vector3(2.15f, 2.7f, 3.25f));
            Crest(root, new Vector3(0f, 3.55f, 3.2f));

            // Jardins nobres (laterais — fora do eixo imperial).
            Garden(root, new Vector3(-5.2f, 0.12f, 1.2f));
            Garden(root, new Vector3(5.2f, 0.12f, 1.2f));
            Garden(root, new Vector3(-4.6f, 0.12f, -2.4f), small: true);
            Garden(root, new Vector3(4.6f, 0.12f, -2.4f), small: true);

            // Muralhas internas / páteo (Art Pass 2) — não substitui o prefab.
            Dec(root, PrimitiveType.Cube, "InnerWallN", new Vector3(0f, 1.0f, -2.4f),
                new Vector3(5.8f, 1.6f, 0.35f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "InnerWallL", new Vector3(-2.9f, 1.0f, 0.2f),
                new Vector3(0.35f, 1.6f, 4.2f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "InnerWallR", new Vector3(2.9f, 1.0f, 0.2f),
                new Vector3(0.35f, 1.6f, 4.2f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "CourtPave", new Vector3(0f, 0.22f, 0.6f),
                new Vector3(4.6f, 0.1f, 3.8f), CityArtMaterialCategory.StoneLight);

            if (CityAmbientQualitySettings.AllowTorches)
            {
                Torch(root, new Vector3(-1.7f, 0.9f, 4.4f));
                Torch(root, new Vector3(1.7f, 0.9f, 4.4f));
                Torch(root, new Vector3(-3.6f, 1.6f, 2.4f));
                Torch(root, new Vector3(3.6f, 1.6f, 2.4f));
            }
        }

        private static void Buttress(Transform root, float x)
        {
            Dec(root, PrimitiveType.Cube, "Buttress", new Vector3(x, 1.1f, 0.3f),
                new Vector3(0.55f, 2.0f, 1.8f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cube, "ButtressCap", new Vector3(x, 2.2f, 0.3f),
                new Vector3(0.7f, 0.28f, 2.0f), CityArtMaterialCategory.StoneLight);
        }

        private static void CornerAccent(Transform root, Vector3 basePos, bool shorter = false)
        {
            var h = shorter ? 3.2f : 3.8f;
            Dec(root, PrimitiveType.Cylinder, "TowerAccent", basePos + new Vector3(0f, h * 0.5f, 0f),
                new Vector3(1.05f, h * 0.5f, 1.05f), CityArtMaterialCategory.StoneLight);
            Dec(root, PrimitiveType.Cylinder, "TowerAccentCap", basePos + new Vector3(0f, h + 0.15f, 0f),
                new Vector3(1.25f, 0.18f, 1.25f), CityArtMaterialCategory.StoneDark);
            Dec(root, PrimitiveType.Cylinder, "TowerAccentRoof", basePos + new Vector3(0f, h + 0.55f, 0f),
                new Vector3(0.95f, 0.45f, 0.95f), CityArtMaterialCategory.Roof);
            Banner(root, basePos + new Vector3(0.35f, h + 0.35f, 0.35f));
        }

        private static void Garden(Transform root, Vector3 pos, bool small = false)
        {
            var s = small ? 1.4f : 2.0f;
            Dec(root, PrimitiveType.Cube, "GardenBed", pos, new Vector3(s, 0.22f, s * 0.85f),
                CityArtMaterialCategory.Dirt);
            Dec(root, PrimitiveType.Cube, "GardenHedge", pos + Vector3.up * 0.22f,
                new Vector3(s * 0.9f, 0.35f, s * 0.75f), CityArtMaterialCategory.Grass);
            Dec(root, PrimitiveType.Sphere, "GardenBush", pos + new Vector3(0.2f, 0.45f, 0f),
                Vector3.one * (small ? 0.45f : 0.6f), CityArtMaterialCategory.Grass);
        }

        private static void Banner(Transform root, Vector3 tip)
        {
            Dec(root, PrimitiveType.Cylinder, "BannerPole", tip + Vector3.down * 0.45f,
                new Vector3(0.07f, 0.45f, 0.07f), CityArtMaterialCategory.Wood);
            var cloth = Dec(root, PrimitiveType.Cube, "BannerCloth", tip + new Vector3(0.22f, -0.1f, 0f),
                new Vector3(0.42f, 0.3f, 0.05f), CityArtMaterialCategory.Metal);
            CityArtMaterialLibrary.ApplyVariant(cloth.GetComponent<Renderer>(), CityArtMaterialCategory.Roof,
                new Color(0.55f, 0.14f, 0.14f), 0.85f);
        }

        private static void Crest(Transform root, Vector3 pos)
        {
            Dec(root, PrimitiveType.Cube, "CrestPlate", pos, new Vector3(0.85f, 0.95f, 0.12f),
                CityArtMaterialCategory.Metal);
            Dec(root, PrimitiveType.Cube, "CrestGem", pos + new Vector3(0f, 0.05f, 0.08f),
                new Vector3(0.35f, 0.35f, 0.08f), CityArtMaterialCategory.Metal);
        }

        private static void Torch(Transform root, Vector3 pos)
        {
            Dec(root, PrimitiveType.Cylinder, "TorchPole", pos,
                new Vector3(0.1f, 0.4f, 0.1f), CityArtMaterialCategory.Wood);
            var flame = Dec(root, PrimitiveType.Sphere, "TorchFlame", pos + Vector3.up * 0.48f,
                Vector3.one * 0.28f, CityArtMaterialCategory.Metal);
            CityVisualMaterials.ApplyEmissiveHint(flame.GetComponent<Renderer>(),
                new Color(1f, 0.55f, 0.15f), 0.55f);
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
