using System;

namespace Valgor.WorldMap.Filters
{
    public sealed class WorldMapFilterSnapshot
    {
        public bool ShowCities { get; set; } = true;
        public bool ShowVillages { get; set; } = true;
        public bool ShowResources { get; set; } = true;
        public bool ShowCreatures { get; set; } = true;
        public bool ShowDragons { get; set; } = true;
        public bool ShowLandmarks { get; set; } = true;
        public bool ShowOccupied { get; set; } = true;
        public bool ShowAvailable { get; set; } = true;
    }

    public interface IWorldMapFilterPersistenceRepository
    {
        WorldMapFilterSnapshot? Load();
        void Save(WorldMapFilterSnapshot snapshot);
    }

    /// <summary>
    /// Persistência técnica dos filtros. Memória cobre City↔WorldMap; PlayerPrefs cobre restart.
    /// </summary>
    public sealed class WorldMapFilterPersistenceRepository : IWorldMapFilterPersistenceRepository
    {
        private readonly string _keyPrefix;
        private WorldMapFilterSnapshot? _memory;

        public WorldMapFilterPersistenceRepository(string keyPrefix)
        {
            _keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        }

        public WorldMapFilterSnapshot? Load()
        {
            if (_memory != null)
            {
                return Clone(_memory);
            }

#if UNITY_5_3_OR_NEWER
            return LoadFromPrefs();
#else
            return null;
#endif
        }

        public void Save(WorldMapFilterSnapshot snapshot)
        {
            _memory = Clone(snapshot);
#if UNITY_5_3_OR_NEWER
            SaveToPrefs(snapshot);
#endif
        }

        private static WorldMapFilterSnapshot Clone(WorldMapFilterSnapshot source) =>
            new()
            {
                ShowCities = source.ShowCities,
                ShowVillages = source.ShowVillages,
                ShowResources = source.ShowResources,
                ShowCreatures = source.ShowCreatures,
                ShowDragons = source.ShowDragons,
                ShowLandmarks = source.ShowLandmarks,
                ShowOccupied = source.ShowOccupied,
                ShowAvailable = source.ShowAvailable
            };

#if UNITY_5_3_OR_NEWER
        private WorldMapFilterSnapshot? LoadFromPrefs()
        {
            if (!UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".cities"))
            {
                return null;
            }

            return new WorldMapFilterSnapshot
            {
                ShowCities = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".cities", 1) != 0,
                ShowVillages = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".villages", 1) != 0,
                ShowResources = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".resources", 1) != 0,
                ShowCreatures = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".creatures", 1) != 0,
                ShowDragons = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".dragons", 1) != 0,
                ShowLandmarks = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".landmarks", 1) != 0,
                ShowOccupied = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".occupied", 1) != 0,
                ShowAvailable = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".available", 1) != 0
            };
        }

        private void SaveToPrefs(WorldMapFilterSnapshot snapshot)
        {
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".cities", snapshot.ShowCities ? 1 : 0);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".villages", snapshot.ShowVillages ? 1 : 0);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".resources", snapshot.ShowResources ? 1 : 0);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".creatures", snapshot.ShowCreatures ? 1 : 0);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".dragons", snapshot.ShowDragons ? 1 : 0);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".landmarks", snapshot.ShowLandmarks ? 1 : 0);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".occupied", snapshot.ShowOccupied ? 1 : 0);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".available", snapshot.ShowAvailable ? 1 : 0);
            UnityEngine.PlayerPrefs.Save();
        }
#endif
    }
}
