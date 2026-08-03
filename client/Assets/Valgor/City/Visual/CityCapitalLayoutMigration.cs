using System;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Migração de layout espacial V1→V2.
    /// Etapa A: stub — NÃO aplicado (preview não persiste versão).
    /// Etapa B (após aprovação): promover e chamar TryMigrate.
    /// </summary>
    public static class CityCapitalLayoutMigration
    {
        public const string PrefsVersionKey = "valgor.city.capitalLayoutVersion";

        public static int ReadPersistedVersion() => CityCapitalLayoutRuntime.Version1;

        /// <summary>
        /// Aplica migração oficial V1→V2. Idempotente. Em falha: mantém V1.
        /// Bloqueada na Etapa A (preview).
        /// </summary>
        public static bool TryMigrate(out string detail)
        {
            detail =
                "Migração V2 bloqueada — aguardando aprovação visual do preview " +
                "(-capitalLayoutV2Preview). Save inalterado.";
            return false;
        }
    }
}
