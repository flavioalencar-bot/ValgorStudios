using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.City.Data;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>
    /// Ícones próprios Valgor (texturas geradas, sem assets de terceiros).
    /// </summary>
    public static class ValgorUiIcons
    {
        private static readonly Dictionary<string, Texture2D> Cache = new();

        public static Texture2D ForResource(ResourceType resource) => resource switch
        {
            ResourceType.Gold => GetOrCreate("gold", new Color(0.92f, 0.75f, 0.28f), DrawCoin),
            ResourceType.Food => GetOrCreate("food", new Color(0.55f, 0.78f, 0.32f), DrawWheat),
            ResourceType.Wood => GetOrCreate("wood", new Color(0.55f, 0.38f, 0.22f), DrawLog),
            ResourceType.Stone => GetOrCreate("stone", new Color(0.62f, 0.62f, 0.66f), DrawRock),
            ResourceType.Iron => GetOrCreate("iron", new Color(0.55f, 0.6f, 0.72f), DrawIngot),
            ResourceType.DragonEssence => GetOrCreate("essence", new Color(0.48f, 0.38f, 0.85f), DrawCrystal),
            ResourceType.Diamonds => GetOrCreate("diamonds", new Color(0.55f, 0.78f, 0.95f), DrawDiamond),
            _ => GetOrCreate("unknown", BetaVisualTheme.TextMuted, DrawCoin)
        };

        public static Texture2D ForEnergy() =>
            GetOrCreate("energy", new Color(0.35f, 0.7f, 0.95f), DrawBolt);

        public static Texture2D ForBuildingRequirement() =>
            GetOrCreate("building-req", BetaVisualTheme.AgedGold, DrawTower);

        public static VisualElement CreateIconElement(Texture2D tex, float size = 22f)
        {
            var el = new VisualElement();
            el.style.width = size;
            el.style.height = size;
            el.style.flexShrink = 0;
            el.style.marginRight = 6;
            el.style.backgroundImage = new StyleBackground(Background.FromTexture2D(tex));
            el.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            el.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            el.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            el.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            el.pickingMode = PickingMode.Ignore;
            return el;
        }

        public static VisualElement CreateResourceChip(ResourceType resource, float size = 22f) =>
            CreateIconElement(ForResource(resource), size);

        private delegate void Paint(Color[] pixels, int size, Color accent);

        private static Texture2D GetOrCreate(string key, Color accent, Paint paint)
        {
            if (Cache.TryGetValue(key, out var existing) && existing != null)
            {
                return existing;
            }

            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"ValgorIcon_{key}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[size * size];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            // Fundo circular pedra escura.
            FillCircle(pixels, size, size * 0.5f, size * 0.5f, size * 0.46f, new Color(0.12f, 0.13f, 0.15f, 0.95f));
            FillRing(pixels, size, size * 0.5f, size * 0.5f, size * 0.46f, size * 0.4f, BetaVisualTheme.AgedGold);
            paint(pixels, size, accent);
            tex.SetPixels(pixels);
            tex.Apply(false, true);
            Cache[key] = tex;
            return tex;
        }

        private static void DrawCoin(Color[] px, int s, Color accent)
        {
            FillCircle(px, s, s * 0.5f, s * 0.5f, s * 0.28f, accent);
            FillCircle(px, s, s * 0.5f, s * 0.5f, s * 0.18f, Color.Lerp(accent, Color.white, 0.35f));
            FillCircle(px, s, s * 0.5f, s * 0.5f, s * 0.08f, new Color(0.45f, 0.32f, 0.1f, 1f));
        }

        private static void DrawWheat(Color[] px, int s, Color accent)
        {
            var stem = Color.Lerp(accent, new Color(0.35f, 0.45f, 0.15f), 0.3f);
            FillRect(px, s, (int)(s * 0.46f), (int)(s * 0.28f), (int)(s * 0.08f), (int)(s * 0.45f), stem);
            for (var i = 0; i < 5; i++)
            {
                var y = s * 0.55f + i * s * 0.06f;
                FillCircle(px, s, s * 0.38f, y, s * 0.06f, accent);
                FillCircle(px, s, s * 0.62f, y, s * 0.06f, accent);
            }

            FillCircle(px, s, s * 0.5f, s * 0.78f, s * 0.07f, Color.Lerp(accent, Color.yellow, 0.2f));
        }

        private static void DrawLog(Color[] px, int s, Color accent)
        {
            FillRect(px, s, (int)(s * 0.22f), (int)(s * 0.38f), (int)(s * 0.56f), (int)(s * 0.24f), accent);
            FillCircle(px, s, s * 0.78f, s * 0.5f, s * 0.12f, Color.Lerp(accent, new Color(0.85f, 0.7f, 0.45f), 0.45f));
            FillCircle(px, s, s * 0.78f, s * 0.5f, s * 0.05f, new Color(0.35f, 0.22f, 0.12f));
        }

        private static void DrawRock(Color[] px, int s, Color accent)
        {
            FillPolyBlob(px, s, accent, 0.5f, 0.48f, 0.26f);
            FillPolyBlob(px, s, Color.Lerp(accent, Color.white, 0.2f), 0.42f, 0.55f, 0.16f);
        }

        private static void DrawIngot(Color[] px, int s, Color accent)
        {
            FillRect(px, s, (int)(s * 0.28f), (int)(s * 0.4f), (int)(s * 0.44f), (int)(s * 0.22f), accent);
            FillRect(px, s, (int)(s * 0.24f), (int)(s * 0.48f), (int)(s * 0.52f), (int)(s * 0.1f), Color.Lerp(accent, Color.white, 0.25f));
        }

        private static void DrawCrystal(Color[] px, int s, Color accent)
        {
            FillDiamond(px, s, s * 0.5f, s * 0.5f, s * 0.22f, s * 0.32f, accent);
            FillDiamond(px, s, s * 0.5f, s * 0.48f, s * 0.1f, s * 0.16f, Color.Lerp(accent, Color.white, 0.4f));
        }

        private static void DrawDiamond(Color[] px, int s, Color accent)
        {
            FillDiamond(px, s, s * 0.5f, s * 0.52f, s * 0.2f, s * 0.28f, accent);
            FillRect(px, s, (int)(s * 0.38f), (int)(s * 0.36f), (int)(s * 0.24f), (int)(s * 0.08f), Color.Lerp(accent, Color.white, 0.35f));
        }

        private static void DrawBolt(Color[] px, int s, Color accent)
        {
            // Raio simplificado em três retângulos inclinados.
            FillRect(px, s, (int)(s * 0.48f), (int)(s * 0.22f), (int)(s * 0.14f), (int)(s * 0.28f), accent);
            FillRect(px, s, (int)(s * 0.36f), (int)(s * 0.42f), (int)(s * 0.28f), (int)(s * 0.12f), Color.Lerp(accent, Color.white, 0.25f));
            FillRect(px, s, (int)(s * 0.42f), (int)(s * 0.52f), (int)(s * 0.14f), (int)(s * 0.28f), accent);
        }

        private static void DrawTower(Color[] px, int s, Color accent)
        {
            FillRect(px, s, (int)(s * 0.38f), (int)(s * 0.3f), (int)(s * 0.24f), (int)(s * 0.4f), accent);
            FillRect(px, s, (int)(s * 0.32f), (int)(s * 0.28f), (int)(s * 0.36f), (int)(s * 0.1f), Color.Lerp(accent, Color.white, 0.2f));
            FillRect(px, s, (int)(s * 0.44f), (int)(s * 0.22f), (int)(s * 0.12f), (int)(s * 0.1f), accent);
        }

        private static void FillCircle(Color[] px, int s, float cx, float cy, float r, Color c)
        {
            var r2 = r * r;
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var dx = x + 0.5f - cx;
                var dy = y + 0.5f - cy;
                if (dx * dx + dy * dy <= r2)
                {
                    px[y * s + x] = c;
                }
            }
        }

        private static void FillRing(Color[] px, int s, float cx, float cy, float rOuter, float rInner, Color c)
        {
            var o2 = rOuter * rOuter;
            var i2 = rInner * rInner;
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var dx = x + 0.5f - cx;
                var dy = y + 0.5f - cy;
                var d = dx * dx + dy * dy;
                if (d <= o2 && d >= i2)
                {
                    px[y * s + x] = c;
                }
            }
        }

        private static void FillRect(Color[] px, int s, int x0, int y0, int w, int h, Color c)
        {
            for (var y = y0; y < y0 + h && y < s; y++)
            for (var x = x0; x < x0 + w && x < s; x++)
            {
                if (x >= 0 && y >= 0)
                {
                    px[y * s + x] = c;
                }
            }
        }

        private static void FillDiamond(Color[] px, int s, float cx, float cy, float halfW, float halfH, Color c)
        {
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var nx = Mathf.Abs(x + 0.5f - cx) / halfW;
                var ny = Mathf.Abs(y + 0.5f - cy) / halfH;
                if (nx + ny <= 1f)
                {
                    px[y * s + x] = c;
                }
            }
        }

        private static void FillPolyBlob(Color[] px, int s, Color c, float cxN, float cyN, float rN)
        {
            FillCircle(px, s, s * cxN, s * cyN, s * rN, c);
            FillCircle(px, s, s * (cxN + 0.08f), s * (cyN - 0.05f), s * (rN * 0.7f), Color.Lerp(c, Color.white, 0.15f));
        }
    }
}
