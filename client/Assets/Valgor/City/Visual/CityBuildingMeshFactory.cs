using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Silhuetas medievais provisórias (base, paredes, telhado, torre, bandeira, porta).
    /// </summary>
    public static class CityBuildingMeshFactory
    {
        private static readonly Color Stone = new(0.48f, 0.46f, 0.44f);
        private static readonly Color StoneDark = new(0.36f, 0.34f, 0.32f);
        private static readonly Color Wood = new(0.32f, 0.22f, 0.12f);
        private static readonly Color RoofRed = new(0.55f, 0.22f, 0.16f);
        private static readonly Color RoofBlue = new(0.22f, 0.28f, 0.42f);
        private static readonly Color Gold = new(0.72f, 0.58f, 0.28f);
        private static readonly Color Door = new(0.22f, 0.14f, 0.08f);
        private static readonly Color Banner = new(0.55f, 0.18f, 0.16f);
        private static readonly Color Grass = new(0.28f, 0.42f, 0.24f);

        public static Bounds Build(string buildingId, Transform parent, Color color)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);

            switch (buildingId)
            {
                case "castle":
                    BuildCastle(visual.transform, color);
                    break;
                case "dragon-tower":
                    BuildDragonTower(visual.transform, color);
                    break;
                case "farm":
                    BuildFarm(visual.transform, color);
                    break;
                case "lumbermill":
                    BuildLumbermill(visual.transform, color);
                    break;
                case "quarry":
                    BuildQuarry(visual.transform, color);
                    break;
                case "mine":
                    BuildMine(visual.transform, color);
                    break;
                case "warehouse":
                    BuildWarehouse(visual.transform, color);
                    break;
                case "market":
                    BuildMarket(visual.transform, color);
                    break;
                case "temple":
                    BuildTemple(visual.transform, color);
                    break;
                case "hospital":
                    BuildHospital(visual.transform, color);
                    break;
                case "academy":
                    BuildAcademy(visual.transform, color);
                    break;
                case "institute":
                    BuildInstitute(visual.transform, color);
                    break;
                case "arena":
                    BuildArena(visual.transform, color);
                    break;
                case "laboratory":
                    BuildLaboratory(visual.transform, color);
                    break;
                default:
                    BuildHouse(visual.transform, Color.Lerp(color, Stone, 0.35f), RoofRed);
                    break;
            }

            return Encapsulate(visual.transform);
        }

        private static void BuildCastle(Transform root, Color color)
        {
            var stone = Color.Lerp(Stone, color, 0.25f);
            Plinth(root, 4.6f, 4.6f);
            Walls(root, new Vector3(0f, 1.5f, 0f), new Vector3(4.2f, 2.8f, 4.2f), stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 3.4f, 0f), new Vector3(2.8f, 1.4f, 2.8f), stone);
            CornerTower(root, -1.85f, -1.85f, stone, RoofRed);
            CornerTower(root, 1.85f, -1.85f, stone, RoofRed);
            CornerTower(root, -1.85f, 1.85f, stone, RoofBlue);
            CornerTower(root, 1.85f, 1.85f, stone, RoofBlue);
            Doorway(root, new Vector3(0f, 0.85f, 2.15f));
            Flag(root, new Vector3(0f, 5.2f, 0f), Gold);
            Battlement(root, 0f, 2.95f, 4.4f, stone);
            // Detalhes dourados na porta e cume.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.6f, 2.2f), new Vector3(0.9f, 0.12f, 0.12f), Gold);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.0f, 0f), new Vector3(1.2f, 0.15f, 1.2f), Gold);
        }

        private static void BuildDragonTower(Transform root, Color color)
        {
            var dark = Color.Lerp(StoneDark, color, 0.4f);
            var ember = new Color(0.75f, 0.32f, 0.14f);
            Plinth(root, 2.8f, 2.8f);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 2.2f, 0f), new Vector3(2.1f, 2.2f, 2.1f), dark);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 4.8f, 0f), new Vector3(1.55f, 1.4f, 1.55f), Color.Lerp(dark, color, 0.3f));
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 6.8f, 0f), new Vector3(1.0f, 1.0f, 1.0f), dark);
            PitchedRoof(root, new Vector3(0f, 8.0f, 0f), 1.6f, RoofBlue);
            Flag(root, new Vector3(0f, 9.0f, 0f), ember);
            Doorway(root, new Vector3(0f, 0.7f, 1.1f));

            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.25f, 2.4f), new Vector3(3.0f, 0.22f, 3.0f), Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.5f, 2.4f), new Vector3(2.4f, 0.18f, 2.4f), Wood);
            // Anéis de brasas no corpo da torre (marco secundário).
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 3.5f, 0f), new Vector3(2.25f, 0.12f, 2.25f), ember);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 5.6f, 0f), new Vector3(1.7f, 0.1f, 1.7f), Gold);
            var nestRoot = new GameObject("NestOccupants");
            nestRoot.transform.SetParent(root, false);
            nestRoot.transform.localPosition = new Vector3(0f, 0.8f, 2.4f);
        }

        private static void BuildFarm(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(color, Wood, 0.2f), RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.12f, -1.7f), new Vector3(3.2f, 0.18f, 1.6f), Grass);
            Part(PrimitiveType.Cube, root, new Vector3(1.3f, 0.45f, -1.7f), new Vector3(0.35f, 0.5f, 0.35f), new Color(0.22f, 0.5f, 0.2f));
            Part(PrimitiveType.Cube, root, new Vector3(-1.1f, 0.4f, -1.5f), new Vector3(0.3f, 0.4f, 0.3f), new Color(0.25f, 0.48f, 0.2f));
        }

        private static void BuildLumbermill(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(color, Wood, 0.35f), RoofRed);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.6f, 1.2f, 0.2f), new Vector3(0.4f, 1.1f, 0.4f), Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.5f, 0.45f, 0.9f), new Vector3(0.5f, 0.35f, 0.5f), Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.5f, 0.45f, -0.3f), new Vector3(0.5f, 0.35f, 0.5f), Wood);
            // Lâmina da serra (disco).
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.6f, 1.5f, 0.55f), new Vector3(1.1f, 0.08f, 1.1f),
                new Color(0.55f, 0.55f, 0.58f));
        }

        private static void BuildQuarry(Transform root, Color color)
        {
            Plinth(root, 3.2f, 2.8f);
            Part(PrimitiveType.Cube, root, new Vector3(-0.8f, 0.8f, 0.2f), new Vector3(1.8f, 1.4f, 1.6f), StoneDark);
            Part(PrimitiveType.Cube, root, new Vector3(0.9f, 1.2f, -0.3f), new Vector3(1.5f, 2.2f, 1.4f), Color.Lerp(Stone, color, 0.2f));
            Part(PrimitiveType.Cube, root, new Vector3(0.2f, 0.4f, 1.2f), new Vector3(1.1f, 0.7f, 1f), Stone);
            Flag(root, new Vector3(0.9f, 2.6f, -0.3f), Gold);
        }

        private static void BuildMine(Transform root, Color color)
        {
            Plinth(root, 2.8f, 2.6f);
            Walls(root, new Vector3(0f, 1.0f, 0.5f), new Vector3(2.4f, 1.8f, 1.8f), Color.Lerp(Stone, color, 0.2f));
            PitchedRoof(root, new Vector3(0f, 2.1f, 0.5f), 2.7f, RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.7f, -0.9f), new Vector3(1.5f, 1.2f, 1.3f), StoneDark);
            Doorway(root, new Vector3(0f, 0.55f, -1.5f));
        }

        private static void BuildWarehouse(Transform root, Color color)
        {
            Plinth(root, 3.8f, 2.6f);
            Walls(root, new Vector3(0f, 1.2f, 0f), new Vector3(3.4f, 2.2f, 2.2f), Color.Lerp(Wood, color, 0.25f));
            PitchedRoof(root, new Vector3(0f, 2.55f, 0f), 3.7f, RoofRed);
            Doorway(root, new Vector3(0f, 0.75f, 1.15f));
        }

        private static void BuildMarket(Transform root, Color color)
        {
            Plinth(root, 3.6f, 2.8f);
            Walls(root, new Vector3(0f, 0.75f, 0f), new Vector3(3.2f, 1.2f, 2.4f), Color.Lerp(Wood, color, 0.3f));
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.55f, 0f), new Vector3(3.5f, 0.22f, 2.7f), RoofRed);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.4f, 0.95f, 0f), new Vector3(0.18f, 0.9f, 0.18f), Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.4f, 0.95f, 0f), new Vector3(0.18f, 0.9f, 0.18f), Wood);
            Flag(root, new Vector3(0f, 2.1f, 0f), Banner);
        }

        private static void BuildTemple(Transform root, Color color)
        {
            Plinth(root, 3.0f, 3.0f);
            Walls(root, new Vector3(0f, 1.3f, 0f), new Vector3(2.6f, 2.4f, 2.6f), Color.Lerp(Stone, color, 0.25f));
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.2f, 1.2f, 1.2f), new Vector3(0.35f, 1.2f, 0.35f), Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(0.35f, 1.2f, 0.35f), Stone);
            PitchedRoof(root, new Vector3(0f, 2.8f, 0f), 2.9f, RoofBlue);
            Flag(root, new Vector3(0f, 3.7f, 0f), Gold);
            Doorway(root, new Vector3(0f, 0.8f, 1.35f));
        }

        private static void BuildHospital(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(Stone, color, 0.35f), RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.55f, 0f), new Vector3(1.0f, 0.28f, 0.28f), Banner);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.55f, 0f), new Vector3(0.28f, 0.28f, 1.0f), Banner);
        }

        private static void BuildAcademy(Transform root, Color color)
        {
            Plinth(root, 3.2f, 2.8f);
            Walls(root, new Vector3(0f, 1.4f, 0f), new Vector3(2.8f, 2.6f, 2.4f), Color.Lerp(Stone, color, 0.3f));
            Part(PrimitiveType.Cube, root, new Vector3(0f, 3.0f, 0f), new Vector3(1.6f, 1.0f, 1.6f), StoneDark);
            PitchedRoof(root, new Vector3(0f, 3.7f, 0f), 1.9f, RoofBlue);
            Flag(root, new Vector3(0f, 4.4f, 0f), Gold);
            Doorway(root, new Vector3(0f, 0.85f, 1.25f));
        }

        private static void BuildInstitute(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(Stone, color, 0.25f), RoofBlue);
            Part(PrimitiveType.Cube, root, new Vector3(0.9f, 1.8f, 1.15f), new Vector3(0.45f, 1.0f, 0.12f), new Color(0.55f, 0.7f, 0.82f));
        }

        private static void BuildArena(Transform root, Color color)
        {
            var sand = new Color(0.68f, 0.56f, 0.38f);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.3f, 0f), new Vector3(3.5f, 0.3f, 3.5f), sand);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.85f, 0f), new Vector3(3.7f, 0.5f, 3.7f), Color.Lerp(Stone, color, 0.25f));
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.4f, 0f), new Vector3(3.1f, 0.35f, 3.1f), StoneDark);
            Flag(root, new Vector3(0f, 2.2f, 1.6f), Banner);
            Flag(root, new Vector3(0f, 2.2f, -1.6f), Gold);
        }

        private static void BuildLaboratory(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(Stone, color, 0.3f), RoofBlue);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.7f, 0f), new Vector3(1.2f, 0.8f, 1.2f), new Color(0.4f, 0.65f, 0.7f));
            Part(PrimitiveType.Sphere, root, new Vector3(0f, 3.4f, 0f), new Vector3(0.55f, 0.55f, 0.55f), new Color(0.45f, 0.75f, 0.78f));
        }

        private static void BuildHouse(Transform root, Color wall, Color roof)
        {
            Plinth(root, 2.8f, 2.5f);
            Walls(root, new Vector3(0f, 1.05f, 0.2f), new Vector3(2.4f, 1.9f, 2.0f), wall);
            PitchedRoof(root, new Vector3(0f, 2.2f, 0.2f), 2.7f, roof);
            Doorway(root, new Vector3(0f, 0.7f, 1.25f));
        }

        private static void Plinth(Transform root, float x, float z) =>
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.12f, 0f), new Vector3(x, 0.24f, z), StoneDark);

        private static void Walls(Transform root, Vector3 pos, Vector3 scale, Color color) =>
            Part(PrimitiveType.Cube, root, pos, scale, color);

        private static void Doorway(Transform root, Vector3 pos) =>
            Part(PrimitiveType.Cube, root, pos, new Vector3(0.7f, 1.15f, 0.18f), Door);

        private static void PitchedRoof(Transform root, Vector3 center, float width, Color color)
        {
            // Telhado compacto (base + cume) — evita placas vermelhas “soltas”.
            Part(PrimitiveType.Cube, root, center, new Vector3(width, 0.22f, width * 0.72f), color);
            Part(PrimitiveType.Cube, root, center + Vector3.up * 0.28f,
                new Vector3(width * 0.55f, 0.28f, width * 0.45f), Color.Lerp(color, Color.black, 0.12f));
        }

        private static void CornerTower(Transform root, float x, float z, Color stone, Color roof)
        {
            Part(PrimitiveType.Cube, root, new Vector3(x, 3.5f, z), new Vector3(1.05f, 2.2f, 1.05f), stone);
            PitchedRoof(root, new Vector3(x, 4.75f, z), 1.25f, roof);
        }

        private static void Battlement(Transform root, float x, float y, float width, Color stone)
        {
            for (var i = -2; i <= 2; i++)
            {
                Part(PrimitiveType.Cube, root, new Vector3(x + i * 0.85f, y, width * 0.48f), new Vector3(0.45f, 0.45f, 0.35f), stone);
            }
        }

        private static void Flag(Transform root, Vector3 tip, Color color)
        {
            Part(PrimitiveType.Cylinder, root, tip + Vector3.down * 0.55f, new Vector3(0.08f, 0.55f, 0.08f), Wood);
            Part(PrimitiveType.Cube, root, tip + new Vector3(0.28f, -0.15f, 0f), new Vector3(0.55f, 0.35f, 0.06f), color);
        }

        private static GameObject Part(
            PrimitiveType type,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = type.ToString();
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
            return go;
        }

        private static Bounds Encapsulate(Transform visual)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.up, Vector3.one * 2f);
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var localCenter = visual.InverseTransformPoint(bounds.center);
            return new Bounds(localCenter, bounds.size);
        }
    }
}
