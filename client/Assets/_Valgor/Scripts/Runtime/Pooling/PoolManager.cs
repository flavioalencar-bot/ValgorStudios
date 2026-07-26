using System.Collections.Generic;
using UnityEngine;

namespace Valgor.Pooling
{
    public sealed class PoolManager : MonoBehaviour
    {
        private readonly Dictionary<Component, object> pools = new();

        public ObjectPool<T> GetPool<T>(T prefab, int initialCapacity = 0) where T : Component
        {
            if (pools.TryGetValue(prefab, out var existing))
                return (ObjectPool<T>)existing;

            var pool = new ObjectPool<T>(prefab, transform, initialCapacity);
            pools.Add(prefab, pool);
            return pool;
        }
    }
}
