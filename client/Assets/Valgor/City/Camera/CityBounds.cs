using System;

namespace Valgor.City.Camera
{
    [Serializable]
    public sealed class CityBounds
    {
        public CityBounds(float minX = -22f, float maxX = 22f, float minZ = -22f, float maxZ = 22f)
        {
            if (minX > maxX || minZ > maxZ)
            {
                throw new ArgumentException("Os mínimos devem ser menores ou iguais aos máximos.");
            }

            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }

        public float MinX { get; }
        public float MaxX { get; }
        public float MinZ { get; }
        public float MaxZ { get; }

        public CityPosition ClampPosition(CityPosition position) =>
            new(Math.Clamp(position.X, MinX, MaxX), position.Y, Math.Clamp(position.Z, MinZ, MaxZ));

        public bool Contains(CityPosition position) =>
            position.X >= MinX && position.X <= MaxX && position.Z >= MinZ && position.Z <= MaxZ;
    }

    public readonly struct CityPosition
    {
        public CityPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
    }
}
