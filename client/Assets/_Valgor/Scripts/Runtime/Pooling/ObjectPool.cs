using System.Collections.Generic;
using UnityEngine;

namespace Valgor.Pooling
{
    public sealed class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> inactive = new();

        public ObjectPool(T prefab, Transform parent, int initialCapacity = 0)
        {
            this.prefab = prefab;
            this.parent = parent;
            for (var i = 0; i < initialCapacity; i++) Release(Create());
        }

        public T Get()
        {
            var instance = inactive.Count > 0 ? inactive.Pop() : Create();
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Release(T instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(parent, false);
            inactive.Push(instance);
        }

        private T Create() => Object.Instantiate(prefab, parent);
    }
}
