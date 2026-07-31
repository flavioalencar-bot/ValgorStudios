using System;

namespace Valgor.WorldMap.Raids
{
    /// <summary>Resolvedor puro de combate à Muralha (sem UI/Save).</summary>
    public static class WallCombatResolver
    {
        public static WallCombatResult Resolve(WallCombatInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var max = Math.Max(0, input.MaxDurability);
            var before = Math.Clamp(input.CurrentDurability, 0, max);
            var reduction = Clamp01(input.DamageReduction);
            var defenseBonus = Math.Max(0m, input.DefenseBonus);
            var incoming = Math.Max(0, input.IncomingWallDamage);

            if (input.DominatorWrathActive)
            {
                incoming = (long)Math.Floor(incoming * (1m + Math.Max(0m, input.DominatorWrathDamageBonus)));
                defenseBonus = Math.Max(0m, defenseBonus * (1m - Clamp01(input.DominatorWrathDefenseIgnore)));
            }

            var damageTaken = (long)Math.Floor(incoming * (1m - reduction));
            if (damageTaken < 0)
            {
                damageTaken = 0;
            }

            var after = Math.Clamp(before - damageTaken, 0, max);
            var applied = before - after;

            return new WallCombatResult
            {
                WallLevel = input.WallLevel,
                MaxDurability = max,
                CurrentDurabilityBefore = before,
                CurrentDurabilityAfter = after,
                DamageApplied = applied,
                EffectiveDefenseBonus = after > 0 ? defenseBonus : 0m
            };
        }

        public static long ApplyRepair(
            long current,
            long max,
            WallRepairJob job,
            ProcessedOperationRegistry processed,
            out string error)
        {
            error = string.Empty;
            if (job == null || string.IsNullOrWhiteSpace(job.WallRepairId))
            {
                error = "WallRepairId inválido.";
                return current;
            }

            if (processed.WallRepairIds.Contains(job.WallRepairId) || job.Completed)
            {
                error = "Reparo já aplicado.";
                return current;
            }

            var capped = Math.Clamp(current, 0, Math.Max(0, max));
            var restore = Math.Max(0, job.DurabilityToRestore);
            var next = Math.Min(Math.Max(0, max), capped + restore);
            job.Completed = true;
            processed.WallRepairIds.Add(job.WallRepairId);
            return next;
        }

        private static decimal Clamp01(decimal value) =>
            value < 0m ? 0m : value > 1m ? 1m : value;
    }
}
