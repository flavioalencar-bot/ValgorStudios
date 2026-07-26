using System;
using System.Collections.Generic;
using UnityEngine;
using Valgor.Heroes.Data;

namespace Valgor.Heroes.Factions
{
    [CreateAssetMenu(menuName = "Valgor/Heroes/Faction Config", fileName = "FactionConfig")]
    public sealed class FactionConfigSO : ScriptableObject
    {
        [Serializable]
        public sealed class AdvantageEntry
        {
            public string AttackerFactionId;
            public string DefenderFactionId;
        }

        [Serializable]
        public sealed class TeamBonusEntry
        {
            public int SameFaction;
            public int OtherFaction;
            public float TotalTroopAttackMultiplier = 1f;
        }

        public float AdvantageDamageMultiplier = 1.15f;
        public List<AdvantageEntry> Advantages = new()
        {
            new AdvantageEntry { AttackerFactionId = HeroFactionIds.RosaDeSangue, DefenderFactionId = HeroFactionIds.GuardaDaOrdem },
            new AdvantageEntry { AttackerFactionId = HeroFactionIds.GuardaDaOrdem, DefenderFactionId = HeroFactionIds.AsasDoAmanhecer },
            new AdvantageEntry { AttackerFactionId = HeroFactionIds.AsasDoAmanhecer, DefenderFactionId = HeroFactionIds.RosaDeSangue }
        };

        public List<TeamBonusEntry> TeamBonuses = new()
        {
            new TeamBonusEntry { SameFaction = 3, OtherFaction = 0, TotalTroopAttackMultiplier = 1.05f },
            new TeamBonusEntry { SameFaction = 3, OtherFaction = 2, TotalTroopAttackMultiplier = 1.07f },
            new TeamBonusEntry { SameFaction = 4, OtherFaction = 0, TotalTroopAttackMultiplier = 1.10f },
            new TeamBonusEntry { SameFaction = 5, OtherFaction = 0, TotalTroopAttackMultiplier = 1.15f }
        };
    }

    public sealed class FactionAdvantageResolver
    {
        private readonly Dictionary<string, string> _beats;
        private readonly float _multiplier;

        public FactionAdvantageResolver(FactionConfigSO config)
        {
            _multiplier = config.AdvantageDamageMultiplier;
            _beats = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var entry in config.Advantages)
            {
                _beats[entry.AttackerFactionId] = entry.DefenderFactionId;
            }
        }

        public bool HasAdvantage(string attackerFactionId, string defenderFactionId) =>
            _beats.TryGetValue(attackerFactionId, out var beaten)
            && string.Equals(beaten, defenderFactionId, StringComparison.Ordinal);

        public float ResolveDamageMultiplier(string attackerFactionId, string defenderFactionId) =>
            HasAdvantage(attackerFactionId, defenderFactionId) ? _multiplier : 1f;
    }

    public readonly struct TeamBonusResult
    {
        public TeamBonusResult(float multiplier, int sameFactionCount, string dominantFactionId)
        {
            TotalTroopAttackMultiplier = multiplier;
            SameFactionCount = sameFactionCount;
            DominantFactionId = dominantFactionId;
        }

        public float TotalTroopAttackMultiplier { get; }
        public int SameFactionCount { get; }
        public string DominantFactionId { get; }
    }

    public sealed class FactionBonusCalculator
    {
        private readonly List<FactionConfigSO.TeamBonusEntry> _rules;

        public FactionBonusCalculator(FactionConfigSO config)
        {
            _rules = new List<FactionConfigSO.TeamBonusEntry>(config.TeamBonuses);
            _rules.Sort((a, b) => b.TotalTroopAttackMultiplier.CompareTo(a.TotalTroopAttackMultiplier));
        }

        public TeamBonusResult Calculate(IReadOnlyList<string> factionIds)
        {
            if (factionIds == null || factionIds.Count == 0)
            {
                return new TeamBonusResult(1f, 0, null);
            }

            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var id in factionIds)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                counts.TryGetValue(id, out var current);
                counts[id] = current + 1;
            }

            string dominant = null;
            var same = 0;
            foreach (var pair in counts)
            {
                if (pair.Value <= same) continue;
                same = pair.Value;
                dominant = pair.Key;
            }

            var other = factionIds.Count - same;
            foreach (var rule in _rules)
            {
                if (rule.OtherFaction == 0)
                {
                    if (same >= rule.SameFaction && other == 0)
                    {
                        return new TeamBonusResult(rule.TotalTroopAttackMultiplier, same, dominant);
                    }
                }
                else if (same >= rule.SameFaction && other == rule.OtherFaction)
                {
                    return new TeamBonusResult(rule.TotalTroopAttackMultiplier, same, dominant);
                }
            }

            return new TeamBonusResult(1f, same, dominant);
        }
    }

    public static class HeroFactionResolver
    {
        public static string Describe(HeroFaction faction) => faction switch
        {
            HeroFaction.RosaDeSangue => "Agressão, explosão, assassinas e duelistas",
            HeroFaction.AsasDoAmanhecer => "Velocidade, precisão, magia e controle",
            HeroFaction.GuardaDaOrdem => "Defesa, liderança, proteção e suporte",
            _ => string.Empty
        };
    }
}
