using UnityEngine;
using Valgor.City.Data;

namespace Valgor.City.Buildings
{
    public sealed class BuildingSlot : MonoBehaviour
    {
        [SerializeField] private string slotId = string.Empty;
        [SerializeField] private string startingDefinitionId = string.Empty;

        public string SlotId => slotId;
        public string StartingDefinitionId => startingDefinitionId;
        public BuildingInstance? Building { get; private set; }

        public void Initialize(string id, string definitionId, BuildingInstance building)
        {
            slotId = id;
            startingDefinitionId = definitionId;
            Building = building;
        }
    }
}
