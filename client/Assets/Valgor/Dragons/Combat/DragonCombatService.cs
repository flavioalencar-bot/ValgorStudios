using System;
using Valgor.Dragons.Data;
using Valgor.Dragons.Growth;

namespace Valgor.Dragons.Combat
{
    /// <summary>Faixa de dificuldade vista pelo módulo Dragão (espelha encontro PvE).</summary>
    public enum DragonCombatDifficulty
    {
        Trivial = 0,
        Easy = 1,
        Fair = 2,
        Hard = 3,
        Failed = 4
    }

    /// <summary>
    /// Combate PvE: dragão como suporte automático (sem controle manual).
    /// </summary>
    public sealed class DragonCombatService
    {
        private readonly DragonSettings _settings;
        private readonly DragonAbilityService _abilities;

        public DragonCombatService(DragonSettings settings, DragonAbilityService abilities)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _abilities = abilities ?? throw new ArgumentNullException(nameof(abilities));
        }

        public bool CanSupportCombat(DragonInstance dragon, out string error)
        {
            if (dragon.DragonLevel < 1)
            {
                error = "Dragão ainda não nasceu.";
                return false;
            }

            if (dragon.IsLevelingUp)
            {
                error = "Dragão em evolução/ritual.";
                return false;
            }

            if (dragon.State != DragonState.Deployed && dragon.State != DragonState.Ready)
            {
                error = "Dragão precisa estar READY ou DEPLOYED.";
                return false;
            }

            if (dragon.Energy < _settings.MinEnergyToCombat)
            {
                error = $"Energia insuficiente para combate (mín. {_settings.MinEnergyToCombat}).";
                return false;
            }

            if (dragon.Health < _settings.MinHealthToCombat)
            {
                error = $"Saúde insuficiente para combate (mín. {_settings.MinHealthToCombat}).";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public int ResolveSupportPower(DragonInstance dragon, DragonDefinition definition)
        {
            _abilities.EnsureDefaults(dragon);
            var power = definition.BasePower *
                        DragonGrowthService.PowerMultiplier(dragon.GrowthStage) *
                        DragonBondService.PowerMultiplier(dragon.BondLevel);

            var bonus = 0.0;
            foreach (var id in _abilities.Equipped(dragon))
            {
                if (!DragonAbilityCatalog.TryGet(id, out var ability))
                {
                    continue;
                }

                bonus += ability.PowerBonusRatio;
                if (id == DragonAbilityId.BondRoar)
                {
                    bonus += dragon.BondLevel * 0.02;
                }
            }

            power *= 1.0 + bonus;
            power *= 1.0 + dragon.DragonLevel * 0.01;
            if (dragon.IsMounted && !string.IsNullOrEmpty(dragon.BondedHeroId))
            {
                power *= Mount.DragonMountService.MountPowerMultiplier(dragon.MountBondLevel);
            }

            return Math.Max(1, (int)Math.Round(power));
        }

        public DragonCombatResult ApplyOutcome(
            DragonInstance dragon,
            bool victory,
            DragonCombatDifficulty difficulty,
            int supportPower)
        {
            _abilities.EnsureDefaults(dragon);

            var energyCost = _settings.CombatEnergyCost;
            var damageMult = 1.0;
            var heal = 0;
            foreach (var id in _abilities.Equipped(dragon))
            {
                if (!DragonAbilityCatalog.TryGet(id, out var ability))
                {
                    continue;
                }

                energyCost += ability.ExtraEnergyCost;
                damageMult *= ability.DamageTakenMultiplier;
                heal += ability.PostCombatHeal;
            }

            var baseDamage = BaseDamageForBand(difficulty, victory);
            var healthLost = (int)Math.Round(baseDamage * damageMult);
            if (!victory)
            {
                healthLost = (int)Math.Round(healthLost * 1.35);
                energyCost += 5;
            }

            dragon.Energy = Math.Max(0, dragon.Energy - energyCost);
            dragon.Health = Math.Max(0, dragon.Health - healthLost);
            if (heal > 0 && victory)
            {
                dragon.Health = Math.Min(_settings.MaxHealth, dragon.Health + heal);
            }

            var xp = 0;
            if (victory)
            {
                xp = _settings.CombatExperienceReward + BandXpBonus(difficulty);
                dragon.Experience += xp;
            }

            var injured = !victory ||
                          dragon.Health < _settings.MinHealthToCombat ||
                          (difficulty == DragonCombatDifficulty.Hard && healthLost >= 20);

            var summary = victory
                ? $"Vitória suporte · poder {supportPower} · −{energyCost} energia · −{healthLost} vida"
                : $"Derrota suporte · −{energyCost} energia · −{healthLost} vida";

            return new DragonCombatResult(
                victory,
                supportPower,
                energyCost,
                healthLost,
                injured,
                xp,
                summary);
        }

        private static int BaseDamageForBand(DragonCombatDifficulty difficulty, bool victory) =>
            difficulty switch
            {
                DragonCombatDifficulty.Trivial => victory ? 2 : 8,
                DragonCombatDifficulty.Easy => victory ? 6 : 12,
                DragonCombatDifficulty.Fair => victory ? 12 : 20,
                DragonCombatDifficulty.Hard => victory ? 22 : 32,
                _ => victory ? 18 : 28
            };

        private static int BandXpBonus(DragonCombatDifficulty difficulty) =>
            difficulty switch
            {
                DragonCombatDifficulty.Trivial => 0,
                DragonCombatDifficulty.Easy => 2,
                DragonCombatDifficulty.Fair => 5,
                DragonCombatDifficulty.Hard => 10,
                _ => 0
            };
    }
}
