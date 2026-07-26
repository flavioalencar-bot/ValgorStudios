using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.Scenes
{
    /// <summary>
    /// Anexa UIDocument + LoadingScreenController na cena Loading.
    /// </summary>
    public sealed class LoadingSceneHost : MonoBehaviour
    {
        private void Awake()
        {
            _ = gameObject.GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _ = gameObject.GetComponent<LoadingScreenController>() ?? gameObject.AddComponent<LoadingScreenController>();
        }
    }
}
