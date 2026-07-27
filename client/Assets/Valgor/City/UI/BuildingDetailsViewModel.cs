using System.Text;
using Valgor.City.Buildings;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.Production;

namespace Valgor.City.UI
{
    /// <summary>Dados do painel Detalhes (edifícios com UX contextual).</summary>
    public sealed class BuildingDetailsViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public static BuildingDetailsViewModel From(
            CityController city,
            BuildingInstance building,
            BuildingDefinition definition,
            bool openMode)
        {
            var sb = new StringBuilder();
            sb.AppendLine(definition.DisplayName);
            sb.AppendLine($"Nível: {building.Level}/{definition.MaxLevel}");
            sb.AppendLine($"Estado: {FriendlyState(building.State)}");

            if (string.Equals(building.DefinitionId, "castle", System.StringComparison.Ordinal))
            {
                sb.AppendLine("Função: coração da cidade — limita o nível dos demais edifícios.");
                sb.AppendLine($"Bônus atuais: Castelo Nv.{building.Level} (teto de upgrade).");
                sb.AppendLine($"Próximo nível: {System.Math.Min(definition.MaxLevel, building.Level + 1)}");
                sb.AppendLine($"Duração upgrade: {(int)definition.GetUpgradeDuration(building.Level).TotalSeconds}s");
                sb.AppendLine(DescribeRequirementsShort(city, building));
            }
            else if (string.Equals(building.DefinitionId, "warehouse", System.StringComparison.Ordinal))
            {
                sb.AppendLine($"Capacidade: {WarehouseRules.GetCapacity(building.Level):N0}");
                sb.AppendLine($"Proteção de recursos: {WarehouseRules.GetProtection(building.Level):N0}");
                sb.AppendLine(
                    $"Próximo benefício: capacidade {WarehouseRules.GetNextCapacity(building.Level):N0} · " +
                    $"proteção {WarehouseRules.GetNextProtection(building.Level):N0}");
                sb.AppendLine(DescribeRequirementsShort(city, building));
                if (openMode)
                {
                    sb.AppendLine();
                    sb.AppendLine("Armazém aberto — estoque e proteção da cidade.");
                }
            }
            else if (string.Equals(building.DefinitionId, "academy", System.StringComparison.Ordinal))
            {
                sb.AppendLine("Função: centro de conhecimento da cidade (pesquisas na beta seguinte).");
                sb.AppendLine(
                    building.Level > 0
                        ? $"Bônus atuais: Academia Nv.{building.Level} (desbloqueios futuros)."
                        : "Bônus atuais: — (ainda não construída).");
                sb.AppendLine($"Próximo benefício: eleva o teto acadêmico para Nv.{building.Level + 1}");
                sb.AppendLine(DescribeRequirementsShort(city, building));
            }
            else if (ProductionCatalog.TryGet(building.DefinitionId, out _))
            {
                sb.AppendLine(ProductionBuildingDetails.BuildBlock(building, city.Economy.Production));
            }

            if (building.State == BuildingState.Upgrading && building.UpgradeCompletesAtUtc.HasValue)
            {
                var remaining = building.UpgradeCompletesAtUtc.Value - city.Economy.Clock.UtcNow;
                if (remaining < System.TimeSpan.Zero)
                {
                    remaining = System.TimeSpan.Zero;
                }

                sb.AppendLine($"Melhorando → Nv.{building.Level + 1} ({(int)remaining.TotalSeconds}s)");
            }

            var block = city.GetUpgradeBlockReason(building, definition);
            if (!string.IsNullOrEmpty(block))
            {
                sb.AppendLine(block);
            }

            return new BuildingDetailsViewModel
            {
                Title = openMode
                    ? $"Abrir — {definition.DisplayName}"
                    : $"Detalhes — {definition.DisplayName}",
                Body = sb.ToString().Trim()
            };
        }

        private static string DescribeRequirementsShort(CityController city, BuildingInstance building)
        {
            var sb = new StringBuilder("Requisitos: ");
            var first = true;
            foreach (var req in city.GetUpgradeRequirements(building))
            {
                if (req.Required <= 0)
                {
                    continue;
                }

                if (!first)
                {
                    sb.Append(", ");
                }

                sb.Append($"{ProductionBuildingDetails.FriendlyResource(req.Resource)} {req.Required}");
                first = false;
            }

            return first ? "Requisitos: —" : sb.ToString();
        }

        private static string FriendlyState(BuildingState state) => state switch
        {
            BuildingState.Ready => "Pronto",
            BuildingState.Available => "Disponível",
            BuildingState.Locked => "Bloqueado",
            BuildingState.Upgrading => "Melhorando",
            _ => state.ToString()
        };
    }
}
