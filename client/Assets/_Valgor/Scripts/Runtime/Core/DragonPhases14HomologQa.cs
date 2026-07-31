using System;

namespace Valgor.Core
{
    /// <summary>
    /// Homologação jogável Dragão Fases 1–4 (sem Fase 5).
    /// </summary>
    public static class DragonPhases14HomologQa
    {
        public const string CliFlag = "-dragonPhases14Homolog";
        public const string EvidenceDir =
            @"C:\Valgor_Studio\docs\releases\dragon-phases-1-4-homolog-evidence";

        private static bool? _forced;

        public static bool IsActive =>
            _forced ?? HasFlag(CliFlag);

        public static void ForceForTests(bool active) => _forced = active;

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
