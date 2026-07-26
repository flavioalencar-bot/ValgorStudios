namespace Valgor.Core
{
    /// <summary>
    /// Dicas de foco entre cenas (ex.: abrir Torre dos Dragões ao voltar à cidade).
    /// </summary>
    public static class BetaFocusHints
    {
        public const string DragonTowerBuildingId = "dragon-tower";

        public static string? PendingBuildingDefinitionId { get; set; }

        public static void RequestDragonTower() =>
            PendingBuildingDefinitionId = DragonTowerBuildingId;

        public static bool TryConsumeBuildingFocus(out string definitionId)
        {
            definitionId = PendingBuildingDefinitionId ?? string.Empty;
            if (string.IsNullOrEmpty(definitionId))
            {
                return false;
            }

            PendingBuildingDefinitionId = null;
            return true;
        }
    }
}
