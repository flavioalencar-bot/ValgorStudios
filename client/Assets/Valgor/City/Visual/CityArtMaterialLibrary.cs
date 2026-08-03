using System.Collections.Generic;
using UnityEngine;
using Valgor.Core;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Biblioteca reutilizável de materiais compartilhados (Art Pass 1.0).
    /// GPU Instancing + sharedMaterial — evita duplicatas por primitiva.
    /// </summary>
    public static class CityArtMaterialLibrary
    {
        private static readonly Dictionary<CityArtMaterialCategory, Material> Shared = new();
        private static readonly Dictionary<int, Material> VariantCache = new();

        public static Color ColorOf(CityArtMaterialCategory category) => category switch
        {
            CityArtMaterialCategory.StoneLight => CityVisualMaterials.StoneLight,
            CityArtMaterialCategory.StoneDark => CityVisualMaterials.StoneDark,
            CityArtMaterialCategory.Wood => CityVisualMaterials.Wood,
            CityArtMaterialCategory.Roof => CityVisualMaterials.RoofRed,
            CityArtMaterialCategory.Dirt => CityVisualMaterials.Dirt,
            CityArtMaterialCategory.Grass => CityVisualMaterials.Vegetation,
            CityArtMaterialCategory.Water => new Color(0.28f, 0.42f, 0.52f),
            CityArtMaterialCategory.Metal => CityVisualMaterials.Gold,
            _ => Color.gray
        };

        public static Material Get(CityArtMaterialCategory category)
        {
            if (Shared.TryGetValue(category, out var existing) && existing != null)
            {
                return existing;
            }

            var mat = RuntimeSafeMaterials.Create(ColorOf(category), $"CityArt_{category}");
            mat.enableInstancing = true;
            ApplyNoise(mat, ToSurfaceKind(category));
            Shared[category] = mat;
            return mat;
        }

        /// <summary>Variante tonal compartilhada (ex.: telhado azul, pedra um pouco mais clara).</summary>
        public static Material GetVariant(CityArtMaterialCategory category, Color tint, float mix = 0.35f)
        {
            var baseColor = ColorOf(category);
            var color = Color.Lerp(baseColor, tint, Mathf.Clamp01(mix));
            var key = Hash(category, color);
            if (VariantCache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var mat = RuntimeSafeMaterials.Create(color, $"CityArt_{category}_v{key:X8}");
            mat.enableInstancing = true;
            ApplyNoise(mat, ToSurfaceKind(category));
            VariantCache[key] = mat;
            return mat;
        }

        private static SurfaceKind ToSurfaceKind(CityArtMaterialCategory category) => category switch
        {
            CityArtMaterialCategory.StoneLight => SurfaceKind.Stone,
            CityArtMaterialCategory.StoneDark => SurfaceKind.Stone,
            CityArtMaterialCategory.Wood => SurfaceKind.Wood,
            CityArtMaterialCategory.Roof => SurfaceKind.Roof,
            CityArtMaterialCategory.Dirt => SurfaceKind.Dirt,
            CityArtMaterialCategory.Grass => SurfaceKind.Vegetation,
            CityArtMaterialCategory.Water => SurfaceKind.Plain,
            CityArtMaterialCategory.Metal => SurfaceKind.Metal,
            _ => SurfaceKind.Plain
        };

        public static void Apply(Renderer renderer, CityArtMaterialCategory category)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = Get(category);
        }

        public static void ApplyVariant(Renderer renderer, CityArtMaterialCategory category, Color tint, float mix = 0.35f)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = GetVariant(category, tint, mix);
        }

        public static int SharedCount => Shared.Count;
        public static int VariantCount => VariantCache.Count;

        public static void WarmPalette()
        {
            foreach (var category in CityArtMaterialCatalog.AllCategories)
            {
                Get(category);
            }
        }

        private static void ApplyNoise(Material mat, SurfaceKind kind)
        {
            if (mat == null || kind == SurfaceKind.Plain)
            {
                return;
            }

            // Reusa o noise da CityVisualMaterials via ApplySurface em um renderer temporário
            // seria caro — aplica tiling neutro; CityVisualMaterials continua para meshes tintados.
            if (mat.HasProperty("_MainTex"))
            {
                mat.mainTextureScale = new Vector2(2.2f, 2.2f);
            }
        }

        private static int Hash(CityArtMaterialCategory category, Color color)
        {
            unchecked
            {
                var r = Mathf.RoundToInt(color.r * 32f);
                var g = Mathf.RoundToInt(color.g * 32f);
                var b = Mathf.RoundToInt(color.b * 32f);
                return ((int)category * 397) ^ (r * 17) ^ (g * 31) ^ (b * 53);
            }
        }
    }
}
