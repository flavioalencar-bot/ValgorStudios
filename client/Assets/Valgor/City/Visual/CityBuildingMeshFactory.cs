using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Modelos modulares provisórios. P0 (castle, dragon-tower, farm, warehouse, academy)
    /// com silhuetas reconhecíveis; demais edifícios mantêm kits básicos.
    /// </summary>
    public static class CityBuildingMeshFactory
    {
        private static Color Stone => CityVisualMaterials.StoneLight;
        private static Color StoneDark => CityVisualMaterials.StoneDark;
        private static Color Wood => CityVisualMaterials.Wood;
        private static Color RoofRed => CityVisualMaterials.RoofRed;
        private static Color RoofBlue => CityVisualMaterials.RoofBlue;
        private static Color Gold => CityVisualMaterials.Gold;
        private static Color Door => new(0.2f, 0.12f, 0.07f);
        private static Color Banner => new(0.55f, 0.16f, 0.14f);
        private static Color Grass => CityVisualMaterials.Vegetation;
        private static Color Dirt => CityVisualMaterials.Dirt;

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

        // ─── P0: Castelo ───────────────────────────────────────────────

        private static void BuildCastle(Transform root, Color color)
        {
            var stone = Color.Lerp(Stone, color, 0.2f);
            var dark = Color.Lerp(StoneDark, color, 0.15f);

            // Praça frontal (sul).
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.06f, 3.6f), new Vector3(5.4f, 0.12f, 3.2f),
                CityVisualMaterials.Path, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.1f, 3.6f), new Vector3(4.2f, 0.08f, 2.2f),
                Color.Lerp(CityVisualMaterials.Path, Gold, 0.08f), SurfaceKind.Stone);

            // Plataforma / muralhas baixas do pátio.
            Plinth(root, 5.6f, 5.2f);
            RingWall(root, 2.35f, 2.1f, 1.35f, dark);
            BattlementRing(root, 2.35f, 2.1f, 2.05f, stone);

            // Corpo principal (keep).
            Walls(root, new Vector3(0f, 1.85f, -0.15f), new Vector3(3.4f, 3.4f, 3.2f), stone, SurfaceKind.Stone);
            Walls(root, new Vector3(0f, 3.85f, -0.15f), new Vector3(2.6f, 1.1f, 2.5f), dark, SurfaceKind.Stone);
            Battlement(root, 0f, 4.45f, 2.7f, stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.7f, -0.15f), new Vector3(1.4f, 0.18f, 1.4f), Gold, SurfaceKind.Metal);

            // Torre principal central (mais alta).
            Part(PrimitiveType.Cube, root, new Vector3(0f, 5.6f, -0.15f), new Vector3(1.55f, 2.0f, 1.55f), stone, SurfaceKind.Stone);
            ConeRoof(root, new Vector3(0f, 6.85f, -0.15f), 1.9f, 1.1f, RoofBlue);
            Flag(root, new Vector3(0f, 8.1f, -0.15f), Gold);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 6.7f, -0.15f), new Vector3(0.55f, 0.12f, 0.55f), Gold, SurfaceKind.Metal);

            // Torres laterais (4 cantos, alturas distintas).
            KeepTower(root, -2.15f, -2.0f, 4.8f, stone, RoofBlue, true);
            KeepTower(root, 2.15f, -2.0f, 4.8f, stone, RoofBlue, true);
            KeepTower(root, -2.15f, 1.85f, 4.2f, dark, RoofRed, false);
            KeepTower(root, 2.15f, 1.85f, 4.2f, dark, RoofRed, false);

            // Portão monumental.
            Part(PrimitiveType.Cube, root, new Vector3(-1.15f, 1.35f, 2.55f), new Vector3(0.7f, 2.5f, 0.7f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.15f, 1.35f, 2.55f), new Vector3(0.7f, 2.5f, 0.7f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.7f, 2.55f), new Vector3(2.5f, 0.45f, 0.75f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.95f, 2.55f), new Vector3(1.2f, 0.18f, 0.4f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.1f, 2.7f), new Vector3(1.35f, 1.9f, 0.22f), Door, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.1f, 2.82f), new Vector3(0.18f, 1.7f, 0.08f), Gold, SurfaceKind.Metal);

            // Bandeiras nas torres do portão.
            Flag(root, new Vector3(-1.15f, 3.0f, 2.55f), Banner);
            Flag(root, new Vector3(1.15f, 3.0f, 2.55f), Banner);

            // Janelas / frestas.
            for (var i = -1; i <= 1; i++)
            {
                Part(PrimitiveType.Cube, root, new Vector3(i * 0.7f, 2.4f, 1.5f), new Vector3(0.28f, 0.55f, 0.12f),
                    new Color(0.12f, 0.14f, 0.22f), SurfaceKind.Plain);
            }
        }

        private static void KeepTower(Transform root, float x, float z, float height, Color stone, Color roof, bool blueBanner)
        {
            Part(PrimitiveType.Cube, root, new Vector3(x, height * 0.5f, z), new Vector3(1.25f, height, 1.25f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(x, height + 0.2f, z), new Vector3(1.4f, 0.35f, 1.4f), Color.Lerp(stone, StoneDark, 0.3f), SurfaceKind.Stone);
            ConeRoof(root, new Vector3(x, height + 0.55f, z), 1.55f, 0.95f, roof);
            Flag(root, new Vector3(x, height + 1.7f, z), blueBanner ? new Color(0.22f, 0.32f, 0.62f) : Banner);
            Part(PrimitiveType.Cube, root, new Vector3(x, height * 0.55f, z + 0.62f), new Vector3(0.3f, 0.45f, 0.1f),
                new Color(0.1f, 0.12f, 0.18f), SurfaceKind.Plain);
        }

        private static void RingWall(Transform root, float halfX, float halfZ, float height, Color color)
        {
            Part(PrimitiveType.Cube, root, new Vector3(0f, height * 0.5f, halfZ), new Vector3(halfX * 2f + 0.4f, height, 0.45f), color, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, height * 0.5f, -halfZ), new Vector3(halfX * 2f + 0.4f, height, 0.45f), color, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(halfX, height * 0.5f, 0f), new Vector3(0.45f, height, halfZ * 2f), color, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(-halfX, height * 0.5f, 0f), new Vector3(0.45f, height, halfZ * 2f), color, SurfaceKind.Stone);
            // Abertura do portão (sul): corta visualmente com portal mais escuro — já coberto pelo portão.
            Part(PrimitiveType.Cube, root, new Vector3(0f, height * 0.35f, halfZ), new Vector3(2.0f, height * 0.7f, 0.5f),
                Color.Lerp(color, Color.black, 0.35f), SurfaceKind.Stone);
        }

        private static void BattlementRing(Transform root, float halfX, float halfZ, float y, Color stone)
        {
            for (var i = -2; i <= 2; i++)
            {
                if (i == 0)
                {
                    continue; // abertura portão
                }

                Part(PrimitiveType.Cube, root, new Vector3(i * 0.9f, y, halfZ), new Vector3(0.4f, 0.4f, 0.35f), stone, SurfaceKind.Stone);
            }

            for (var i = -2; i <= 2; i++)
            {
                Part(PrimitiveType.Cube, root, new Vector3(i * 0.9f, y, -halfZ), new Vector3(0.4f, 0.4f, 0.35f), stone, SurfaceKind.Stone);
            }

            for (var i = -1; i <= 1; i++)
            {
                Part(PrimitiveType.Cube, root, new Vector3(halfX, y, i * 0.9f), new Vector3(0.35f, 0.4f, 0.4f), stone, SurfaceKind.Stone);
                Part(PrimitiveType.Cube, root, new Vector3(-halfX, y, i * 0.9f), new Vector3(0.35f, 0.4f, 0.4f), stone, SurfaceKind.Stone);
            }
        }

        // ─── P0: Torre dos Dragões ─────────────────────────────────────

        private static void BuildDragonTower(Transform root, Color color)
        {
            var dark = Color.Lerp(StoneDark, color, 0.35f);
            var mid = Color.Lerp(dark, color, 0.25f);
            var ember = new Color(0.82f, 0.35f, 0.12f);
            var crystal = new Color(0.45f, 0.55f, 0.85f);

            // Colina / base circular elevada.
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.35f, 0f), new Vector3(4.2f, 0.35f, 4.2f), Dirt, SurfaceKind.Dirt);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.7f, 0f), new Vector3(3.4f, 0.28f, 3.4f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.95f, 0f), new Vector3(2.8f, 0.22f, 2.8f), dark, SurfaceKind.Stone);

            // Fuste principal (mais alto que o castelo keep).
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 3.2f, 0f), new Vector3(2.35f, 2.2f, 2.35f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 5.8f, 0f), new Vector3(1.85f, 1.5f, 1.85f), mid, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 7.9f, 0f), new Vector3(1.25f, 1.15f, 1.25f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 9.5f, 0f), new Vector3(0.85f, 0.75f, 0.85f), mid, SurfaceKind.Stone);

            // Plataformas circulares.
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 4.5f, 0f), new Vector3(2.9f, 0.14f, 2.9f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 6.9f, 0f), new Vector3(2.25f, 0.12f, 2.25f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 8.7f, 0f), new Vector3(1.55f, 0.1f, 1.55f), Gold, SurfaceKind.Metal);

            // Anéis de brasa / brasões.
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 3.6f, 0f), new Vector3(2.5f, 0.1f, 2.5f), ember, SurfaceKind.Metal);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 5.5f, 0f), new Vector3(2.0f, 0.08f, 2.0f), ember, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.9f, 1.05f), new Vector3(0.55f, 0.7f, 0.12f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.9f, -1.05f), new Vector3(0.55f, 0.7f, 0.12f), Gold, SurfaceKind.Metal);

            // Cristais / brasas nos terraços.
            Part(PrimitiveType.Sphere, root, new Vector3(1.1f, 4.85f, 0.4f), Vector3.one * 0.35f, crystal, SurfaceKind.Metal);
            Part(PrimitiveType.Sphere, root, new Vector3(-1.0f, 4.85f, -0.3f), Vector3.one * 0.28f, ember, SurfaceKind.Metal);
            Part(PrimitiveType.Sphere, root, new Vector3(0.6f, 7.2f, 0.7f), Vector3.one * 0.22f, crystal, SurfaceKind.Metal);

            // Coroa / telhado cônico azul-profundo.
            ConeRoof(root, new Vector3(0f, 10.3f, 0f), 1.5f, 1.35f, RoofBlue);
            Flag(root, new Vector3(0f, 11.8f, 0f), ember);

            // Porta na base.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.55f, 1.35f), new Vector3(0.75f, 1.35f, 0.2f), Door, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.35f, 1.35f), new Vector3(0.9f, 0.15f, 0.22f), Gold, SurfaceKind.Metal);

            // Área de pouso (plataforma à frente) + âncora NestOccupants.
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.15f, 2.85f), new Vector3(3.4f, 0.2f, 3.4f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.35f, 2.85f), new Vector3(2.7f, 0.16f, 2.7f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.45f, 2.85f), new Vector3(1.6f, 0.08f, 1.6f), ember, SurfaceKind.Metal);

            var nestRoot = new GameObject("NestOccupants");
            nestRoot.transform.SetParent(root, false);
            nestRoot.transform.localPosition = new Vector3(0f, 1.7f, 2.85f);
        }

        // ─── P0: Fazenda ───────────────────────────────────────────────

        private static void BuildFarm(Transform root, Color color)
        {
            var wall = Color.Lerp(Wood, color, 0.25f);
            var crop = new Color(0.42f, 0.58f, 0.28f);
            var cropRipe = new Color(0.72f, 0.62f, 0.28f);

            // Páteo de terra.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.05f, 0f), new Vector3(5.2f, 0.1f, 4.6f), Dirt, SurfaceKind.Dirt);

            // Casa rural.
            Part(PrimitiveType.Cube, root, new Vector3(-1.35f, 0.95f, 0.9f), new Vector3(2.2f, 1.7f, 2.0f), wall, SurfaceKind.Wood);
            PitchedRoofLong(root, new Vector3(-1.35f, 2.05f, 0.9f), 2.5f, 2.3f, RoofRed);
            Doorway(root, new Vector3(-1.35f, 0.7f, 1.95f));
            Part(PrimitiveType.Cube, root, new Vector3(-0.7f, 1.35f, 1.95f), new Vector3(0.4f, 0.4f, 0.08f),
                new Color(0.45f, 0.65f, 0.75f), SurfaceKind.Plain);

            // Celeiro.
            Part(PrimitiveType.Cube, root, new Vector3(1.5f, 1.15f, 0.7f), new Vector3(2.0f, 2.1f, 2.4f),
                Color.Lerp(wall, Stone, 0.2f), SurfaceKind.Wood);
            PitchedRoofLong(root, new Vector3(1.5f, 2.45f, 0.7f), 2.3f, 2.7f, RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(1.5f, 0.85f, 1.95f), new Vector3(1.1f, 1.4f, 0.12f), Door, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(1.5f, 1.7f, 1.95f), new Vector3(0.35f, 0.35f, 0.1f), Gold, SurfaceKind.Metal);

            // Campos cultivados (sul).
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.12f, -1.55f), new Vector3(4.6f, 0.14f, 1.9f), Grass, SurfaceKind.Vegetation);
            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 5; col++)
                {
                    var ripe = (row + col) % 2 == 0;
                    Part(PrimitiveType.Cube, root,
                        new Vector3(-1.6f + col * 0.8f, 0.35f, -1.0f - row * 0.45f),
                        new Vector3(0.35f, 0.4f, 0.22f),
                        ripe ? cropRipe : crop, SurfaceKind.Vegetation);
                }
            }

            // Cercas.
            FenceLine(root, new Vector3(-2.4f, 0.35f, -1.55f), new Vector3(0.12f, 0.55f, 2.0f));
            FenceLine(root, new Vector3(2.4f, 0.35f, -1.55f), new Vector3(0.12f, 0.55f, 2.0f));
            FenceLine(root, new Vector3(0f, 0.35f, -2.5f), new Vector3(4.9f, 0.55f, 0.12f));

            // Sacos / caixas.
            Part(PrimitiveType.Cube, root, new Vector3(-0.2f, 0.35f, 1.6f), new Vector3(0.45f, 0.45f, 0.45f),
                new Color(0.55f, 0.45f, 0.28f), SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0.35f, 0.28f, 1.55f), new Vector3(0.4f, 0.35f, 0.4f),
                new Color(0.48f, 0.38f, 0.22f), SurfaceKind.Wood);
            Part(PrimitiveType.Sphere, root, new Vector3(-2.2f, 0.35f, 0.2f), Vector3.one * 0.45f,
                new Color(0.62f, 0.52f, 0.3f), SurfaceKind.Dirt);
            Part(PrimitiveType.Sphere, root, new Vector3(-2.0f, 0.3f, -0.15f), Vector3.one * 0.38f,
                new Color(0.55f, 0.48f, 0.28f), SurfaceKind.Dirt);

            // Arbusto.
            Part(PrimitiveType.Sphere, root, new Vector3(2.3f, 0.45f, -0.4f), Vector3.one * 0.7f, Grass, SurfaceKind.Vegetation);
        }

        private static void FenceLine(Transform root, Vector3 pos, Vector3 scale) =>
            Part(PrimitiveType.Cube, root, pos, scale, Wood, SurfaceKind.Wood);

        // ─── P0: Armazém ───────────────────────────────────────────────

        private static void BuildWarehouse(Transform root, Color color)
        {
            var wall = Color.Lerp(Wood, color, 0.3f);
            var beam = Color.Lerp(Wood, StoneDark, 0.25f);

            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.06f, 0f), new Vector3(5.0f, 0.12f, 3.8f), Dirt, SurfaceKind.Dirt);
            Plinth(root, 4.4f, 3.2f);

            // Corpo amplo.
            Walls(root, new Vector3(0f, 1.35f, 0f), new Vector3(4.0f, 2.5f, 2.8f), wall, SurfaceKind.Wood);
            // Reforços / vigas.
            Part(PrimitiveType.Cube, root, new Vector3(-1.3f, 1.35f, 1.42f), new Vector3(0.2f, 2.5f, 0.18f), beam, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(1.3f, 1.35f, 1.42f), new Vector3(0.2f, 2.5f, 0.18f), beam, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.5f, 1.42f), new Vector3(4.0f, 0.18f, 0.18f), beam, SurfaceKind.Wood);

            // Telhado reforçado (dupla água).
            PitchedRoofLong(root, new Vector3(0f, 2.9f, 0f), 4.5f, 3.2f, RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 3.15f, 0f), new Vector3(4.6f, 0.12f, 0.25f), StoneDark, SurfaceKind.Stone);

            // Portão de carga (largo).
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.0f, 1.48f), new Vector3(2.0f, 1.8f, 0.16f), Door, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.0f, 1.5f), new Vector3(2.2f, 0.2f, 0.2f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(-0.55f, 1.0f, 1.55f), new Vector3(0.12f, 1.5f, 0.08f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0.55f, 1.0f, 1.55f), new Vector3(0.12f, 1.5f, 0.08f), Gold, SurfaceKind.Metal);

            // Caixas e barris na frente / lado.
            Crate(root, new Vector3(-1.7f, 0.4f, 2.0f));
            Crate(root, new Vector3(-1.15f, 0.4f, 2.15f));
            Crate(root, new Vector3(-1.45f, 0.85f, 2.05f));
            Barrel(root, new Vector3(1.6f, 0.45f, 1.95f));
            Barrel(root, new Vector3(2.05f, 0.45f, 1.7f));
            Barrel(root, new Vector3(1.85f, 0.95f, 1.85f));

            // Área cercada.
            FenceLine(root, new Vector3(-2.35f, 0.4f, 0.2f), new Vector3(0.12f, 0.7f, 3.2f));
            FenceLine(root, new Vector3(2.35f, 0.4f, 0.2f), new Vector3(0.12f, 0.7f, 3.2f));
            FenceLine(root, new Vector3(0f, 0.4f, -1.55f), new Vector3(4.8f, 0.7f, 0.12f));

            Flag(root, new Vector3(0f, 3.7f, 0f), Banner);
        }

        private static void Crate(Transform root, Vector3 pos) =>
            Part(PrimitiveType.Cube, root, pos, new Vector3(0.5f, 0.5f, 0.5f),
                new Color(0.5f, 0.38f, 0.22f), SurfaceKind.Wood);

        private static void Barrel(Transform root, Vector3 pos) =>
            Part(PrimitiveType.Cylinder, root, pos, new Vector3(0.55f, 0.4f, 0.55f),
                new Color(0.4f, 0.28f, 0.16f), SurfaceKind.Wood);

        // ─── P0: Academia ──────────────────────────────────────────────

        private static void BuildAcademy(Transform root, Color color)
        {
            var stone = Color.Lerp(Stone, color, 0.35f);
            var mystic = Color.Lerp(RoofBlue, color, 0.25f);
            var rune = new Color(0.35f, 0.55f, 0.85f);

            Plinth(root, 4.0f, 3.6f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.08f, 2.2f), new Vector3(3.2f, 0.1f, 1.6f),
                CityVisualMaterials.Path, SurfaceKind.Stone);

            // Corpo de pedra (biblioteca).
            Walls(root, new Vector3(0f, 1.5f, 0f), new Vector3(3.2f, 2.8f, 2.6f), stone, SurfaceKind.Stone);
            Walls(root, new Vector3(0f, 3.2f, 0f), new Vector3(2.4f, 0.9f, 2.0f), Color.Lerp(stone, StoneDark, 0.25f), SurfaceKind.Stone);

            // Torre de estudo (lateral).
            Part(PrimitiveType.Cylinder, root, new Vector3(1.55f, 2.6f, -0.3f), new Vector3(1.35f, 2.4f, 1.35f),
                Color.Lerp(stone, mystic, 0.2f), SurfaceKind.Stone);
            ConeRoof(root, new Vector3(1.55f, 5.2f, -0.3f), 1.6f, 1.2f, RoofBlue);
            Flag(root, new Vector3(1.55f, 6.55f, -0.3f), new Color(0.25f, 0.35f, 0.72f));

            // Telhado principal azul.
            PitchedRoofLong(root, new Vector3(-0.4f, 3.85f, 0f), 2.8f, 2.4f, RoofBlue);
            Part(PrimitiveType.Cube, root, new Vector3(-0.4f, 4.05f, 0f), new Vector3(1.0f, 0.12f, 0.35f), Gold, SurfaceKind.Metal);

            // Colunas de entrada.
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.0f, 1.2f, 1.45f), new Vector3(0.35f, 1.2f, 0.35f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0.2f, 1.2f, 1.45f), new Vector3(0.35f, 1.2f, 0.35f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(-0.4f, 2.45f, 1.45f), new Vector3(1.8f, 0.25f, 0.45f), stone, SurfaceKind.Stone);
            Doorway(root, new Vector3(-0.4f, 0.9f, 1.4f));

            // Símbolos rúnicos / orbe.
            Part(PrimitiveType.Cube, root, new Vector3(-0.9f, 2.2f, 1.35f), new Vector3(0.35f, 0.45f, 0.08f), rune, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0.1f, 2.2f, 1.35f), new Vector3(0.35f, 0.45f, 0.08f), rune, SurfaceKind.Metal);
            Part(PrimitiveType.Sphere, root, new Vector3(-0.4f, 2.85f, 1.5f), Vector3.one * 0.28f, rune, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.6f, -1.35f), new Vector3(1.6f, 0.9f, 0.12f),
                new Color(0.2f, 0.22f, 0.32f), SurfaceKind.Plain);

            // Bandeiras azuis.
            Flag(root, new Vector3(-1.4f, 3.5f, 1.1f), new Color(0.22f, 0.32f, 0.7f));
            Flag(root, new Vector3(0.6f, 3.5f, 1.1f), new Color(0.22f, 0.32f, 0.7f));

            // Estantes sugeridas (volumes na lateral).
            Part(PrimitiveType.Cube, root, new Vector3(-1.7f, 1.4f, 0f), new Vector3(0.25f, 1.6f, 1.8f), Wood, SurfaceKind.Wood);
            for (var i = 0; i < 4; i++)
            {
                Part(PrimitiveType.Cube, root, new Vector3(-1.55f, 0.8f + i * 0.4f, -0.5f + (i % 2) * 0.3f),
                    new Vector3(0.12f, 0.3f, 0.45f),
                    i % 2 == 0 ? new Color(0.45f, 0.25f, 0.2f) : mystic, SurfaceKind.Plain);
            }
        }

        // ─── Demais (kits estáveis) ────────────────────────────────────

        private static void BuildLumbermill(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(color, Wood, 0.35f), RoofRed);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.6f, 1.2f, 0.2f), new Vector3(0.4f, 1.1f, 0.4f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.5f, 0.45f, 0.9f), new Vector3(0.5f, 0.35f, 0.5f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.5f, 0.45f, -0.3f), new Vector3(0.5f, 0.35f, 0.5f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.6f, 1.5f, 0.55f), new Vector3(1.1f, 0.08f, 1.1f),
                new Color(0.55f, 0.55f, 0.58f), SurfaceKind.Metal);
        }

        private static void BuildQuarry(Transform root, Color color)
        {
            Plinth(root, 3.2f, 2.8f);
            Part(PrimitiveType.Cube, root, new Vector3(-0.8f, 0.8f, 0.2f), new Vector3(1.8f, 1.4f, 1.6f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0.9f, 1.2f, -0.3f), new Vector3(1.5f, 2.2f, 1.4f),
                Color.Lerp(Stone, color, 0.2f), SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0.2f, 0.4f, 1.2f), new Vector3(1.1f, 0.7f, 1f), Stone, SurfaceKind.Stone);
            Flag(root, new Vector3(0.9f, 2.6f, -0.3f), Gold);
        }

        private static void BuildMine(Transform root, Color color)
        {
            Plinth(root, 2.8f, 2.6f);
            Walls(root, new Vector3(0f, 1.0f, 0.5f), new Vector3(2.4f, 1.8f, 1.8f), Color.Lerp(Stone, color, 0.2f), SurfaceKind.Stone);
            PitchedRoof(root, new Vector3(0f, 2.1f, 0.5f), 2.7f, RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.7f, -0.9f), new Vector3(1.5f, 1.2f, 1.3f), StoneDark, SurfaceKind.Stone);
            Doorway(root, new Vector3(0f, 0.55f, -1.5f));
        }

        private static void BuildMarket(Transform root, Color color)
        {
            Plinth(root, 3.6f, 2.8f);
            Walls(root, new Vector3(0f, 0.75f, 0f), new Vector3(3.2f, 1.2f, 2.4f), Color.Lerp(Wood, color, 0.3f), SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.55f, 0f), new Vector3(3.5f, 0.22f, 2.7f), RoofRed, SurfaceKind.Roof);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.4f, 0.95f, 0f), new Vector3(0.18f, 0.9f, 0.18f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.4f, 0.95f, 0f), new Vector3(0.18f, 0.9f, 0.18f), Wood, SurfaceKind.Wood);
            Flag(root, new Vector3(0f, 2.1f, 0f), Banner);
        }

        private static void BuildTemple(Transform root, Color color)
        {
            Plinth(root, 3.0f, 3.0f);
            Walls(root, new Vector3(0f, 1.3f, 0f), new Vector3(2.6f, 2.4f, 2.6f), Color.Lerp(Stone, color, 0.25f), SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.2f, 1.2f, 1.2f), new Vector3(0.35f, 1.2f, 0.35f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(0.35f, 1.2f, 0.35f), Stone, SurfaceKind.Stone);
            PitchedRoof(root, new Vector3(0f, 2.8f, 0f), 2.9f, RoofBlue);
            Flag(root, new Vector3(0f, 3.7f, 0f), Gold);
            Doorway(root, new Vector3(0f, 0.8f, 1.35f));
        }

        private static void BuildHospital(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(Stone, color, 0.35f), RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.55f, 0f), new Vector3(1.0f, 0.28f, 0.28f), Banner, SurfaceKind.Plain);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.55f, 0f), new Vector3(0.28f, 0.28f, 1.0f), Banner, SurfaceKind.Plain);
        }

        private static void BuildInstitute(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(Stone, color, 0.25f), RoofBlue);
            Part(PrimitiveType.Cube, root, new Vector3(0.9f, 1.8f, 1.15f), new Vector3(0.45f, 1.0f, 0.12f),
                new Color(0.55f, 0.7f, 0.82f), SurfaceKind.Plain);
        }

        private static void BuildArena(Transform root, Color color)
        {
            var sand = new Color(0.68f, 0.56f, 0.38f);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.3f, 0f), new Vector3(3.5f, 0.3f, 3.5f), sand, SurfaceKind.Dirt);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.85f, 0f), new Vector3(3.7f, 0.5f, 3.7f),
                Color.Lerp(Stone, color, 0.25f), SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.4f, 0f), new Vector3(3.1f, 0.35f, 3.1f), StoneDark, SurfaceKind.Stone);
            Flag(root, new Vector3(0f, 2.2f, 1.6f), Banner);
            Flag(root, new Vector3(0f, 2.2f, -1.6f), Gold);
        }

        private static void BuildLaboratory(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(Stone, color, 0.3f), RoofBlue);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.7f, 0f), new Vector3(1.2f, 0.8f, 1.2f),
                new Color(0.4f, 0.65f, 0.7f), SurfaceKind.Stone);
            Part(PrimitiveType.Sphere, root, new Vector3(0f, 3.4f, 0f), new Vector3(0.55f, 0.55f, 0.55f),
                new Color(0.45f, 0.75f, 0.78f), SurfaceKind.Metal);
        }

        private static void BuildHouse(Transform root, Color wall, Color roof)
        {
            Plinth(root, 2.8f, 2.5f);
            Walls(root, new Vector3(0f, 1.05f, 0.2f), new Vector3(2.4f, 1.9f, 2.0f), wall, SurfaceKind.Wood);
            PitchedRoof(root, new Vector3(0f, 2.2f, 0.2f), 2.7f, roof);
            Doorway(root, new Vector3(0f, 0.7f, 1.25f));
        }

        // ─── Helpers ───────────────────────────────────────────────────

        private static void Plinth(Transform root, float x, float z) =>
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.12f, 0f), new Vector3(x, 0.24f, z), StoneDark, SurfaceKind.Stone);

        private static void Walls(Transform root, Vector3 pos, Vector3 scale, Color color, SurfaceKind surface = SurfaceKind.Stone) =>
            Part(PrimitiveType.Cube, root, pos, scale, color, surface);

        private static void Doorway(Transform root, Vector3 pos) =>
            Part(PrimitiveType.Cube, root, pos, new Vector3(0.7f, 1.15f, 0.18f), Door, SurfaceKind.Wood);

        private static void PitchedRoof(Transform root, Vector3 center, float width, Color color)
        {
            Part(PrimitiveType.Cube, root, center, new Vector3(width, 0.22f, width * 0.72f), color, SurfaceKind.Roof);
            Part(PrimitiveType.Cube, root, center + Vector3.up * 0.28f,
                new Vector3(width * 0.55f, 0.28f, width * 0.45f), Color.Lerp(color, Color.black, 0.12f), SurfaceKind.Roof);
        }

        private static void PitchedRoofLong(Transform root, Vector3 center, float width, float depth, Color color)
        {
            Part(PrimitiveType.Cube, root, center, new Vector3(width, 0.25f, depth), color, SurfaceKind.Roof);
            Part(PrimitiveType.Cube, root, center + Vector3.up * 0.32f,
                new Vector3(width * 0.55f, 0.32f, depth * 0.55f), Color.Lerp(color, Color.black, 0.15f), SurfaceKind.Roof);
            // Águas inclinadas (aproximação).
            var wing = Part(PrimitiveType.Cube, root, center + new Vector3(0f, 0.15f, depth * 0.28f),
                new Vector3(width * 0.98f, 0.18f, depth * 0.42f), Color.Lerp(color, Color.black, 0.08f), SurfaceKind.Roof);
            wing.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            var wing2 = Part(PrimitiveType.Cube, root, center + new Vector3(0f, 0.15f, -depth * 0.28f),
                new Vector3(width * 0.98f, 0.18f, depth * 0.42f), Color.Lerp(color, Color.black, 0.08f), SurfaceKind.Roof);
            wing2.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
        }

        private static void ConeRoof(Transform root, Vector3 tip, float width, float height, Color color)
        {
            // Pirâmide aproximada com cubos empilhados (silhueta cônica).
            var layers = 4;
            for (var i = 0; i < layers; i++)
            {
                var t = i / (float)(layers - 1);
                var y = tip.y - height * (1f - t) * 0.85f;
                var w = Mathf.Lerp(width, 0.25f, t);
                Part(PrimitiveType.Cube, root, new Vector3(tip.x, y, tip.z), new Vector3(w, height / layers * 1.1f, w),
                    Color.Lerp(color, Color.black, t * 0.15f), SurfaceKind.Roof);
            }
        }

        private static void Battlement(Transform root, float x, float y, float width, Color stone)
        {
            for (var i = -2; i <= 2; i++)
            {
                Part(PrimitiveType.Cube, root, new Vector3(x + i * 0.55f, y, 0f), new Vector3(0.4f, 0.4f, width), stone, SurfaceKind.Stone);
            }
        }

        private static void Flag(Transform root, Vector3 tip, Color color)
        {
            Part(PrimitiveType.Cylinder, root, tip + Vector3.down * 0.55f, new Vector3(0.08f, 0.55f, 0.08f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, tip + new Vector3(0.32f, -0.12f, 0f), new Vector3(0.62f, 0.38f, 0.06f), color, SurfaceKind.Plain);
        }

        private static GameObject Part(
            PrimitiveType type,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            SurfaceKind surface = SurfaceKind.Plain)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = type.ToString();
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.ApplySurface(go.GetComponent<Renderer>(), color, surface);
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
