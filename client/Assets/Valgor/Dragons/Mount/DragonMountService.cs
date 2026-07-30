using System;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Mount
{
    /// <summary>
    /// Vínculo de montaria Dragão↔Herói (estratégico, sem controle de voo).
    /// </summary>
    public sealed class DragonMountService
    {
        private readonly DragonSettings _settings;

        public DragonMountService(DragonSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public static double MountPowerMultiplier(int mountBondLevel) =>
            1.0 + Math.Max(0, mountBondLevel) * 0.04;

        public bool CanCreateBond(DragonInstance dragon, string heroId, out string error)
        {
            if (dragon.DragonLevel < 1)
            {
                error = "Dragão ainda não nasceu.";
                return false;
            }

            if (dragon.IsLevelingUp)
            {
                error = "Aguarde a evolução/ritual.";
                return false;
            }

            if (dragon.State is DragonState.Deployed or DragonState.Recovering or DragonState.Injured
                or DragonState.Exhausted or DragonState.Hatching or DragonState.Egg)
            {
                error = "Configure o vínculo com o dragão no ninho.";
                return false;
            }

            if (!string.IsNullOrEmpty(dragon.BondedHeroId) &&
                !string.Equals(dragon.BondedHeroId, heroId, StringComparison.Ordinal))
            {
                error = "Já existe vínculo com outro herói. Desvincule antes.";
                return false;
            }

            return DragonMountCompatibility.IsCompatible(heroId, dragon.DragonLevel, out error);
        }

        public bool TryCreateBond(DragonInstance dragon, string heroId, out string error)
        {
            if (!CanCreateBond(dragon, heroId, out error))
            {
                return false;
            }

            if (string.Equals(dragon.BondedHeroId, heroId, StringComparison.Ordinal))
            {
                error = string.Empty;
                return true;
            }

            dragon.BondedHeroId = heroId;
            dragon.MountBondLevel = Math.Max(1, dragon.MountBondLevel);
            dragon.MountBondPoints = Math.Max(0, dragon.MountBondPoints);
            dragon.IsMounted = false;
            error = string.Empty;
            return true;
        }

        public bool TryClearBond(DragonInstance dragon, out string error)
        {
            if (dragon.State == DragonState.Deployed)
            {
                error = "Recoloque o dragão antes de desvincular.";
                return false;
            }

            dragon.BondedHeroId = null;
            dragon.IsMounted = false;
            error = string.Empty;
            return true;
        }

        public bool TryEquipMount(DragonInstance dragon, out string error)
        {
            if (string.IsNullOrEmpty(dragon.BondedHeroId))
            {
                error = "Crie o vínculo de montaria primeiro.";
                return false;
            }

            if (dragon.State is DragonState.Deployed or DragonState.Recovering or DragonState.Injured
                or DragonState.Exhausted)
            {
                error = "Equipe a montaria com o dragão no ninho.";
                return false;
            }

            if (dragon.IsLevelingUp)
            {
                error = "Aguarde a evolução/ritual.";
                return false;
            }

            if (!DragonMountCompatibility.IsCompatible(dragon.BondedHeroId, dragon.DragonLevel, out error))
            {
                return false;
            }

            if (dragon.MountBondLevel < 1)
            {
                dragon.MountBondLevel = 1;
            }

            dragon.IsMounted = true;
            error = string.Empty;
            return true;
        }

        public bool TryUnequipMount(DragonInstance dragon, out string error)
        {
            if (dragon.State == DragonState.Deployed)
            {
                error = "Desequipe após o retorno da marcha.";
                return false;
            }

            dragon.IsMounted = false;
            error = string.Empty;
            return true;
        }

        public void AddMountBondPoints(DragonInstance dragon, int points)
        {
            if (points <= 0 || string.IsNullOrEmpty(dragon.BondedHeroId))
            {
                return;
            }

            if (dragon.MountBondLevel >= _settings.MaxMountBondLevel)
            {
                return;
            }

            dragon.MountBondPoints += points;
            while (dragon.MountBondLevel < _settings.MaxMountBondLevel &&
                   dragon.MountBondPoints >= _settings.MountBondPointsPerLevel)
            {
                dragon.MountBondPoints -= _settings.MountBondPointsPerLevel;
                dragon.MountBondLevel++;
            }

            if (dragon.MountBondLevel >= _settings.MaxMountBondLevel)
            {
                dragon.MountBondPoints = 0;
            }
        }

        public string Describe(DragonInstance dragon)
        {
            if (string.IsNullOrEmpty(dragon.BondedHeroId))
            {
                return "Sem vínculo de montaria.";
            }

            DragonMountCompatibility.TryGetDisplayName(dragon.BondedHeroId, out var hero);
            var mount = dragon.IsMounted ? "montado" : "vínculo ocioso";
            return $"{hero} · {mount} · vínculo montaria Nv.{dragon.MountBondLevel} ({dragon.MountBondPoints} pts)";
        }
    }
}
