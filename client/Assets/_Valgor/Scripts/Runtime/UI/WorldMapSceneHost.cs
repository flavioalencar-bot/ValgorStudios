using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.UI
{
    public sealed class WorldMapSceneHost : MonoBehaviour
    {
        private void Awake()
        {
            var document = gameObject.GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            BetaUiPanels.ApplyTo(document);

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
