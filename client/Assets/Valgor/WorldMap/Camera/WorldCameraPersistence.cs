using System;

namespace Valgor.WorldMap.Camera
{
    public sealed class WorldCameraState
    {
        public float X { get; set; }
        public float Y { get; set; } = 30f;
        public float Z { get; set; }
        public float OrthographicSize { get; set; } = 14f;
        public bool HasSavedPose { get; set; }
    }

    public interface IWorldCameraStateRepository
    {
        WorldCameraState? Load();
        void Save(WorldCameraState state);
    }

    /// <summary>
    /// Persistência técnica da câmera. Memória cobre City↔WorldMap; PlayerPrefs cobre restart.
    /// </summary>
    public sealed class WorldCameraStateRepository : IWorldCameraStateRepository
    {
        private readonly string _keyPrefix;
        private WorldCameraState? _memory;

        public WorldCameraStateRepository(string keyPrefix)
        {
            _keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        }

        public WorldCameraState? Load()
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

        public void Save(WorldCameraState state)
        {
            _memory = Clone(state);
#if UNITY_5_3_OR_NEWER
            SaveToPrefs(state);
#endif
        }

        private static WorldCameraState Clone(WorldCameraState source) =>
            new()
            {
                X = source.X,
                Y = source.Y,
                Z = source.Z,
                OrthographicSize = source.OrthographicSize,
                HasSavedPose = source.HasSavedPose
            };

#if UNITY_5_3_OR_NEWER
        private WorldCameraState? LoadFromPrefs()
        {
            if (!UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".saved"))
            {
                return null;
            }

            var inv = System.Globalization.CultureInfo.InvariantCulture;
            return new WorldCameraState
            {
                HasSavedPose = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".saved", 0) != 0,
                X = float.Parse(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".x", "0"), inv),
                Y = float.Parse(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".y", "30"), inv),
                Z = float.Parse(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".z", "0"), inv),
                OrthographicSize = float.Parse(
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".zoom", "14"),
                    inv)
            };
        }

        private void SaveToPrefs(WorldCameraState state)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".saved", state.HasSavedPose ? 1 : 0);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".x", state.X.ToString(inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".y", state.Y.ToString(inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".z", state.Z.ToString(inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".zoom", state.OrthographicSize.ToString(inv));
            UnityEngine.PlayerPrefs.Save();
        }
#endif
    }

    public sealed class WorldCameraPersistenceService
    {
        private readonly IWorldCameraStateRepository _repository;
        private readonly WorldMapBounds _bounds;
        private readonly float _defaultX;
        private readonly float _defaultY;
        private readonly float _defaultZ;
        private readonly float _defaultZoom;

        public WorldCameraPersistenceService(
            IWorldCameraStateRepository repository,
            WorldMapBounds? bounds = null,
            float defaultX = 0f,
            float defaultY = 30f,
            float defaultZ = 0f,
            float defaultZoom = 14f)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _bounds = bounds ?? new WorldMapBounds();
            _defaultX = defaultX;
            _defaultY = defaultY;
            _defaultZ = defaultZ;
            _defaultZoom = defaultZoom;
        }

        public WorldCameraState ResolveForRestore()
        {
            var saved = _repository.Load();
            if (saved is { HasSavedPose: true })
            {
                return Clamp(saved);
            }

            return Clamp(new WorldCameraState
            {
                X = _defaultX,
                Y = _defaultY,
                Z = _defaultZ,
                OrthographicSize = _defaultZoom,
                HasSavedPose = false
            });
        }

        public void SavePose(float x, float y, float z, float orthographicSize)
        {
            var state = Clamp(new WorldCameraState
            {
                X = x,
                Y = y,
                Z = z,
                OrthographicSize = orthographicSize,
                HasSavedPose = true
            });
            _repository.Save(state);
        }

        private WorldCameraState Clamp(WorldCameraState state)
        {
            var clamped = _bounds.ClampPosition(new MapPosition(state.X, state.Y, state.Z));
            state.X = clamped.X;
            state.Y = clamped.Y;
            state.Z = clamped.Z;
            if (state.OrthographicSize < 8f)
            {
                state.OrthographicSize = 8f;
            }
            else if (state.OrthographicSize > 28f)
            {
                state.OrthographicSize = 28f;
            }

            return state;
        }
    }
}
