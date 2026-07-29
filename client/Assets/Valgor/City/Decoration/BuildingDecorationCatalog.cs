using System;
using System.Collections.Generic;

namespace Valgor.City.Decoration
{
    /// <summary>
    /// Skin/decoração de edifício — estrutura futura (sem loja/backend agora).
    /// </summary>
    public sealed class BuildingSkinDefinition
    {
        public BuildingSkinDefinition(
            string id,
            string displayName,
            string buildingDefinitionId,
            bool unlocked = false)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            BuildingDefinitionId = buildingDefinitionId
                                   ?? throw new ArgumentNullException(nameof(buildingDefinitionId));
            Unlocked = unlocked;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string BuildingDefinitionId { get; }
        public bool Unlocked { get; }
    }

    /// <summary>
    /// Catálogo mínimo para o gancho `decoration`.
    /// Hoje só lista placeholders do Castelo; a UI mostra "em breve".
    /// </summary>
    public static class BuildingDecorationCatalog
    {
        public const string ActionId = "decoration";
        public const string ComingSoonMessage = "Sistema de skins em breve";

        private static readonly IReadOnlyList<BuildingSkinDefinition> CastleSkins =
            new[]
            {
                new BuildingSkinDefinition("castle-default", "Castelo clássico", "castle", unlocked: true),
                new BuildingSkinDefinition("castle-royal", "Castelo real", "castle", unlocked: false),
                new BuildingSkinDefinition("castle-obsidian", "Castelo de obsidiana", "castle", unlocked: false)
            };

        public static bool SupportsDecoration(string buildingDefinitionId) =>
            string.Equals(buildingDefinitionId, "castle", StringComparison.Ordinal);

        public static IReadOnlyList<BuildingSkinDefinition> ListSkins(string buildingDefinitionId)
        {
            if (SupportsDecoration(buildingDefinitionId))
            {
                return CastleSkins;
            }

            return Array.Empty<BuildingSkinDefinition>();
        }
    }
}
