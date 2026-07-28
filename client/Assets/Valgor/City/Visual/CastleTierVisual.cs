using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Visual do Castelo separado da lógica do edifício.
    /// Tier 1 segue a referência oficial docs/references/city/castle_tier1_reference.png.
    /// Tiers futuros entram por <see cref="Build"/> sem reescrever BuildingView/Slot.
    /// </summary>
    public static class CastleTierVisual
    {
        private static Color Stone => CityVisualMaterials.StoneLight;
        private static Color StoneDark => CityVisualMaterials.StoneDark;
        private static Color Wood => CityVisualMaterials.Wood;
        private static Color RoofBlue => CityVisualMaterials.RoofBlue;
        private static Color Gold => CityVisualMaterials.Gold;
        private static Color Door => new(0.18f, 0.1f, 0.06f);
        private static Color BannerBlue => new(0.18f, 0.28f, 0.55f);
        private static Color WarmGlow => new(0.95f, 0.78f, 0.35f);

        /// <param name="visualTier">1 = Demo City referência; ≥2 reserva evolução futura.</param>
        public static void Build(Transform visualRoot, Color tint, int visualTier = 1)
        {
            var tierRoot = new GameObject($"CastleTier{Mathf.Max(1, visualTier)}");
            tierRoot.transform.SetParent(visualRoot, false);

            switch (Mathf.Max(1, visualTier))
            {
                case 1:
                default:
                    BuildTier1(tierRoot.transform, tint);
                    break;
            }
        }

        private static void BuildTier1(Transform root, Color tint)
        {
            var stone = Color.Lerp(Stone, tint, 0.12f);
            var dark = Color.Lerp(StoneDark, tint, 0.1f);
            var roof = new Color(0.14f, 0.26f, 0.62f);

            // Plataforma / silhueta de base.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.1f, 0.15f), new Vector3(7.4f, 0.22f, 6.6f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.28f, 0.1f), new Vector3(6.8f, 0.18f, 6.0f), stone, SurfaceKind.Stone);

            // Escadaria + terraço de entrada.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.12f, 4.35f), new Vector3(4.2f, 0.12f, 2.4f),
                CityVisualMaterials.Path, SurfaceKind.Path);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.28f, 3.55f), new Vector3(3.4f, 0.18f, 0.85f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.48f, 3.05f), new Vector3(3.0f, 0.18f, 0.75f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.68f, 2.65f), new Vector3(2.6f, 0.16f, 0.55f), dark, SurfaceKind.Stone);

            // Lanternas na base da escada.
            Lantern(root, new Vector3(-1.55f, 0.55f, 4.15f));
            Lantern(root, new Vector3(1.55f, 0.55f, 4.15f));

            // Muralhas externas contínuas (frente aberta no portão).
            Curtain(root, new Vector3(0f, 1.35f, -2.85f), new Vector3(6.4f, 2.4f, 0.55f), dark);
            Curtain(root, new Vector3(-3.15f, 1.35f, 0f), new Vector3(0.55f, 2.4f, 5.5f), dark);
            Curtain(root, new Vector3(3.15f, 1.35f, 0f), new Vector3(0.55f, 2.4f, 5.5f), dark);
            Curtain(root, new Vector3(-2.35f, 1.35f, 2.85f), new Vector3(1.85f, 2.4f, 0.55f), dark);
            Curtain(root, new Vector3(2.35f, 1.35f, 2.85f), new Vector3(1.85f, 2.4f, 0.55f), dark);
            // Lintel / ameias sobre o vão.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.65f, 2.85f), new Vector3(2.9f, 0.45f, 0.6f), stone, SurfaceKind.Stone);
            Battlements(root, new Vector3(0f, 2.7f, -2.85f), 6.0f, stone);
            Battlements(root, new Vector3(-3.15f, 2.7f, 0f), 5.0f, stone, alongZ: true);
            Battlements(root, new Vector3(3.15f, 2.7f, 0f), 5.0f, stone, alongZ: true);

            // Torres circulares de canto + meio.
            RoundTower(root, new Vector3(-3.05f, 0f, -2.75f), 3.9f, stone, roof, withBanner: true);
            RoundTower(root, new Vector3(3.05f, 0f, -2.75f), 3.9f, stone, roof, withBanner: true);
            RoundTower(root, new Vector3(-3.05f, 0f, 2.55f), 3.45f, dark, roof, withBanner: true);
            RoundTower(root, new Vector3(3.05f, 0f, 2.55f), 3.45f, dark, roof, withBanner: true);
            RoundTower(root, new Vector3(-3.15f, 0f, 0f), 3.2f, stone, roof, withBanner: false);
            RoundTower(root, new Vector3(3.15f, 0f, 0f), 3.2f, stone, roof, withBanner: false);

            // Keep central dominante.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.55f, -0.35f), new Vector3(3.35f, 4.6f, 3.05f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 5.05f, -0.35f), new Vector3(2.75f, 0.7f, 2.45f), dark, SurfaceKind.Stone);
            // Balcões / cornijas.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 3.55f, 1.25f), new Vector3(2.2f, 0.18f, 0.45f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 4.55f, 1.2f), new Vector3(1.8f, 0.14f, 0.35f), Gold, SurfaceKind.Metal);
            KeepWindows(root, new Vector3(0f, 0f, -0.35f));
            PyramidRoof(root, new Vector3(0f, 5.55f, -0.35f), 3.1f, 2.1f, roof);
            GoldFinial(root, new Vector3(0f, 7.85f, -0.35f));
            CrestBanner(root, new Vector3(-0.85f, 5.35f, 1.15f));
            CrestBanner(root, new Vector3(0.85f, 5.35f, 1.15f));

            // Portão principal (leitura clara da entrada).
            Part(PrimitiveType.Cube, root, new Vector3(-1.35f, 1.55f, 2.95f), new Vector3(0.85f, 2.7f, 0.85f), dark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(1.35f, 1.55f, 2.95f), new Vector3(0.85f, 2.7f, 0.85f), dark, SurfaceKind.Stone);
            // Arco / moldura.
            Part(PrimitiveType.Cube, root, new Vector3(0f, 2.95f, 3.05f), new Vector3(2.55f, 0.4f, 0.55f), stone, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 3.25f, 3.05f), new Vector3(1.9f, 0.28f, 0.45f), Gold, SurfaceKind.Metal);
            // Porta de madeira.
            var gate = Part(PrimitiveType.Cube, root, new Vector3(0f, 1.35f, 3.12f), new Vector3(1.7f, 2.35f, 0.22f), Door, SurfaceKind.Wood);
            gate.name = "MainGate";
            // Brasão na porta principal (escala legível na iso).
            LionCrest(gate.transform, new Vector3(0f, 0.2f, 0.62f), 0.72f);

            // Águia / fênix dourada acima do portão.
            GateEagle(root, new Vector3(0f, 3.75f, 3.15f));

            // Estátuas de cavaleiros flanqueando a entrada.
            KnightStatue(root, new Vector3(-2.15f, 0.7f, 3.35f));
            KnightStatue(root, new Vector3(2.15f, 0.7f, 3.35f));

            // Bandeiras nas muralhas frontais (brasão).
            CrestBanner(root, new Vector3(-2.0f, 2.55f, 3.15f));
            CrestBanner(root, new Vector3(2.0f, 2.55f, 3.15f));
        }

        private static void Curtain(Transform root, Vector3 pos, Vector3 scale, Color stone) =>
            Part(PrimitiveType.Cube, root, pos, scale, stone, SurfaceKind.Stone);

        private static void Battlements(Transform root, Vector3 center, float length, Color stone, bool alongZ = false)
        {
            var count = 5;
            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0.5f : i / (float)(count - 1);
                var offset = Mathf.Lerp(-length * 0.45f, length * 0.45f, t);
                var pos = alongZ
                    ? center + new Vector3(0f, 0f, offset)
                    : center + new Vector3(offset, 0f, 0f);
                var scale = alongZ
                    ? new Vector3(0.28f, 0.38f, 0.55f)
                    : new Vector3(0.55f, 0.38f, 0.28f);
                Part(PrimitiveType.Cube, root, pos, scale, stone, SurfaceKind.Stone);
            }
        }

        private static void RoundTower(
            Transform root,
            Vector3 basePos,
            float height,
            Color stone,
            Color roof,
            bool withBanner)
        {
            var tower = new GameObject("RoundTower");
            tower.transform.SetParent(root, false);
            tower.transform.localPosition = basePos;

            Part(PrimitiveType.Cylinder, tower.transform, new Vector3(0f, height * 0.5f, 0f),
                new Vector3(1.35f, height * 0.5f, 1.35f), stone, SurfaceKind.Stone);
            // Ameia circular.
            Part(PrimitiveType.Cylinder, tower.transform, new Vector3(0f, height + 0.12f, 0f),
                new Vector3(1.5f, 0.14f, 1.5f), Color.Lerp(stone, StoneDark, 0.2f), SurfaceKind.Stone);
            ConeRoof(tower.transform, new Vector3(0f, height + 1.35f, 0f), 1.65f, 1.25f, roof);
            GoldFinial(tower.transform, new Vector3(0f, height + 1.55f, 0f));

            // Janela aquecida.
            var win = Part(PrimitiveType.Cube, tower.transform, new Vector3(0f, height * 0.55f, 0.68f),
                new Vector3(0.28f, 0.4f, 0.08f), WarmGlow, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(win.GetComponent<Renderer>(), WarmGlow, 0.55f);

            if (withBanner)
            {
                CrestBanner(tower.transform, new Vector3(0.55f, height * 0.72f, 0.15f));
            }
        }

        private static void KeepWindows(Transform root, Vector3 keepCenter)
        {
            var accent = new GameObject("Accent_KeepWindows");
            accent.transform.SetParent(root, false);
            float[] ys = { 1.6f, 2.7f, 3.8f };
            float[] xs = { -0.85f, 0f, 0.85f };
            foreach (var y in ys)
            {
                foreach (var x in xs)
                {
                    var win = Part(PrimitiveType.Cube, accent.transform,
                        keepCenter + new Vector3(x, y, 1.55f),
                        new Vector3(0.32f, 0.42f, 0.08f), WarmGlow, SurfaceKind.Plain);
                    CityVisualMaterials.ApplyEmissiveHint(win.GetComponent<Renderer>(), WarmGlow, 0.5f);
                }
            }
        }

        private static void PyramidRoof(Transform root, Vector3 baseCenter, float width, float height, Color color)
        {
            var accent = new GameObject("Accent_KeepRoof");
            accent.transform.SetParent(root, false);
            for (var i = 0; i < 4; i++)
            {
                var t = i / 3f;
                var y = baseCenter.y + height * t;
                var w = Mathf.Lerp(width, width * 0.22f, t);
                Part(PrimitiveType.Cube, accent.transform, new Vector3(baseCenter.x, y, baseCenter.z),
                    new Vector3(w, height / 4f * 1.05f, w),
                    Color.Lerp(color, Color.black, t * 0.1f), SurfaceKind.Plain);
            }

            Part(PrimitiveType.Cube, accent.transform, baseCenter + Vector3.up * (height * 0.35f),
                new Vector3(width * 0.55f, 0.08f, width * 0.55f), Gold, SurfaceKind.Plain);
        }

        private static void ConeRoof(Transform root, Vector3 tip, float width, float height, Color color)
        {
            var accent = new GameObject("Accent_ConeRoof");
            accent.transform.SetParent(root, false);
            for (var i = 0; i < 3; i++)
            {
                var t = i / 2f;
                var y = tip.y - height * (1f - t) * 0.75f;
                var w = Mathf.Lerp(width, width * 0.28f, t);
                Part(PrimitiveType.Cube, accent.transform, new Vector3(tip.x, y, tip.z),
                    new Vector3(w, height / 3f * 1.05f, w),
                    Color.Lerp(color, Color.black, t * 0.1f), SurfaceKind.Plain);
            }
        }

        private static void GoldFinial(Transform root, Vector3 tip)
        {
            var accent = new GameObject("Accent_Finial");
            accent.transform.SetParent(root, false);
            Part(PrimitiveType.Cylinder, accent.transform, tip + Vector3.down * 0.15f, new Vector3(0.08f, 0.2f, 0.08f), Gold, SurfaceKind.Plain);
            Part(PrimitiveType.Sphere, accent.transform, tip + Vector3.up * 0.05f, Vector3.one * 0.22f, Gold, SurfaceKind.Plain);
        }

        /// <summary>Bandeira azul com brasão do leão dourado.</summary>
        private static void CrestBanner(Transform root, Vector3 tip)
        {
            var banner = new GameObject("CrestBanner");
            banner.transform.SetParent(root, false);
            banner.transform.localPosition = tip;

            Part(PrimitiveType.Cylinder, banner.transform, new Vector3(0f, 0.2f, 0f),
                new Vector3(0.07f, 0.45f, 0.07f), Wood, SurfaceKind.Wood);
            var cloth = Part(PrimitiveType.Cube, banner.transform, new Vector3(0.38f, -0.25f, 0f),
                new Vector3(0.72f, 1.15f, 0.06f), BannerBlue, SurfaceKind.Plain);
            cloth.name = "BannerCloth";
            LionCrest(cloth.transform, new Vector3(0f, 0.12f, 0.7f), 0.48f);
        }

        /// <summary>Brasão: leão dourado estilizado (porta e bandeiras).</summary>
        private static void LionCrest(Transform parent, Vector3 localPos, float scale)
        {
            var crest = new GameObject("LionCrest");
            crest.transform.SetParent(parent, false);
            crest.transform.localPosition = localPos;
            crest.transform.localScale = Vector3.one * scale;

            // Escudo.
            Part(PrimitiveType.Cube, crest.transform, new Vector3(0f, 0f, 0f),
                new Vector3(0.7f, 0.9f, 0.08f), new Color(0.12f, 0.14f, 0.22f), SurfaceKind.Plain);
            // Corpo.
            Part(PrimitiveType.Cube, crest.transform, new Vector3(0f, -0.05f, 0.12f),
                new Vector3(0.28f, 0.38f, 0.12f), Gold, SurfaceKind.Metal);
            // Cabeça + juba.
            Part(PrimitiveType.Sphere, crest.transform, new Vector3(0f, 0.28f, 0.14f),
                Vector3.one * 0.28f, Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, crest.transform, new Vector3(0f, 0.28f, 0.08f),
                new Vector3(0.42f, 0.28f, 0.1f), Color.Lerp(Gold, new Color(0.85f, 0.55f, 0.15f), 0.35f), SurfaceKind.Metal);
            // Patas.
            Part(PrimitiveType.Cube, crest.transform, new Vector3(-0.16f, -0.32f, 0.12f),
                new Vector3(0.1f, 0.22f, 0.1f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, crest.transform, new Vector3(0.16f, -0.32f, 0.12f),
                new Vector3(0.1f, 0.22f, 0.1f), Gold, SurfaceKind.Metal);
            // Cauda.
            Part(PrimitiveType.Cube, crest.transform, new Vector3(0.22f, 0.05f, 0.1f),
                new Vector3(0.22f, 0.08f, 0.08f), Gold, SurfaceKind.Metal);
        }

        private static void GateEagle(Transform root, Vector3 pos)
        {
            var eagle = new GameObject("GateEagle");
            eagle.transform.SetParent(root, false);
            eagle.transform.localPosition = pos;

            Part(PrimitiveType.Sphere, eagle.transform, Vector3.zero, Vector3.one * 0.35f, Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, eagle.transform, new Vector3(-0.45f, 0.05f, 0f),
                new Vector3(0.55f, 0.12f, 0.28f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, eagle.transform, new Vector3(0.45f, 0.05f, 0f),
                new Vector3(0.55f, 0.12f, 0.28f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, eagle.transform, new Vector3(0f, 0.28f, 0.05f),
                new Vector3(0.18f, 0.22f, 0.18f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, eagle.transform, new Vector3(0f, -0.25f, 0f),
                new Vector3(0.12f, 0.28f, 0.12f), Gold, SurfaceKind.Metal);
        }

        private static void KnightStatue(Transform root, Vector3 pos)
        {
            var knight = new GameObject("KnightStatue");
            knight.transform.SetParent(root, false);
            knight.transform.localPosition = pos;

            Part(PrimitiveType.Cube, knight.transform, new Vector3(0f, 0.15f, 0f),
                new Vector3(0.55f, 0.3f, 0.55f), StoneDark, SurfaceKind.Stone);
            Part(PrimitiveType.Cube, knight.transform, new Vector3(0f, 0.7f, 0f),
                new Vector3(0.35f, 0.7f, 0.28f), Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Sphere, knight.transform, new Vector3(0f, 1.2f, 0f),
                Vector3.one * 0.28f, Gold, SurfaceKind.Metal);
            Part(PrimitiveType.Cube, knight.transform, new Vector3(0.22f, 0.75f, 0f),
                new Vector3(0.08f, 0.55f, 0.08f), Gold, SurfaceKind.Metal);
        }

        private static void Lantern(Transform root, Vector3 pos)
        {
            Part(PrimitiveType.Cylinder, root, pos, new Vector3(0.12f, 0.35f, 0.12f), StoneDark, SurfaceKind.Stone);
            var flame = Part(PrimitiveType.Sphere, root, pos + Vector3.up * 0.4f, Vector3.one * 0.22f, WarmGlow, SurfaceKind.Plain);
            CityVisualMaterials.ApplyEmissiveHint(flame.GetComponent<Renderer>(), WarmGlow, 0.65f);
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
    }
}
