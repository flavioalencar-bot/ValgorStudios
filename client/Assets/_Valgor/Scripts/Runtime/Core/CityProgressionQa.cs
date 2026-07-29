using System;

namespace Valgor.Core
{
    /// <summary>
    /// Gate do modo de homologação da progressão da cidade.
    /// Ativo com define VALGOR_CITY_PROGRESSION_QA (build QA) e/ou -cityProgressionQA.
    /// Build normal sem a define permanece sem QA.
    /// </summary>
    public static class CityProgressionQa
    {
        public const string ScriptingDefine = "VALGOR_CITY_PROGRESSION_QA";
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
        public const float HomologDurationSeconds = 1f;

        public const string BannerText = "MODO HOMOLOGAÇÃO";

        private static bool? _active;
        private static bool? _autoTest;

        public static bool IsCompiledIn
        {
            get
            {
#if VALGOR_CITY_PROGRESSION_QA
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsActive
        {
            get
            {
                if (_active.HasValue)
                {
                    return _active.Value;
                }

                _active = IsCompiledIn || HasFlag(CliFlag);
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
