using System;
using UnityEngine;
using Valgor.City.Data;
using Valgor.Core;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Aplica save isolado e recursos do modo -cityProgressionQA antes da economia.
    /// </summary>
    public static class CityProgressionQaBootstrap
    {
        public static void ApplyBeforeEconomy()
        {
            if (!CityProgressionQa.IsActive)
            {
                return;
            }

            ProductionCatalog.Settings.PersistenceKey = CityProgressionQa.PersistenceKey;
            Debug.Log(
                $"[Valgor.QA] City progression homologation ON — save={CityProgressionQa.SaveSlotId} " +
                $"key={CityProgressionQa.PersistenceKey}");
        }

        public static void TopUpWallet(ResourceWallet wallet)
        {
            if (!CityProgressionQa.IsActive || wallet == null)
            {
                return;
            }

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                wallet.SetAmount(resource, CityProgressionQa.ResourceAmount);
            }
        }

        public static void TopUpEnergyPrefs()
        {
            if (!CityProgressionQa.IsActive)
            {
                return;
            }

            var prefix = CityProgressionQa.EnergyPrefsPrefix;
            PlayerPrefs.SetInt(prefix + ".current", CityProgressionQa.EnergyAmount);
            PlayerPrefs.SetInt(prefix + ".max", CityProgressionQa.EnergyMax);
            PlayerPrefs.SetString(prefix + ".updated", DateTime.UtcNow.ToString("o"));
            // HUD da City lê o prefixo padrão — espelha também para a barra de recursos.
            PlayerPrefs.SetInt("valgor.worldmap.energy.v1.current", CityProgressionQa.EnergyAmount);
            PlayerPrefs.SetInt("valgor.worldmap.energy.v1.max", CityProgressionQa.EnergyMax);
            PlayerPrefs.Save();
        }

        public static TimeSpan GetEffectiveUpgradeDuration(BuildingDefinition definition, int currentLevel)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (CityProgressionQa.IsActive)
            {
                return TimeSpan.FromSeconds(CityProgressionQa.HomologDurationSeconds);
            }

            return definition.GetUpgradeDuration(currentLevel);
        }
    }
}
