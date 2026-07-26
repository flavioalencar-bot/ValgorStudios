using UnityEngine;
using UnityEngine.UIElements;
using Valgor.World;

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

            if (gameObject.GetComponent<WorldMapBootstrap>() == null)
            {
                gameObject.AddComponent<WorldMapBootstrap>();
            }
        }
    }
}
