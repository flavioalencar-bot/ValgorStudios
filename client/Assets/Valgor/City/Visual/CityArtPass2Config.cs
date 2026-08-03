namespace Valgor.City.Visual
{
    /// <summary>Constantes do Art Pass 2.0 (sem UnityEngine — testável).</summary>
    public static class CityArtPass2Config
    {
        public const bool Enabled = true;
        public const int NatureSpeciesCount = 11;
        public const int NpcCategoryCount = 9;
        public const int FaunaKindCount = 5;
        public const int DistrictAmbienceCount = 8;
        public const int DistrictRoadStyles = 6;

        public static readonly string[] NatureSpecies =
        {
            "arvore-grande", "arvore-media", "pinheiro", "arvore-seca", "arbusto",
            "flores", "samambaia", "musgo", "graminea", "raiz", "pedra-natural"
        };

        public static readonly string[] NpcCategories =
        {
            "guarda", "mercador", "ferreiro", "minerador", "lenhador",
            "fazendeiro", "sacerdote", "curandeiro", "mensageiro"
        };

        public static readonly string[] FaunaKinds =
        {
            "passaro", "corvo", "borboleta", "peixe", "vagalume"
        };

        public static readonly string[] DistrictAmbiences =
        {
            "mercado", "praca", "castelo", "templo", "fazenda", "forja", "rio", "torre-dragoes"
        };

        public static readonly string[] RoadStyles =
        {
            "imperial", "militar", "industrial", "rural", "sagrado", "mercado"
        };
    }
}
