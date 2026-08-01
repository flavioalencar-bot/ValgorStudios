using System;
using System.Collections.Generic;
using Valgor.City.Data;
using Valgor.City.Production;

namespace Valgor.City.Buildings
{
    /// <summary>
    /// IDs canônicos dos 17 edifícios oficiais + aliases legados (save 15→17).
    /// Única camada de compatibilidade — não espalhar aliases no restante do código.
    /// </summary>
    public static class BuildingIdMigration
    {
        public const int OfficialCount = 17;

        /// <summary>Ordem oficial imutável (produto).</summary>
        public static readonly string[] CanonicalIds =
        {
            "castle",
            "wall",
            "farm",
            "lumbermill",
            "quarry",
            "mine",
            "warehouse",
            "residence",
            "market",
            "training-ground",
            "foundry",
            "arena",
            "academy",
            "hospital",
            "temple",
            "hero-hall",
            "dragon-tower"
        };

        private static readonly Dictionary<string, string> LegacyToCanonical =
            new(StringComparer.Ordinal)
            {
                ["institute"] = "hero-hall",
                ["laboratory"] = "training-ground",
                ["sawmill"] = "lumbermill"
            };

        private static readonly HashSet<string> CanonicalSet =
            new(CanonicalIds, StringComparer.Ordinal);

        public static bool IsCanonical(string? id) =>
            !string.IsNullOrWhiteSpace(id) && CanonicalSet.Contains(id);

        public static string ToCanonical(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return string.Empty;
            }

            if (LegacyToCanonical.TryGetValue(id, out var mapped))
            {
                return mapped;
            }

            return id;
        }

        public static bool TryResolveLegacy(string legacyId, out string canonicalId)
        {
            if (LegacyToCanonical.TryGetValue(legacyId, out canonicalId!))
            {
                return true;
            }

            canonicalId = string.Empty;
            return false;
        }

        public static IReadOnlyDictionary<string, string> LegacyAliases => LegacyToCanonical;

        /// <summary>
        /// Migra progresso/produção de aliases para IDs canônicos.
        /// Novos edifícios (residence, foundry) nascem Available Nv.0 se ausentes.
        /// </summary>
        public static ProductionSnapshot MigrateSnapshot(ProductionSnapshot? source)
        {
            var result = new ProductionSnapshot
            {
                SavedAtUtc = source?.SavedAtUtc ?? DateTime.UtcNow
            };

            if (source != null)
            {
                foreach (var pair in source.Wallet)
                {
                    result.Wallet[pair.Key] = pair.Value;
                }

                foreach (var pair in source.Buildings)
                {
                    var canonical = ToCanonical(pair.Key);
                    if (string.IsNullOrEmpty(canonical))
                    {
                        continue;
                    }

                    if (!result.Buildings.ContainsKey(canonical))
                    {
                        result.Buildings[canonical] = new BuildingProductionState(
                            canonical,
                            pair.Value.LastUpdatedUtc)
                        {
                            Accumulated = pair.Value.Accumulated
                        };
                    }
                }

                foreach (var pair in source.BuildingProgress)
                {
                    var canonical = ToCanonical(string.IsNullOrEmpty(pair.Value.DefinitionId)
                        ? pair.Key
                        : pair.Value.DefinitionId);
                    if (string.IsNullOrEmpty(canonical) || !IsCanonical(canonical))
                    {
                        continue;
                    }

                    var record = new BuildingProgressRecord
                    {
                        DefinitionId = canonical,
                        Level = pair.Value.Level,
                        State = pair.Value.State,
                        UpgradeCompletesAtUtc = pair.Value.UpgradeCompletesAtUtc
                    };

                    if (!result.BuildingProgress.TryGetValue(canonical, out var existing))
                    {
                        result.BuildingProgress[canonical] = record;
                        continue;
                    }

                    // Em conflito de alias, preserva o maior nível / estado mais avançado.
                    if (record.Level > existing.Level ||
                        (record.Level == existing.Level && IsRicherState(record.State, existing.State)))
                    {
                        result.BuildingProgress[canonical] = record;
                    }
                }
            }

            EnsureNewBuildingsInitialized(result);
            return result;
        }

        private static void EnsureNewBuildingsInitialized(ProductionSnapshot snapshot)
        {
            // Residência e Fundição: novos — Available Nv.0 (não eleva progresso indevidamente).
            EnsureProgress(snapshot, "residence", 0, BuildingState.Available);
            EnsureProgress(snapshot, "foundry", 0, BuildingState.Available);

            // Garante hero-hall / training-ground existem após migração de aliases.
            if (!snapshot.BuildingProgress.ContainsKey("hero-hall"))
            {
                EnsureProgress(snapshot, "hero-hall", 0, BuildingState.Locked);
            }

            if (!snapshot.BuildingProgress.ContainsKey("training-ground"))
            {
                EnsureProgress(snapshot, "training-ground", 0, BuildingState.Available);
            }
        }

        private static void EnsureProgress(
            ProductionSnapshot snapshot,
            string id,
            int level,
            BuildingState state)
        {
            if (snapshot.BuildingProgress.ContainsKey(id))
            {
                return;
            }

            snapshot.BuildingProgress[id] = new BuildingProgressRecord
            {
                DefinitionId = id,
                Level = level,
                State = state,
                UpgradeCompletesAtUtc = null
            };
        }

        private static bool IsRicherState(BuildingState candidate, BuildingState current) =>
            StateRank(candidate) > StateRank(current);

        private static int StateRank(BuildingState state) => state switch
        {
            BuildingState.Ready => 4,
            BuildingState.Upgrading => 3,
            BuildingState.Available => 2,
            BuildingState.Locked => 1,
            _ => 0
        };
    }
}
