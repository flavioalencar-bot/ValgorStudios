using System;

namespace Valgor.City.UI
{
    /// <summary>
    /// Ações do menu contextual de edifício (reutilizam sistemas existentes).
    /// </summary>
    public enum BuildingContextAction
    {
        Details,
        Upgrade,
        Collect,
        Produce,
        Train,
        Research,
        Open,
        Send,
        Feed,
        /// <summary>Gancho futuro de skins/decoração (placeholder UI).</summary>
        Decoration
    }

    /// <summary>Ícone visual do botão circular (glyphs vetoriais simples).</summary>
    public enum BuildingContextIcon
    {
        None,
        Brush,
        Info,
        Upgrade,
        Collect,
        Open,
        Feed,
        Send,
        Train,
        Research,
        Produce
    }

    public readonly struct BuildingContextActionInfo
    {
        public BuildingContextActionInfo(
            BuildingContextAction action,
            string label,
            bool enabled,
            string? disabledReason = null,
            BuildingContextIcon icon = BuildingContextIcon.None)
        {
            Action = action;
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Enabled = enabled;
            DisabledReason = disabledReason;
            Icon = icon == BuildingContextIcon.None ? ResolveDefaultIcon(action) : icon;
        }

        public BuildingContextAction Action { get; }
        public string Label { get; }
        public bool Enabled { get; }
        public string? DisabledReason { get; }
        public BuildingContextIcon Icon { get; }

        public static BuildingContextIcon ResolveDefaultIcon(BuildingContextAction action) =>
            action switch
            {
                BuildingContextAction.Decoration => BuildingContextIcon.Brush,
                BuildingContextAction.Details => BuildingContextIcon.Info,
                BuildingContextAction.Upgrade => BuildingContextIcon.Upgrade,
                BuildingContextAction.Collect => BuildingContextIcon.Collect,
                BuildingContextAction.Open => BuildingContextIcon.Open,
                BuildingContextAction.Feed => BuildingContextIcon.Feed,
                BuildingContextAction.Send => BuildingContextIcon.Send,
                BuildingContextAction.Train => BuildingContextIcon.Train,
                BuildingContextAction.Research => BuildingContextIcon.Research,
                BuildingContextAction.Produce => BuildingContextIcon.Produce,
                _ => BuildingContextIcon.Info
            };
    }
}
