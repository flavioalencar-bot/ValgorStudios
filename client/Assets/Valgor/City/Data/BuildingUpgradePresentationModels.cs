using System;
using System.Collections.Generic;

namespace Valgor.City.Data
{
    /// <summary>View-model do painel Atualizar (agrega domínio sem reimplementar regras).</summary>
    public sealed class BuildingUpgradePresentation
    {
        public string BuildingId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int CurrentLevel { get; set; }
        public int NextLevel { get; set; }
        public int MaxLevel { get; set; }
        public bool IsMaxLevel { get; set; }
        public string BenefitTitle { get; set; } = string.Empty;
        public string CurrentBenefit { get; set; } = string.Empty;
        public string BenefitIncrease { get; set; } = string.Empty;
        public string BenefitDescription { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public TimeSpan EffectiveDuration { get; set; }
        public long InstantFinishCost { get; set; }
        public bool CanInstantFinish { get; set; }
        public bool CanUpgrade { get; set; }
        public string? BlockReason { get; set; }
        public bool IsUpgrading { get; set; }
        public string RemainingUpgradeText { get; set; } = string.Empty;
        public int ConstructionUsed { get; set; }
        public int ConstructionSlots { get; set; }
        public string PreviewLabel { get; set; } = string.Empty;
        public IReadOnlyList<BuildingRequirementView> Requirements { get; set; } =
            Array.Empty<BuildingRequirementView>();
        public IReadOnlyList<ResourceRequirementView> ResourceCosts { get; set; } =
            Array.Empty<ResourceRequirementView>();
    }

    public sealed class BuildingRequirementView
    {
        public string TargetBuildingId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int RequiredLevel { get; set; }
        public int CurrentLevel { get; set; }
        public bool IsSatisfied { get; set; }
        public string Detail { get; set; } = string.Empty;
    }

    public sealed class ResourceRequirementView
    {
        public ResourceType ResourceId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public long Available { get; set; }
        public long Required { get; set; }
        public long Missing => Math.Max(0, Required - Available);
        public bool IsSatisfied => Available >= Required;
        public bool CanAutoRefill { get; set; }
    }

    public sealed class BuildingAttributeView
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public sealed class BuildingDetailsPresentation
    {
        public string BuildingId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int MaxLevel { get; set; }
        public string Function { get; set; } = string.Empty;
        public string Narrative { get; set; } = string.Empty;
        public string PowerText { get; set; } = string.Empty;
        public string PreviewLabel { get; set; } = string.Empty;
        public IReadOnlyList<BuildingAttributeView> Attributes { get; set; } =
            Array.Empty<BuildingAttributeView>();
    }
}
