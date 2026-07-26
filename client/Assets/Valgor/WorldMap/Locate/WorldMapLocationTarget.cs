using System;

namespace Valgor.WorldMap.Locate
{
    public enum WorldMapLocationKind
    {
        PlayerHome,
        ActiveMarch,
        SelectedNode,
        Creature,
        Resource
    }

    public sealed class WorldMapLocationTarget
    {
        public WorldMapLocationTarget(
            WorldMapLocationKind kind,
            string id,
            string displayName,
            float x,
            float z)
        {
            Kind = kind;
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName ?? string.Empty;
            X = x;
            Z = z;
        }

        public WorldMapLocationKind Kind { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public float X { get; }
        public float Z { get; }
    }

    public sealed class WorldCameraFocusRequest
    {
        public WorldCameraFocusRequest(float x, float z, float orthographicSize)
        {
            X = x;
            Z = z;
            OrthographicSize = orthographicSize;
        }

        public float X { get; }
        public float Z { get; }
        public float OrthographicSize { get; }
    }
}
