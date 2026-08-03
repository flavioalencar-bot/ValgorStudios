namespace Valgor.City.Visual
{
    /// <summary>
    /// Sprint 3 — Layer Architecture: elimina blockout dos 17 edifícios oficiais.
    /// Só visual; BuildingId / Save / seleção / construção / upgrade intactos.
    /// </summary>
    public static class CityArchitectureSprint3Config
    {
        public const bool Enabled = true;

        /// <summary>Remove anéis de casas coloridas / vernacular de protótipo.</summary>
        public const bool SuppressPrototypeHousingRings = true;

        /// <summary>Fundação + escadas + muros baixos em cada slot oficial.</summary>
        public const bool SiteIntegrationEnabled = true;

        /// <summary>Dressing temático por BuildingId (tendas, forjas, etc.).</summary>
        public const bool ThematicDressingEnabled = true;

        public const string DressingRootName = "ArchSprint3Dressing";

        /// <summary>17 BuildingIds oficiais — mesma ordem do produto.</summary>
        public static readonly string[] OfficialBuildings =
        {
            "castle", "wall", "dragon-tower", "market", "warehouse", "hospital", "arena",
            "academy", "training-ground", "residence", "foundry", "temple", "hero-hall",
            "farm", "lumbermill", "quarry", "mine"
        };

        /// <summary>HF externo ou proxy de alta fidelidade (procedural + pipeline).</summary>
        public static readonly string[] ArchitectureReady =
        {
            "castle", "wall", "dragon-tower", "market", "warehouse", "hospital", "arena",
            "academy", "training-ground", "residence", "foundry", "temple", "hero-hall",
            "farm", "lumbermill", "quarry", "mine"
        };

        public static readonly string[] Stages =
        {
            "proxies-oficiais", "dressing-tematico", "integracao-terreno",
            "eliminar-casas-coloridas", "materiais-pipeline"
        };
    }
}
