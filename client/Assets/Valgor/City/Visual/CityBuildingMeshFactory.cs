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
                case "wall":
                    BuildWallGatehouse(visual.transform, color);
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

        // ─── P0: Castelo (dominante, proporções medievais) ──────────────

        private static void BuildCastle(Transform root, Color color)
        {
            var stone = Color.Lerp(Stone, color, 0.18f);
            var dark = Color.Lerp(StoneDark, color, 0.12f);

            // Escadaria / praça frontal.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.08f, 4.0f), new Vector3(5.0f, 0.14f, 3.4f),
                CityVisualMaterials.Path, SurfaceKind.Path);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.22f, 2.85f), new Vector3(3.6f, 0.2f, 1.1f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.4f, 2.35f), new Vector3(3.2f, 0.2f, 0.9f), stone, SurfaceKind.Stone);

            // Base larga do pátio.
            Plinth(root, 6.2f, 5.6f);

            // Muralhas laterais (volumes contínuos — menos blocos).
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.15f, -2.35f), new Vector3(5.6f, 2.1f, 0.55f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(-2.75f, 1.15f, 0f), new Vector3(0.55f, 2.1f, 4.6f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(2.75f, 1.15f, 0f), new Vector3(0.55f, 2.1f, 4.6f), dark, SurfaceKind.Stone);
            // Frente aberta no portão (alas).
            Part(PrimitiveType.Cube, root, new Vector3(-2.05f, 1.15f, 2.35f), new Vector3(1.6f, 2.1f, 0.55f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(2.05f, 1.15f, 2.35f), new Vector3(1.6f, 2.1f, 0.55f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.35f, 2.35f), new Vector3(2.6f, 0.4f, 0.55f), stone, SurfaceKind.Stone);

            // Keep / torre central dominante.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.4f, -0.2f), new Vector3(3.0f, 4.2f, 2.8f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.7f, -0.2f), new Vector3(2.4f, 0.85f, 2.2f), dark, SurfaceKind.Stone);
            ConeRoof(root, new Vector3(0f, 5.85f, -0.2f), 2.5f, 1.35f, RoofBlue);
            SmallFlag(root, new Vector3(0f, 7.35f, -0.2f), Gold);

            // Quatro torres de canto (menores que o keep).
            CornerKeep(root, -2.55f, -2.15f, 3.6f, stone, RoofBlue);
            CornerKeep(root, 2.55f, -2.15f, 3.6f, stone, RoofBlue);
            CornerKeep(root, -2.55f, 1.95f, 3.2f, dark, RoofRed);
            CornerKeep(root, 2.55f, 1.95f, 3.2f, dark, RoofRed);

            // Portão frontal.
            Part(PrimitiveType.Cube, root, new Vector3(-1.25f, 1.35f, 2.55f), new Vector3(0.7f, 2.5f, 0.7f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.25f, 1.35f, 2.55f), new Vector3(0.7f, 2.5f, 0.7f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.15f, 2.72f), new Vector3(1.55f, 2.0f, 0.22f), Door, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.75f, 2.6f), new Vector3(1.0f, 0.16f, 0.35f), Gold, SurfaceKind.Metal);

            // Ameias só no keep (contínuas).
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.55f, 1.05f), new Vector3(2.6f, 0.35f, 0.28f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.55f, -1.4f), new Vector3(2.6f, 0.35f, 0.28f), stone, SurfaceKind.Stone);
        }

        private static void CornerKeep(Transform root, float x, float z, float height, Color stone, Color roof)
        {
            Part(PrimitiveType.Cube, root, new Vector3(x, height * 0.5f, z), new Vector3(1.15f, height, 1.15f), stone, SurfaceKind.Stone);
            ConeRoof(root, new Vector3(x, height + 0.15f, z), 1.35f, 0.85f, roof);
            SmallFlag(root, new Vector3(x, height + 1.15f, z), Banner);
        }

        // ─── P0: Torre dos Dragões (circular, distinta do Castelo) ──────

        private static void BuildDragonTower(Transform root, Color color)
        {
            var dark = Color.Lerp(StoneDark, color, 0.3f);
            var mid = Color.Lerp(dark, Stone, 0.25f);
            var ember = new Color(0.85f, 0.38f, 0.12f);
            var crystal = new Color(0.4f, 0.55f, 0.9f);

            // Base de pedra escura (colina + anéis).
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.4f, 0f), new Vector3(4.0f, 0.4f, 4.0f), Dirt, SurfaceKind.Dirt);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.85f, 0f), new Vector3(3.2f, 0.3f, 3.2f), dark, SurfaceKind.Stone);

            // Fuste circular contínuo (poucas seções).
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 3.4f, 0f), new Vector3(2.2f, 2.5f, 2.2f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 6.4f, 0f), new Vector3(1.7f, 1.4f, 1.7f), mid, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 8.5f, 0f), new Vector3(1.15f, 1.0f, 1.15f), dark, SurfaceKind.Stone);

            // Plataformas.
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 4.85f, 0f), new Vector3(2.7f, 0.12f, 2.7f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 7.3f, 0f), new Vector3(2.05f, 0.1f, 2.05f), Stone, SurfaceKind.Stone);

            // Brasão de dragão (placa frontal).
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.2f, 1.15f), new Vector3(0.7f, 0.9f, 0.12f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.45f, 1.22f), new Vector3(0.35f, 0.35f, 0.08f), ember, SurfaceKind.Plain);

            // Brasas / cristais (poucos, legíveis).
            var emberA = Part(PrimitiveType.Sphere, root, new Vector3(1.15f, 5.15f, 0.2f), Vector3.one * 0.32f, ember, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(emberA.GetComponent<Renderer>(), ember, 0.5f);
            var crystalA = Part(PrimitiveType.Sphere, root, new Vector3(-1.05f, 5.15f, -0.25f), Vector3.one * 0.26f, crystal, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(crystalA.GetComponent<Renderer>(), crystal, 0.4f);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 3.9f, 0f), new Vector3(2.35f, 0.08f, 2.35f), ember, SurfaceKind.Metal);

            // Coroa cônica azul (diferente do keep quadrado).
            ConeRoof(root, new Vector3(0f, 9.55f, 0f), 1.55f, 1.2f, RoofBlue);
            SmallFlag(root, new Vector3(0f, 10.9f, 0f), ember);

            // Porta.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.55f, 1.25f), new Vector3(0.7f, 1.25f, 0.18f), Door, SurfaceKind.Wood);

            // Área de pouso.
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.2f, 2.7f), new Vector3(3.2f, 0.18f, 3.2f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.38f, 2.7f), new Vector3(2.4f, 0.12f, 2.4f), Wood, SurfaceKind.Wood);
            var padGlow = Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.48f, 2.7f), new Vector3(1.3f, 0.06f, 1.3f), ember, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(padGlow.GetComponent<Renderer>(), ember, 0.45f);

            var nestRoot = new GameObject("NestOccupants");
            nestRoot.transform.SetParent(root, false);
            nestRoot.transform.localPosition = new Vector3(0f, 1.7f, 2.7f);
        }

        // ─── P0: Fazenda ───────────────────────────────────────────────

        private static void BuildFarm(Transform root, Color color)
        {
            var wall = Color.Lerp(Wood, color, 0.22f);
            var crop = new Color(0.48f, 0.58f, 0.3f);

            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.05f, 0f), new Vector3(4.8f, 0.08f, 4.2f), Dirt, SurfaceKind.Dirt);

            // Casa.
            Part(PrimitiveType.Cube, root, new Vector3(-1.25f, 0.95f, 0.85f), new Vector3(2.0f, 1.7f, 1.85f), wall, SurfaceKind.Wood);
            SimpleRoof(root, new Vector3(-1.25f, 2.0f, 0.85f), 2.25f, 2.1f, RoofRed);
            Doorway(root, new Vector3(-1.25f, 0.7f, 1.8f));

            // Celeiro (mais largo, telhado baixo).
            Part(PrimitiveType.Cube, root, new Vector3(1.4f, 1.05f, 0.65f), new Vector3(1.9f, 1.9f, 2.2f),
                Color.Lerp(wall, Stone, 0.15f), SurfaceKind.Wood);
            SimpleRoof(root, new Vector3(1.4f, 2.2f, 0.65f), 2.15f, 2.5f, RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(1.4f, 0.85f, 1.8f), new Vector3(1.0f, 1.35f, 0.12f), Door, SurfaceKind.Wood);

            // Campo — faixas contínuas (não dezenas de cubinhos).
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.1f, -1.45f), new Vector3(4.2f, 0.12f, 1.7f), Grass, SurfaceKind.Vegetation);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.28f, -1.2f), new Vector3(3.8f, 0.22f, 0.35f), crop, SurfaceKind.Vegetation);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.28f, -1.65f), new Vector3(3.8f, 0.22f, 0.35f),
                Color.Lerp(crop, new Color(0.7f, 0.6f, 0.3f), 0.35f), SurfaceKind.Vegetation);

            // Cercas finas.
            Part(PrimitiveType.Cube, root, new Vector3(-2.15f, 0.32f, -1.45f), new Vector3(0.08f, 0.5f, 1.8f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(2.15f, 0.32f, -1.45f), new Vector3(0.08f, 0.5f, 1.8f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.32f, -2.3f), new Vector3(4.4f, 0.5f, 0.08f), Wood, SurfaceKind.Wood);

            // Sacos / caixas (3 peças).
            Part(PrimitiveType.Cube, root, new Vector3(-0.15f, 0.32f, 1.55f), new Vector3(0.4f, 0.4f, 0.4f),
                new Color(0.52f, 0.42f, 0.26f), SurfaceKind.Wood);
            Part(PrimitiveType.Sphere, root, new Vector3(-2.0f, 0.32f, 0.15f), Vector3.one * 0.4f,
                new Color(0.58f, 0.5f, 0.3f), SurfaceKind.Dirt);
            Part(PrimitiveType.Sphere, root, new Vector3(2.15f, 0.4f, -0.35f), Vector3.one * 0.55f, Grass, SurfaceKind.Vegetation);
        }

        // ─── P0: Armazém ───────────────────────────────────────────────

        private static void BuildWarehouse(Transform root, Color color)
        {
            var wall = Color.Lerp(Wood, color, 0.28f);

            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.05f, 0.2f), new Vector3(4.6f, 0.08f, 3.6f), Dirt, SurfaceKind.Dirt);
            Plinth(root, 4.2f, 3.0f);

            // Volume amplo, telhado baixo.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.15f, 0f), new Vector3(3.9f, 2.1f, 2.6f), wall, SurfaceKind.Wood);
            SimpleRoof(root, new Vector3(0f, 2.4f, 0f), 4.3f, 3.0f, RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.55f, 0f), new Vector3(4.4f, 0.1f, 0.22f), StoneDark, SurfaceKind.Stone);

            // Portão de carga largo.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.95f, 1.38f), new Vector3(1.9f, 1.7f, 0.14f), Door, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.9f, 1.42f), new Vector3(2.05f, 0.16f, 0.16f), Gold, SurfaceKind.Metal);

            // Área de carga + estoque.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.08f, 2.05f), new Vector3(3.2f, 0.08f, 1.2f),
                CityVisualMaterials.Path, SurfaceKind.Path);
            Part(PrimitiveType.Cube, root, new Vector3(-1.55f, 0.35f, 1.95f), new Vector3(0.45f, 0.45f, 0.45f),
                new Color(0.5f, 0.38f, 0.22f), SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(-1.05f, 0.35f, 2.1f), new Vector3(0.4f, 0.4f, 0.4f),
                new Color(0.48f, 0.36f, 0.2f), SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.55f, 0.4f, 1.95f), new Vector3(0.5f, 0.38f, 0.5f),
                new Color(0.4f, 0.28f, 0.16f), SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(2.0f, 0.4f, 1.7f), new Vector3(0.45f, 0.35f, 0.45f),
                new Color(0.38f, 0.26f, 0.15f), SurfaceKind.Wood);
            Part(PrimitiveType.Sphere, root, new Vector3(-0.4f, 0.28f, 2.15f), Vector3.one * 0.35f,
                new Color(0.55f, 0.48f, 0.3f), SurfaceKind.Dirt);

            // Cerca baixa.
            Part(PrimitiveType.Cube, root, new Vector3(-2.2f, 0.28f, 0.15f), new Vector3(0.08f, 0.45f, 2.8f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(2.2f, 0.28f, 0.15f), new Vector3(0.08f, 0.45f, 2.8f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.28f, -1.45f), new Vector3(4.5f, 0.45f, 0.08f), Wood, SurfaceKind.Wood);
        }

        // ─── P0: Academia ──────────────────────────────────────────────

        private static void BuildAcademy(Transform root, Color color)
        {
            var stone = Color.Lerp(Stone, color, 0.32f);
            var rune = new Color(0.38f, 0.58f, 0.9f);
            var blueFlag = new Color(0.22f, 0.34f, 0.7f);

            Plinth(root, 3.8f, 3.4f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.08f, 2.05f), new Vector3(2.8f, 0.08f, 1.4f),
                CityVisualMaterials.Path, SurfaceKind.Path);

            // Biblioteca (corpo).
            Part(PrimitiveType.Cube, root, new Vector3(-0.35f, 1.45f, 0f), new Vector3(2.8f, 2.7f, 2.4f), stone, SurfaceKind.Stone);
            SimpleRoof(root, new Vector3(-0.35f, 3.05f, 0f), 3.1f, 2.7f, RoofBlue);

            // Torre de estudo (cilíndrica — volume distinto).
            Part(PrimitiveType.Cylinder, root, new Vector3(1.55f, 2.5f, -0.2f), new Vector3(1.4f, 2.3f, 1.4f),
                Color.Lerp(stone, RoofBlue, 0.15f), SurfaceKind.Stone);
            ConeRoof(root, new Vector3(1.55f, 5.0f, -0.2f), 1.55f, 1.15f, RoofBlue);
            SmallFlag(root, new Vector3(1.55f, 6.3f, -0.2f), blueFlag);

            // Entrada com colunas.
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.0f, 1.1f, 1.35f), new Vector3(0.32f, 1.1f, 0.32f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0.3f, 1.1f, 1.35f), new Vector3(0.32f, 1.1f, 0.32f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(-0.35f, 2.3f, 1.35f), new Vector3(1.7f, 0.22f, 0.4f), stone, SurfaceKind.Stone);
            Doorway(root, new Vector3(-0.35f, 0.85f, 1.3f));

            // Runas + orbe (emissivo azul discreto).
            var runeA = Part(PrimitiveType.Cube, root, new Vector3(-0.9f, 2.15f, 1.28f), new Vector3(0.32f, 0.4f, 0.06f), rune, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(runeA.GetComponent<Renderer>(), rune, 0.4f);
            var runeB = Part(PrimitiveType.Cube, root, new Vector3(0.2f, 2.15f, 1.28f), new Vector3(0.32f, 0.4f, 0.06f), rune, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(runeB.GetComponent<Renderer>(), rune, 0.4f);
            var orb = Part(PrimitiveType.Sphere, root, new Vector3(-0.35f, 2.7f, 1.4f), Vector3.one * 0.26f, rune, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(orb.GetComponent<Renderer>(), rune, 0.5f);

            SmallFlag(root, new Vector3(-1.35f, 3.35f, 1.0f), blueFlag);
            SmallFlag(root, new Vector3(0.55f, 3.35f, 1.0f), blueFlag);

            // Estante lateral (um volume).
            Part(PrimitiveType.Cube, root, new Vector3(-1.65f, 1.35f, 0f), new Vector3(0.22f, 1.5f, 1.6f), Wood, SurfaceKind.Wood);
        }

        /// <summary>Proxy selecionável no portão — o anel completo vive em CityEnvironmentBuilder.</summary>
        private static void BuildWallGatehouse(Transform root, Color color)
        {
            var stone = Color.Lerp(Stone, color, 0.2f);
            var dark = Color.Lerp(StoneDark, color, 0.15f);

            Plinth(root, 4.2f, 2.4f);
            Part(PrimitiveType.Cube, root, new Vector3(-1.55f, 1.4f, 0f), new Vector3(1.2f, 2.6f, 1.3f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.55f, 1.4f, 0f), new Vector3(1.2f, 2.6f, 1.3f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.85f, 0f), new Vector3(4.0f, 0.5f, 1.5f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.25f, 0.55f), new Vector3(1.7f, 2.2f, 0.2f), Door, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 3.2f, 0f), new Vector3(1.2f, 0.16f, 0.4f), Gold, SurfaceKind.Metal);
            ConeRoof(root, new Vector3(-1.55f, 3.0f, 0f), 1.35f, 0.7f, RoofBlue);
            ConeRoof(root, new Vector3(1.55f, 3.0f, 0f), 1.35f, 0.7f, RoofBlue);
            SmallFlag(root, new Vector3(0f, 3.7f, 0f), Banner);
        }

        // ─── Demais (kits estáveis) ────────────────────────────────────

        // ─── Fase 2: kits modulares restantes ──────────────────────────

        private static void BuildLumbermill(Transform root, Color color)
        {
            var wall = Color.Lerp(Wood, color, 0.35f);
            Plinth(root, 3.4f, 2.8f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.05f, 0.15f), new Vector3(2.6f, 1.9f, 2.1f), wall, SurfaceKind.Wood);
            SimpleRoof(root, new Vector3(0f, 2.2f, 0.15f), 2.9f, 2.4f, RoofRed);
            Doorway(root, new Vector3(0f, 0.7f, 1.25f));
            // Roda / lâmina.
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.7f, 1.35f, 0.3f), new Vector3(0.35f, 1.2f, 0.35f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.7f, 1.6f, 0.55f), new Vector3(1.15f, 0.08f, 1.15f),
                new Color(0.55f, 0.55f, 0.58f), SurfaceKind.Metal);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.45f, 0.4f, 0.9f), new Vector3(0.55f, 0.35f, 0.55f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.45f, 0.4f, -0.2f), new Vector3(0.55f, 0.35f, 0.55f), Wood, SurfaceKind.Wood);
            SmallFlag(root, new Vector3(0f, 2.9f, 0.15f), Banner);
        }

        private static void BuildQuarry(Transform root, Color color)
        {
            Plinth(root, 3.4f, 3.0f);
            Part(PrimitiveType.Cube, root, new Vector3(-0.9f, 0.85f, 0.2f), new Vector3(1.9f, 1.5f, 1.7f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0.95f, 1.3f, -0.25f), new Vector3(1.6f, 2.4f, 1.5f),
                Color.Lerp(Stone, color, 0.2f), SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0.15f, 0.45f, 1.25f), new Vector3(1.2f, 0.75f, 1.1f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0.95f, 2.7f, -0.25f), new Vector3(1.0f, 0.35f, 1.0f), RoofRed, SurfaceKind.Roof);
            SmallFlag(root, new Vector3(0.95f, 3.2f, -0.25f), Gold);
        }

        private static void BuildMine(Transform root, Color color)
        {
            Plinth(root, 3.0f, 2.8f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.05f, 0.55f), new Vector3(2.5f, 1.9f, 1.9f),
                Color.Lerp(Stone, color, 0.2f), SurfaceKind.Stone);
            SimpleRoof(root, new Vector3(0f, 2.2f, 0.55f), 2.8f, 2.2f, RoofRed);
            // Boca da mina.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.75f, -0.95f), new Vector3(1.6f, 1.35f, 1.4f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.7f, -1.55f), new Vector3(1.0f, 1.1f, 0.2f),
                new Color(0.08f, 0.08f, 0.1f), SurfaceKind.Plain);
            Doorway(root, new Vector3(0f, 0.7f, 1.55f));
        }

        private static void BuildMarket(Transform root, Color color)
        {
            Plinth(root, 3.8f, 3.0f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.7f, 0f), new Vector3(3.3f, 1.15f, 2.5f),
                Color.Lerp(Wood, color, 0.3f), SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.5f, 0f), new Vector3(3.7f, 0.2f, 2.9f), RoofRed, SurfaceKind.Roof);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.45f, 0.95f, 0f), new Vector3(0.16f, 0.9f, 0.16f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.45f, 0.95f, 0f), new Vector3(0.16f, 0.9f, 0.16f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(-0.8f, 0.4f, 1.4f), new Vector3(0.45f, 0.45f, 0.45f),
                new Color(0.5f, 0.38f, 0.22f), SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(0.9f, 0.4f, 1.35f), new Vector3(0.5f, 0.35f, 0.5f),
                new Color(0.4f, 0.28f, 0.16f), SurfaceKind.Wood);
            SmallFlag(root, new Vector3(0f, 2.05f, 0f), Banner);
        }

        private static void BuildTemple(Transform root, Color color)
        {
            Plinth(root, 3.2f, 3.2f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.35f, 0f), new Vector3(2.7f, 2.5f, 2.7f),
                Color.Lerp(Stone, color, 0.25f), SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.25f, 1.25f, 1.25f), new Vector3(0.38f, 1.25f, 0.38f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.25f, 1.25f, 1.25f), new Vector3(0.38f, 1.25f, 0.38f), Stone, SurfaceKind.Stone);
            ConeRoof(root, new Vector3(0f, 2.9f, 0f), 2.6f, 1.1f, RoofBlue);
            SmallFlag(root, new Vector3(0f, 4.2f, 0f), Gold);
            Doorway(root, new Vector3(0f, 0.85f, 1.4f));
        }

        private static void BuildHospital(Transform root, Color color)
        {
            var wall = Color.Lerp(Stone, color, 0.35f);
            Plinth(root, 3.0f, 2.6f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.1f, 0.15f), new Vector3(2.5f, 2.0f, 2.1f), wall, SurfaceKind.Stone);
            SimpleRoof(root, new Vector3(0f, 2.35f, 0.15f), 2.8f, 2.4f, RoofRed);
            Doorway(root, new Vector3(0f, 0.75f, 1.25f));
            // Cruz.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.75f, 0.15f), new Vector3(1.05f, 0.22f, 0.22f), Banner, SurfaceKind.Plain);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.75f, 0.15f), new Vector3(0.22f, 0.22f, 1.05f), Banner, SurfaceKind.Plain);
        }

        private static void BuildInstitute(Transform root, Color color)
        {
            BuildHouse(root, Color.Lerp(Stone, color, 0.25f), RoofBlue);
            Part(PrimitiveType.Cube, root, new Vector3(0.95f, 1.85f, 1.15f), new Vector3(0.5f, 1.1f, 0.12f),
                new Color(0.55f, 0.7f, 0.82f), SurfaceKind.Plain);
            SmallFlag(root, new Vector3(0f, 2.9f, 0.2f), new Color(0.25f, 0.4f, 0.7f));
        }

        private static void BuildArena(Transform root, Color color)
        {
            var sand = new Color(0.68f, 0.56f, 0.38f);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.25f, 0f), new Vector3(3.7f, 0.28f, 3.7f), sand, SurfaceKind.Dirt);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.85f, 0f), new Vector3(3.9f, 0.55f, 3.9f),
                Color.Lerp(Stone, color, 0.25f), SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.45f, 0f), new Vector3(3.2f, 0.4f, 3.2f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.1f, 1.85f), new Vector3(1.4f, 1.4f, 0.35f), Stone, SurfaceKind.Stone);
            SmallFlag(root, new Vector3(0f, 2.35f, 1.6f), Banner);
            SmallFlag(root, new Vector3(0f, 2.35f, -1.6f), Gold);
        }

        private static void BuildLaboratory(Transform root, Color color)
        {
            var wall = Color.Lerp(Stone, color, 0.3f);
            Plinth(root, 2.9f, 2.6f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.1f, 0.15f), new Vector3(2.4f, 2.0f, 2.0f), wall, SurfaceKind.Stone);
            SimpleRoof(root, new Vector3(0f, 2.35f, 0.15f), 2.7f, 2.3f, RoofBlue);
            Doorway(root, new Vector3(0f, 0.75f, 1.2f));
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.85f, 0.15f), new Vector3(1.15f, 0.75f, 1.15f),
                new Color(0.4f, 0.65f, 0.7f), SurfaceKind.Stone);
            var orb = Part(PrimitiveType.Sphere, root, new Vector3(0f, 3.55f, 0.15f), new Vector3(0.5f, 0.5f, 0.5f),
                new Color(0.45f, 0.75f, 0.78f), SurfaceKind.Metal);
            CityVisualMaterials.ApplyEmissiveHint(orb.GetComponent<Renderer>(), new Color(0.4f, 0.8f, 0.85f), 0.4f);
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
            SimpleRoof(root, center, width, depth, color);
        }

        private static void SimpleRoof(Transform root, Vector3 center, float width, float depth, Color color)
        {
            // Telhado em 2 volumes — sem “asas” rotacionadas que parecem peças quebradas.
            Part(PrimitiveType.Cube, root, center, new Vector3(width, 0.28f, depth), color, SurfaceKind.Roof);
            Part(PrimitiveType.Cube, root, center + Vector3.up * 0.28f,
                new Vector3(width * 0.62f, 0.28f, depth * 0.55f), Color.Lerp(color, Color.black, 0.12f), SurfaceKind.Roof);
        }

        private static void ConeRoof(Transform root, Vector3 tip, float width, float height, Color color)
        {
            // 3 camadas — silhueta cônica limpa.
            for (var i = 0; i < 3; i++)
            {
                var t = i / 2f;
                var y = tip.y - height * (1f - t) * 0.75f;
                var w = Mathf.Lerp(width, width * 0.28f, t);
                Part(PrimitiveType.Cube, root, new Vector3(tip.x, y, tip.z),
                    new Vector3(w, height / 3f * 1.05f, w),
                    Color.Lerp(color, Color.black, t * 0.12f), SurfaceKind.Roof);
            }
        }

        private static void Battlement(Transform root, float x, float y, float width, Color stone)
        {
            Part(PrimitiveType.Cube, root, new Vector3(x, y, 0f), new Vector3(width, 0.35f, 0.3f), stone, SurfaceKind.Stone);
        }

        private static void Flag(Transform root, Vector3 tip, Color color) => SmallFlag(root, tip, color);

        private static void SmallFlag(Transform root, Vector3 tip, Color color)
        {
            Part(PrimitiveType.Cylinder, root, tip + Vector3.down * 0.4f, new Vector3(0.06f, 0.4f, 0.06f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, tip + new Vector3(0.22f, -0.08f, 0f), new Vector3(0.4f, 0.26f, 0.05f), color, SurfaceKind.Plain);
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
