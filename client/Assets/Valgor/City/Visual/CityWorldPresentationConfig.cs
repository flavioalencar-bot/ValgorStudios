namespace Valgor.City.Visual
{
    /// <summary>
    /// Apresentação da fundação ambiental.
    /// Clean = screenshots / player homologação visual.
    /// Audit = anchors/labels/bounds.
    /// </summary>
    public static class CityWorldPresentationConfig
    {
        /// <summary>Oculta labels técnicos, anchors e bounds no mundo.</summary>
        public static bool CleanPresentation { get; private set; } = true;

        /// <summary>Mostra anchors/IDs para auditoria (-capitalWorldAudit).</summary>
        public static bool AuditOverlays { get; private set; }

        public static void ApplyFromCommandLine()
        {
            AuditOverlays = HasFlag("-capitalWorldAudit") || HasFlag("-capitalWorldDebug");
            CleanPresentation = !AuditOverlays;
            if (HasFlag("-capitalWorldDebugLabels"))
            {
                CleanPresentation = false;
            }

            // Correção NoTripo: limpo por padrão (sem debug).
            if (HasFlag("-capitalWorldNoTripoCorrection") && !AuditOverlays)
            {
                CleanPresentation = true;
            }
        }

        public static void SetCleanForTests(bool clean)
        {
            CleanPresentation = clean;
            AuditOverlays = !clean;
        }

        private static bool HasFlag(string flag)
        {
            foreach (var arg in System.Environment.GetCommandLineArgs())
            {
                if (string.Equals(arg, flag, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
