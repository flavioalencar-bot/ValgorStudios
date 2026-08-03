using System;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Catálogo lógico das categorias de material do Art Pass 1.0 (sem UnityEngine).
    /// </summary>
    public enum CityArtMaterialCategory
    {
        StoneLight = 0,
        StoneDark = 1,
        Wood = 2,
        Roof = 3,
        Dirt = 4,
        Grass = 5,
        Water = 6,
        Metal = 7
    }

    public static class CityArtMaterialCatalog
    {
        public const int CategoryCount = 8;

        public static readonly CityArtMaterialCategory[] AllCategories =
        {
            CityArtMaterialCategory.StoneLight,
            CityArtMaterialCategory.StoneDark,
            CityArtMaterialCategory.Wood,
            CityArtMaterialCategory.Roof,
            CityArtMaterialCategory.Dirt,
            CityArtMaterialCategory.Grass,
            CityArtMaterialCategory.Water,
            CityArtMaterialCategory.Metal
        };

        public static string DisplayName(CityArtMaterialCategory category) => category switch
        {
            CityArtMaterialCategory.StoneLight => "pedra clara",
            CityArtMaterialCategory.StoneDark => "pedra escura",
            CityArtMaterialCategory.Wood => "madeira",
            CityArtMaterialCategory.Roof => "telhado",
            CityArtMaterialCategory.Dirt => "terra",
            CityArtMaterialCategory.Grass => "grama",
            CityArtMaterialCategory.Water => "água",
            CityArtMaterialCategory.Metal => "metal",
            _ => category.ToString()
        };

        public static void ValidateInvariants(Action<string> issue)
        {
            if (AllCategories.Length != CategoryCount)
            {
                issue($"Esperadas {CategoryCount} categorias, encontradas {AllCategories.Length}.");
            }

            var seen = new bool[CategoryCount];
            foreach (var c in AllCategories)
            {
                var i = (int)c;
                if (i < 0 || i >= CategoryCount)
                {
                    issue($"Categoria fora do intervalo: {c}");
                    continue;
                }

                if (seen[i])
                {
                    issue($"Categoria duplicada: {c}");
                }

                seen[i] = true;
            }

            for (var i = 0; i < CategoryCount; i++)
            {
                if (!seen[i])
                {
                    issue($"Categoria ausente: {(CityArtMaterialCategory)i}");
                }
            }
        }
    }
}
