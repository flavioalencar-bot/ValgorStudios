using System.Collections.Generic;
using UnityEngine;

namespace Valgor.Heroes.Characters
{
    /// <summary>Maps socket ids to transforms under a hero prefab.</summary>
    public sealed class HeroSocketRegistry : MonoBehaviour
    {
        [SerializeField] private List<SocketEntry> sockets = new();

        [System.Serializable]
        public sealed class SocketEntry
        {
            public string Id;
            public Transform Transform;
        }

        public Transform Get(string socketId)
        {
            for (var i = 0; i < sockets.Count; i++)
            {
                if (sockets[i] != null && sockets[i].Id == socketId)
                    return sockets[i].Transform;
            }

            return null;
        }

        public void Bind(string socketId, Transform transform)
        {
            for (var i = 0; i < sockets.Count; i++)
            {
                if (sockets[i].Id == socketId)
                {
                    sockets[i].Transform = transform;
                    return;
                }
            }

            sockets.Add(new SocketEntry { Id = socketId, Transform = transform });
        }

        public IReadOnlyList<SocketEntry> Entries => sockets;

        public bool HasAllRequired(out List<string> missing)
        {
            missing = new List<string>();
            foreach (var id in HeroSocketIds.Required)
            {
                if (Get(id) == null) missing.Add(id);
            }

            return missing.Count == 0;
        }
    }
}
