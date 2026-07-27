using System.Collections.Generic;
using UnityEngine;
using Valgor.Core;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Materiais provisórios com variação (noise) — nunca magenta no URP.
    /// </summary>
    public static class CityVisualMaterials
    {
        public static readonly Color StoneLight = new(0.58f, 0.55f, 0.5f);
        public static readonly Color StoneDark = new(0.34f, 0.32f, 0.3f);
        public static readonly Color Wood = new(0.38f, 0.26f, 0.14f);
        public static readonly Color RoofRed = new(0.58f, 0.24f, 0.16f);
        public static readonly Color RoofBlue = new(0.18f, 0.26f, 0.48f);
        public static readonly Color Gold = new(0.78f, 0.62f, 0.28f);
        public static readonly Color Vegetation = new(0.3f, 0.48f, 0.26f);
        public static readonly Color Dirt = new(0.42f, 0.34f, 0.22f);
        public static readonly Color Path = new(0.46f, 0.42f, 0.36f);

        private static readonly Dictionary<SurfaceKind, Texture2D> NoiseCache = new();

        public static void Apply(Renderer renderer, Color color) =>
            ApplySurface(renderer, color, SurfaceKind.Plain);

        public static void ApplySurface(Renderer renderer, Color color, SurfaceKind kind)
        {
            if (renderer == null)
            {
                return;
            }

            RuntimeSafeMaterials.Apply(renderer, color);
            var mat = renderer.material;
            if (mat == null)
            {
                return;
            }

            if (kind == SurfaceKind.Plain)
            {
                return;
            }

            var tex = GetNoise(kind);
            if (mat.HasProperty("_MainTex"))
            {
                mat.SetTexture("_MainTex", tex);
                mat.mainTextureScale = new Vector2(2.2f, 2.2f);
            }

            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", tex);
            }
        }

        public static Color MixState(Color identity, BuildingStateTint tint) => tint switch
        {
            BuildingStateTint.Ready => identity,
            BuildingStateTint.Available => Color.Lerp(identity, new Color(0.95f, 0.78f, 0.35f), 0.35f),
            BuildingStateTint.Locked => Color.Lerp(identity, new Color(0.22f, 0.24f, 0.28f), 0.72f),
            _ => identity
        };

        private static Texture2D GetNoise(SurfaceKind kind)
        {
            if (NoiseCache.TryGetValue(kind, out var cached) && cached != null)
            {
                return cached;
            }

            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"CityNoise_{kind}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat
            };

            var seed = (int)kind * 97 + 13;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var n = Hash01(x + seed, y + seed * 3);
                    var m = Hash01(x * 3 + seed, y * 2);
                    var v = kind switch
                    {
                        SurfaceKind.Stone => 0.72f + n * 0.28f,
                        SurfaceKind.Wood => 0.7f + Mathf.Lerp(n, m, 0.4f) * 0.3f,
                        SurfaceKind.Roof => 0.68f + n * 0.32f,
                        SurfaceKind.Metal => 0.78f + n * 0.22f,
                        SurfaceKind.Vegetation => 0.65f + n * 0.35f,
                        SurfaceKind.Dirt => 0.62f + n * 0.38f,
                        _ => 1f
                    };
                    // Faixas leves na madeira.
                    if (kind == SurfaceKind.Wood)
                    {
                        v *= 0.85f + 0.15f * Mathf.Abs(Mathf.Sin((y + n * 4f) * 0.55f));
                    }

                    tex.SetPixel(x, y, new Color(v, v, v, 1f));
                }
            }

            tex.Apply(false, true);
            NoiseCache[kind] = tex;
            return tex;
        }

        private static float Hash01(int x, int y)
        {
            var h = x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0x7fffffff) / (float)0x7fffffff;
        }
    }

    public enum SurfaceKind
    {
        Plain,
        Stone,
        Wood,
        Roof,
        Metal,
        Vegetation,
        Dirt
    }

    public enum BuildingStateTint
    {
        Ready,
        Available,
        Locked
    }
}
