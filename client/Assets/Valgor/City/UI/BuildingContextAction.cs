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
        Send
    }

    public readonly struct BuildingContextActionInfo
    {
        public BuildingContextActionInfo(
            BuildingContextAction action,
            string label,
            bool enabled,
            string? disabledReason = null)
        {
            Action = action;
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Enabled = enabled;
            DisabledReason = disabledReason;
        }

        public BuildingContextAction Action { get; }
        public string Label { get; }
        public bool Enabled { get; }
        public string? DisabledReason { get; }
    }
}
