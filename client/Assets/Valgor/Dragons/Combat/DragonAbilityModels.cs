using System;
using System.Collections.Generic;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Combat
{
    public sealed class DragonAbilityDefinition
    {
        public DragonAbilityDefinition(
            DragonAbilityId id,
            string displayName,
            string description,
            int unlockLevel,
            double powerBonusRatio,
            double damageTakenMultiplier,
            int extraEnergyCost,
            int postCombatHeal)
        {
            Id = id;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Description = description ?? string.Empty;
            UnlockLevel = unlockLevel;
            PowerBonusRatio = powerBonusRatio;
            DamageTakenMultiplier = damageTakenMultiplier;
            ExtraEnergyCost = extraEnergyCost;
            PostCombatHeal = postCombatHeal;
        }

        public DragonAbilityId Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int UnlockLevel { get; }
        public double PowerBonusRatio { get; }
        /// <summary>1 = normal; &lt;1 reduz dano recebido.</summary>
        public double DamageTakenMultiplier { get; }
        public int ExtraEnergyCost { get; }
        public int PostCombatHeal { get; }
    }

    public static class DragonAbilityCatalog
    {
        private static readonly Dictionary<DragonAbilityId, DragonAbilityDefinition> Map = new()
        {
            [DragonAbilityId.EmberBreath] = new(
                DragonAbilityId.EmberBreath,
                "Sopro de Brasa",
                "Suporte ofensivo: +15% de poder do dragão.",
                unlockLevel: 1,
                powerBonusRatio: 0.15,
                damageTakenMultiplier: 1.0,
                extraEnergyCost: 0,
                postCombatHeal: 0),
            [DragonAbilityId.ScaleGuard] = new(
                DragonAbilityId.ScaleGuard,
                "Escama Protetora",
                "Suporte defensivo: −25% de dano recebido.",
                unlockLevel: 6,
                powerBonusRatio: 0.0,
                damageTakenMultiplier: 0.75,
                extraEnergyCost: 0,
                postCombatHeal: 0),
            [DragonAbilityId.BondRoar] = new(
                DragonAbilityId.BondRoar,
                "Rugido do Vínculo",
                "Amplifica o poder conforme o vínculo.",
                unlockLevel: 11,
                powerBonusRatio: 0.10,
                damageTakenMultiplier: 1.0,
                extraEnergyCost: 0,
                postCombatHeal: 0),
            [DragonAbilityId.AshSurge] = new(
                DragonAbilityId.AshSurge,
                "Surto de Cinzas",
                "+25% de poder; consome energia extra.",
                unlockLevel: 16,
                powerBonusRatio: 0.25,
                damageTakenMultiplier: 1.0,
                extraEnergyCost: 5,
                postCombatHeal: 0),
            [DragonAbilityId.AncestralAegis] = new(
                DragonAbilityId.AncestralAegis,
                "Égide Ancestral",
                "−40% de dano e cura leve após a batalha.",
                unlockLevel: 26,
                powerBonusRatio: 0.05,
                damageTakenMultiplier: 0.60,
                extraEnergyCost: 0,
                postCombatHeal: 12)
        };

        public static IReadOnlyDictionary<DragonAbilityId, DragonAbilityDefinition> All => Map;

        public static bool TryGet(DragonAbilityId id, out DragonAbilityDefinition definition) =>
            Map.TryGetValue(id, out definition!);

        public static DragonAbilityDefinition Get(DragonAbilityId id) => Map[id];

        public static bool TryParse(string raw, out DragonAbilityId id)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                id = DragonAbilityId.None;
                return false;
            }

            if (Enum.TryParse(raw, ignoreCase: true, out id) && id != DragonAbilityId.None)
            {
                return Map.ContainsKey(id);
            }

            id = raw.Trim().ToLowerInvariant() switch
            {
                "ember-breath" or "emberbreath" => DragonAbilityId.EmberBreath,
                "scale-guard" or "scaleguard" => DragonAbilityId.ScaleGuard,
                "bond-roar" or "bondroar" => DragonAbilityId.BondRoar,
                "ash-surge" or "ashsurge" => DragonAbilityId.AshSurge,
                "ancestral-aegis" or "ancestralaegis" => DragonAbilityId.AncestralAegis,
                _ => DragonAbilityId.None
            };
            return id != DragonAbilityId.None;
        }

        public static string ToPersistId(DragonAbilityId id) =>
            id switch
            {
                DragonAbilityId.EmberBreath => "ember-breath",
                DragonAbilityId.ScaleGuard => "scale-guard",
                DragonAbilityId.BondRoar => "bond-roar",
                DragonAbilityId.AshSurge => "ash-surge",
                DragonAbilityId.AncestralAegis => "ancestral-aegis",
                _ => string.Empty
            };
    }

    public readonly struct DragonCombatResult
    {
        public DragonCombatResult(
            bool victory,
            int supportPower,
            int energySpent,
            int healthLost,
            bool injured,
            int experienceGained,
            string summary)
        {
            Victory = victory;
            SupportPower = supportPower;
            EnergySpent = energySpent;
            HealthLost = healthLost;
            Injured = injured;
            ExperienceGained = experienceGained;
            Summary = summary ?? string.Empty;
        }

        public bool Victory { get; }
        public int SupportPower { get; }
        public int EnergySpent { get; }
        public int HealthLost { get; }
        public bool Injured { get; }
        public int ExperienceGained { get; }
        public string Summary { get; }
    }
}
