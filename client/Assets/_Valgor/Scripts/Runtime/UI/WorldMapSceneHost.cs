using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.UI
{
    public sealed class WorldMapSceneHost : MonoBehaviour
    {
        private void Awake()
        {
            if (gameObject.GetComponent<UIDocument>() == null)
            {
                gameObject.AddComponent<UIDocument>();
            }

            var bootstrapType = Type.GetType("Valgor.WorldMap.WorldMapBootstrap, Valgor.WorldMap");
            if (bootstrapType == null)
            {
                throw new InvalidOperationException("Valgor.WorldMap.WorldMapBootstrap não foi encontrado.");
            }

            if (gameObject.GetComponent(bootstrapType) == null)
            {
                gameObject.AddComponent(bootstrapType);
            }
        }
    }
}
