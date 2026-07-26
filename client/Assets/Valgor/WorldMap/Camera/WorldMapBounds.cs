using System;

namespace Valgor.WorldMap.Camera
{
    public readonly struct MapPosition
    {
        public MapPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
    }

    public sealed class WorldMapBounds
    {
        public WorldMapBounds(float minX = -22f, float maxX = 22f, float minZ = -18f, float maxZ = 22f)
        {
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }

        public float MinX { get; }
        public float MaxX { get; }
        public float MinZ { get; }
        public float MaxZ { get; }

        public MapPosition ClampPosition(MapPosition position) =>
            new(
                Math.Clamp(position.X, MinX, MaxX),
                position.Y,
                Math.Clamp(position.Z, MinZ, MaxZ));
    }
}
