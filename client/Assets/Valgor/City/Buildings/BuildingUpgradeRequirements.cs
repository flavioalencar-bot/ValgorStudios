using System;
using System.Collections.Generic;
using Valgor.City.Data;

namespace Valgor.City.Buildings
{
    public readonly struct UpgradeResourceRequirement
    {
        public UpgradeResourceRequirement(ResourceType resource, long available, long required)
        {
            Resource = resource;
            Available = available;
            Required = required;
        }

        public ResourceType Resource { get; }
        public long Available { get; }
        public long Required { get; }
        public bool Satisfied => Available >= Required;
    }

    public static class BuildingUpgradeRequirements
    {
        public static readonly ResourceType[] DisplayOrder =
        {
            ResourceType.Gold,
            ResourceType.Food,
            ResourceType.Wood,
            ResourceType.Stone,
            ResourceType.Iron,
            ResourceType.DragonEssence
        };

        public static IReadOnlyList<UpgradeResourceRequirement> Build(
            BuildingDefinition definition,
            BuildingInstance building,
            ResourceWallet wallet)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (building == null) throw new ArgumentNullException(nameof(building));
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));

            var list = new List<UpgradeResourceRequirement>(DisplayOrder.Length);
            foreach (var resource in DisplayOrder)
            {
                var required = definition.GetUpgradeCost(resource, building.Level);
                list.Add(new UpgradeResourceRequirement(resource, wallet.Get(resource), required));
            }

            return list;
        }

        /// <summary>Custo em diamantes para concluir agora (beta).</summary>
        public static long InstantCompleteDiamondCost(TimeSpan remaining)
        {
            if (remaining <= TimeSpan.Zero)
            {
                return 0;
            }

            return Math.Max(1, (long)Math.Ceiling(remaining.TotalSeconds / 5.0));
        }
    }
}
