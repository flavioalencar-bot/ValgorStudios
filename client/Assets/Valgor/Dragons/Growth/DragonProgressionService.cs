using System;
using Valgor.Core.Modules;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Growth
{
    /// <summary>
    /// Fase 2: avanço de nível (normal + ritual), XP, aceleração e conclusão por timer.
    /// </summary>
    public sealed class DragonProgressionService
    {
        private readonly DragonSettings _settings;
        private readonly Func<DateTime> _utcNow;

        public DragonProgressionService(DragonSettings settings, Func<DateTime>? utcNow = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        public void EnsureCombatStats(DragonInstance dragon)
        {
            if (dragon.DragonLevel < 1)
            {
                return;
            }

            if (dragon.Energy <= 0 && dragon.Health <= 0 &&
                dragon.State is not (DragonState.Locked or DragonState.Egg or DragonState.Hatching))
            {
                dragon.Energy = _settings.MaxEnergy;
                dragon.Health = _settings.MaxHealth;
            }

            if (dragon.Energy < 0)
            {
                dragon.Energy = 0;
            }

            if (dragon.Health < 0)
            {
                dragon.Health = 0;
            }

            dragon.Energy = Math.Min(_settings.MaxEnergy, dragon.Energy);
            dragon.Health = Math.Min(_settings.MaxHealth, dragon.Health);
            ApplyStageFromLevel(dragon);
        }

        public void ApplyStageFromLevel(DragonInstance dragon)
        {
            if (dragon.DragonLevel < 1)
            {
                return;
            }

            dragon.GrowthStage = DragonProgressionRules.StageForLevel(dragon.DragonLevel);
        }

        public void AddExperience(DragonInstance dragon, int amount)
        {
            if (amount <= 0 || dragon.DragonLevel < 1 || dragon.IsLevelingUp)
            {
                return;
            }

            if (dragon.DragonLevel >= DragonProgressionRules.AbsoluteMaxLevel)
            {
                dragon.Experience = 0;
                return;
            }

            dragon.Experience += amount;
        }

        public bool CanStartLevelUp(
            DragonInstance dragon,
            int maxAllowedLevel,
            out string error)
        {
            if (dragon.DragonLevel < 1)
            {
                error = "Dragão ainda não nasceu.";
                return false;
            }

            if (dragon.IsLevelingUp)
            {
                error = "Evolução já em andamento.";
                return false;
            }

            if (dragon.State is DragonState.Deployed or DragonState.Recovering or DragonState.Exhausted
                or DragonState.Injured or DragonState.Locked or DragonState.Egg or DragonState.Hatching)
            {
                error = "Dragão indisponível para evoluir neste estado.";
                return false;
            }

            var next = dragon.DragonLevel + 1;
            if (next > DragonProgressionRules.AbsoluteMaxLevel)
            {
                error = "Nível máximo atingido.";
                return false;
            }

            if (next > maxAllowedLevel)
            {
                error =
                    $"Limite atual Nv.{maxAllowedLevel} (Castelo/Torre). Evolua os edifícios para avançar.";
                return false;
            }

            var needXp = DragonProgressionRules.ExperienceRequiredForLevel(dragon.DragonLevel);
            if (dragon.Experience < needXp)
            {
                error = $"Experiência insuficiente ({dragon.Experience}/{needXp}). Alimente o dragão.";
                return false;
            }

            if (dragon.Energy < _settings.MinEnergyToLevelUp)
            {
                error = $"Energia insuficiente (mín. {_settings.MinEnergyToLevelUp}).";
                return false;
            }

            if (dragon.Health < _settings.MinHealthToLevelUp)
            {
                error = $"Saúde insuficiente (mín. {_settings.MinHealthToLevelUp}).";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public bool TryStartLevelUp(
            DragonInstance dragon,
            int maxAllowedLevel,
            IDragonResourceWallet wallet,
            out string error)
        {
            if (!CanStartLevelUp(dragon, maxAllowedLevel, out error))
            {
                return false;
            }

            var next = dragon.DragonLevel + 1;
            var ritual = DragonProgressionRules.IsRitualTarget(next);
            var food = ritual ? _settings.RitualFoodCost : _settings.LevelUpFoodCost;
            var essence = ritual ? _settings.RitualEssenceCost : _settings.LevelUpEssenceCost;

            if (wallet.GetFood() < food || wallet.GetDragonEssence() < essence)
            {
                error = ritual
                    ? "Recursos insuficientes para o ritual."
                    : "Recursos insuficientes para evoluir.";
                return false;
            }

            if (!wallet.TrySpendFood(food) || !wallet.TrySpendDragonEssence(essence))
            {
                error = "Falha ao debitar recursos.";
                return false;
            }

            var needXp = DragonProgressionRules.ExperienceRequiredForLevel(dragon.DragonLevel);
            dragon.Experience = Math.Max(0, dragon.Experience - needXp);
            dragon.IsLevelingUp = true;
            dragon.PendingLevel = next;
            var hours = ritual ? _settings.RitualDurationHours : _settings.LevelUpDurationHours;
            dragon.LevelUpEndsAtUtc = _utcNow().AddHours(hours);
            dragon.LastUpdatedUtc = _utcNow();
            error = string.Empty;
            return true;
        }

        public bool TryInstantComplete(
            DragonInstance dragon,
            IDragonResourceWallet wallet,
            out string error)
        {
            if (!dragon.IsLevelingUp || !dragon.LevelUpEndsAtUtc.HasValue)
            {
                error = "Nenhuma evolução em andamento.";
                return false;
            }

            var remaining = dragon.LevelUpEndsAtUtc.Value - _utcNow();
            var cost = InstantDiamondCost(remaining);
            if (cost > 0 && !wallet.TrySpendDiamonds(cost))
            {
                error = $"Diamantes insuficientes (precisa {cost}).";
                return false;
            }

            CompleteLevelUp(dragon);
            error = string.Empty;
            return true;
        }

        public static long InstantDiamondCost(TimeSpan remaining)
        {
            if (remaining <= TimeSpan.Zero)
            {
                return 0;
            }

            return Math.Max(1, (long)Math.Ceiling(remaining.TotalSeconds / 5.0));
        }

        public void AdvanceTimers(DragonInstance dragon)
        {
            if (!dragon.IsLevelingUp || !dragon.LevelUpEndsAtUtc.HasValue)
            {
                return;
            }

            if (_utcNow() >= dragon.LevelUpEndsAtUtc.Value)
            {
                CompleteLevelUp(dragon);
            }
        }

        public void CompleteLevelUp(DragonInstance dragon)
        {
            if (!dragon.IsLevelingUp)
            {
                return;
            }

            var target = dragon.PendingLevel > 0
                ? dragon.PendingLevel
                : dragon.DragonLevel + 1;
            dragon.DragonLevel = Math.Min(DragonProgressionRules.AbsoluteMaxLevel, target);
            dragon.IsLevelingUp = false;
            dragon.PendingLevel = 0;
            dragon.LevelUpEndsAtUtc = null;
            dragon.Energy = Math.Max(0, dragon.Energy - _settings.EnergyCostOnLevelUp);
            ApplyStageFromLevel(dragon);
            dragon.LastUpdatedUtc = _utcNow();
        }

        public void TickEnergyDecay(DragonInstance dragon)
        {
            if (dragon.DragonLevel < 1 || dragon.IsLevelingUp)
            {
                return;
            }

            if (dragon.State is DragonState.Ready or DragonState.Resting)
            {
                // Decaimento leve por tick de serviço (City chama Tick frequentemente).
                // Controlado por acumulador no serviço principal — aqui só API de ajuste.
            }
        }

        public void ApplyFeedRestores(DragonInstance dragon)
        {
            dragon.Energy = Math.Min(
                _settings.MaxEnergy,
                dragon.Energy + _settings.EnergyRestorePerFeed);
            dragon.Health = Math.Min(
                _settings.MaxHealth,
                dragon.Health + _settings.HealthRestorePerFeed);
        }
    }
}
