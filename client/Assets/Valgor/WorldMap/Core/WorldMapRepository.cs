using System;
using System.Collections.Generic;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Core
{
    public sealed class WorldMapSnapshot
    {
        public DateTime SavedAtUtc { get; set; }
        public DateTime LastAdvanceUtc { get; set; }
        public Dictionary<string, WorldNodeInstance> Nodes { get; } = new();
        public MarchOrder? March { get; set; }
    }

    public interface IWorldMapRepository
    {
        WorldMapSnapshot? Load();
        void Save(WorldMapSnapshot snapshot);
    }

    /// <summary>
    /// Persistência local técnica. Memória da instância cobre City↔WorldMap via ServiceRegistry;
    /// PlayerPrefs cobre restart. Contrato pronto para backend.
    /// </summary>
    public sealed class LocalWorldMapRepository : IWorldMapRepository
    {
        private readonly string _keyPrefix;
        private WorldMapSnapshot? _memory;

        public LocalWorldMapRepository(string keyPrefix)
        {
            _keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        }

        public WorldMapSnapshot? Load()
        {
            if (_memory != null)
            {
                return Clone(_memory);
            }

#if UNITY_5_3_OR_NEWER
            var loaded = LoadFromPrefs();
            if (loaded != null)
            {
                _memory = Clone(loaded);
            }

            return loaded;
#else
            return null;
#endif
        }

        public void Save(WorldMapSnapshot snapshot)
        {
            _memory = Clone(snapshot);
#if UNITY_5_3_OR_NEWER
            SaveToPrefs(snapshot);
#endif
        }

        private static WorldMapSnapshot Clone(WorldMapSnapshot source)
        {
            var clone = new WorldMapSnapshot
            {
                SavedAtUtc = source.SavedAtUtc,
                LastAdvanceUtc = source.LastAdvanceUtc,
                March = source.March == null
                    ? null
                    : new MarchOrder(
                        source.March.Id,
                        source.March.ReservationId,
                        source.March.OriginNodeId,
                        source.March.TargetNodeId,
                        source.March.DepartedAtUtc,
                        source.March.ArrivesAtUtc,
                        source.March.Phase)
                    {
                        CurrentNodeId = source.March.CurrentNodeId
                    }
            };

            foreach (var pair in source.Nodes)
            {
                clone.Nodes[pair.Key] = new WorldNodeInstance(
                    pair.Value.DefinitionId,
                    pair.Value.Status,
                    pair.Value.RemainingAmount);
            }

            return clone;
        }

#if UNITY_5_3_OR_NEWER
        private WorldMapSnapshot? LoadFromPrefs()
        {
            if (!UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".meta"))
            {
                return null;
            }

            var snapshot = new WorldMapSnapshot
            {
                SavedAtUtc = ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".meta")),
                LastAdvanceUtc = ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".advance"))
            };

            foreach (var definition in WorldNodeCatalog.All.Values)
            {
                var key = _keyPrefix + ".n." + definition.Id;
                if (!UnityEngine.PlayerPrefs.HasKey(key + ".status"))
                {
                    continue;
                }

                var status = (WorldNodeStatus)UnityEngine.PlayerPrefs.GetInt(key + ".status");
                var amount = long.Parse(
                    UnityEngine.PlayerPrefs.GetString(key + ".amount", "0"),
                    System.Globalization.CultureInfo.InvariantCulture);
                snapshot.Nodes[definition.Id] = new WorldNodeInstance(definition.Id, status, amount);
            }

            if (UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".march.id"))
            {
                snapshot.March = new MarchOrder(
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.id"),
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.res"),
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.origin"),
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.target"),
                    ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.dep")),
                    ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.arr")),
                    (MarchPhase)UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".march.phase"))
                {
                    CurrentNodeId = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.current", null)
                };
            }

            return snapshot;
        }

        private void SaveToPrefs(WorldMapSnapshot snapshot)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".meta", snapshot.SavedAtUtc.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".advance", snapshot.LastAdvanceUtc.ToString("O", inv));

            foreach (var pair in snapshot.Nodes)
            {
                var key = _keyPrefix + ".n." + pair.Key;
                UnityEngine.PlayerPrefs.SetInt(key + ".status", (int)pair.Value.Status);
                UnityEngine.PlayerPrefs.SetString(key + ".amount", pair.Value.RemainingAmount.ToString(inv));
            }

            if (snapshot.March == null)
            {
                UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + ".march.id");
            }
            else
            {
                var m = snapshot.March;
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.id", m.Id);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.res", m.ReservationId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.origin", m.OriginNodeId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.target", m.TargetNodeId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.dep", m.DepartedAtUtc.ToString("O", inv));
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.arr", m.ArrivesAtUtc.ToString("O", inv));
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".march.phase", (int)m.Phase);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.current", m.CurrentNodeId ?? string.Empty);
            }

            UnityEngine.PlayerPrefs.Save();
        }

        private static DateTime ParseTime(string raw) =>
            DateTime.Parse(raw, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
#endif
    }
}
