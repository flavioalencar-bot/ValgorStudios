using System;
using System.Collections.Generic;
using Valgor.City.Data;

namespace Valgor.City.Economy
{
    /// <summary>Catálogo data-driven de pacotes/baús (sem loja/monetização).</summary>
    public static class ResourceItemCatalog
    {
        private static readonly ResourceItemDefinition[] Definitions =
        {
            // Prioridade baixa = usar primeiro (evitar desperdício de premium).
            Def("pack-food-small", "Pacote simples de comida", ResourceType.Food, 500, ResourceItemRarity.Common, 10),
            Def("pack-wood-small", "Pacote simples de madeira", ResourceType.Wood, 500, ResourceItemRarity.Common, 10),
            Def("pack-stone-small", "Pacote simples de pedra", ResourceType.Stone, 400, ResourceItemRarity.Common, 10),
            Def("pack-iron-small", "Pacote simples de ferro", ResourceType.Iron, 300, ResourceItemRarity.Common, 10),
            Def("pack-gold-small", "Pacote simples de ouro", ResourceType.Gold, 400, ResourceItemRarity.Common, 10),

            Def("box-food-basic", "Caixa básica de comida", ResourceType.Food, 2_500, ResourceItemRarity.Uncommon, 20),
            Def("box-wood-basic", "Caixa básica de madeira", ResourceType.Wood, 2_500, ResourceItemRarity.Uncommon, 20),
            Def("box-stone-basic", "Caixa básica de pedra", ResourceType.Stone, 2_000, ResourceItemRarity.Uncommon, 20),
            Def("box-iron-basic", "Caixa básica de ferro", ResourceType.Iron, 1_500, ResourceItemRarity.Uncommon, 20),
            Def("box-gold-basic", "Caixa básica de ouro", ResourceType.Gold, 2_000, ResourceItemRarity.Uncommon, 20),

            Def("chest-blue-food", "Baú azul de comida", ResourceType.Food, 12_000, ResourceItemRarity.Rare, 30),
            Def("chest-blue-wood", "Baú azul de madeira", ResourceType.Wood, 12_000, ResourceItemRarity.Rare, 30),
            Def("chest-blue-stone", "Baú azul de pedra", ResourceType.Stone, 10_000, ResourceItemRarity.Rare, 30),
            Def("chest-blue-iron", "Baú azul de ferro", ResourceType.Iron, 8_000, ResourceItemRarity.Rare, 30),
            Def("chest-blue-gold", "Baú azul de ouro", ResourceType.Gold, 10_000, ResourceItemRarity.Rare, 30),

            Def("chest-purple-food", "Baú roxo de comida", ResourceType.Food, 50_000, ResourceItemRarity.Epic, 40),
            Def("chest-purple-wood", "Baú roxo de madeira", ResourceType.Wood, 50_000, ResourceItemRarity.Epic, 40),
            Def("chest-purple-stone", "Baú roxo de pedra", ResourceType.Stone, 40_000, ResourceItemRarity.Epic, 40),
            Def("chest-purple-iron", "Baú roxo de ferro", ResourceType.Iron, 35_000, ResourceItemRarity.Epic, 40),
            Def("chest-purple-gold", "Baú roxo de ouro", ResourceType.Gold, 40_000, ResourceItemRarity.Epic, 40),

            Def("crate-select-food", "Caixa de seleção — comida", ResourceType.Food, 25_000, ResourceItemRarity.Epic, 45, selectable: true),
            Def("crate-select-wood", "Caixa de seleção — madeira", ResourceType.Wood, 25_000, ResourceItemRarity.Epic, 45, selectable: true),
            Def("crate-select-stone", "Caixa de seleção — pedra", ResourceType.Stone, 20_000, ResourceItemRarity.Epic, 45, selectable: true),

            Def("pack-premium-food", "Recompensa premium de comida", ResourceType.Food, 100_000, ResourceItemRarity.Premium, 90),
            Def("pack-premium-wood", "Recompensa premium de madeira", ResourceType.Wood, 100_000, ResourceItemRarity.Premium, 90),
            Def("pack-essence", "Essência concentrada", ResourceType.DragonEssence, 50, ResourceItemRarity.Rare, 30),
        };

        public static IReadOnlyList<ResourceItemDefinition> All => Definitions;

        public static bool TryGet(string itemId, out ResourceItemDefinition definition)
        {
            foreach (var d in Definitions)
            {
                if (string.Equals(d.ItemId, itemId, StringComparison.Ordinal))
                {
                    definition = d;
                    return true;
                }
            }

            definition = null!;
            return false;
        }

        public static IEnumerable<ResourceItemDefinition> ForResource(ResourceType resource)
        {
            foreach (var d in Definitions)
            {
                if (d.ResourceId == resource)
                {
                    yield return d;
                }
            }
        }

        private static ResourceItemDefinition Def(
            string id,
            string name,
            ResourceType resource,
            long value,
            ResourceItemRarity rarity,
            int priority,
            bool selectable = false) =>
            new(id, name, resource, value, rarity, priority, selectable);
    }
}
