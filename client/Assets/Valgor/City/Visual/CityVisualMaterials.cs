using System.Collections.Generic;
using UnityEngine;
using Valgor.Core;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Materiais com tiling em espaço de mundo (evita textura esticada).
    /// </summary>
    public static class CityVisualMaterials
    {
        public static readonly Color StoneLight = new(0.56f, 0.53f, 0.48f);
        public static readonly Color StoneDark = new(0.32f, 0.3f, 0.28f);
        public static readonly Color Wood = new(0.4f, 0.28f, 0.16f);
        public static readonly Color RoofRed = new(0.52f, 0.22f, 0.16f);
        public static readonly Color RoofBlue = new(0.2f, 0.28f, 0.46f);
        public static readonly Color Gold = new(0.74f, 0.6f, 0.3f);
        public static readonly Color Vegetation = new(0.32f, 0.46f, 0.28f);
        public static readonly Color Dirt = new(0.44f, 0.36f, 0.26f);
        public static readonly Color Path = new(0.5f, 0.46f, 0.4f);

        /// <summary>Repetições por unidade de mundo — densidade estável entre objetos.</summary>
        private const float TilesPerMeter = 0.55f;

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
            if (mat == null || kind == SurfaceKind.Plain)
            {
                return;
            }

            var tex = GetNoise(kind);
            var tiling = WorldTiling(renderer.transform.lossyScale);
            if (mat.HasProperty("_MainTex"))
            {
                mat.SetTexture("_MainTex", tex);
                mat.mainTextureScale = tiling;
                mat.mainTextureOffset = Vector2.zero;
            }

            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", tex);
            }
        }

        /// <summary>Emissivo discreto (brasas / runas) — multiplica cor base.</summary>
        public static void ApplyEmissiveHint(Renderer renderer, Color glow, float intensity = 0.35f)
        {
            if (renderer == null)
            {
                return;
            }

            var lit = Color.Lerp(ReadColor(renderer), glow, intensity);
            lit = new Color(
                Mathf.Min(1f, lit.r * (1f + intensity * 0.4f)),
                Mathf.Min(1f, lit.g * (1f + intensity * 0.25f)),
                Mathf.Min(1f, lit.b * (1f + intensity * 0.15f)),
                1f);
            RuntimeSafeMaterials.Apply(renderer, lit);
        }

        public static Color MixState(Color identity, BuildingStateTint tint) => tint switch
        {
            BuildingStateTint.Ready => identity,
            BuildingStateTint.Available => Color.Lerp(identity, new Color(0.95f, 0.78f, 0.35f), 0.35f),
            BuildingStateTint.Locked => Color.Lerp(identity, new Color(0.22f, 0.24f, 0.28f), 0.72f),
            _ => identity
        };

        private static Vector2 WorldTiling(Vector3 lossyScale)
        {
            var ax = Mathf.Abs(lossyScale.x);
            var ay = Mathf.Abs(lossyScale.y);
            var az = Mathf.Abs(lossyScale.z);

            // Planos de chão / caminhos (Y fino): tiling em XZ.
            if (ay < 0.45f && Mathf.Max(ax, az) > 1.2f)
            {
                return new Vector2(
                    Mathf.Max(0.4f, ax * TilesPerMeter),
                    Mathf.Max(0.4f, az * TilesPerMeter));
            }

            // Paredes / volumes: face dominante.
            var u = Mathf.Max(ax, az) * TilesPerMeter;
            var v = Mathf.Max(0.35f, ay * TilesPerMeter);
            return new Vector2(Mathf.Max(0.4f, u), Mathf.Max(0.4f, v));
        }

        private static Texture2D GetNoise(SurfaceKind kind)
        {
            if (NoiseCache.TryGetValue(kind, out var cached) && cached != null)
            {
                return cached;
            }

            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"CitySurf_{kind}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat,
                anisoLevel = 2
            };

            var seed = (int)kind * 97 + 13;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var n = Hash01(x + seed, y + seed * 3);
                    var m = Hash01(x / 4 + seed, y / 4);
                    // Contraste baixo — evita “ruído de protótipo”.
                    var v = kind switch
                    {
                        SurfaceKind.Stone => 0.88f + n * 0.08f + m * 0.04f,
                        SurfaceKind.Wood => 0.9f + n * 0.06f,
                        SurfaceKind.Roof => 0.9f + (n * 0.05f) + ((x + y) % 8 < 1 ? -0.04f : 0f),
                        SurfaceKind.Metal => 0.92f + n * 0.05f,
                        SurfaceKind.Vegetation => 0.88f + n * 0.1f,
                        SurfaceKind.Dirt => 0.86f + n * 0.1f,
                        SurfaceKind.Path => 0.9f + m * 0.06f + (n * 0.04f),
                        _ => 1f
                    };

                    if (kind == SurfaceKind.Wood)
                    {
                        v *= 0.94f + 0.06f * Mathf.Abs(Mathf.Sin(y * 0.22f));
                    }

                    // Pedra de caminho: junta suave (não tábuas).
                    if (kind == SurfaceKind.Path)
                    {
                        var mortar = (x % 10 == 0 || y % 8 == 0) ? 0.92f : 1f;
                        v *= mortar;
                    }

                    tex.SetPixel(x, y, new Color(v, v, v, 1f));
                }
            }

            tex.Apply(false, true);
            NoiseCache[kind] = tex;
            return tex;
        }

        private static Color ReadColor(Renderer renderer)
        {
            var material = renderer.sharedMaterial;
            if (material == null)
            {
                return Color.gray;
            }

            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }

            return material.color;
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
        Dirt,
        Path
    }

    public enum BuildingStateTint
    {
        Ready,
        Available,
        Locked
    }
}
