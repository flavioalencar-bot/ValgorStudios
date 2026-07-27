using System;
using System.Text;
using Valgor.City.Data;
using Valgor.City.Production;

namespace Valgor.City.Buildings
{
    /// <summary>Texto de Detalhes/benefício para edifícios de produção (Fazenda, Serraria, Pedreira, Mina).</summary>
    public static class ProductionBuildingDetails
    {
        public static string BuildBlock(
            BuildingInstance building,
            ResourceProductionService production)
        {
            if (building == null) throw new ArgumentNullException(nameof(building));
            if (production == null) throw new ArgumentNullException(nameof(production));
            if (!ProductionCatalog.TryGet(building.DefinitionId, out var definition))
            {
                return string.Empty;
            }

            var rate = production.GetRatePerHour(building);
            var capacity = production.GetCapacity(building);
            production.TryGetState(building.DefinitionId, out var state);
            var accumulated = state?.Accumulated ?? 0L;
            var resource = FriendlyResource(definition.Resource);
            var nextRate = definition.GetRatePerHour(building.Level + 1);
            var nextCapacity = definition.GetCapacity(building.Level + 1);

            var sb = new StringBuilder();
            sb.AppendLine($"Produção de {resource}: {rate:0.#}/h");
            sb.AppendLine($"Produção armazenada: {accumulated:N0}");
            sb.AppendLine($"Capacidade: {capacity:N0}");
            sb.AppendLine($"Tempo até lotar: {FormatTimeToFill(accumulated, capacity, rate)}");
            sb.AppendLine(
                building.Level > 0
                    ? $"Bônus atuais: Nv.{building.Level} (taxa ×{building.Level}, capacidade ×{building.Level})"
                    : "Bônus atuais: — (edifício inativo)");
            sb.Append(
                $"Próximo benefício: {nextRate:0.#}/h · capacidade {nextCapacity:N0} no Nv.{building.Level + 1}");
            return sb.ToString();
        }

        public static string DescribeUpgradeBenefit(string buildingDefinitionId)
        {
            if (!ProductionCatalog.TryGet(buildingDefinitionId, out var definition))
            {
                return string.Empty;
            }

            return definition.Resource switch
            {
                ResourceType.Food => "Aumenta taxa e capacidade de comida",
                ResourceType.Wood => "Aumenta taxa e capacidade de madeira",
                ResourceType.Stone => "Aumenta taxa e capacidade de pedra",
                ResourceType.Iron => "Aumenta taxa e capacidade de ferro",
                ResourceType.Gold => "Aumenta taxa e capacidade de ouro",
                ResourceType.DragonEssence => "Aumenta taxa e capacidade de essência",
                _ => $"Aumenta produção de {FriendlyResource(definition.Resource)}"
            };
        }

        public static string FormatTimeToFill(long accumulated, long capacity, double ratePerHour)
        {
            if (capacity <= 0)
            {
                return "—";
            }

            if (accumulated >= capacity)
            {
                return "Cheio";
            }

            if (ratePerHour <= 0)
            {
                return "Sem produção";
            }

            var hours = (capacity - accumulated) / ratePerHour;
            if (hours < 1.0 / 60.0)
            {
                return "<1 min";
            }

            if (hours < 1)
            {
                return $"{Math.Max(1, (int)Math.Ceiling(hours * 60))} min";
            }

            var wholeHours = (int)Math.Floor(hours);
            var minutes = (int)Math.Round((hours - wholeHours) * 60);
            return minutes <= 0 ? $"{wholeHours}h" : $"{wholeHours}h {minutes}min";
        }

        public static string FriendlyResource(ResourceType resource) => resource switch
        {
            ResourceType.Gold => "Ouro",
            ResourceType.Food => "Comida",
            ResourceType.Wood => "Madeira",
            ResourceType.Stone => "Pedra",
            ResourceType.Iron => "Ferro",
            ResourceType.DragonEssence => "Essência de Dragão",
            ResourceType.Diamonds => "Diamantes",
            _ => resource.ToString()
        };
    }
}
