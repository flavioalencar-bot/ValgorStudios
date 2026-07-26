using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Filters
{
    /// <summary>
    /// Resolve se um nó deve aparecer no mapa com base nos filtros ativos.
    /// Não altera estado de marcha, ocupação ou recursos.
    /// </summary>
    public static class WorldNodeVisibilityResolver
    {
        public static bool IsVisible(
            WorldMapNodeDefinition definition,
            WorldNodeInstance instance,
            WorldMapFilterState filters)
        {
            if (definition == null || instance == null || filters == null)
            {
                return false;
            }

            if (!IsKindEnabled(definition.Kind, filters))
            {
                return false;
            }

            return IsStatusVisible(instance, filters);
        }

        public static bool IsKindEnabled(WorldNodeKind kind, WorldMapFilterState filters) =>
            kind switch
            {
                WorldNodeKind.City => filters.ShowCities,
                WorldNodeKind.Village => filters.ShowVillages,
                WorldNodeKind.Resource => filters.ShowResources,
                WorldNodeKind.Creature => filters.ShowCreatures,
                WorldNodeKind.Dragon => filters.ShowDragons,
                WorldNodeKind.Landmark => filters.ShowLandmarks,
                _ => true
            };

        public static bool IsStatusVisible(WorldNodeInstance instance, WorldMapFilterState filters)
        {
            var occupied = IsOccupied(instance);
            var available = instance.Status == WorldNodeStatus.Available && !occupied;

            if (occupied)
            {
                return filters.ShowOccupied;
            }

            if (available)
            {
                return filters.ShowAvailable;
            }

            // Locked / Depleted / Respawning / Cleared: visíveis se o tipo está ativo.
            return true;
        }

        public static bool IsOccupied(WorldNodeInstance instance) =>
            instance.Status == WorldNodeStatus.Occupied ||
            !string.IsNullOrEmpty(instance.OccupiedByMarchId);
    }
}
