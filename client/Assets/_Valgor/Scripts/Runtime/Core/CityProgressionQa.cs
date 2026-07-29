using System;

namespace Valgor.Core
{
    /// <summary>
    /// Gate do modo de homologação da progressão da cidade.
    /// Ativo somente com -cityProgressionQA (nunca na build normal sem o flag).
    /// </summary>
    public static class CityProgressionQa
    {
        public const string CliFlag = "-cityProgressionQA";
        public const string CliAutoTestFlag = "-cityProgressionQATest";
        public const string SaveSlotId = "city-progression-qa";
        public const string PersistenceKey = "valgor.city.production.v1.city-progression-qa";
        public const string EnergyPrefsPrefix = "valgor.worldmap.energy.v1.city-progression-qa";

        /// <summary>Evita overflow (não usar int.MaxValue).</summary>
        public const long ResourceAmount = 999_999_999L;
        public const int EnergyAmount = 999_999;
        public const int EnergyMax = 999_999;

        /// <summary>Duração efetiva de construção no modo QA (segundos).</summary>
        public const float HomologDurationSeconds = 2f;

        public const string BannerText = "MODO HOMOLOGAÇÃO";

        private static bool? _active;
        private static bool? _autoTest;

        public static bool IsActive
        {
            get
            {
                _active ??= HasFlag(CliFlag);
                return _active.Value;
            }
        }

        public static bool IsAutoTest
        {
            get
            {
                _autoTest ??= IsActive && HasFlag(CliAutoTestFlag);
                return _autoTest.Value;
            }
        }

        /// <summary>Força estado (Editor/testes). Não afeta build normal em runtime.</summary>
        public static void ForceActiveForTests(bool active, bool autoTest = false)
        {
            _active = active;
            _autoTest = autoTest;
        }

        public static void ApplyPersistenceKeyIfActive()
        {
            if (!IsActive)
            {
                return;
            }

            // ProductionCatalog vive em Valgor.City — setado por CityProgressionQaBootstrap.
        }

        private static bool HasFlag(string flag)
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
