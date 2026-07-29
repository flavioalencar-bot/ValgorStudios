using System;

namespace Valgor.Core
{
    /// <summary>
    /// Homologação P1 Dragão Fase 2 — visuais por estágio + E2E Nv.1→30.
    /// </summary>
    public static class DragonPhase2Qa
    {
        public const string CliFlag = "-dragonPhase2QA";
        public const string CliE2EFlag = "-dragonPhase2E2E";
        public const string PersistenceKey = "valgor.dragons.v5.phase2-e2e";
        public const string EvidenceDir =
            @"C:\Valgor_Studio\docs\releases\dragon-phase2-p1-evidence";

        private static bool? _forcedActive;
        private static bool? _forcedE2E;

        public static bool IsActive =>
            _forcedActive ?? (HasFlag(CliFlag) || HasFlag(CliE2EFlag));

        public static bool IsE2ETest =>
            _forcedE2E ?? HasFlag(CliE2EFlag);

        public static void ForceForTests(bool active, bool e2e = false)
        {
            _forcedActive = active;
            _forcedE2E = e2e;
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
