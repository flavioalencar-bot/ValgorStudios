using System;
using Valgor.City.Data;

namespace Valgor.City.Core
{
    public sealed class BuildingSelectionService
    {
        public event Action<BuildingInstance?>? SelectionChanged;

        public BuildingInstance? Selected { get; private set; }

        public void Select(BuildingInstance building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            if (ReferenceEquals(Selected, building))
            {
                return;
            }

            Selected = building;
            SelectionChanged?.Invoke(Selected);
        }

        public void Deselect()
        {
            if (Selected == null)
            {
                return;
            }

            Selected = null;
            SelectionChanged?.Invoke(null);
        }
    }
}
