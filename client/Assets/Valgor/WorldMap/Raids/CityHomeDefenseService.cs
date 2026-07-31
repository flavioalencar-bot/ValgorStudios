using System;
using Valgor.City.Buildings;
using Valgor.WorldMap.Creatures;

namespace Valgor.WorldMap.Raids
{
    /// <summary>
    /// Defesa dracônica provisória da cidade do jogador (muralha + guarnição + Guard).
    /// Não é PvP multiplayer — simulação local para o fluxo Fase 5.
    /// </summary>
    public static class CityHomeDefenseService
    {
        public static int ComputeDefensePower(int wallLevel, int heroGarrisonPower, int dragonGuardPower)
        {
            var wall = SupportBuildingRules.GetWallCityDefense(Math.Max(0, wallLevel));
            return Math.Max(1, wall + Math.Max(0, heroGarrisonPower) + Math.Max(0, dragonGuardPower));
        }

        public static bool TryResolve(
            int attackerPower,
            int defensePower,
            out CreatureDifficultyBand band,
            out bool defended,
            out int woundedPercent,
            out string report)
        {
            band = CreatureDifficultyResolver.Resolve(defensePower, Math.Max(1, attackerPower));
            defended = CreatureDifficultyResolver.CanDefeatProvisional(defensePower, Math.Max(1, attackerPower));
            woundedPercent = defended
                ? band switch
                {
                    CreatureDifficultyBand.Trivial => 5,
                    CreatureDifficultyBand.Easy => 10,
                    CreatureDifficultyBand.Fair => 18,
                    CreatureDifficultyBand.Hard => 28,
                    _ => 40
                }
                : band switch
                {
                    CreatureDifficultyBand.Hard => 35,
                    CreatureDifficultyBand.Fair => 45,
                    _ => 55
                };

            report = defended
                ? $"Defesa vitoriosa. Guarnição {defensePower} repeliu ataque {attackerPower} ({band}). Feridos ~{woundedPercent}%."
                : $"Cidade invadida. Ataque {attackerPower} superou defesa {defensePower} ({band}). Feridos ~{woundedPercent}%.";
            return true;
        }
    }
}
