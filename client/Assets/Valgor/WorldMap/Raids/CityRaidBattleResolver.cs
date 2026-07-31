using System;
using Valgor.Dragons.Data;
using Valgor.WorldMap.Creatures;

namespace Valgor.WorldMap.Raids
{
    /// <summary>Resolvedor puro de batalha de raid/defesa (sem Save/UI).</summary>
    public static class CityRaidBattleResolver
    {
        public static CityRaidBattleResult Resolve(CityRaidBattleInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (string.IsNullOrWhiteSpace(input.BattleId))
            {
                throw new ArgumentException("BattleId obrigatório.", nameof(input));
            }

            var attacker = input.Attacker ?? new FormationSnapshot();
            var defender = input.Defender ?? new FormationSnapshot();
            var wall = input.Wall ?? new WallSnapshot();
            var dragon = input.Dragon ?? new DragonBattleSnapshot();

            var role = dragon.Role;
            var formationBonus = role switch
            {
                DragonBattleRole.Assault => RaidCombatCatalog.AssaultFormationBonus,
                DragonBattleRole.Guard => RaidCombatCatalog.GuardFormationBonus,
                _ => RaidCombatCatalog.SupportFormationBonus
            };

            var attackerPower = (int)Math.Floor(attacker.Power * (1m + formationBonus)) + Math.Max(0, dragon.SupportPower);
            if (role == DragonBattleRole.Assault)
            {
                attackerPower = (int)Math.Floor(attackerPower * (1m + RaidCombatCatalog.AssaultFormationBonus));
            }

            var wrathEligible = input.IsCityRaid &&
                                !input.IsDefense &&
                                role == DragonBattleRole.Assault &&
                                dragon.DominatorWrathUnlocked &&
                                dragon.DominatorWrathRequested &&
                                !dragon.DominatorWrathAlreadyUsedInBattle &&
                                dragon.DragonLevel >= RaidCombatCatalog.DominatorWrathUnlockLevel;

            var wallDamageRatio = role == DragonBattleRole.Assault
                ? RaidCombatCatalog.AssaultWallDamageRatio
                : RaidCombatCatalog.SupportWallDamageRatio;
            var incomingWall = (long)Math.Floor(Math.Max(0, attackerPower) * wallDamageRatio);

            var wallResult = WallCombatResolver.Resolve(new WallCombatInput
            {
                WallLevel = wall.WallLevel,
                MaxDurability = wall.MaxDurability,
                CurrentDurability = wall.CurrentDurability,
                DefenseBonus = wall.DefenseBonus,
                DamageReduction = wall.DamageReduction,
                IncomingWallDamage = incomingWall,
                DominatorWrathActive = wrathEligible,
                DominatorWrathDamageBonus = RaidCombatCatalog.DominatorWrathDamageBonus,
                DominatorWrathDefenseIgnore = RaidCombatCatalog.DominatorWrathDefenseIgnore
            });

            // Poder dos defensores = catálogo. Bônus da muralha vale na fase WallCombatResolver
            // (durabilidade / redução); não infla DEF do confronto para não regressar CanDefeat homologado.
            var defenderPower = Math.Max(1, defender.Power);

            if (input.IsDefense && role == DragonBattleRole.Guard)
            {
                defenderPower = (int)Math.Floor(defenderPower * (1m + RaidCombatCatalog.GuardFormationBonus));
            }

            var band = CreatureDifficultyResolver.Resolve(
                Math.Max(1, attackerPower),
                Math.Max(1, defenderPower));
            var canWin = CreatureDifficultyResolver.CanDefeatProvisional(
                Math.Max(1, attackerPower),
                Math.Max(1, defenderPower));
            BattleOutcome outcome;
            if (canWin)
            {
                outcome = BattleOutcome.Victory;
            }
            else if (attackerPower == defenderPower)
            {
                outcome = BattleOutcome.Draw;
            }
            else
            {
                outcome = BattleOutcome.Defeat;
            }

            var attackerTroops = Math.Max(1, attacker.TroopCount);
            var defenderTroops = Math.Max(1, defender.TroopCount);
            var attackerRaw = CasualtyByBand(attackerTroops, band, won: outcome == BattleOutcome.Victory);
            var defenderRaw = CasualtyByBand(defenderTroops, band, won: outcome != BattleOutcome.Victory);

            var hospitalFree = Math.Max(0, input.HospitalFreeCapacity);
            // Em raid ofensivo o Hospital do alvo trata defensores; atacante usa Hospital próprio (mesma capacidade passada se defesa).
            var defenderCasualties = CasualtyResolver.Resolve(
                CasualtyResolver.TroopsOnlyCasualties(defenderRaw),
                input.IsDefense ? 0 : hospitalFree);
            var attackerCasualties = CasualtyResolver.Resolve(
                CasualtyResolver.TroopsOnlyCasualties(attackerRaw),
                input.IsDefense ? hospitalFree : Math.Max(0, hospitalFree / 4));

            var protection = WarehouseProtectionResolver.Resolve(
                input.DefenderResources ?? new ResourceBundle(),
                input.WarehouseProtection ?? new ResourceBundle(),
                input.MinimumReserve ?? new ResourceBundle());

            var loot = RaidLootCalculator.Calculate(
                outcome,
                protection.LootableResources,
                input.MarchCapacity,
                string.IsNullOrWhiteSpace(input.LootTransactionId)
                    ? input.BattleId + ":loot"
                    : input.LootTransactionId);

            var energy = role switch
            {
                DragonBattleRole.Assault => RaidCombatCatalog.AssaultEnergyCost,
                DragonBattleRole.Guard => RaidCombatCatalog.GuardEnergyCost,
                _ => RaidCombatCatalog.SupportEnergyCost
            };

            var summary =
                $"{(input.IsDefense ? "Defesa" : "Raid")} {outcome} · ATK {attackerPower} vs DEF {defenderPower} ({band}) · " +
                $"Muralha {wallResult.CurrentDurabilityBefore}→{wallResult.CurrentDurabilityAfter}" +
                (wrathEligible ? " · Ira do Dominador" : string.Empty);

            return new CityRaidBattleResult
            {
                BattleId = input.BattleId,
                Outcome = outcome,
                WallDamage = wallResult.DamageApplied,
                WallDurabilityBefore = wallResult.CurrentDurabilityBefore,
                WallDurabilityAfter = wallResult.CurrentDurabilityAfter,
                AttackerCasualties = attackerCasualties,
                DefenderCasualties = defenderCasualties,
                Loot = loot,
                DragonEnergySpent = energy,
                DominatorWrathUsed = wrathEligible,
                AttackerPowerFinal = attackerPower,
                DefenderPowerFinal = defenderPower,
                Summary = summary
            };
        }

        private static int CasualtyByBand(int troops, CreatureDifficultyBand band, bool won)
        {
            var ratio = band switch
            {
                CreatureDifficultyBand.Trivial => won ? 0.08 : 0.20,
                CreatureDifficultyBand.Easy => won ? 0.12 : 0.28,
                CreatureDifficultyBand.Fair => won ? 0.18 : 0.35,
                CreatureDifficultyBand.Hard => won ? 0.28 : 0.45,
                _ => won ? 0.35 : 0.55
            };
            return (int)Math.Floor(Math.Max(0, troops) * ratio);
        }
    }
}
