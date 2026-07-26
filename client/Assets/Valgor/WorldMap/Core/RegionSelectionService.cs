using System;

namespace Valgor.WorldMap.Core
{
    using Valgor.WorldMap.Data;

    public sealed class RegionSelectionService
    {
        public RegionInstance? Selected { get; private set; }
        public event Action<RegionInstance?>? SelectionChanged;

        public void Select(RegionInstance region)
        {
            Selected = region ?? throw new ArgumentNullException(nameof(region));
            SelectionChanged?.Invoke(Selected);
        }

        public void Deselect()
        {
            Selected = null;
            SelectionChanged?.Invoke(null);
        }
    }
}
