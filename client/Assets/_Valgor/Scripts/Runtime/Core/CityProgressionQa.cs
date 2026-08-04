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
        public const string CliCaptureFlag = "-cityProgressionQACapture";
        public const string CliUpgradeUxTestFlag = "-buildingUpgradeUxTest";
        public const string CliResponsiveUiTestFlag = "-responsiveUiTest";
        /// <summary>Preview do Capital Layout V2 — não migra save, não promove oficial.</summary>
        public const string CliLayoutV2PreviewFlag = "-capitalLayoutV2Preview";
        /// <summary>Preview da fundação ambiental (terreno/rios/horizonte).</summary>
        public const string CliWorldPreviewFlag = "-capitalWorldPreview";
        /// <summary>Correção visual sem Tripo — composição curada.</summary>
        public const string CliWorldNoTripoCorrectionFlag = "-capitalWorldNoTripoCorrection";
        public const string CliWorldDebugFlag = "-capitalWorldDebug";
        public const string LayoutV2EvidenceDir =
            @"C:\Valgor_Studio\docs\releases\capital-layout-v2\screenshots";
        public const string WorldFoundationEvidenceDir =
            @"C:\Valgor_Studio\docs\releases\capital-world-foundation\screenshots";
        public const string WorldCorrectionEvidenceDir =
            @"C:\Valgor_Studio\docs\releases\capital-world-correction-no-tripo\screenshots";
        public const string RestoredEvidenceDir =
            @"C:\Valgor_Studio\docs\releases\capital-world-restoration";
        public const string ResponsiveEvidenceDir =
            @"C:\Valgor_Studio\docs\releases\ui-responsive-p1-fix-evidence";
        public const string SaveSlotId = "city-progression-qa";
        public const string PersistenceKey = "valgor.city.production.v1.city-progression-qa";
        public const string EnergyPrefsPrefix = "valgor.worldmap.energy.v1.city-progression-qa";

        /// <summary>Evita overflow (não usar int.MaxValue).</summary>
        public const long ResourceAmount = 999_999_999L;
        public const int EnergyAmount = 999_999;
        public const int EnergyMax = 999_999;

        /// <summary>Duração efetiva de construção no modo QA (segundos).</summary>
        public const float HomologDurationSeconds = 3f;

        public const string BannerText = "MODO HOMOLOGAÇÃO";
        public const string LayoutV2PreviewBanner = "Capital Layout V2 Preview — sem migração de save";
        public const string WorldPreviewBanner = "Capital World Foundation Preview";
        public const string WorldNoTripoBanner = "Capital World Correction — No Tripo";

        private static bool? _active;
        private static bool? _autoTest;
        private static bool? _upgradeUxTest;
        private static bool? _responsiveUiTest;
        private static bool? _layoutV2Preview;
        private static bool? _worldPreview;
        private static bool? _worldNoTripo;

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

                _active = IsCompiledIn || HasFlag(CliFlag) || HasFlag(CliUpgradeUxTestFlag) ||
                          HasFlag(CliResponsiveUiTestFlag) || HasFlag(CliLayoutV2PreviewFlag) ||
                          HasFlag(CliWorldPreviewFlag) || HasFlag(CliWorldNoTripoCorrectionFlag) ||
                          HasFlag(DragonPhase2Qa.CliFlag) || HasFlag(DragonPhase2Qa.CliE2EFlag) ||
                          HasFlag(DragonPhases14HomologQa.CliFlag);
                return _active.Value;
            }
        }

        public static bool IsLayoutV2Preview
        {
            get
            {
                _layoutV2Preview ??= HasFlag(CliLayoutV2PreviewFlag);
                return _layoutV2Preview.Value;
            }
        }

        public static bool IsWorldPreview
        {
            get
            {
                _worldPreview ??= HasFlag(CliWorldPreviewFlag) || HasFlag(CliWorldNoTripoCorrectionFlag);
                return _worldPreview.Value;
            }
        }

        public static bool IsWorldNoTripoCorrection
        {
            get
            {
                _worldNoTripo ??= HasFlag(CliWorldNoTripoCorrectionFlag);
                return _worldNoTripo.Value;
            }
        }

        public static bool IsAutoTest
        {
            get
            {
                _autoTest ??= IsActive && HasFlag(CliAutoTestFlag) && !HasFlag(CliUpgradeUxTestFlag) &&
                              !HasFlag(CliResponsiveUiTestFlag) && !HasFlag(DragonPhase2Qa.CliE2EFlag) &&
                              !HasFlag(CliLayoutV2PreviewFlag) && !HasFlag(CliWorldPreviewFlag) &&
                              !HasFlag(CliWorldNoTripoCorrectionFlag);
                return _autoTest.Value;
            }
        }

        public static bool IsUpgradeUxTest
        {
            get
            {
                _upgradeUxTest ??= IsActive && HasFlag(CliUpgradeUxTestFlag);
                return _upgradeUxTest.Value;
            }
        }

        public static bool IsResponsiveUiTest
        {
            get
            {
                _responsiveUiTest ??= IsActive && HasFlag(CliResponsiveUiTestFlag);
                return _responsiveUiTest.Value;
            }
        }

        /// <summary>Tour de câmera + screenshots de evidência (só com -cityProgressionQACapture).</summary>
        public static bool IsCapturePass => IsActive && HasFlag(CliCaptureFlag);

        /// <summary>Força estado (Editor/testes). Não afeta build normal em runtime.</summary>
        public static void ForceActiveForTests(bool active, bool autoTest = false)
        {
            _active = active;
            _autoTest = autoTest;
            _upgradeUxTest = false;
            _responsiveUiTest = false;
            _layoutV2Preview = false;
            _worldPreview = false;
            _worldNoTripo = false;
        }

        public static void ForceLayoutV2PreviewForTests(bool enabled)
        {
            _layoutV2Preview = enabled;
            if (enabled)
            {
                _active = true;
            }
        }

        public static void ForceWorldPreviewForTests(bool enabled)
        {
            _worldPreview = enabled;
            if (enabled)
            {
                _active = true;
            }
        }

        public static void ForceWorldNoTripoForTests(bool enabled)
        {
            _worldNoTripo = enabled;
            if (enabled)
            {
                _worldPreview = true;
                _active = true;
            }
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
