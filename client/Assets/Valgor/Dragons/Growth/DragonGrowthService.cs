using System;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Growth
{
    /// <summary>
    /// Progressão EGG → HATCHLING → JUVENILE → ADULT → ELDER → ANCIENT.
    /// </summary>
    public sealed class DragonGrowthService
    {
        private readonly DragonSettings _settings;

        public DragonGrowthService(DragonSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public static double PowerMultiplier(DragonGrowthStage stage) =>
            stage switch
            {
                DragonGrowthStage.Egg => 0.1,
                DragonGrowthStage.Hatchling => 0.4,
                DragonGrowthStage.Juvenile => 0.7,
                DragonGrowthStage.Adult => 1.0,
                DragonGrowthStage.Elder => 1.25,
                DragonGrowthStage.Ancient => 1.5,
                _ => 1.0
            };

        public void EnsureSeedDefaults(DragonInstance dragon)
        {
            if (dragon.State is DragonState.Locked or DragonState.Egg or DragonState.Hatching)
            {
                dragon.GrowthStage = DragonGrowthStage.Egg;
                return;
            }

            if (dragon.State == DragonState.Juvenile)
            {
                if (dragon.GrowthStage < DragonGrowthStage.Hatchling)
                {
                    dragon.GrowthStage = DragonGrowthStage.Hatchling;
                }

                return;
            }

            if (dragon.GrowthStage < DragonGrowthStage.Adult &&
                dragon.State is DragonState.Ready or DragonState.Deployed or DragonState.Hungry
                    or DragonState.Resting or DragonState.Exhausted or DragonState.Injured
                    or DragonState.Recovering)
            {
                dragon.GrowthStage = DragonGrowthStage.Adult;
            }
        }

        public bool SyncWithLifecycle(DragonInstance dragon, DragonState previous, DragonState current)
        {
            if (previous == current)
            {
                return false;
            }

            var before = dragon.GrowthStage;
            if (current is DragonState.Locked or DragonState.Egg or DragonState.Hatching)
            {
                dragon.GrowthStage = DragonGrowthStage.Egg;
            }
            else if (previous == DragonState.Hatching && current == DragonState.Juvenile)
            {
                dragon.GrowthStage = DragonGrowthStage.Hatchling;
                dragon.GrowthPoints = 0;
            }
            else if (previous == DragonState.Juvenile && current == DragonState.Resting)
            {
                if (dragon.GrowthStage < DragonGrowthStage.Juvenile)
                {
                    dragon.GrowthStage = DragonGrowthStage.Juvenile;
                    dragon.GrowthPoints = 0;
                }
            }
            else if (current == DragonState.Ready && dragon.GrowthStage < DragonGrowthStage.Adult)
            {
                dragon.GrowthStage = DragonGrowthStage.Adult;
                dragon.GrowthPoints = 0;
            }

            return before != dragon.GrowthStage;
        }

        public void AddGrowthPoints(DragonInstance dragon, int points)
        {
            if (points <= 0 ||
                dragon.GrowthStage is DragonGrowthStage.Egg or DragonGrowthStage.Ancient)
            {
                return;
            }

            dragon.GrowthPoints += points;
            TryAdvance(dragon);
        }

        public bool TryAdvance(DragonInstance dragon)
        {
            var advanced = false;
            while (true)
            {
                var need = RequiredPoints(dragon.GrowthStage);
                if (need <= 0 || dragon.GrowthPoints < need)
                {
                    break;
                }

                dragon.GrowthPoints -= need;
                dragon.GrowthStage = NextStage(dragon.GrowthStage);
                advanced = true;
                if (dragon.GrowthStage == DragonGrowthStage.Ancient)
                {
                    dragon.GrowthPoints = 0;
                    break;
                }
            }

            return advanced;
        }

        public int RequiredPoints(DragonGrowthStage stage) =>
            stage switch
            {
                DragonGrowthStage.Hatchling => _settings.HatchlingToJuvenilePoints,
                DragonGrowthStage.Juvenile => _settings.JuvenileToAdultPoints,
                DragonGrowthStage.Adult => _settings.AdultToElderPoints,
                DragonGrowthStage.Elder => _settings.ElderToAncientPoints,
                _ => 0
            };

        private static DragonGrowthStage NextStage(DragonGrowthStage stage) =>
            stage switch
            {
                DragonGrowthStage.Egg => DragonGrowthStage.Hatchling,
                DragonGrowthStage.Hatchling => DragonGrowthStage.Juvenile,
                DragonGrowthStage.Juvenile => DragonGrowthStage.Adult,
                DragonGrowthStage.Adult => DragonGrowthStage.Elder,
                DragonGrowthStage.Elder => DragonGrowthStage.Ancient,
                _ => stage
            };
    }
}
