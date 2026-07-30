using System;
using System.Collections.Generic;

namespace Valgor.Dragons.Mount
{
    /// <summary>Compatibilidade herói ↔ dragão para montaria estratégica (Fase 4).</summary>
    public static class DragonMountCompatibility
    {
        private static readonly Dictionary<string, (string DisplayName, int MinDragonLevel)> Riders =
            new(StringComparer.Ordinal)
            {
                ["HERO_VORTEX_000"] = ("Vortex", 1),
                ["HERO_ELYRA_001"] = ("Elyra", 6),
                ["HERO_VESPERA_010"] = ("Vespera", 11)
            };

        public static IReadOnlyList<string> AllRiderIds
        {
            get
            {
                var list = new List<string>(Riders.Keys);
                list.Sort(StringComparer.Ordinal);
                return list;
            }
        }

        public static bool TryGetDisplayName(string heroId, out string displayName)
        {
            if (Riders.TryGetValue(heroId, out var info))
            {
                displayName = info.DisplayName;
                return true;
            }

            displayName = string.Empty;
            return false;
        }

        public static bool IsCompatible(string heroId, int dragonLevel, out string error)
        {
            if (string.IsNullOrWhiteSpace(heroId) || !Riders.TryGetValue(heroId, out var info))
            {
                error = "Herói incompatível com montaria.";
                return false;
            }

            if (dragonLevel < info.MinDragonLevel)
            {
                error = $"{info.DisplayName} exige Dragão Nv.{info.MinDragonLevel}+.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static int MinDragonLevel(string heroId) =>
            Riders.TryGetValue(heroId, out var info) ? info.MinDragonLevel : int.MaxValue;
    }
}
