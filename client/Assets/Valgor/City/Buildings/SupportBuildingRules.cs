using System;
using System.Text;
using Valgor.City.Data;
using Valgor.Core.Modules;

namespace Valgor.City.Buildings
{
    /// <summary>
    /// Stats de exibição para Arena/Hospital/Templo/Mercado/Lab/Torre/Muralha (sem sistemas novos).
    /// </summary>
    public static class SupportBuildingRules
    {
        public static string BuildWallDetails(int level)
        {
            var lv = Math.Max(0, level);
            var sb = new StringBuilder();
            sb.AppendLine("Função: defesa perimetral da cidade (combate completo na beta seguinte).");
            sb.AppendLine($"Defesa da cidade: +{GetWallCityDefense(lv)}");
            sb.AppendLine($"Durabilidade (HP): {GetWallHitPoints(lv):N0}");
            sb.AppendLine($"Resistência / proteção: +{GetWallResistancePercent(lv)}%");
            sb.Append(
                $"Próximo benefício: defesa +{GetWallCityDefense(lv + 1)} · " +
                $"HP {GetWallHitPoints(lv + 1):N0} · resistência +{GetWallResistancePercent(lv + 1)}%");
            return sb.ToString();
        }

        public static int GetWallCityDefense(int level) => level <= 0 ? 0 : 25 * level;
        public static int GetWallHitPoints(int level) => level <= 0 ? 0 : 500 + (350 * level);
        public static int GetWallResistancePercent(int level) => level <= 0 ? 0 : 4 * level;

        public static string BuildArenaDetails(int level)
        {
            var lv = Math.Max(0, level);
            var sb = new StringBuilder();
            sb.AppendLine($"Capacidade de formação: {GetArenaFormationCapacity(lv)}");
            sb.AppendLine($"Bônus de treinamento: +{GetArenaTrainingBonusPercent(lv)}%");
            sb.AppendLine($"Bônus de combate (provisório): +{GetArenaCombatBonusPercent(lv)}%");
            sb.Append($"Próximo benefício: formação {GetArenaFormationCapacity(lv + 1)} · treino +{GetArenaTrainingBonusPercent(lv + 1)}%");
            return sb.ToString();
        }

        public static int GetArenaFormationCapacity(int level) => level <= 0 ? 0 : 2 + level;
        public static int GetArenaTrainingBonusPercent(int level) => level <= 0 ? 0 : 5 * level;
        public static int GetArenaCombatBonusPercent(int level) => level <= 0 ? 0 : 3 * level;

        public static string BuildHospitalDetails(int level)
        {
            var lv = Math.Max(0, level);
            var sb = new StringBuilder();
            sb.AppendLine($"Capacidade de tratamento: {GetHospitalCapacity(lv)}");
            sb.AppendLine($"Velocidade de recuperação: +{GetHospitalRecoverySpeedPercent(lv)}%");
            sb.AppendLine($"Unidades em tratamento: {GetHospitalUnitsInCare(lv)} (provisório)");
            sb.Append($"Próximo benefício: capacidade {GetHospitalCapacity(lv + 1)} · recuperação +{GetHospitalRecoverySpeedPercent(lv + 1)}%");
            return sb.ToString();
        }

        public static int GetHospitalCapacity(int level) => level <= 0 ? 0 : 10 * level;
        public static int GetHospitalRecoverySpeedPercent(int level) => level <= 0 ? 0 : 8 * level;
        public static int GetHospitalUnitsInCare(int level) => 0; // sem sistema de feridos ainda

        public static string BuildTempleDetails(int level)
        {
            var lv = Math.Max(0, level);
            var sb = new StringBuilder();
            sb.AppendLine($"Bônus de recuperação: +{GetTempleRecoveryBonusPercent(lv)}%");
            sb.AppendLine($"Bônus de proteção: +{GetTempleProtectionBonusPercent(lv)}%");
            sb.Append($"Próximo benefício: recuperação +{GetTempleRecoveryBonusPercent(lv + 1)}% · proteção +{GetTempleProtectionBonusPercent(lv + 1)}%");
            return sb.ToString();
        }

        public static int GetTempleRecoveryBonusPercent(int level) => level <= 0 ? 0 : 6 * level;
        public static int GetTempleProtectionBonusPercent(int level) => level <= 0 ? 0 : 4 * level;

        public static string BuildMarketDetails(int level)
        {
            var lv = Math.Max(0, level);
            var sb = new StringBuilder();
            sb.AppendLine($"Capacidade de trocas: {GetMarketTradeCapacity(lv)}");
            sb.AppendLine($"Taxa da casa: {GetMarketFeePercent(lv)}%");
            sb.AppendLine($"Limite diário: {GetMarketDailyLimit(lv)} trocas");
            sb.Append($"Próximo benefício: {GetMarketTradeCapacity(lv + 1)} trocas · taxa {GetMarketFeePercent(lv + 1)}%");
            return sb.ToString();
        }

        public static int GetMarketTradeCapacity(int level) => level <= 0 ? 0 : 5 * level;
        public static int GetMarketFeePercent(int level) => level <= 0 ? 0 : Math.Max(5, 15 - level);
        public static int GetMarketDailyLimit(int level) => level <= 0 ? 0 : 3 + level;

        public static string BuildLaboratoryDetails(int level)
        {
            var lv = Math.Max(0, level);
            var sb = new StringBuilder();
            sb.AppendLine($"Bônus tecnológico: +{GetLabTechBonusPercent(lv)}%");
            sb.AppendLine($"Capacidade de projetos: {GetLabProjectSlots(lv)}");
            sb.AppendLine($"Próximo desbloqueio: {(lv <= 0 ? "slot de projeto inicial" : $"projeto Nv.{lv + 1} (beta)")}");
            sb.Append($"Próximo benefício: tech +{GetLabTechBonusPercent(lv + 1)}% · {GetLabProjectSlots(lv + 1)} projetos");
            return sb.ToString();
        }

        public static int GetLabTechBonusPercent(int level) => level <= 0 ? 0 : 5 * level;
        public static int GetLabProjectSlots(int level) => level <= 0 ? 0 : level;

        public static string BuildDragonTowerDetails(int level, IDragonGateway? dragons)
        {
            var lv = Math.Max(0, level);
            var sb = new StringBuilder();
            var capacity = dragons?.RoostCapacity ?? Math.Max(1, lv);
            var occupied = dragons?.RoostOccupantCount ?? 0;
            var ready = dragons?.GetReadyDragonCount() ?? 0;
            sb.AppendLine($"Capacidade do ninho: {occupied}/{capacity}");
            sb.AppendLine($"Dragões prontos: {ready}");
            if (dragons != null)
            {
                sb.AppendLine($"Jornada do ovo: {dragons.EggJourneyPhaseLabel}");
                sb.AppendLine(dragons.DescribeEggJourney());
                sb.AppendLine($"Desbloqueio: Castelo Nv.{dragons.EggUnlockCastleLevel}");
            }

            sb.AppendLine($"Bônus de vínculo: +{GetDragonBondBonusPercent(lv)}%");
            sb.AppendLine($"Recuperação no ninho: +{GetDragonRecoveryBonusPercent(lv)}%");
            sb.AppendLine($"Próximo desbloqueio: {(lv < 2 ? "segundo slot estável" : "evolução assistida (módulo existente)")}");
            sb.Append($"Próximo benefício: vínculo +{GetDragonBondBonusPercent(lv + 1)}% · recuperação +{GetDragonRecoveryBonusPercent(lv + 1)}%");

            if (dragons != null)
            {
                foreach (var d in dragons.GetDragonStatuses())
                {
                    sb.AppendLine();
                    var levelLabel = d.DragonLevel > 0 ? $"Nv.{d.DragonLevel}" : "ovo";
                    sb.Append(
                        $"{d.DisplayName} ({levelLabel}): {d.StateLabel} · fome {d.Hunger}/{d.MaxHunger}");
                    if (d.CareRequired > 0 &&
                        string.Equals(d.StateLabel, "HATCHING", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append($" · cuidados {d.CareCount}/{d.CareRequired}");
                    }
                    else
                    {
                        sb.Append($" · vínculo Nv.{d.BondLevel}");
                    }
                }
            }

            return sb.ToString();
        }

        public static int GetDragonBondBonusPercent(int level) => level <= 0 ? 0 : 4 * level;
        public static int GetDragonRecoveryBonusPercent(int level) => level <= 0 ? 0 : 5 * level;

        public static string DescribeUpgradeBenefit(string buildingDefinitionId, int level) =>
            buildingDefinitionId switch
            {
                "arena" => $"Formação {GetArenaFormationCapacity(level + 1)} · treino +{GetArenaTrainingBonusPercent(level + 1)}%",
                "hospital" => $"Capacidade {GetHospitalCapacity(level + 1)} · recuperação +{GetHospitalRecoverySpeedPercent(level + 1)}%",
                "temple" => $"Recuperação +{GetTempleRecoveryBonusPercent(level + 1)}% · proteção +{GetTempleProtectionBonusPercent(level + 1)}%",
                "market" => $"Trocas {GetMarketTradeCapacity(level + 1)} · taxa {GetMarketFeePercent(level + 1)}%",
                "laboratory" => $"Tech +{GetLabTechBonusPercent(level + 1)}% · {GetLabProjectSlots(level + 1)} projetos",
                "dragon-tower" => $"Vínculo +{GetDragonBondBonusPercent(level + 1)}% · recuperação +{GetDragonRecoveryBonusPercent(level + 1)}%",
                "wall" =>
                    $"Defesa +{GetWallCityDefense(level + 1)} · HP {GetWallHitPoints(level + 1):N0} · " +
                    $"resistência +{GetWallResistancePercent(level + 1)}%",
                _ => string.Empty
            };
    }
}
