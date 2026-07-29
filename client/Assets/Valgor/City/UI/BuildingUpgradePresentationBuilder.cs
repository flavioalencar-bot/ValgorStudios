using System;
using System.Collections.Generic;
using Valgor.City.Buildings;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.Economy;
using Valgor.City.Production;
using Valgor.City.Visual;
using Valgor.Core;

namespace Valgor.City.UI
{
    /// <summary>Monta view-models a partir do domínio (sem duplicar regras de custo/deps).</summary>
    public static class BuildingUpgradePresentationBuilder
    {
        public static BuildingUpgradePresentation Build(
            CityController city,
            BuildingInstance building,
            ResourceItemInventory? inventory = null)
        {
            var definition = city.GetDefinition(building);
            var isMax = building.Level >= definition.MaxLevel;
            var next = isMax ? building.Level : Math.Min(definition.MaxLevel, building.Level + 1);
            var duration = definition.GetUpgradeDuration(building.Level);
            var effective = CityProgressionQa.IsActive
                ? TimeSpan.FromSeconds(CityProgressionQa.HomologDurationSeconds)
                : duration;

            var remaining = duration;
            var upgrading = building.State == BuildingState.Upgrading && building.UpgradeCompletesAtUtc.HasValue;
            if (upgrading)
            {
                remaining = building.UpgradeCompletesAtUtc!.Value - city.Economy.Clock.UtcNow;
                if (remaining < TimeSpan.Zero)
                {
                    remaining = TimeSpan.Zero;
                }

                effective = CityProgressionQa.IsActive
                    ? TimeSpan.FromSeconds(Math.Min(CityProgressionQa.HomologDurationSeconds, remaining.TotalSeconds))
                    : remaining;
            }

            var benefit = BuildingBenefitCatalog.DescribeUpgrade(city, building, definition);
            var deps = city.GetDependencyChecks(building);
            var reqViews = new List<BuildingRequirementView>(deps.Count);
            foreach (var dep in deps)
            {
                var currentLevel = 0;
                if (!string.IsNullOrEmpty(dep.JumpToDefinitionId))
                {
                    currentLevel = city.GetBuildingLevel(dep.JumpToDefinitionId!);
                }

                reqViews.Add(new BuildingRequirementView
                {
                    TargetBuildingId = dep.JumpToDefinitionId ?? string.Empty,
                    DisplayName = dep.Label,
                    RequiredLevel = dep.RequiredMinimumLevel,
                    CurrentLevel = currentLevel,
                    IsSatisfied = dep.Satisfied,
                    Detail = dep.Detail
                });
            }

            var resources = city.GetUpgradeRequirements(building);
            var resViews = new List<ResourceRequirementView>(resources.Count);
            foreach (var req in resources)
            {
                if (req.Required <= 0)
                {
                    continue;
                }

                var missing = Math.Max(0, req.Required - req.Available);
                resViews.Add(new ResourceRequirementView
                {
                    ResourceId = req.Resource,
                    DisplayName = FriendlyResource(req.Resource),
                    Available = req.Available,
                    Required = req.Required,
                    CanAutoRefill = inventory != null &&
                                    !req.Satisfied &&
                                    inventory.CanAutoRefill(req.Resource, missing)
                });
            }

            var canUpgrade = !isMax &&
                             building.State != BuildingState.Upgrading &&
                             city.CanUpgrade(building, definition) &&
                             string.IsNullOrEmpty(city.GetUpgradeBlockReason(building, definition));

            return new BuildingUpgradePresentation
            {
                BuildingId = building.DefinitionId,
                DisplayName = definition.DisplayName,
                CurrentLevel = building.Level,
                NextLevel = next,
                MaxLevel = definition.MaxLevel,
                IsMaxLevel = isMax,
                BenefitTitle = benefit.Title,
                CurrentBenefit = benefit.CurrentValue,
                BenefitIncrease = benefit.Increase,
                BenefitDescription = benefit.Description,
                Duration = duration,
                EffectiveDuration = effective,
                InstantFinishCost = BuildingUpgradeRequirements.InstantCompleteDiamondCost(effective),
                CanInstantFinish = upgrading,
                CanUpgrade = canUpgrade,
                BlockReason = city.GetUpgradeBlockReason(building, definition),
                IsUpgrading = upgrading,
                RemainingUpgradeText = upgrading ? $"{(int)remaining.TotalSeconds}s" : string.Empty,
                ConstructionUsed = city.GetActiveConstructionCount(),
                ConstructionSlots = CityController.ConstructionQueueSlots,
                PreviewLabel = BuildPreviewLabel(definition.DisplayName, building),
                Requirements = reqViews,
                ResourceCosts = resViews
            };
        }

        public static BuildingDetailsPresentation BuildDetails(
            CityController city,
            BuildingInstance building)
        {
            var definition = city.GetDefinition(building);
            var catalog = BuildingBenefitCatalog.DescribeDetails(city, building, definition);
            return new BuildingDetailsPresentation
            {
                BuildingId = building.DefinitionId,
                DisplayName = definition.DisplayName,
                Level = building.Level,
                MaxLevel = definition.MaxLevel,
                Function = catalog.Function,
                Narrative = catalog.Narrative,
                PowerText = catalog.PowerText,
                PreviewLabel = BuildPreviewLabel(definition.DisplayName, building),
                Attributes = catalog.Attributes
            };
        }

        private static string BuildPreviewLabel(string name, BuildingInstance building)
        {
            if (string.Equals(building.DefinitionId, "castle", StringComparison.Ordinal) ||
                string.Equals(building.DefinitionId, "wall", StringComparison.Ordinal))
            {
                var tier = CastleRealVisualLoader.ResolveTier(Math.Max(1, building.Level));
                return $"{name}\nTier {tier}";
            }

            return name;
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

        public static string FormatAmount(long amount)
        {
            if (amount >= 1_000_000)
            {
                return $"{amount / 1_000_000d:0.#} M";
            }

            if (amount >= 10_000)
            {
                return $"{amount / 1_000d:0.#} K";
            }

            return amount.ToString("N0");
        }
    }

    public readonly struct BuildingBenefitSnapshot
    {
        public BuildingBenefitSnapshot(
            string title,
            string currentValue,
            string increase,
            string description,
            string function = "",
            string narrative = "",
            string powerText = "",
            IReadOnlyList<BuildingAttributeView>? attributes = null)
        {
            Title = title;
            CurrentValue = currentValue;
            Increase = increase;
            Description = description;
            Function = function;
            Narrative = narrative;
            PowerText = powerText;
            Attributes = attributes ?? Array.Empty<BuildingAttributeView>();
        }

        public string Title { get; }
        public string CurrentValue { get; }
        public string Increase { get; }
        public string Description { get; }
        public string Function { get; }
        public string Narrative { get; }
        public string PowerText { get; }
        public IReadOnlyList<BuildingAttributeView> Attributes { get; }
    }

    /// <summary>Benefícios/atributos derivados do catálogo (não texto fixo na UI).</summary>
    public static class BuildingBenefitCatalog
    {
        public static BuildingBenefitSnapshot DescribeUpgrade(
            CityController city,
            BuildingInstance building,
            BuildingDefinition definition)
        {
            var id = building.DefinitionId;
            var lv = building.Level;

            if (string.Equals(id, "castle", StringComparison.Ordinal))
            {
                return new BuildingBenefitSnapshot(
                    "Limite de nível dos heróis / cidade",
                    $"Nv.{lv}",
                    $"+1 → Nv.{lv + 1}",
                    "Eleva o teto de evolução da cidade e dos demais edifícios.");
            }

            if (string.Equals(id, "warehouse", StringComparison.Ordinal))
            {
                var cur = WarehouseRules.GetCapacity(lv);
                var next = WarehouseRules.GetNextCapacity(lv);
                return new BuildingBenefitSnapshot(
                    "Capacidade de armazenamento",
                    cur.ToString("N0"),
                    $"+{(next - cur):N0}",
                    $"Proteção sobe para {WarehouseRules.GetNextProtection(lv):N0}.");
            }

            if (string.Equals(id, "academy", StringComparison.Ordinal))
            {
                return new BuildingBenefitSnapshot(
                    "Bônus de pesquisa",
                    $"Nv.{lv}",
                    $"+1 → Nv.{lv + 1}",
                    "Eleva o teto acadêmico para desbloqueios futuros.");
            }

            if (string.Equals(id, "hospital", StringComparison.Ordinal))
            {
                var cur = SupportBuildingRules.GetHospitalCapacity(lv);
                var next = SupportBuildingRules.GetHospitalCapacity(lv + 1);
                return new BuildingBenefitSnapshot(
                    "Capacidade do hospital",
                    cur.ToString("N0"),
                    $"+{next - cur}",
                    $"Recuperação +{SupportBuildingRules.GetHospitalRecoverySpeedPercent(lv + 1)}%.");
            }

            if (string.Equals(id, "wall", StringComparison.Ordinal))
            {
                var cur = SupportBuildingRules.GetWallCityDefense(lv);
                var next = SupportBuildingRules.GetWallCityDefense(lv + 1);
                return new BuildingBenefitSnapshot(
                    "Defesa da muralha",
                    $"+{cur}",
                    $"+{next - cur}",
                    $"HP {SupportBuildingRules.GetWallHitPoints(lv + 1):N0} · resistência +{SupportBuildingRules.GetWallResistancePercent(lv + 1)}%.");
            }

            if (string.Equals(id, "arena", StringComparison.Ordinal))
            {
                var cur = SupportBuildingRules.GetArenaFormationCapacity(lv);
                var next = SupportBuildingRules.GetArenaFormationCapacity(lv + 1);
                return new BuildingBenefitSnapshot(
                    "Capacidade de treinamento",
                    cur.ToString(),
                    $"+{next - cur}",
                    $"Bônus de treino +{SupportBuildingRules.GetArenaTrainingBonusPercent(lv + 1)}%.");
            }

            if (string.Equals(id, "dragon-tower", StringComparison.Ordinal))
            {
                return new BuildingBenefitSnapshot(
                    "Limite do dragão / vínculo",
                    $"+{SupportBuildingRules.GetDragonBondBonusPercent(lv)}%",
                    $"+{SupportBuildingRules.GetDragonBondBonusPercent(lv + 1) - SupportBuildingRules.GetDragonBondBonusPercent(lv)}%",
                    "Melhora vínculo e recuperação no ninho.");
            }

            if (ProductionCatalog.TryGet(id, out var prod))
            {
                var rate = prod.GetRatePerHour(lv);
                var nextRate = prod.GetRatePerHour(lv + 1);
                return new BuildingBenefitSnapshot(
                    "Produção por hora",
                    $"{rate:0.#}/h",
                    $"+{(nextRate - rate):0.#}/h",
                    ProductionBuildingDetails.DescribeUpgradeBenefit(id));
            }

            var support = SupportBuildingRules.DescribeUpgradeBenefit(id, lv);
            return new BuildingBenefitSnapshot(
                "Benefício da evolução",
                $"Nv.{lv}",
                $"+1 → Nv.{lv + 1}",
                string.IsNullOrEmpty(support) ? $"Melhora {definition.DisplayName}" : support);
        }

        public static BuildingBenefitSnapshot DescribeDetails(
            CityController city,
            BuildingInstance building,
            BuildingDefinition definition)
        {
            var id = building.DefinitionId;
            var lv = building.Level;
            var attrs = new List<BuildingAttributeView>();

            if (string.Equals(id, "castle", StringComparison.Ordinal))
            {
                var tier = CastleRealVisualLoader.ResolveTier(Math.Max(1, lv));
                attrs.Add(Attr("Limite de nível dos heróis", $"Nv.{lv}"));
                attrs.Add(Attr("Poder defensivo", $"+{20 * Math.Max(0, lv)}"));
                attrs.Add(Attr("Capacidade da cidade", $"Teto Nv.{lv}"));
                attrs.Add(Attr("Tier visual atual", $"Tier {tier}"));
                return new BuildingBenefitSnapshot(
                    "Limite de nível",
                    $"Nv.{lv}",
                    string.Empty,
                    string.Empty,
                    "Coração da cidade — limita o nível dos demais edifícios.",
                    "O Castelo é o marco da prosperidade de Valgor. Cada pedra erguida amplia o horizonte da cidade.",
                    $"Poder do edifício: {100 + lv * 75}",
                    attrs);
            }

            if (ProductionCatalog.TryGet(id, out var prodDef))
            {
                var rate = city.Economy.Production.GetRatePerHour(building);
                var capacity = city.Economy.Production.GetCapacity(building);
                city.Economy.Production.TryGetState(building.DefinitionId, out var state);
                var accumulated = state?.Accumulated ?? 0L;
                attrs.Add(Attr("Produção por hora", $"{rate:0.#}/h"));
                attrs.Add(Attr("Capacidade", capacity.ToString("N0")));
                attrs.Add(Attr(
                    "Tempo até lotar",
                    ProductionBuildingDetails.FormatTimeToFill(accumulated, capacity, rate)));
                return new BuildingBenefitSnapshot(
                    "Produção",
                    $"{rate:0.#}/h",
                    string.Empty,
                    string.Empty,
                    $"Produz {BuildingUpgradePresentationBuilder.FriendlyResource(prodDef.Resource)} para a cidade.",
                    "Os campos e oficinas alimentam a economia de Valgor.",
                    $"Poder do edifício: {40 + lv * 18}",
                    attrs);
            }

            if (string.Equals(id, "warehouse", StringComparison.Ordinal))
            {
                attrs.Add(Attr("Proteção de recursos", WarehouseRules.GetProtection(lv).ToString("N0")));
                attrs.Add(Attr("Capacidade", WarehouseRules.GetCapacity(lv).ToString("N0")));
                attrs.Add(Attr("Recursos protegidos", WarehouseRules.GetProtection(lv).ToString("N0")));
                return new BuildingBenefitSnapshot(
                    "Armazenamento",
                    WarehouseRules.GetCapacity(lv).ToString("N0"),
                    string.Empty,
                    string.Empty,
                    "Guarda e protege os recursos da cidade.",
                    "O Armazém resguarda o fruto do trabalho contra perdas e saques.",
                    $"Poder do edifício: {50 + lv * 22}",
                    attrs);
            }

            if (string.Equals(id, "hospital", StringComparison.Ordinal))
            {
                attrs.Add(Attr("Capacidade de feridos", SupportBuildingRules.GetHospitalCapacity(lv).ToString()));
                attrs.Add(Attr(
                    "Velocidade de recuperação",
                    $"+{SupportBuildingRules.GetHospitalRecoverySpeedPercent(lv)}%"));
                return new BuildingBenefitSnapshot(
                    "Hospital",
                    SupportBuildingRules.GetHospitalCapacity(lv).ToString(),
                    string.Empty,
                    string.Empty,
                    "Trata feridos e acelera a recuperação.",
                    "Sob o signo da cura, o Hospital mantém a força de Valgor de pé.",
                    $"Poder do edifício: {45 + lv * 20}",
                    attrs);
            }

            if (string.Equals(id, "wall", StringComparison.Ordinal))
            {
                var tier = CastleRealVisualLoader.ResolveTier(Math.Max(1, lv));
                attrs.Add(Attr("Defesa", $"+{SupportBuildingRules.GetWallCityDefense(lv)}"));
                attrs.Add(Attr("HP", SupportBuildingRules.GetWallHitPoints(lv).ToString("N0")));
                attrs.Add(Attr("Resistência", $"+{SupportBuildingRules.GetWallResistancePercent(lv)}%"));
                attrs.Add(Attr("Tier visual", $"Tier {tier}"));
                return new BuildingBenefitSnapshot(
                    "Muralha",
                    $"+{SupportBuildingRules.GetWallCityDefense(lv)}",
                    string.Empty,
                    string.Empty,
                    "Defesa perimetral da cidade.",
                    "A Muralha é o escudo de pedra que guarda o povo de Valgor.",
                    $"Poder do edifício: {60 + lv * 28}",
                    attrs);
            }

            if (string.Equals(id, "dragon-tower", StringComparison.Ordinal))
            {
                var dragons = city.Dragons;
                var capacity = dragons?.RoostCapacity ?? Math.Max(1, lv);
                attrs.Add(Attr("Limite do dragão", capacity.ToString()));
                attrs.Add(Attr("Bônus de vínculo", $"+{SupportBuildingRules.GetDragonBondBonusPercent(lv)}%"));
                attrs.Add(Attr("Capacidade de alimento", $"{10 + lv * 5}"));
                return new BuildingBenefitSnapshot(
                    "Torre dos Dragões",
                    capacity.ToString(),
                    string.Empty,
                    string.Empty,
                    "Abriga e fortalece o vínculo com os dragões.",
                    "Das alturas da Torre, o rugido dos dragões ecoa sobre Valgor.",
                    $"Poder do edifício: {70 + lv * 30}",
                    attrs);
            }

            // Fallback genérico a partir do texto de detalhes existente.
            var legacy = BuildingDetailsViewModel.From(city, building, definition, openMode: false);
            attrs.Add(Attr("Nível", $"{lv}/{definition.MaxLevel}"));
            return new BuildingBenefitSnapshot(
                definition.DisplayName,
                $"Nv.{lv}",
                string.Empty,
                string.Empty,
                "Edifício da cidade.",
                legacy.Body,
                $"Poder do edifício: {30 + lv * 15}",
                attrs);
        }

        private static BuildingAttributeView Attr(string label, string value) =>
            new() { Label = label, Value = value };
    }
}
