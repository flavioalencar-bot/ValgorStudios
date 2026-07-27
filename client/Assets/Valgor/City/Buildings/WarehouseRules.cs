using System;
using Valgor.City.Data;

namespace Valgor.City.Buildings
{
    /// <summary>
    /// Capacidade e proteção do Armazém por nível (lógica pura).
    /// </summary>
    public static class WarehouseRules
    {
        public static long GetCapacity(int level) =>
            5_000L + Math.Max(0, level) * 2_500L;

        /// <summary>Recursos protegidos contra saque (por tipo, soft). </summary>
        public static long GetProtection(int level) =>
            1_000L + Math.Max(0, level) * 500L;

        public static long GetNextCapacity(int level) => GetCapacity(level + 1);

        public static long GetNextProtection(int level) => GetProtection(level + 1);
    }
}
