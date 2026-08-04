using System;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Correção visual da Capital sem geração Tripo.
    /// Flag: -capitalWorldNoTripoCorrection
    /// </summary>
    public static class CityCapitalWorldNoTripo
    {
        public const string CliFlag = "-capitalWorldNoTripoCorrection";
        public const string CliDebugFlag = "-capitalWorldDebug";
        public const string ModeName = "CapitalWorldNoTripoCorrection";

        public const string EvidenceDir =
            @"C:\Valgor_Studio\docs\releases\capital-world-correction-no-tripo";
        public const string ScreenshotsDir =
            @"C:\Valgor_Studio\docs\releases\capital-world-correction-no-tripo\screenshots";

        private static bool? _active;
        private static bool? _debug;
        private static int _tripoApiInvocationCount;
        private static string _lastBlockReason = string.Empty;

        /// <summary>Contador obrigatório de auditoria — deve permanecer 0.</summary>
        public static int TripoApiInvocationCount => _tripoApiInvocationCount;

        public static string LastBlockReason => _lastBlockReason;

        public static bool IsActive
        {
            get
            {
                _active ??= HasFlag(CliFlag);
                return _active.Value;
            }
        }

        public static bool DebugOverlays
        {
            get
            {
                _debug ??= HasFlag(CliDebugFlag);
                return _debug.Value;
            }
        }

        /// <summary>
        /// Único ponto permitido para “chamar Tripo” — sempre bloqueia.
        /// Não incrementa o contador (chamada intencional de proteção).
        /// </summary>
        public static void NeverCallTripoApi(string reason = "blocked")
        {
            _lastBlockReason = reason ?? "blocked";
        }

        /// <summary>Somente wrappers reais de API devem chamar isto — prova de regressão.</summary>
        public static void RecordIllegalTripoApiInvocation(string reason)
        {
            _tripoApiInvocationCount++;
            _lastBlockReason = reason ?? "illegal";
        }

        public static void AssertZeroTripoUsage()
        {
            if (_tripoApiInvocationCount != 0)
            {
                throw new InvalidOperationException(
                    $"TripoApiInvocationCount={_tripoApiInvocationCount} (esperado 0).");
            }
        }

        public static void ForceForTests(bool enabled)
        {
            _active = enabled;
            if (enabled)
            {
                _tripoApiInvocationCount = 0;
            }
        }

        public static void ResetInvocationCountForTests() => _tripoApiInvocationCount = 0;

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
