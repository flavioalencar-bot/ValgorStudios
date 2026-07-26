using System;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Core
{
    public sealed class WorldNodeSelectionService
    {
        public WorldNodeInstance? Selected { get; private set; }
        public event Action<WorldNodeInstance?>? SelectionChanged;

        public void Select(WorldNodeInstance node)
        {
            Selected = node ?? throw new ArgumentNullException(nameof(node));
            SelectionChanged?.Invoke(Selected);
        }

        public void Deselect()
        {
            Selected = null;
            SelectionChanged?.Invoke(null);
        }
    }
}
