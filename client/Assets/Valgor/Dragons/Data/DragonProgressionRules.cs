using System;
using System.Collections.Generic;

namespace Valgor.Dragons.Data
{
    /// <summary>
    /// Regras Fase 2: Nv.1→30, caps Castelo/Torre, XP, rituais e estágios.
    /// </summary>
    public static class DragonProgressionRules
    {
        public const int AbsoluteMaxLevel = 30;
        public static readonly int[] RitualTargetLevels = { 6, 11, 16, 21, 26 };

        public static bool IsRitualTarget(int targetLevel)
        {
            for (var i = 0; i < RitualTargetLevels.Length; i++)
            {
                if (RitualTargetLevels[i] == targetLevel)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Cap pelo Castelo: Nv.dragão ≤ Nv.castelo (mín. 20 para conteúdo).</summary>
        public static int CapFromCastle(int castleLevel)
        {
            if (castleLevel < 20)
            {
                return 0;
            }

            return Math.Min(AbsoluteMaxLevel, Math.Max(1, castleLevel));
        }

        /// <summary>Cap pela Torre: Nv.1 → 5, +2 por nível da torre (teto 30).</summary>
        public static int CapFromTower(int towerLevel)
        {
            var lv = Math.Max(0, towerLevel);
            if (lv <= 0)
            {
                return 0;
            }

            return Math.Min(AbsoluteMaxLevel, 5 + (lv - 1) * 2);
        }

        public static int EffectiveMaxLevel(int castleLevel, int towerLevel) =>
            Math.Min(AbsoluteMaxLevel, Math.Min(CapFromCastle(castleLevel), CapFromTower(towerLevel)));

        public static int ExperienceRequiredForLevel(int currentLevel)
        {
            if (currentLevel < 1 || currentLevel >= AbsoluteMaxLevel)
            {
                return 0;
            }

            return 40 + currentLevel * 20;
        }

        public static DragonGrowthStage StageForLevel(int level)
        {
            if (level <= 0)
            {
                return DragonGrowthStage.Egg;
            }

            if (level < 6)
            {
                return DragonGrowthStage.Hatchling;
            }

            if (level < 11)
            {
                return DragonGrowthStage.Juvenile;
            }

            if (level < 16)
            {
                return DragonGrowthStage.Adult;
            }

            if (level < 21)
            {
                return DragonGrowthStage.Elder;
            }

            return DragonGrowthStage.Ancient;
        }

        public static string RitualName(int targetLevel) =>
            targetLevel switch
            {
                6 => "Ritual das Brasas",
                11 => "Ritual das Escamas",
                16 => "Ritual da Chama",
                21 => "Ritual do Vínculo",
                26 => "Ritual Ancestral",
                _ => "Ritual de Evolução"
            };

        public static IReadOnlyList<int> AllRitualTargets => RitualTargetLevels;
    }
}
