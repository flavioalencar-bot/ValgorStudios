using UnityEngine;
using Valgor.City.Buildings;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Proxies arquitetônicos oficiais (Sprint 3) + fallback procedural.
    /// HF externo (castle / wall / dragon-tower) via loaders/catálogo; demais = proxy HF.
    /// </summary>
    public static class CityBuildingMeshFactory
    {
        private static Color Stone => CityVisualMaterials.StoneLight;
        private static Color StoneDark => CityVisualMaterials.StoneDark;
        private static Color Wood => CityVisualMaterials.Wood;
        private static Color RoofRed => CityArtPass3Config.Enabled
            ? new Color(0.52f, 0.28f, 0.22f)
            : CityVisualMaterials.RoofRed;
        private static Color RoofBlue => CityArtPass3Config.Enabled
            ? new Color(0.34f, 0.36f, 0.40f)
            : CityVisualMaterials.RoofBlue;
        private static Color Gold => CityVisualMaterials.Gold;
        private static Color Door => new(0.2f, 0.12f, 0.07f);
        private static Color Banner => new(0.55f, 0.16f, 0.14f);
        private static Color Grass => CityVisualMaterials.Vegetation;
        private static Color Dirt => CityVisualMaterials.Dirt;

        public static Bounds Build(string buildingId, Transform parent, Color color, int buildingLevel = 1)
        {
            var id = BuildingIdMigration.ToCanonical(buildingId);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);

            switch (id)
            {
                case "castle":
                    // Asset real por faixa de nível → fallback procedural se falhar.
                    if (!CastleRealVisualLoader.TryAttach(visual.transform, buildingLevel, out var castleDetail))
                    {
                        Debug.LogWarning($"[Valgor.City] Castle real missing ({castleDetail}) — procedural fallback.");
                        CastleTierVisual.Build(visual.transform, color, visualTier: 1);
                    }

                    // Art Pass 1.0 — vestimenta externa (não altera prefab lógico).
                    CityCastleArtDressing.Attach(visual.transform);
                    break;
                case "dragon-tower":
                    BuildDragonTower(visual.transform, color);
                    CityDragonTowerArtDressing.Attach(visual.transform);
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
                case "hero-hall":
                    BuildHeroHall(visual.transform, color);
                    break;
                case "arena":
                    BuildArena(visual.transform, color);
                    break;
                case "training-ground":
                    BuildTrainingGround(visual.transform, color);
                    break;
                case "residence":
                    BuildResidence(visual.transform, color);
                    break;
                case "foundry":
                    BuildFoundry(visual.transform, color);
                    break;
                default:
                    BuildHouse(visual.transform, Color.Lerp(color, Stone, 0.35f), RoofRed);
                    break;
            }

            // Sprint 3 — integração ao terreno + dressing temático (só visual).
            CityOfficialBuildingDressing.Attach(id, visual.transform);

            return Encapsulate(visual.transform);
        }

        // ─── Castelo: ver CastleTierVisual (Tier 1 = referência oficial) ─

        // ─── P0: Torre dos Dragões (circular, distinta do Castelo) ──────

        private static void BuildDragonTower(Transform root, Color color)
        {
            var dark = Color.Lerp(StoneDark, color, 0.3f);
            var mid = Color.Lerp(dark, Stone, 0.25f);
            var ember = new Color(0.85f, 0.38f, 0.12f);
            var crystal = new Color(0.4f, 0.55f, 0.9f);

            // Platô rochoso irregular (Art Pass 1.0).
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.35f, 0.2f), new Vector3(5.2f, 0.45f, 5.0f), Dirt, SurfaceKind.Dirt);
            Part(PrimitiveType.Cube, root, new Vector3(-1.8f, 0.55f, 1.1f), new Vector3(1.6f, 0.7f, 1.4f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.6f, 0.5f, -0.8f), new Vector3(1.4f, 0.6f, 1.5f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.95f, 0f), new Vector3(3.6f, 0.35f, 3.6f), dark, SurfaceKind.Stone);

            // Fuste + pilares.
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 3.5f, 0f), new Vector3(2.35f, 2.6f, 2.35f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 6.5f, 0f), new Vector3(1.85f, 1.45f, 1.85f), mid, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 8.6f, 0f), new Vector3(1.2f, 1.05f, 1.2f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.35f, 2.4f, 0.9f), new Vector3(0.35f, 1.8f, 0.35f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.35f, 2.4f, 0.9f), new Vector3(0.35f, 1.8f, 0.35f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.2f, 2.2f, -1.0f), new Vector3(0.3f, 1.5f, 0.3f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.2f, 2.2f, -1.0f), new Vector3(0.3f, 1.5f, 0.3f), StoneDark, SurfaceKind.Stone);

            // Plataformas / cornijas.
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 4.95f, 0f), new Vector3(2.9f, 0.14f, 2.9f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 7.4f, 0f), new Vector3(2.2f, 0.12f, 2.2f), Stone, SurfaceKind.Stone);

            // Brasão + brasas + cristais discretos.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.3f, 1.25f), new Vector3(0.8f, 1.0f, 0.14f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.55f, 1.32f), new Vector3(0.38f, 0.38f, 0.08f), ember, SurfaceKind.Plain);
            var emberA = Part(PrimitiveType.Sphere, root, new Vector3(1.25f, 5.25f, 0.25f), Vector3.one * 0.34f, ember, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(emberA.GetComponent<Renderer>(), ember, 0.55f);
            var emberB = Part(PrimitiveType.Sphere, root, new Vector3(-1.2f, 3.2f, 1.1f), Vector3.one * 0.28f, ember, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(emberB.GetComponent<Renderer>(), ember, 0.45f);
            var crystalA = Part(PrimitiveType.Sphere, root, new Vector3(-1.1f, 5.25f, -0.3f), Vector3.one * 0.24f, crystal, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(crystalA.GetComponent<Renderer>(), crystal, 0.4f);
            var crystalB = Part(PrimitiveType.Sphere, root, new Vector3(0.85f, 7.7f, 0.4f), Vector3.one * 0.18f, crystal, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(crystalB.GetComponent<Renderer>(), crystal, 0.35f);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 4.0f, 0f), new Vector3(2.45f, 0.08f, 2.45f), ember, SurfaceKind.Metal);

            // Correntes decorativas (elos simples).
            Part(PrimitiveType.Cube, root, new Vector3(-1.6f, 3.6f, 1.35f), new Vector3(0.08f, 1.4f, 0.08f),
                Color.Lerp(StoneDark, Gold, 0.2f), SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(1.6f, 3.6f, 1.35f), new Vector3(0.08f, 1.4f, 0.08f),
                Color.Lerp(StoneDark, Gold, 0.2f), SurfaceKind.Metal);
            Part(PrimitiveType.Sphere, root, new Vector3(-1.6f, 2.8f, 1.35f), Vector3.one * 0.16f, Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Sphere, root, new Vector3(1.6f, 2.8f, 1.35f), Vector3.one * 0.16f, Gold, SurfaceKind.Metal);

            ConeRoof(root, new Vector3(0f, 9.7f, 0f), 1.65f, 1.25f, RoofBlue);
            SmallFlag(root, new Vector3(0f, 11.1f, 0f), ember);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.6f, 1.35f), new Vector3(0.75f, 1.35f, 0.2f), Door, SurfaceKind.Wood);

            // Plataforma de pouso ampla — área livre para o Dragão (NestOccupants intacto).
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.15f, 2.85f), new Vector3(3.8f, 0.2f, 3.8f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.35f, 2.85f), new Vector3(2.9f, 0.12f, 2.9f), Wood, SurfaceKind.Wood);
            var padGlow = Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.45f, 2.85f), new Vector3(1.5f, 0.06f, 1.5f), ember, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(padGlow.GetComponent<Renderer>(), ember, 0.5f);
            // Braseiros na borda do pad.
            Brazier(root, new Vector3(-1.55f, 1.55f, 3.9f), ember);
            Brazier(root, new Vector3(1.55f, 1.55f, 3.9f), ember);

            var nestRoot = new GameObject("NestOccupants");
            nestRoot.transform.SetParent(root, false);
            nestRoot.transform.localPosition = new Vector3(0f, 1.7f, 2.85f);
        }

        private static void Brazier(Transform root, Vector3 pos, Color ember)
        {
            Part(PrimitiveType.Cylinder, root, pos, new Vector3(0.35f, 0.22f, 0.35f), StoneDark, SurfaceKind.Stone);
            var glow = Part(PrimitiveType.Sphere, root, pos + Vector3.up * 0.28f, Vector3.one * 0.28f, ember, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(glow.GetComponent<Renderer>(), ember, 0.55f);
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

            Plinth(root, 4.6f, 2.8f);
            // Torres do portão + arco.
            Part(PrimitiveType.Cube, root, new Vector3(-1.7f, 1.55f, 0f), new Vector3(1.35f, 2.9f, 1.5f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.7f, 1.55f, 0f), new Vector3(1.35f, 2.9f, 1.5f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 3.15f, 0f), new Vector3(4.5f, 0.55f, 1.7f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 3.55f, 0f), new Vector3(3.2f, 0.35f, 1.4f), dark, SurfaceKind.Stone);
            // Portas de madeira reforçadas + ferragens.
            Part(PrimitiveType.Cube, root, new Vector3(-0.45f, 1.35f, 0.65f), new Vector3(0.85f, 2.4f, 0.22f), Door, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0.45f, 1.35f, 0.65f), new Vector3(0.85f, 2.4f, 0.22f), Door, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.9f, 0.78f), new Vector3(0.12f, 1.6f, 0.08f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(-0.45f, 1.35f, 0.78f), new Vector3(0.55f, 0.1f, 0.08f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0.45f, 1.35f, 0.78f), new Vector3(0.55f, 0.1f, 0.08f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 3.7f, 0f), new Vector3(1.4f, 0.18f, 0.45f), Gold, SurfaceKind.Metal);
            ConeRoof(root, new Vector3(-1.7f, 3.3f, 0f), 1.45f, 0.75f, RoofBlue);
            ConeRoof(root, new Vector3(1.7f, 3.3f, 0f), 1.45f, 0.75f, RoofBlue);
            SmallFlag(root, new Vector3(-1.7f, 4.2f, 0f), Banner);
            SmallFlag(root, new Vector3(1.7f, 4.2f, 0f), Banner);
            // Ponte de acesso curta.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.12f, 1.55f), new Vector3(2.8f, 0.14f, 1.6f),
                CityVisualMaterials.Path, SurfaceKind.Path);
            Part(PrimitiveType.Cube, root, new Vector3(-1.35f, 0.35f, 1.9f), new Vector3(0.18f, 0.55f, 1.2f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(1.35f, 0.35f, 1.9f), new Vector3(0.18f, 0.55f, 1.2f), Wood, SurfaceKind.Wood);
        }

        // ─── Demais (kits estáveis) ────────────────────────────────────

        // ─── Fase 2: kits modulares restantes ──────────────────────────

        private static void BuildLumbermill(Transform root, Color color)
        {
            var wall = Color.Lerp(Wood, color, 0.28f);
            Plinth(root, 4.0f, 3.4f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.15f, 0.1f), new Vector3(3.0f, 2.1f, 2.4f), wall, SurfaceKind.Wood);
            SimpleRoof(root, new Vector3(0f, 2.4f, 0.1f), 3.4f, 2.8f, RoofRed);
            Doorway(root, new Vector3(0f, 0.75f, 1.35f));
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.9f, 1.45f, 0.25f), new Vector3(0.4f, 1.35f, 0.4f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.9f, 1.7f, 0.55f), new Vector3(1.35f, 0.1f, 1.35f),
                new Color(0.55f, 0.55f, 0.58f), SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(1.7f, 0.85f, -0.6f), new Vector3(1.5f, 1.4f, 1.5f), wall, SurfaceKind.Wood);
            SimpleRoof(root, new Vector3(1.7f, 1.7f, -0.6f), 1.7f, 1.7f, RoofRed);
            SmallFlag(root, new Vector3(0f, 3.1f, 0.1f), Banner);
        }

        private static void BuildQuarry(Transform root, Color color)
        {
            Plinth(root, 4.2f, 3.6f);
            Part(PrimitiveType.Cube, root, new Vector3(-1.1f, 1.0f, 0.15f), new Vector3(2.2f, 1.8f, 2.0f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.15f, 1.55f, -0.2f), new Vector3(1.9f, 2.8f, 1.8f),
                Color.Lerp(Stone, color, 0.18f), SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0.1f, 0.55f, 1.45f), new Vector3(1.5f, 0.9f, 1.3f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.15f, 3.15f, -0.2f), new Vector3(1.2f, 0.35f, 1.2f), RoofRed, SurfaceKind.Roof);
            Part(PrimitiveType.Cube, root, new Vector3(2.1f, 1.8f, 0.9f), new Vector3(0.22f, 3.0f, 0.22f), Wood, SurfaceKind.Wood);
            SmallFlag(root, new Vector3(1.15f, 3.7f, -0.2f), Gold);
        }

        private static void BuildMine(Transform root, Color color)
        {
            Plinth(root, 3.6f, 3.4f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.15f, 0.55f), new Vector3(2.8f, 2.1f, 2.2f),
                Color.Lerp(Stone, color, 0.18f), SurfaceKind.Stone);
            SimpleRoof(root, new Vector3(0f, 2.4f, 0.55f), 3.1f, 2.5f, RoofRed);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.95f, -1.15f), new Vector3(1.9f, 1.7f, 1.6f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.85f, -1.85f), new Vector3(1.15f, 1.3f, 0.25f),
                new Color(0.08f, 0.08f, 0.1f), SurfaceKind.Plain);
            Part(PrimitiveType.Cube, root, new Vector3(-1.0f, 1.6f, -1.0f), new Vector3(0.2f, 2.4f, 0.2f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(1.0f, 1.6f, -1.0f), new Vector3(0.2f, 2.4f, 0.2f), Wood, SurfaceKind.Wood);
            Doorway(root, new Vector3(0f, 0.75f, 1.7f));
        }

        private static void BuildMarket(Transform root, Color color)
        {
            // Centro comercial — hall + toldo (props em ArchSprint3Dressing).
            Plinth(root, 4.4f, 3.6f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.95f, 0f), new Vector3(3.6f, 1.6f, 2.8f),
                Color.Lerp(Wood, color, 0.22f), SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.9f, 0f), new Vector3(4.0f, 0.22f, 3.2f), RoofRed, SurfaceKind.Roof);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.15f, 0f), new Vector3(2.4f, 0.35f, 1.6f),
                Color.Lerp(RoofRed, StoneDark, 0.2f), SurfaceKind.Roof);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.7f, 1.15f, 0f), new Vector3(0.18f, 1.1f, 0.18f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.7f, 1.15f, 0f), new Vector3(0.18f, 1.1f, 0.18f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.7f, 1.55f), new Vector3(2.4f, 1.1f, 0.35f), Wood, SurfaceKind.Wood);
            SmallFlag(root, new Vector3(0f, 2.75f, 0f), Banner);
        }

        private static void BuildTemple(Transform root, Color color)
        {
            Plinth(root, 3.8f, 3.8f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.55f, 0f), new Vector3(3.0f, 2.9f, 3.0f),
                Color.Lerp(Stone, color, 0.2f), SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.4f, 1.4f, 1.4f), new Vector3(0.42f, 1.4f, 0.42f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.4f, 1.4f, 1.4f), new Vector3(0.42f, 1.4f, 0.42f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.4f, 1.4f, -1.2f), new Vector3(0.36f, 1.3f, 0.36f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(1.4f, 1.4f, -1.2f), new Vector3(0.36f, 1.3f, 0.36f), Stone, SurfaceKind.Stone);
            ConeRoof(root, new Vector3(0f, 3.25f, 0f), 2.9f, 1.25f, RoofBlue);
            SmallFlag(root, new Vector3(0f, 4.7f, 0f), Gold);
            Doorway(root, new Vector3(0f, 0.9f, 1.55f));
        }

        private static void BuildHospital(Transform root, Color color)
        {
            var wall = Color.Lerp(Stone, color, 0.28f);
            Plinth(root, 3.6f, 3.2f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.25f, 0.1f), new Vector3(2.9f, 2.3f, 2.4f), wall, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(-1.55f, 1.0f, -0.15f), new Vector3(1.4f, 1.7f, 1.8f),
                Color.Lerp(wall, Wood, 0.15f), SurfaceKind.Stone);
            SimpleRoof(root, new Vector3(0f, 2.55f, 0.1f), 3.2f, 2.7f, RoofRed);
            SimpleRoof(root, new Vector3(-1.55f, 2.0f, -0.15f), 1.6f, 2.0f, RoofRed);
            Doorway(root, new Vector3(0f, 0.8f, 1.35f));
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.95f, 0.1f), new Vector3(1.15f, 0.22f, 0.22f), Banner, SurfaceKind.Plain);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.95f, 0.1f), new Vector3(0.22f, 0.22f, 1.15f), Banner, SurfaceKind.Plain);
        }

        private static void BuildHeroHall(Transform root, Color color)
        {
            // Salão nobre monumental — distinto do Castelo e da Academia.
            Plinth(root, 4.2f, 3.6f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.55f, 0.1f), new Vector3(3.4f, 2.9f, 2.9f),
                Color.Lerp(Stone, color, 0.28f), SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.2f, 1.55f), new Vector3(2.2f, 2.2f, 0.55f),
                Color.Lerp(Stone, Gold, 0.15f), SurfaceKind.Stone);
            SimpleRoof(root, new Vector3(0f, 3.25f, 0.1f), 3.9f, 3.3f, RoofBlue);
            Doorway(root, new Vector3(0f, 0.9f, 1.85f));
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.45f, 1.75f), new Vector3(1.6f, 1.0f, 0.18f),
                new Color(0.55f, 0.7f, 0.82f), SurfaceKind.Plain);
            // Colunas + estátuas na entrada.
            Part(PrimitiveType.Cube, root, new Vector3(-1.85f, 1.55f, 1.55f), new Vector3(0.55f, 2.8f, 0.55f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.85f, 1.55f, 1.55f), new Vector3(0.55f, 2.8f, 0.55f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(-1.85f, 3.1f, 1.55f), new Vector3(0.75f, 0.4f, 0.75f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(1.85f, 3.1f, 1.55f), new Vector3(0.75f, 0.4f, 0.75f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(-2.4f, 1.1f, 0.2f), new Vector3(0.5f, 1.9f, 0.5f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(2.4f, 1.1f, 0.2f), new Vector3(0.5f, 1.9f, 0.5f), StoneDark, SurfaceKind.Stone);
            SmallFlag(root, new Vector3(0f, 4.0f, 0.1f), new Color(0.25f, 0.4f, 0.7f));
            SmallFlag(root, new Vector3(-1.2f, 3.7f, 0.1f), Gold);
            SmallFlag(root, new Vector3(1.2f, 3.7f, 0.1f), Gold);
        }

        private static void BuildArena(Transform root, Color color)
        {
            var sand = new Color(0.68f, 0.56f, 0.38f);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.2f, 0f), new Vector3(4.2f, 0.28f, 4.2f), sand, SurfaceKind.Dirt);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.75f, 0f), new Vector3(4.4f, 0.65f, 4.4f),
                Color.Lerp(Stone, color, 0.25f), SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.45f, 0f), new Vector3(3.6f, 0.55f, 3.6f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.95f, 0f), new Vector3(3.0f, 0.35f, 3.0f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.35f, 2.15f), new Vector3(1.7f, 1.7f, 0.4f), Stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(2.0f, 1.5f, 0f), new Vector3(0.35f, 1.4f, 1.2f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(-2.0f, 1.5f, 0f), new Vector3(0.35f, 1.4f, 1.2f), StoneDark, SurfaceKind.Stone);
            SmallFlag(root, new Vector3(0f, 2.65f, 1.9f), Banner);
            SmallFlag(root, new Vector3(0f, 2.65f, -1.9f), Gold);
        }

        private static void BuildTrainingGround(Transform root, Color color)
        {
            // Campo aberto — cercas, alvos, barraca, bonecos (≠ Arena fechada).
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.1f, 0f), new Vector3(4.8f, 0.18f, 4.2f), Dirt, SurfaceKind.Dirt);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.05f, 0f), new Vector3(5.1f, 0.08f, 4.5f),
                Color.Lerp(Dirt, Wood, 0.2f), SurfaceKind.Dirt);
            // Cercas.
            Part(PrimitiveType.Cube, root, new Vector3(-2.4f, 0.45f, 0f), new Vector3(0.12f, 0.85f, 4.0f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(2.4f, 0.45f, 0f), new Vector3(0.12f, 0.85f, 4.0f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.45f, -2.05f), new Vector3(4.8f, 0.85f, 0.12f), Wood, SurfaceKind.Wood);
            // Alvos e bonecos.
            Part(PrimitiveType.Cube, root, new Vector3(-1.6f, 0.75f, 1.4f), new Vector3(0.2f, 1.35f, 1.3f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.6f, 1.5f, 1.4f), new Vector3(0.85f, 0.08f, 0.85f), Banner, SurfaceKind.Plain);
            Part(PrimitiveType.Cube, root, new Vector3(1.5f, 0.85f, -0.6f), new Vector3(0.3f, 1.55f, 0.3f), Wood, SurfaceKind.Wood);
            Part(PrimitiveType.Sphere, root, new Vector3(1.5f, 1.75f, -0.6f), new Vector3(0.55f, 0.55f, 0.55f),
                Color.Lerp(Banner, Color.white, 0.2f), SurfaceKind.Plain);
            Part(PrimitiveType.Cube, root, new Vector3(1.7f, 0.85f, 1.2f), new Vector3(0.3f, 1.55f, 0.3f), Wood, SurfaceKind.Wood);
            // Barraca + armação.
            Part(PrimitiveType.Cube, root, new Vector3(-0.3f, 0.55f, -1.4f), new Vector3(1.9f, 1.05f, 1.2f),
                Color.Lerp(Wood, color, 0.25f), SurfaceKind.Wood);
            Part(PrimitiveType.Cube, root, new Vector3(-0.3f, 1.2f, -1.4f), new Vector3(2.1f, 0.2f, 1.35f), RoofRed, SurfaceKind.Roof);
            Part(PrimitiveType.Cube, root, new Vector3(0.9f, 0.4f, 1.3f), new Vector3(1.0f, 0.75f, 0.6f), Wood, SurfaceKind.Wood);
            SmallFlag(root, new Vector3(-1.4f, 1.85f, -1.3f), Banner);
        }

        private static void BuildResidence(Transform root, Color color)
        {
            // Residência nobre — pedra/ardósia (sem casas coloridas de blockout).
            var wall = Color.Lerp(Stone, color, 0.22f);
            Plinth(root, 4.0f, 3.6f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.35f, 0.1f), new Vector3(3.1f, 2.5f, 2.7f), wall, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.75f, 1.0f, 0.1f), new Vector3(1.5f, 1.7f, 2.0f),
                Color.Lerp(wall, Wood, 0.18f), SurfaceKind.Stone);
            PitchedRoof(root, new Vector3(0f, 2.8f, 0.1f), 3.5f, RoofBlue);
            Part(PrimitiveType.Cube, root, new Vector3(1.75f, 2.05f, 0.1f), new Vector3(1.7f, 0.32f, 2.2f), RoofBlue, SurfaceKind.Roof);
            Doorway(root, new Vector3(0f, 0.85f, 1.5f));
            Part(PrimitiveType.Cube, root, new Vector3(-0.75f, 1.7f, 1.45f), new Vector3(0.55f, 0.6f, 0.12f),
                new Color(0.42f, 0.55f, 0.62f), SurfaceKind.Plain);
            Part(PrimitiveType.Cube, root, new Vector3(0.75f, 1.7f, 1.45f), new Vector3(0.55f, 0.6f, 0.12f),
                new Color(0.42f, 0.55f, 0.62f), SurfaceKind.Plain);
            Part(PrimitiveType.Cube, root, new Vector3(-1.7f, 0.28f, 1.55f), new Vector3(1.0f, 0.35f, 0.65f), Grass, SurfaceKind.Vegetation);
            SmallFlag(root, new Vector3(0f, 3.5f, 0.1f), Banner);
        }

        private static void BuildFoundry(Transform root, Color color)
        {
            // Fundição industrial — fornalha + chaminé altas (≠ prop ambiental).
            Plinth(root, 3.6f, 3.2f);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 1.25f, 0.1f), new Vector3(2.9f, 2.3f, 2.5f),
                Color.Lerp(StoneDark, color, 0.25f), SurfaceKind.Stone);
            SimpleRoof(root, new Vector3(0f, 2.6f, 0.1f), 3.2f, 2.8f, RoofRed);
            Doorway(root, new Vector3(0f, 0.8f, 1.4f));
            // Fornalha frontal.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.85f, 1.55f), new Vector3(1.4f, 1.3f, 0.7f), StoneDark, SurfaceKind.Stone);
            var glow = Part(PrimitiveType.Sphere, root, new Vector3(0f, 0.85f, 1.75f), new Vector3(0.7f, 0.55f, 0.45f),
                new Color(1f, 0.45f, 0.15f), SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(glow.GetComponent<Renderer>(), new Color(1f, 0.4f, 0.1f), 0.65f);
            // Chaminé dupla.
            Part(PrimitiveType.Cylinder, root, new Vector3(1.15f, 2.4f, -0.35f), new Vector3(0.55f, 2.2f, 0.55f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cylinder, root, new Vector3(-1.0f, 2.1f, -0.5f), new Vector3(0.4f, 1.6f, 0.4f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(-1.45f, 0.5f, 1.15f), new Vector3(0.85f, 0.85f, 0.85f),
                new Color(0.35f, 0.35f, 0.38f), SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(1.4f, 0.4f, 1.1f), new Vector3(0.7f, 0.55f, 0.7f),
                new Color(0.4f, 0.32f, 0.22f), SurfaceKind.Metal);
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
